namespace GoldCasino.ApiModule.Common.Exceptions;

/// <summary>
/// Represents upstream authentication failures (e.g., invalid LID credentials).
/// Allows callers to distinguish authentication issues from generic upstream faults.
/// </summary>
public class AuthenticationServiceException : UpstreamServiceException
{
    public AuthenticationServiceException(string message, int? remoteCode = null, object? extra = null)
        : base(ErrorCodes.AuthFailed, message, remoteCode, extra)
    {
    }

    public AuthenticationServiceException(string message, Exception innerException, int? remoteCode = null, object? extra = null)
        : base(ErrorCodes.AuthFailed, message, innerException, remoteCode, extra)
    {
    }
}
