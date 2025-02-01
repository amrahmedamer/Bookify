namespace Bookify.Services
{
    public interface IEmailBodyBuilder
    {
        string GetEmailBody(string templateName, Dictionary<string, string> placeholders);
    }
}
