using System.Collections;
using System.Linq.Expressions;
using System.Text;

namespace HTML_Engine_Library
{
    internal static class ExpressionMaker
    {
        private static readonly IEngineHTMLService _engine = new EngineHTMLService();

        public static string ExtractExpression(string template, int position)
        {
            var sb = new StringBuilder();

            while (template[position] != '}')
            {
                sb.Append(template[position]);
                position++;
            }

            return sb.ToString();
        }

        public static dynamic MakeExpression(string template, string expression, object model)
        {
            Expression exp = Expression.Constant(model);

            if (expression.StartsWith("if"))
            {
                var result = MakeConditionalExpression(expression, model);
                var afterIfBlock = template.Split(expression)[1].Remove(0, 2);

                var ifTrueBLock = GetBlock(afterIfBlock,
                    (ifCount, elseCount, endCount, forCount) => elseCount > ifCount || endCount > ifCount, true);

                if (result)
                {
                    return _engine.GetHTML(ifTrueBLock.ToString(), model);
                }
                else
                {
                    var elseBlock = afterIfBlock.Replace(ifTrueBLock, "");
                    if (elseBlock.StartsWith("{{else}}"))
                    {
                        elseBlock = GetBlock(elseBlock.Remove(0, 8),
                            (ifCount, elseCount, endCount, forCount) => endCount > elseCount, true);

                        return _engine.GetHTML(elseBlock, model);
                    }
                    else return "";
                }

            }
            else if (expression.StartsWith("for"))
            {
                var source = MakeLoopExpression(expression, model);
                var body = GetBlock(template.Split(expression)[1].Remove(0, 2),
                    (ifCount, elseCount, endCount, forCount) => endCount > forCount && endCount > ifCount, true);

                var result = new StringBuilder();

                foreach (var item in source)
                {
                    result.Append(_engine.GetHTML(body, item));
                }

                return result.ToString();
            }
            return MakeCallExpression(expression, model, exp);
        }

        public static dynamic MakeCallExpression(string expression, object model, Expression result)
        {
            expression = expression.SkipWhile(c => c != '.').AsString();

            if (string.IsNullOrEmpty(expression))
            {
                if (result.Type.IsValueType)
                    result = Expression.Convert(result, typeof(object));
                return Expression.Lambda<Func<dynamic>>(result).Compile()();
            }

            var properties = model.GetType().GetProperties().Select(p => p.Name);
            var methods = model.GetType().GetMethods();
            var calls = expression[1..];
            var call = calls.Split(".")[0];

            if (properties.Contains(call))
            {
                result = Expression.PropertyOrField(result, call);
            }
            else if (call.EndsWith(']') && properties.Contains(call.Remove(call.Length - 3)))
            {
                var index = int.Parse(call.Replace("[", "").Replace("]", "")[^1].ToString());
                var actualCall = call.Remove(call.Length - 3);

                result = Expression.PropertyOrField(result, actualCall);

                var array = Expression.Lambda<Func<dynamic>>(result).Compile()().ToArray();

                result = Expression.ArrayIndex(
                    Expression.Constant(array),
                    Expression.Constant(index));
            }
            else if (call.EndsWith(')') && methods.Select(m => m.Name).Contains(call.Remove(call.Length - 2)))
            {
                var actualCall = call.Remove(call.Length - 2);

                result = Expression.Call(result, methods.FirstOrDefault(m => m.Name == actualCall));
            }

            if (result.Type.IsValueType)
                result = Expression.Convert(result, typeof(object));
            var newModel = Expression.Lambda<Func<dynamic>>(result).Compile()();

            return MakeCallExpression(calls, newModel, result);
        }

        private static IEnumerable MakeLoopExpression(string expression, object model)
        {
            var body = expression.Replace("for ", "");
            var parts = body.Split(" ");

            var source = MakeCallExpression(parts[2], model, Expression.Constant(model));

            return source;
        }

        private static bool MakeConditionalExpression(string expression, object model)
        {
            var body = expression.Replace("if ", "");
            return Parser.ParseIfCondition(body, model)();
        }

        public static string GetBlock(string template, Func<int, int, int, int, bool> stopPredicate, bool removeEnding)
        {
            var newTemplate = new StringBuilder();
            var ifCount = 0;
            var elseCount = 0;
            var endCount = 0;
            var forCount = 0;

            for (int i = 0; i < template.Length; i++)
            {
                if (stopPredicate(ifCount, elseCount, endCount, forCount))
                {
                    if (elseCount > ifCount && removeEnding)
                        return newTemplate.ToString().Remove(newTemplate.Length - 8);
                    if ((endCount > ifCount || endCount > forCount) && removeEnding)
                        return newTemplate.ToString().Remove(newTemplate.Length - 7);

                    return newTemplate.ToString();
                }
                newTemplate.Append(template[i]);

                if (newTemplate.Length >= 4 && newTemplate.ToString().Substring(newTemplate.Length - 4, 4) == "{{if")
                    ifCount++;
                if (newTemplate.Length >= 8 && newTemplate.ToString().Substring(newTemplate.Length - 8, 8) == "{{else}}")
                    elseCount++;
                if (newTemplate.Length >= 7 && newTemplate.ToString().Substring(newTemplate.Length - 7, 7) == "{{end}}")
                    endCount++;
                if (newTemplate.Length >= 5 && newTemplate.ToString().Substring(newTemplate.Length - 5, 5) == "{{for")
                    forCount++;
            }

            return newTemplate.ToString();
        }

    }
}
