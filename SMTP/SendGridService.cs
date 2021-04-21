using InStockChecker.Utility;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Configuration;
using System.Threading.Tasks;

namespace SMTP
{
    public class SendGridService
    {
        private readonly string API_KEY = ConfigurationManager.AppSettings.Get(AppConfig.SendGridKey);
        private readonly string SEND_FROM_EMAIL = ConfigurationManager.AppSettings.Get(AppConfig.SendGridSendFromEmail);
        private readonly string SEND_FROM_NAME = ConfigurationManager.AppSettings.Get(AppConfig.SendGridSendFromName);

        public Task<Response> SendEmail(string subject, string plainTextBody, string htmlBody, EmailAddress recipient)
        {
            SendGridClient client = new SendGridClient(API_KEY);
            EmailAddress from = new EmailAddress(SEND_FROM_EMAIL, SEND_FROM_NAME);

            SendGridMessage msg = MailHelper.CreateSingleEmail(from, recipient, subject, plainTextBody, htmlBody);

            return client.SendEmailAsync(msg);
        }
    }
}