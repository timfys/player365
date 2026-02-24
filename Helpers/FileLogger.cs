using System;
using Microsoft.AspNetCore.Hosting;

namespace SmartWinners.Helpers;

public class FileLogger
{
    public static IWebHostEnvironment Environment { get; set; }

    public static void Log(string fileName, string text)
    {
        var logString = DateTime.Now + text + "\n \n \n \n";

        System.IO.File.AppendAllText(EnvironmentHelper.Environment.WebRootPath.Replace("wwwroot", "") + fileName, logString);
    }
    
    public static void CoinBaseWebHookLog(string body)
    {
        var folderPath = $"{EnvironmentHelper.Environment.WebRootPath}\\CoinbaseLog";

        var filePath = $"{folderPath}\\{DateTime.Now:yyyy-MM-dd}.txt";

        if (!System.IO.File.Exists(filePath))
        {
            var stream = System.IO.File.Create(filePath);
            stream.Close();
        }

        System.IO.File.AppendAllText(filePath, $"{body} \n \n \n \n");
    }
}