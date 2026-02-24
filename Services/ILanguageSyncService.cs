using System.Threading;
using System.Threading.Tasks;

namespace SmartWinners.Services;

public interface ILanguageSyncService
{
    Task SyncLanguagesAsync(CancellationToken cancellationToken);
}
