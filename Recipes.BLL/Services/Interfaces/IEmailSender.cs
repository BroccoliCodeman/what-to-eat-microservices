namespace Recipes.BLL.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string recipientEmail, string subject, string body);
    }
}