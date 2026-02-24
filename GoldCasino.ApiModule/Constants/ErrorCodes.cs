namespace GoldCasino.ApiModule.Constants
{
  public static class ErrorCodes
  {
    // Generic, transport-agnostic error codes
    public const string UpstreamError = "UPSTREAM_ERROR";       // Any upstream/service failure
    public const string EmptyResponse = "EMPTY_RESPONSE";       // Upstream returned empty body
    public const string ParseError = "PARSE_ERROR";             // Failed to parse upstream payload

    // Authentication-specific error codes
    public const string AuthFailed = "AUTH_FAILED";             // Upstream rejected credentials

    // Database-specific error codes
    public const string DatabaseError = "DATABASE_ERROR";       // Database-related errors

    // Generic validation umbrella (use specific codes below when possible)
    public const string ValidationFailed = "VALIDATION_FAILED";

    // Domain-specific validation codes
    public const string ValidationMissingCountry = "VALIDATION_MISSING_COUNTRY";
    public const string EntityAdd_EmployeeEntityIdInvalid = "ENTITYADD_EMPLOYEE_ENTITYID_INVALID";
  }
}