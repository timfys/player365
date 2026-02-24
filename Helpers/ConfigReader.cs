using System;
using Microsoft.Extensions.Configuration;
using SmartWinners.Configuration;

namespace SmartWinners.Helpers;

public static class ConfigReader
{
    public static bool ReadFromJsonConfig<T>(string fileName, out T config, bool throwError = false) where T : MyConfiguration
    {
        try
        {
            var section = "";

            switch (typeof(T))
            {
                case { } businessApi when businessApi == typeof(BusinessApiConfiguration):
                {
                    section = "BusinessApi";
                    break;
                }
                case { } smartWinnersApi when smartWinnersApi == typeof(SmartWinnersApiConfiguration):
                {
                    section = "SmartWinnersApi";
                    break;
                }
                case { } smtpClient when smtpClient == typeof(SmtpClientConfiguration):
                {
                    section = "ErrorReporting";
                    break;
                }
                case { } paymentConfig when paymentConfig == typeof(PaymentConfiguration):
                {
                    section = "PaymentConfiguration";
                    break;
                }
                case { } telegramConfig when telegramConfig == typeof(TelegramConfiguration):
                {
                    section = "Telegram";
                    break;
                }
                case { } seoConfiguration when seoConfiguration == typeof(SeoConfiguration):
                {
                    section = "SeoConfiguration";
                    break;
                }
                case { } whatsAppConfig when whatsAppConfig == typeof(WhatsAppConfiguration):
                {
                    section = "WhatsAppConfiguration";
                    break;
                }
                case { } stripeConfiguration when stripeConfiguration == typeof(StripeConfiguration):
                {
                    section = "StripeConfiguration";
                    break;  
                }
                case { } casinoGameApiConfiguration when casinoGameApiConfiguration == typeof(CasinoGamesApiConfiguration):
                {
                    section = "CasinoGamesApi";
                    break;  
                }
                default:
                {
                    config = null;
                    return false;
                }
            }

            var configManager = new ConfigurationBuilder()
                .SetBasePath($"{EnvironmentHelper.Environment.WebRootPath}/appsettings").AddJsonFile(fileName)
                .Build();

            var jsonSection = configManager.GetSection(section);

            config = jsonSection.Get<T>();

            return true;
        }
        catch (Exception e)
        {
            config = default;

            if (throwError)
                throw e;

            return false;
        }
    }
}