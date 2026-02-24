
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoldCasino.ApiModule.Dtos.Providers;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;
using Microsoft.Extensions.Caching.Memory;
using SmartWinners.Models;

namespace SmartWinners.Services;

public class StudiosService(IMemoryCache cache, IPlayerClub365ApiService client)
{
  public async Task<StudiosListModel> Get(string title, int? start = null, int? max = null, string? searchTerm = null, bool showAtRootOnly = false, bool showAtStudiosOnly = false)
  {
    var filter = new Dictionary<string, string>();

    if (!string.IsNullOrEmpty(searchTerm))
      filter.Add("game_name", $"like '%{searchTerm}%'");

    filter.Add("status", "> 0");
    if (showAtRootOnly)
      filter.Add("show_at_root", "1");
    if (showAtStudiosOnly)
      filter.Add("show_at_studios", "1");

    var apiResp = await client.ProvidersGetAsync(new()
    {
      Fields = FieldHelper<ProviderSimpleDto>.Fields,
      Filter = filter,
      LimitFrom = start ?? 0,
      LimitCount = max is null ? 0 : max.Value + 1
    });

    var providers = apiResp.IsSuccess ? apiResp.Value?.Providers : [];

    providers = providers?.Where(p => p.ParentProviderId == 0).ToList();

    return new StudiosListModel
    {
      CarouselName = "Studios",
      List = providers,
      Title = title,
    };
  }

  public async Task<List<Provider>> Get(int? start = null, int? max = null, string? searchTerm = null, bool showAtRootOnly = false, bool showAtStudiosOnly = false)
  {
    var filter = new Dictionary<string, string>();

    if (!string.IsNullOrEmpty(searchTerm))
      filter.Add("ps.provider_name", $"like '%{searchTerm}%'");

    filter.Add("status", "> 0");
    if (showAtRootOnly)
      filter.Add("show_at_root", "1");
    if (showAtStudiosOnly)
      filter.Add("show_at_studios", "1");

    var apiResp = await client.ProvidersGetAsync(new()
    {
      Fields = FieldHelper<ProviderSimpleDto>.Fields,
      Filter = filter,
      LimitFrom = start ?? 0,
      LimitCount = max is null ? 0 : max.Value + 1
    });

    if (apiResp.IsSuccess)
      return [.. (apiResp.Value?.Providers ?? []).Where(p => p.ParentProviderId == 0)];

    return [];
  }

  public async Task<Provider?> GetById(int id)
  {
      var filter = new Dictionary<string, string>
      {
        { "p.provider_id", $"{id}" },
        { "status", "> 0" }
      };

    var apiResp = await client.ProvidersGetAsync(new()
    {
      Fields = FieldHelper<ProviderSimpleDto>.Fields,
      Filter = filter,
      LimitFrom = 0,
      LimitCount = 1
    });

    return apiResp.Value?.Providers?.FirstOrDefault();
  }

  public async Task<List<Provider>> GetChildren(int parentId)
  {
    var filter = new Dictionary<string, string>
    {
      { "parent_providerID", $"{parentId}" },
      { "status", "> 0" }
    };

    var apiResp = await client.ProvidersGetAsync(new()
    {
      Fields = FieldHelper<ProviderSimpleDto>.Fields,
      Filter = filter,
      LimitFrom = 0,
      LimitCount = 0
    });

    return apiResp.IsSuccess ? apiResp.Value?.Providers ?? [] : [];
  }
}