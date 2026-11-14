using System.Net;
using System.Net.Mail;
using System.Text;
using oris1.Framework.Attributes;
using oris1.Framework.Handlers;

namespace oris1.Controllers;

[HttpController("home-work")]
public class HomeworkController
{
    public HttpListenerContext Context { get; set; } = null!;

    private const string GroupNumber = "11-541";
    private const string FullName = "Махмутова Алсу Альбертовна";

    private const string SenderEmail = "mahm07@yandex.ru";
    private const string SenderPassword = "omoqyicuwudevvqe";

    private const string ZipFileName = "homework.zip";

    private static string ZipFilePath =>
        Path.Combine(StaticFileHandler.StaticRoot, ZipFileName);

    private static readonly HashSet<string> AllowedDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "gmail.com", "googlemail.com",
        "mail.ru", "inbox.ru", "list.ru", "bk.ru", "internet.ru",
        "yandex.ru", "ya.ru", "yandex.com", "yandex.kz", "yandex.by", "yandex.ua"
    };

    // GET http://localhost:7777/connection/home-work/send?to=name@gmail.com
    // GET http://localhost:7777/connection/home-work/send?to=name@mail.ru
    // GET http://localhost:7777/connection/home-work/send?to=name@yandex.ru
    [Get("send")]
    public void Send(string to)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            Handler.SendText(Context.Response, 400, "Укажите параметр to");
            return;
        }

        MailAddress toAddress;
        try
        {
            toAddress = new MailAddress(to.Trim());
        }
        catch (FormatException)
        {
            Handler.SendText(Context.Response, 400, "Некорректный адрес почты");
            return;
        }

        if (!AllowedDomains.Contains(toAddress.Host))
        {
            Handler.SendText(Context.Response, 400,
                "Поддерживаются только gmail, mail.ru и yandex");
            return;
        }

        Console.WriteLine("Ищу zip по пути: " + ZipFilePath);
        if (!File.Exists(ZipFilePath))
        {
            Handler.SendText(Context.Response, 500, "Zip-архив не найден: " + ZipFilePath);
            return;
        }

        try
        {
            SendMail(toAddress);
            Console.WriteLine("Письмо отправлено: " + toAddress.Address);
            Handler.SendText(Context.Response, 200, "Письмо отправлено на " + toAddress.Address);
        }
        catch (Exception ex)
        {
            string details = ex.InnerException != null
                ? ex.Message + " | " + ex.InnerException.Message
                : ex.Message;
            Console.WriteLine("Ошибка отправки: " + details);
            Handler.SendText(Context.Response, 500, "Ошибка отправки: " + details);
        }
    }

    private static void SendMail(MailAddress toAddress)
    {
        var from = new MailAddress(SenderEmail, "Homework", Encoding.UTF8);

        using var message = new MailMessage(from, toAddress)
        {
            Subject = $"Домашнее задание, {GroupNumber}, {FullName}",
            SubjectEncoding = Encoding.UTF8,
            Body = $"Группа: {GroupNumber}\nФИО: {FullName}",
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = false
        };

        using var attachment = new Attachment(ZipFilePath);
        message.Attachments.Add(attachment);

        using var smtp = new SmtpClient("smtp.yandex.ru", 587)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(SenderEmail, SenderPassword),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 30000
        };
        smtp.Send(message);
    }
}
