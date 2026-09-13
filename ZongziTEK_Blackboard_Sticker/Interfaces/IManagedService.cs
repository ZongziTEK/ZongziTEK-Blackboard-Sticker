using System.Threading;
using System.Threading.Tasks;

namespace ZongziTEK_Blackboard_Sticker.Interfaces
{
    public interface IManagedService
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
