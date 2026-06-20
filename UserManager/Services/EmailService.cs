using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

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
            // Убираем Task.Run, так как SendMailAsync и так асинхронный
            try
            {
                // Заменяем Debug на Console, чтобы Render видел эти строки железно!
                Console.WriteLine("==================================================");
                Console.WriteLine($"ПИСЬМО ДЛЯ: {toEmail}");
                Console.WriteLine($"ССЫЛКА ДЛЯ ПОДТВЕРЖДЕНИЯ: {confirmationLink}");
                Console.WriteLine("==================================================");

                var fromAddress = new MailAddress("gaukhar.kozhikenova@gmail.com", "User Auth System");
                var toAddress = new MailAddress(toEmail);
                const string fromPassword = "jeod jxpc bghs gpnz";
                const string subject = "Подтверждение регистрации";
                string body = $"Привет! Для подтверждения аккаунта перейдите по ссылке: {confirmationLink}";

                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 10000 // Ставим таймаут 10 секунд, чтобы сайт не зависал, если Google блокирует IP
                })
                {
                    using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body })
                    {
                        await smtp.SendMailAsync(message);
                    }
                }

                Console.WriteLine("Блок отправки SMTP отработал без падения кода.");
            }
            catch (Exception ex)
            {
                // Пишем ошибку в стандартный вывод, чтобы поймать её на Render
                Console.WriteLine($"[ОШИБКА SMTP]: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ДЕТАЛИ ОШИБКИ]: {ex.InnerException.Message}");
                }
            }
        }
    }
}