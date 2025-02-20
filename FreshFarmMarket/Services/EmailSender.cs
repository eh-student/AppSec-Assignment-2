using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using PostmarkDotNet;
using PostmarkDotNet.Model;
using System.Net.Mail;


namespace FreshFarmMarket.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                           ILogger<EmailSender> logger)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

        public AuthMessageSenderOptions Options { get; } 

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrEmpty(Options.PostmarkKey))
            {
                throw new Exception("Null Postmark Token");
            }
            await Execute(Options.PostmarkKey, subject, message, toEmail);
        }

        public async Task Execute(string apiKey, string subject, string message, string toEmail)
        {
            var client = new PostmarkClient(apiKey);
            var msg = new PostmarkMessage()
            {
                To = toEmail,
                From = "233539X@mymail.nyp.edu.sg",
                Subject = subject,
                TextBody = message,
                HtmlBody = message
            };
      

            // Disable click tracking.

            // msg.SetClickTracking(false, false);

            var sendResult = await client.SendMessageAsync(msg);

            _logger.LogInformation(sendResult.Status == PostmarkStatus.Success
                                   ? $"Email to {toEmail} queued successfully!"
                                   : $"Failure Email to {toEmail}");
        }
    }
}
