using GoldCasino.ApiModule.Dtos.Languages;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GoldCasino.ApiModule.Services;

public interface ISupportedLanguagesService
{
    Task<IReadOnlyList<Language>> GetAsync(CancellationToken cancellationToken = default);
}

// 1. Inject IServiceScopeFactory instead of the scoped client
public sealed class SupportedLanguagesService(
    IServiceScopeFactory scopeFactory, 
    ILogger<SupportedLanguagesService> logger) : ISupportedLanguagesService
{
    private static readonly Language[] DefaultLanguages =
    [
        new Language { LanguageIso = "en", Name = "English", NameEnglish = "English" },
        new Language { LanguageIso = "fr", Name = "Français", NameEnglish = "French" },
        new Language { LanguageIso = "he", Name = "עברית", NameEnglish = "Hebrew" },
        new Language { LanguageIso = "ru", Name = "Русский", NameEnglish = "Russian" },
        new Language { LanguageIso = "uk", Name = "Українська", NameEnglish = "Ukrainian" },
        new Language { LanguageIso = "es", Name = "Español", NameEnglish = "Spanish" },
        new Language { LanguageIso = "pt", Name = "Português", NameEnglish = "Portuguese" },
        new Language { LanguageIso = "th", Name = "ไทย", NameEnglish = "Thai" },
        new Language { LanguageIso = "vi", Name = "Tiếng Việt", NameEnglish = "Vietnamese" }
    ];
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private readonly SemaphoreSlim _gate = new(1, 1);

    private Language[]? _cachedLanguages;
    private DateTimeOffset _cacheExpiresAt = DateTimeOffset.MinValue;

    public async Task<IReadOnlyList<Language>> GetAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        // Fast path: return cached data if valid
        if (_cachedLanguages is { Length: > 0 } cached && now < _cacheExpiresAt)
            return cached;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            now = DateTimeOffset.UtcNow;
            if (_cachedLanguages is { Length: > 0 } warm && now < _cacheExpiresAt)
                return warm;

            // Fetch fresh data
            var fresh = await FetchLanguagesAsync(cancellationToken);
            
            _cachedLanguages = fresh.Length > 0 ? fresh : DefaultLanguages;
            _cacheExpiresAt = DateTimeOffset.UtcNow.Add(CacheDuration);
            return _cachedLanguages;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<Language[]> FetchLanguagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 2. Create a TEMPORARY scope.
            // This creates a fresh instance of IPlayerClub365ApiService (and the underlying SOAP channel)
            using var scope = scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<IPlayerClub365ApiService>();

            var response = await client.LanguagesGetAsync(new LanguagesGetRequest
            {
                Fields = FieldHelper<LanguageDto>.Fields,
                LimitCount = 0,
                LimitFrom = 0,
            });

            if (response.IsSuccess && response.Value?.Languages is { Count: > 0 })
            {
                var languages = response.Value.Languages
                  .Where(lang => !string.IsNullOrWhiteSpace(lang.LanguageIso))
                  .DistinctBy(lang => lang.LanguageIso.Trim().ToLowerInvariant())
                  .ToArray();

                if (languages.Length > 0)
                    return languages;
            }
            else if (response.Error is not null)
            {
                logger.LogWarning("Failed to fetch languages: {Message}", response.Error.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching supported languages");
        }
        
        // 3. The scope is disposed here, properly closing the SOAP connection.
        return [];
    }
}