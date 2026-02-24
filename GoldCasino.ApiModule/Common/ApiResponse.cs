namespace GoldCasino.ApiModule.Common;

public class ApiResponse
{
  public int ResultCode { get; set; }
	public string? ResultMessage { get; set; }

	public bool IsOk => ResultCode >= 0;
}
