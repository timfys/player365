using System;
using System.Collections.Generic;

namespace SmartWinners.Models;

public class UserReviewModel
{
    public List<UserReview> List { get; set; }

    public static UserReviewModel GetTest()
    {
        var templates = new List<UserReview>
        {
            new ()
            {
                UserName = "unwindfan",
                Text = "Best way to unwind! I love the variety of machines. The graphics are stunning and it really feels like being on the floor of a big resort. Great for passing the time!",
                FlagUrl = "\\images\\casino\\canada.png"
            },
            new ()
            {
                UserName = "bonusaddict",
                Text = "Addicted to the daily bonuses! I never run out of coins because the daily login rewards are so generous. The mini-games between rounds keep things really fresh.",
                FlagUrl = "\\images\\casino\\turkey.png"
            },
            new ()
            {
                UserName = "clubchatter",
                Text = "The social aspect is awesome. I joined a club with my friends and we send each other free gifts every day. It's a fun way to stay connected and compete on the leaderboard.",
                FlagUrl = "\\images\\casino\\australia.png"
            },
            new ()
            {
                UserName = "jackpotrush",
                Text = "Incredible animations. The 777 classic slots look so realistic. I love the sound effects when you hit a big jackpot - it's a total rush without any of the stress.",
                FlagUrl = "\\images\\casino\\canada.png"
            },
            new ()
            {
                UserName = "thememaven",
                Text = "So many themes to choose from! From ancient Egypt to underwater adventures, there is a slot for every mood. I've reached level 50 and the new unlocks are amazing.",
                FlagUrl = "\\images\\casino\\turkey.png"
            },
            new ()
            {
                UserName = "casualcommuter",
                Text = "Perfect for casual play. I play this during my commute. It's easy to pick up and put down, and the interface is super smooth on my phone.",
                FlagUrl = "\\images\\casino\\turkey.png"
            },
            new ()
            {
                UserName = "tourneyace",
                Text = "The most fun I've had on an app. The tournament mode is my favorite. Racing against other players to see who can get the highest score in 5 minutes is a blast!",
                FlagUrl = "\\images\\casino\\canada.png"
            },
            new ()
            {
                UserName = "chatchamp",
                Text = "Great community! I've met so many cool people in the global chat. We all share tips on which new machines have the coolest bonus rounds.",
                FlagUrl = "\\images\\casino\\turkey.png"
            },
            new ()
            {
                UserName = "artstylefan",
                Text = "High-quality gaming. You can tell the developers put a lot of work into the art style. Each update brings a brand-new game with unique mechanics.",
                FlagUrl = "\\images\\casino\\australia.png"
            },
            new ()
            {
                UserName = "collectorpro",
                Text = "Just pure entertainment. I love collecting the virtual charms and completing the sticker sets. It gives you a great sense of progression as you play.",
                FlagUrl = "\\images\\casino\\turkey.png"
            },
        };

        var rng = new Random();
        var resultList = new List<UserReview>();

        for (var i = 0; i < 10; i++)
        {
            var template = templates[rng.Next(templates.Count)];

            resultList.Add(new UserReview
            {
                UserName = template.UserName + rng.Next(100, 999), 
                Text = template.Text,
                FlagUrl = template.FlagUrl
            });
        }

        return new UserReviewModel
        {
            List = resultList
        };
    }
}

public class UserReview
{
    public string UserName { get; set; }
    public string Text { get; set; }
    public string FlagUrl { get; set; }
}