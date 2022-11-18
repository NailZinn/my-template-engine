using System.Linq.Expressions;

namespace HTML_Engine_Library
{
    internal class Parser
    {
        public static Func<bool> ParseIfCondition(string input, object model)
        {
            var source = ConditionTokenizer.Tokenize(input, model);
            var position = 0;
            var result = RunTokens(source, new Stack<Token>(), ref position, () => false);

            return Expression.Lambda<Func<bool>>(result).Compile();
        }

        private static Expression RunTokens(
            List<Token> source, Stack<Token> operators, ref int position, Func<bool> stopLambda)
        {
            var output = new Stack<Expression>();

            for (;position < source.Count; position++)
            {
                if (source[position].TokenType == TokenType.Number || source[position].TokenType == TokenType.Dynamic)
                    PushExpression(output, source[position]);
                else if (source[position].IsMathOperator)
                {
                    HandleLowerPrecedenceOperatorCase(operators, source[position], output);
                }
                else if (source[position].TokenType == TokenType.Not)
                {
                    // ...
                }
                else if (source[position].IsBoolOperator)
                {
                    var newOperators = new Stack<Token>();
                    var newPosition = position + 1;

                    var left = ForceExpressionComputation(operators, output)();
                    var rightExpression = RunTokens(source, newOperators, ref newPosition,
                        () => newOperators.Count(o => o.Value == ")") != 0 || newPosition + 1 == source.Count ||
                              source[newPosition].Value as string == ")" && source[newPosition + 1].IsBoolOperator);

                    if (rightExpression.Type.IsValueType)
                        rightExpression = Expression.Convert(rightExpression, typeof(object));

                    var right = Expression.Lambda<Func<dynamic>>(rightExpression).Compile()();

                    output.Push(HandleBoolOperatorCase(left, right, source[position]));

                    position = newPosition;

                    if (operators.Count != 0)
                        operators.Pop();
                }
                else if (source[position].TokenType == TokenType.CloseBracket)
                {
                    if (operators.Count == 0)
                        operators.Push(source[position]);
                    else
                        HandleCloseBracketCase(operators, output);
                }
                else
                    operators.Push(source[position]);

                if (stopLambda())
                    return output.Pop();
            }

            foreach (var oper in operators)
                PushExpression(output, oper);

            return output.Pop();
        }

        private static void HandleLowerPrecedenceOperatorCase(Stack<Token> operators, Token token, Stack<Expression> output)
        {
            while (operators.Count > 0 &&
                   ConditionTokenizerHelper.GetOperatorPrecedence(token.TokenType) <=
                   ConditionTokenizerHelper.GetOperatorPrecedence(operators.Peek().TokenType))
                PushExpression(output, operators.Pop());
            operators.Push(token);
        }

        private static Expression HandleBoolOperatorCase(dynamic left, dynamic right, Token boolOperator)
        {
            return boolOperator.TokenType switch
            {
                TokenType.Equal => Expression.Equal(
                    Expression.Constant(left), Expression.Constant(right)),
                TokenType.NotEqual => Expression.NotEqual(
                    Expression.Constant(left), Expression.Constant(right)),
                TokenType.Greater => Expression.GreaterThan(
                    Expression.Constant(left), Expression.Constant(right)),
                TokenType.GreaterOrEqual => Expression.GreaterThanOrEqual(
                    Expression.Constant(left), Expression.Constant(right)),
                TokenType.Less => Expression.LessThan(
                    Expression.Constant(left), Expression.Constant(right)),
                TokenType.LessOrEqual => Expression.LessThanOrEqual(
                    Expression.Constant(left), Expression.Constant(right)),
                TokenType.Or => Expression.Or(
                    Expression.Constant(left), Expression.Constant(right)),
                TokenType.And => Expression.And(
                    Expression.Constant(left), Expression.Constant(right)),
            };
        }

        private static void HandleCloseBracketCase(Stack<Token> operators, Stack<Expression> output)
        {
            while (operators.Peek().TokenType != TokenType.OpenBracket)
                PushExpression(output, operators.Pop());

            operators.Pop();
        }

        private static void PushExpression(Stack<Expression> output, Token token)
        {
            if (!(token.IsMathOperator || token.IsBoolOperator))
            {
                output.Push(
                    Expression.Constant(token.Value));
            }
            else
            {
                switch (token.TokenType)
                {
                    case TokenType.Plus:
                        output.Push(
                            Expression.Add(output.Pop(), output.Pop()));
                        break;
                    case TokenType.Minus:
                        var second = output.Pop();
                        var first = output.Pop();
                        output.Push(
                            Expression.Subtract(first, second));
                        break;
                    case TokenType.Multiply:
                        output.Push(
                            Expression.Multiply(output.Pop(), output.Pop()));
                        break;
                    case TokenType.Divide:
                        var denominator = output.Pop();
                        var numerator = output.Pop();
                        output.Push(
                            Expression.Divide(numerator, denominator));
                        break;
                }
            }
        }

        private static Func<dynamic> ForceExpressionComputation(Stack<Token> operators, Stack<Expression> output)
        {
            foreach (var oper in operators.SkipWhile(token => token.Value == "("))
                PushExpression(output, oper);

            var result = output.Pop();

            if (result.Type.IsValueType)
                result = Expression.Convert(result, typeof(object));

            return Expression.Lambda<Func<dynamic>>(result).Compile();
        }
    }
}
