using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using MimeKit;
using SmartWinners.Configuration;
using SmartWinners.Helpers;
using System;
using System.Text;
using System.Threading.Tasks;


namespace SmartWinners.Middleware;

public class ErrorReportingMiddleware(RequestDelegate next, SmtpClientConfiguration config)
{

    public SmtpClientConfiguration Config { get; } = config;

    public async Task InvokeAsync(HttpContext context)
    {

        /*try
        {*/
            await next(context);
        /*}
        catch (Exception ex)
        {
            return;
            await ReportErrorAsync(context, ex);
            throw;
        }*/

    }

    private async Task ReportErrorAsync(HttpContext context, Exception ex)
    {
        var user = WebStorageUtility.GetSignedUser(context);
        var sb = new StringBuilder();

        AppendException(sb, ex);

        sb.AppendLine("Url string");
        sb.AppendLine($"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}");

        sb.AppendLine("Customer");
        sb.AppendLine($"{user.FirstName} {user.LastName} {user.EntityId}");

        sb.AppendLine("Time on server");
        sb.AppendLine(DateTime.Now.ToString("G"));

        sb.AppendLine("Client IP addresses");
        sb.AppendLine(context.Connection.RemoteIpAddress?.ToString());
        sb.AppendLine();

        sb.AppendLine("Request headers");
        foreach (var h in context.Request.Headers)
            sb.AppendLine($"{h.Key}: {h.Value}");
        sb.AppendLine("");


        try
        {
            var fromAddr = new MailboxAddress(Config.FromName, Config.FromAddress);
            var toAddr = new MailboxAddress(Config.ToName, Config.ToAddress);
            var body = new BodyBuilder { TextBody = sb.ToString() }.ToMessageBody();

            var message = new MimeMessage(new[] { fromAddr }, new[] { toAddr }, Config.Subject, body);

            using var client = new SmtpClient();
            client.Connect(Config.SmtpHost, Config.SmtpPort, Config.SmtpUseSsl);
            client.Authenticate(Config.SmtpLogin, Config.SmtpPassword);
            client.Send(message);
            client.Disconnect(true);
        }
        catch (Exception smtpEx)
        {
            throw;
        }
    }

    private void AppendException(StringBuilder sb, Exception ex)
    {
        sb.AppendLine("Exception Type");
        sb.AppendLine(ex.GetType().Name);
        sb.AppendLine();

        sb.AppendLine("Message");
        sb.AppendLine(ex.Message);
        sb.AppendLine();

        sb.AppendLine("Stack Trace");
        sb.AppendLine(ex.StackTrace);
        sb.AppendLine();

        if (ex.InnerException != null)
            AppendException(sb, ex.InnerException);
    }
}
