using System.Threading;
using System.Threading.Tasks;

namespace ZongziTEK.BlackboardSticker.Interfaces;

public interface IManagedService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
