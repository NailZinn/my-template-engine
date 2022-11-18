using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace HTML_Engine_Library
{
    internal class ConditionTokenizer
    {
        public static List<Token> Tokenize(string input, object model)
        {
            var list = new List<Token>();
            var sb = new StringBuilder();

            int i = 0;
            while (i < input.Length)
            {
                if (char.IsWhiteSpace(input[i]))
                {
                    i++;
                    continue;
                }

                while (i < input.Length && (char.IsLetter(input[i]) || input[i] == '.'))
                {
                    sb.Append(input[i]);
                    i++;
                }

                if (sb.Length != 0)
                {
                    list.Add(CreateTokenForDynamicElement(sb.ToString(), model));
                    sb = sb.Clear();
                    continue;
                }

                while (i < input.Length && (char.IsDigit(input[i]) || input[i] == '.'))
                {
                    if (i > 0 && input[i - 1] == '-')
                    {
                        list.RemoveAt(list.Count - 1);
                        sb.Append(input[i - 1]);
                    }
                    sb.Append(input[i]);
                    i++;
                }

                if (sb.Length != 0)
                {
                    list.Add(CreateTokenForNumberElement(sb.ToString()));
                    sb = sb.Clear();
                    continue;
                }

                if (i + 1 != input.Length && char.IsWhiteSpace(input[i + 1]) || 
                    input[i] == '(' || input[i] == ')' || input[i] == '-')
                {
                    list.Add(CreateTokenForSingleSymbolElement(input, i));
                    i++;
                    continue;
                }
                else
                {
                    list.Add(CreateTokenForDoubleSymbolOperator(input, i));
                    i += 2;
                }
            }

            return list;
        }

        private static Token CreateTokenForDynamicElement(string expression, object model)
        {
            if (model.GetType().Name.ToLower() == expression.Split(".")[0].ToLower() ||
                model.GetType().IsValueType)
            {
                var exp = Expression.Constant(model);
                var result = ExpressionMaker.MakeCallExpression(expression, model, exp);
                return new Token(TokenType.Dynamic, result);
            }

            return new Token(TokenType.Dynamic, expression);
        }

        private static Token CreateTokenForNumberElement(string expresssion)
        {
            if (Regex.IsMatch(expresssion, @"^(-?)(0|([1-9][0-9]*))(\\.[0-9]+)?$"))
            {
                return expresssion.Contains(".")
                    ? new Token(TokenType.Number, double.Parse(expresssion))
                    : new Token(TokenType.Number, int.Parse(expresssion));
            }

            throw new ArgumentException($"value `{expresssion}` is not number!");
        }

        private static Token CreateTokenForSingleSymbolElement(string expression, int position)
        {
            return expression[position] switch
            {
                '+' => new Token(TokenType.Plus, "+"),
                '-' => new Token(TokenType.Minus, "-"),
                '*' => new Token(TokenType.Multiply, "*"),
                '/' => new Token(TokenType.Divide, "/"),
                '(' => new Token(TokenType.OpenBracket, "("),
                ')' => new Token(TokenType.CloseBracket, ")"),
                '!' => new Token(TokenType.Not, "!"),
                '<' => new Token(TokenType.Less, "<"),
                '>' => new Token(TokenType.Greater, ">"),
                _ => throw new ArgumentException($"operation `{expression[position]}` is not yet supported")
            };
        }

        private static Token CreateTokenForDoubleSymbolOperator(string expression, int position)
        {
            return (expression[position], position + 1 != expression.Length, expression[position + 1]) switch
            {
                ('&', true, '&') => new Token(TokenType.And, "&&"),
                ('|', true, '|') => new Token(TokenType.Or, "||"),
                ('<', true, '=') => new Token(TokenType.LessOrEqual, "<="),
                ('>', true, '=') => new Token(TokenType.GreaterOrEqual, ">="),
                ('=', true, '=') => new Token(TokenType.Equal, "=="),
                ('!', true ,'=') => new Token(TokenType.NotEqual, "!="),
                _ => throw new ArgumentException($"operation `{expression[position]}` is not yet supported")
            };
        }
    }
}
