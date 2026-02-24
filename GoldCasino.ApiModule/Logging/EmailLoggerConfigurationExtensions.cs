using MailKit.Security;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Sinks.Email;
using System.Globalization;
using System.Net;

namespace GoldCasino.ApiModule.Logging;

/// <summary>
/// Configuration options for email logging.
/// </summary>
public class EmailOptions
{
	public string From { get; set; }
	public IEnumerable<string> To { get; set; }
	public string Host { get; set; }
	public int Port { get; set; } = 25;
	public bool IsBodyHtml { get; set; } = false;
	public string Template { get; set; }
	public string RowTemplate { get; set; } =
		"<tr><td>{Timestamp:yyyy-MM-dd HH:mm:ss.fff}</td><td>{Level}</td><td>{Message}</td><td>{Exception}</td></tr>";
	public string UserName { get; set; }
	public string Password { get; set; }
	/// <summary>
	/// Security mode for the SMTP connection (e.g., "None", "SslOnConnect", "StartTls").
	/// </summary>
	public string ConnectionSecurity { get; set; } = "None";
	public string EmailSubject { get; set; } = "Log Email";
	public string FormatterType { get; set; }
}

public static class EmailLoggerConfigurationExtensions
{
	/// <summary>
	/// Configures a Serilog email sink with batching support.
	/// </summary>
	public static LoggerConfiguration EmailWithBatching(
		this LoggerSinkConfiguration loggerConfiguration,
		EmailOptions options,
		BatchingOptions batchingOptions,
		LogEventLevel restrictedToMinimumLevel = LogEventLevel.Error,
		IFormatProvider formatProvider = null)
	{
		ArgumentNullException.ThrowIfNull(loggerConfiguration);
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(batchingOptions);

		// Parse the SMTP connection security.
		if (!Enum.TryParse<SecureSocketOptions>(options.ConnectionSecurity, ignoreCase: true, out var connectionSecurity))
		{
			connectionSecurity = SecureSocketOptions.Auto;
		}

		// Determine the formatter to use.
		ITextFormatter formatter;
		if (!string.IsNullOrWhiteSpace(options.FormatterType))
		{
			var type = Type.GetType(options.FormatterType, throwOnError: false) ??
				throw new ArgumentException($"Cannot load type '{options.FormatterType}'. Ensure that the assembly is referenced and the type name is correct.", nameof(options.FormatterType));
			if (!typeof(IBatchTextFormatter).IsAssignableFrom(type))
			{
				throw new ArgumentException($"Type '{options.FormatterType}' does not implement IBatchTextFormatter.", nameof(options.FormatterType));
			}

			try
			{
				var instance = Activator.CreateInstance(type) as IBatchTextFormatter ??
					throw new InvalidOperationException($"Failed to create an instance of '{options.FormatterType}'.");
				formatter = instance;
				if (formatter is TemplateBatchTextFormatter templateFormatter)
				{
					templateFormatter.Template = options.Template;
					if (!string.IsNullOrWhiteSpace(options.RowTemplate))
					{
						templateFormatter.RowTemplate = options.RowTemplate;
					}
					templateFormatter.FormatProvider = formatProvider ?? templateFormatter.FormatProvider;
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create an instance of '{options.FormatterType}'.", ex);
			}
		}
		else
		{
			// If no custom formatter is specified, use default TemplateBatchTextFormatter.
			formatter = new TemplateBatchTextFormatter
			{
				Template = options.Template ?? "{{LogEvents}}",
				RowTemplate = options.RowTemplate,
				FormatProvider = formatProvider ?? CultureInfo.InvariantCulture
			};
		}

		// Ensure that formatter is not null.
		if (formatter == null)
		{
			throw new InvalidOperationException("The formatter instance was not created successfully.");
		}

		var emailConnectionInfo = new EmailSinkOptions
		{
			From = options.From,
			To = [.. options.To],
			Host = options.Host,
			Port = options.Port,
			ConnectionSecurity = connectionSecurity,
			Body = formatter,
			Subject = new MessageTemplateTextFormatter(options.EmailSubject, formatProvider),
			IsBodyHtml = options.IsBodyHtml,
			Credentials = new NetworkCredential(options.UserName, options.Password)
		};

		return loggerConfiguration.Email(emailConnectionInfo, batchingOptions, restrictedToMinimumLevel);
	}
}
