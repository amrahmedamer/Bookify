namespace Bookify.Services
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string GetEmailBody(string templateName, Dictionary<string, string> placeholders)
        {
            var pathTemplates = $"{_webHostEnvironment.WebRootPath}/Templates/{templateName}.html";
            StreamReader str = new(pathTemplates);
            var template = str.ReadToEnd();
            str.Close();
            foreach (var placeholder in placeholders)
                template = template.Replace($"[{placeholder.Key}]", $"{placeholder.Value}");

            return template;
        }
    }
}
