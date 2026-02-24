using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.Email;
using System.Globalization;

	

/// <summary>
/// A batch text formatter that uses an HTML template to format log events.
/// The template must include the placeholder token "{{LogEvents}}".
/// </summary>
public class TemplateBatchTextFormatter : IBatchTextFormatter
{
	/// <summary>
	/// The HTML template. Can be a file path or literal HTML.
	/// </summary>
	public string Template { get; set; }

	/// <summary>
	/// The template for formatting individual log events.
	/// </summary>
	public string RowTemplate { get; set; } =
		"<tr><td>{Timestamp:yyyy-MM-dd HH:mm:ss.fff}</td><td>{Level}</td><td>{Message}</td><td>{Exception}</td></tr>";

	/// <summary>
	/// The format provider for culture-specific formatting.
	/// </summary>
	public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

	// Cache for the full template.
	private string _fullTemplate;

	/// <summary>
	/// Loads the template content from a file if the Template property points to one,
	/// otherwise treats Template as literal content.
	/// </summary>
	private void LoadTemplate()
	{
		if (_fullTemplate != null)
			return;

		if (!string.IsNullOrWhiteSpace(Template) && File.Exists(Template))
		{
			try
			{
				_fullTemplate = File.ReadAllText(Template);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to read the HTML template file '{Template}'.", ex);
			}
		}
		else
		{
			_fullTemplate = Template;
		}
	}

	/// <summary>
	/// Formats a single log event using the RowTemplate.
	/// </summary>
	public void Format(LogEvent logEvent, TextWriter output)
	{
		ArgumentNullException.ThrowIfNull(logEvent);
		ArgumentNullException.ThrowIfNull(output);

		var rowFormatter = new MessageTemplateTextFormatter(RowTemplate, FormatProvider);
		rowFormatter.Format(logEvent, output);
	}

	/// <summary>
	/// Formats a batch of log events by replacing the "{{LogEvents}}" placeholder in the HTML template.
	/// </summary>
	public void FormatBatch(IEnumerable<LogEvent> logEvents, TextWriter output)
	{
		ArgumentNullException.ThrowIfNull(logEvents);
		ArgumentNullException.ThrowIfNull(output);

		LoadTemplate();

		var rowFormatter = new MessageTemplateTextFormatter(RowTemplate, FormatProvider);
		var rows = new List<string>();

		using (var sw = new StringWriter())
		{
			foreach (var logEvent in logEvents)
			{
				sw.GetStringBuilder().Clear();
				rowFormatter.Format(logEvent, sw);
				rows.Add(sw.ToString());
			}
		}

		string logEventsHtml = string.Join(Environment.NewLine, rows);
		string finalHtml = _fullTemplate.Replace("{{LogEvents}}", logEventsHtml);
		output.Write(finalHtml);
	}
}
