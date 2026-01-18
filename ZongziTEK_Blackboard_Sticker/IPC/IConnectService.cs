using dotnetCampus.Ipc.CompilerServices.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZongziTEK_Blackboard_Sticker.Shared.IPC;

[IpcPublic(IgnoresIpcException = true)]
public interface IConnectService
{
    Task<List<Lesson>> GetCurrentTimetable();
    Task<bool> GetIsTimetableSyncEnabled();
    Task<double> GetIslandTerritoryHeight();
    Task<int> GetIslandDockingLocation();
}
