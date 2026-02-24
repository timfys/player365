namespace GoldCasino.ApiModule.Common.Exceptions;

/// <summary>
/// Represents rare, non-actionable infrastructure failures when calling upstream SOAP services
/// such as empty responses, parse errors, or transport faults. These should be handled globally
/// and mapped to HTTP 502 to avoid duplicating controller logic.
/// </summary>
public class UpstreamServiceException : Exception
{
    public string ErrorCode { get; }
    public int? RemoteCode { get; }
    public object? Extra { get; }

    public UpstreamServiceException(string errorCode, string message, int? remoteCode = null, object? extra = null)
        : base(message)
    {
        ErrorCode = errorCode;
        RemoteCode = remoteCode;
        Extra = extra;
    }

    public UpstreamServiceException(string errorCode, string message, Exception innerException, int? remoteCode = null, object? extra = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        RemoteCode = remoteCode;
        Extra = extra;
    }
}
