using System.Reflection.PortableExecutable;
using System.Text;

namespace HTML_Engine_Library
{
    public class EngineHTMLService : IEngineHTMLService
    {
        public void GenerateAndSaveInDirectory(string templatePath, string outputPath, string outputFileName, object model)
        {
            var template = File.ReadAllText(templatePath);
            var result = GetHTML(template, model);

            File.WriteAllText(Path.Combine(outputPath, outputFileName), result);
        }

        public void GenerateAndSaveInDirectory(Stream templatePath, Stream outputPath, string outputFileName, object model)
        {
            var reader = new StreamReader(templatePath);

            var template = File.ReadAllText(reader.ReadToEnd());
            var result = GetHTML(template, model);

            reader = new StreamReader(outputPath);
            File.WriteAllText(Path.Combine(reader.ReadToEnd(), outputFileName), result);
        }

        public void GenerateAndSaveInDirectory(byte[] templatePath, byte[] outputPath, string outputFileName, object model)
        {
            var template = File.ReadAllText(Encoding.UTF8.GetString(templatePath));
            var result = GetHTML(template, model);

            File.WriteAllText(Path.Combine(Encoding.UTF8.GetString(outputPath), outputFileName), result);
        }

        public Task GenerateAndSaveInDirectoryAsync(string templatePath, string outputPath, string outputFileName, object model)
        {
            var template = File.ReadAllText(templatePath);
            var result = GetHTML(template, model);

            return File.WriteAllTextAsync(Path.Combine(outputPath, outputFileName), result);
        }

        public Task GenerateAndSaveInDirectoryAsync(Stream templatePath, Stream outputPath, string outputFileName, object model)
        {
            var reader = new StreamReader(templatePath);

            var template = File.ReadAllText(reader.ReadToEnd());
            var result = GetHTML(template, model);

            reader = new StreamReader(outputPath);
            return File.WriteAllTextAsync(Path.Combine(reader.ReadToEnd(), outputFileName), result);
        }

        public Task GenerateAndSaveInDirectoryAsync(byte[] templatePath, byte[] outputPath, string outputFileName, object model)
        {
            var template = File.ReadAllText(Encoding.UTF8.GetString(templatePath));
            var result = GetHTML(template, model);

            return File.WriteAllTextAsync(Path.Combine(Encoding.UTF8.GetString(outputPath), outputFileName), result);
        }

        public string GetHTML(string template, object model)
        {
            var result = template;

            for (int i = 0; i < template.Length; i++)
            {
                if (template[i] == '{' && template[i + 1] == '{')
                {
                    var expression = ExpressionMaker.ExtractExpression(template, i + 2);
                    if (expression == "end")
                        continue;
                    if (expression.StartsWith("if"))
                    {
                        var toReplace = $"{{{{{expression}}}}}" + ExpressionMaker.GetBlock(template.Split(expression)[1].Remove(0, 2),
                            (ifCount, elseCount, endCount, forCount) => endCount > ifCount && endCount > forCount, false);
                        result = result.Replace($"{toReplace}", ExpressionMaker.MakeExpression(template, expression, model).ToString());
                        i += toReplace.Length;
                    }
                    else if (expression.StartsWith("for"))
                    {
                        var toReplace = $"{{{{{expression}}}}}" + ExpressionMaker.GetBlock(template.Split(expression)[1].Remove(0, 2),
                            (ifCount, elseCount, endCount, forCount) => endCount > forCount && endCount > ifCount, false);
                        result = result.Replace($"{toReplace}", ExpressionMaker.MakeExpression(template, expression, model).ToString());
                        i += toReplace.Length;
                    }
                    else
                    {
                        result = result.Replace($"{{{{{expression}}}}}", ExpressionMaker.MakeExpression(template, expression, model).ToString());
                        i += $"{{{{{expression}}}}}".Length;
                    }
                }
            }

            return result;
        }

        public string GetHTML(Stream templatePath, object model)
        {
            var reader = new StreamReader(templatePath);
            var template = File.ReadAllText(reader.ReadToEnd());
            return GetHTML(template, model);
        }

        public string GetHTML(byte[] bytes, object model)
        {
            var template = File.ReadAllText(Encoding.UTF8.GetString(bytes));
            return GetHTML(template, model);
        }

        public byte[] GetHTMLInByte(Stream templatePath, object model)
        {
            var reader = new StreamReader(templatePath);
            var template = File.ReadAllText(reader.ReadToEnd());
            return Encoding.UTF8.GetBytes(GetHTML(template, model));
        }

        public byte[] GetHTMLInBytes(string templatePath, object model)
        {
            var template = File.ReadAllText(templatePath);
            return Encoding.UTF8.GetBytes(GetHTML(template, model));
        }

        public byte[] GetHTMLInBytes(byte[] bytes, object model)
        {
            var template = File.ReadAllText(Encoding.UTF8.GetString(bytes));
            return Encoding.UTF8.GetBytes(GetHTML(template, model));
        }

        public Stream GetHTMLInStream(string templatePath, object model)
        {
            var template = File.ReadAllText(templatePath);
            var fileStream = File.OpenWrite(templatePath);

            var bytes = Encoding.UTF8.GetBytes(GetHTML(template, model));

            fileStream.Write(bytes, 0, bytes.Length);

            return fileStream;
        }

        public Stream GetHTMLInStream(Stream templatePath, object model)
        {
            var reader = new StreamReader(templatePath);
            var path = reader.ReadToEnd();
            var template = File.ReadAllText(path);
            var fileStream = File.OpenWrite(path);

            var bytes = Encoding.UTF8.GetBytes(GetHTML(template, model));

            fileStream.Write(bytes, 0, bytes.Length);

            return fileStream;
        }

        public Stream GetHTMLInStream(byte[] bytes, object model)
        {
            var path = Encoding.UTF8.GetString(bytes);
            var template = File.ReadAllText(path);
            var fileStream = File.OpenWrite(path);

            var buffer = Encoding.UTF8.GetBytes(GetHTML(template, model));

            fileStream.Write(buffer, 0, buffer.Length);

            return fileStream;
        }
    }
}
