using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using System.Net;

namespace AgapayAidSystem.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public string SenderEmail { get; set; }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Message { get; set; }

        public void OnGet()
        {
        }

        public void OnPost()
        {
            // Your email configuration
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("agapayaidproject@gmail.com", "mdaitttqkqolujdz"),
                EnableSsl = true,
            };

            // Email content
            var mailMessage = new MailMessage
            {
                From = new MailAddress(SenderEmail), // Use the sender's email from the form
                Subject = "Message from AgapayAid Contact Form",
                Body = $"Name: {Name}\n\nMessage: {Message}",
                IsBodyHtml = false,
            };

            // Recipient email address
            mailMessage.To.Add("agapayaidproject@gmail.com");

            try
            {
                // Send the email
                smtpClient.Send(mailMessage);
                _logger.LogInformation("Email sent successfully.");
                Response.Redirect("/about?success=true");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email.");
                Response.Redirect("/about?error=" + ex.Message);
            }
        }
    }
}