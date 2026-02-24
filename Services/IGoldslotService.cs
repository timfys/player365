using PalaceCasino.Agent.Client;
using System.Threading.Tasks;

namespace SmartWinners.Services;

public interface IGoldslotService
{
	public Task<long> GetUserCodeAsync(string accountId);
	public Task<long> RefreshUserCodeAsync(string accountId);
	public Task<_GameUrlResultData> StartGameAsync(string accountId, string gameId);
}