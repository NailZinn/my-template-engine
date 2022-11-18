namespace HTML_Engine_Library
{
    public interface IEngineHTMLService
    {
        string GetHTML(string template, object model);
        string GetHTML(Stream templatePath, object model);
        string GetHTML(byte[] bytes, object model);
        Stream GetHTMLInStream(string templatePath, object model);
        Stream GetHTMLInStream(Stream templatePath, object model);
        Stream GetHTMLInStream(byte[] bytes, object model);
        byte[] GetHTMLInBytes(string templatePath, object model);
        byte[] GetHTMLInByte(Stream templatePath, object model);
        byte[] GetHTMLInBytes(byte[] bytes, object model);
        void GenerateAndSaveInDirectory(string templatePath, string outputPath, string outputFileName, object model);
        void GenerateAndSaveInDirectory(Stream templatePath, Stream outputPath, string outputFileName, object model);
        void GenerateAndSaveInDirectory(byte[] templatePath, byte[] outputPath, string outputFileName, object model);
        Task GenerateAndSaveInDirectoryAsync(string templatePath, string outputPath, string outputFileName, object model);
        Task GenerateAndSaveInDirectoryAsync(Stream templatePath, Stream outputPath, string outputFileName, object model);
        Task GenerateAndSaveInDirectoryAsync(byte[] templatePath, byte[] outputPath, string outputFileName, object model);

    }
}