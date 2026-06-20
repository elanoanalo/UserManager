using System.Net;
using System.Net.Mail;

namespace UserManager.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string confirmationLink);
    }

    public class EmailService : IEmailService
    {
        public async Task SendConfirmationEmailAsync(string toEmail, string confirmationLink)
        {
            await Task.Run(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"===============================");
                    System.Diagnostics.Debug.WriteLine($"ПИСЬМО ДЛЯ: {toEmail}");
                    System.Diagnostics.Debug.WriteLine($"ССЫЛКА ДЛЯ ПОДТВЕРЖДЕНИЯ: {confirmationLink}");
                    System.Diagnostics.Debug.WriteLine($"===============================");

                    var fromAddress = new MailAddress("gaukhar.kozhikenova@gmail.com", "User Auth System");
                    var toAddress = new MailAddress(toEmail);
                    const string fromPassword = "jeod jxpc bghs gpnz"; 
                    const string subject = "Подтверждение регистрации";
                    string body = $"Привет! Для подтверждения аккаунта перейдите по ссылке: {confirmationLink}";

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                    };
                    using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body })
                    {
                        await smtp.SendMailAsync(message);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка отправки почты: {ex.Message}");
                }
            });
        }
    }
}