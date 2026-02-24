using System;
using System.Collections.Generic;
using SmartWinners.Helpers;
using Umbraco.Cms.Web.Common;

namespace SmartWinners.Models;

public class TopWinnersListModel
{
    public int Count { get; set; } = 20;
    public TopWinnersFilterType FilterType { get; set; }
    
    public List<TopWinnersModel> List { get; set; }
    
    public static TopWinnersListModel GetTest(UmbracoHelper umbracoHelper)
    {
        var rnd = new Random();
        var topWinners = new List<TopWinnersModel>();
        
        if (false)
            return null;

        for (int i = 0; i < 50; i++)
        {
            var gameName = i % 2 == 0
                ? "Howling Wolves"
                : "Mega Booming Diamonds";
            var gameUrl = i % 2 == 0
                ? "/images/casino/winning-game.png"
                : "/images/casino/winning-game-2.png";
            var userName = $"user{i + 1:000}****";
            var wonDate = DateTime.UtcNow.AddMinutes(-rnd.Next(1, 1440)); // last 24 hours
            var betUsd = Math.Round((decimal)(rnd.NextDouble() * 50 + 0.5), 2);
            var profitUsd = Math.Round(betUsd * (decimal)(rnd.NextDouble() * 10 + 1), 2);
            var multiplier = Math.Round(betUsd * (decimal)(rnd.NextDouble() * 10 + 1), 2);
            
            var wonTimeSpan = DateTime.UtcNow - wonDate;
            
            topWinners.Add(new TopWinnersModel
            {
                GameUrl = gameUrl,
                GameName = gameName,
                UserName = userName,
                WonDate = wonDate,
                BetUsd = betUsd,
                ProfitUsd = profitUsd,
                Multiplayer = multiplier,
                TimeString = wonTimeSpan.Seconds < 60 ? umbracoHelper.GetDictionaryValue("a few seconds ago") : $"{wonDate.ToString("ddd dd yyyy HH:mm", WebStorageUtility.GetUserCultureInfo())}"
            });
        }

        return new TopWinnersListModel
        {
            List = topWinners
        };
    }
    
    public static TopWinnersModel GetOneTest(UmbracoHelper umbracoHelper)
    {
        var rnd = new Random();

        var gameName = rnd.NextInt64() % 2 == 0
            ? "Howling Wolves"
            : "Mega Booming Diamonds";
        var gameUrl = rnd.NextInt64() % 2 == 0
            ? "/images/casino/winning-game.png"
            : "/images/casino/winning-game-2.png";
        var userName = $"user{rnd.NextInt64() + 1:000}****";
        var wonDate = DateTime.UtcNow.AddMinutes(-rnd.Next(1, 1440)); // last 24 hours
        var betUsd = Math.Round((decimal)(rnd.NextDouble() * 50 + 0.5), 2);
        var profitUsd = Math.Round(betUsd * (decimal)(rnd.NextDouble() * 10 + 1), 2);
        var multiplier = Math.Round(betUsd * (decimal)(rnd.NextDouble() * 10 + 1), 2);

        var wonTimeSpan = DateTime.UtcNow - wonDate;

        return new TopWinnersModel
        {
            GameUrl = gameUrl,
            GameName = gameName,
            UserName = userName,
            WonDate = wonDate,
            BetUsd = betUsd,
            ProfitUsd = profitUsd,
            Multiplayer = multiplier,
            TimeString = wonTimeSpan.Seconds < 60 ? umbracoHelper.GetDictionaryValue("a few seconds ago") : $"{wonDate.ToString("ddd dd yyyy HH:mm", WebStorageUtility.GetUserCultureInfo())}"
        };
        
    }
}

public class TopWinnersModel
{
    public string GameUrl { get; set; }
    public string GameName { get; set; }
    public string UserName { get; set; }
    public string GameImage { get; set; }
    public string CountryISO { get; set; }
    public DateTime WonDate { get; set; }
    public decimal BetUsd { get; set; }
    public decimal ProfitUsd { get; set; }
    public decimal Multiplayer { get; set; }
    public string TimeString { get; set; }
}

public enum TopWinnersFilterType
{
    Latest,
    BigWins,
    TopMultiplayers
}