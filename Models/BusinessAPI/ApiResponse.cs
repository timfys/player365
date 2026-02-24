namespace SmartWinners.Models.BusinessAPI;

	public class ApiResponse
	{
		public int ResultCode { get; set; }
		public int ExecuteTime { get; set; }
		public string? ResultMessage { get; set; }
		public bool IsSuccess() => ResultCode == 0;
	}
