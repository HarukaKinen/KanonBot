#pragma warning disable IDE0044 // 添加只读修饰符
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8604 // 解引用可能出现空引用。

using System.Net.Mail;

namespace KanonBot;
public static class Mail
{
    private static Config.Base config = Config.inner!;
    public class MailStruct
    {
        public required Arr<String> MailTo; //收件人，可添加多个
        public Arr<String> MailCC = Empty; //抄送人，不建议添加
        public required string Subject; //标题
        public required string Body; //正文
        public required bool IsBodyHtml;
    }
    public static void Send(MailStruct ms)
    {
        MailMessage message = new();
        if (ms.MailTo.Length == 0) return;
        ms.MailTo.Iter(s => message.To.Add(s)); //设置收件人
        ms.MailCC.Iter(s => message.CC.Add(s)); //设置发件人
        message.From = new MailAddress(config.mail.userName); //设置发件人
        message.Subject = ms.Subject;
        message.Body = ms.Body;
        message.IsBodyHtml = ms.IsBodyHtml;
        var client = new SmtpClient(config.mail.smtpHost, config.mail.smtpPort)
        {
            Credentials = new System.Net.NetworkCredential(config.mail.userName, config.mail.passWord), //设置邮箱用户名与密码
            EnableSsl = true //启用SSL
        }; //设置邮件服务器
        client.Send(message); //发送
    }
}
