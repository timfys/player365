using System;

namespace SmartWinners.Helpers;

	public class FormatsHelper
	{
		public static string ConvertCentsToNormal(long amount)
		{
			return $"{Math.Round(Convert.ToDecimal(amount) / 100, 2):0.00}";
		}
	}
