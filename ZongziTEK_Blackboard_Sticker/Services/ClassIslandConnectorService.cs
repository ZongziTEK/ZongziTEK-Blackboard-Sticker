using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using dotnetCampus.Ipc.IpcRouteds.DirectRouteds;
using dotnetCampus.Ipc.Pipes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dotnetCampus.Ipc.Context;
using ZongziTEK_Blackboard_Sticker.Helpers;
using ZongziTEK_Blackboard_Sticker.Interfaces;
using ZongziTEK_Blackboard_Sticker.Shared.IPC;

namespace ZongziTEK_Blackboard_Sticker.Services
{
    public class ClassIslandConnectorService : IManagedService
    {
        public bool IsTimetableSyncEnabled => _isTimetableSyncEnabled;
        public List<Lesson> TimetableShared => _timetableShared;

        private IpcProvider? _ipcProvider;
        private PeerProxy? _peerProxy;
        private IConnectService? _connectService;
        private JsonIpcDirectRoutedProvider? _ipcDirectRoutedProvider;

        private bool _isTimetableSyncEnabled;
        private List<Lesson> _timetableShared = new();

        private void RegisterNotificationHandlers()
        {
            _ipcDirectRoutedProvider!.AddNotifyHandler(
                "ZongziTEK_Blackboard_Sticker_Connector.TimetableUpdated",
                OnClassIslandTimetableUpdated);
            ConsoleHelper.WriteLog("订阅 ClassIsland 课程表变化事件", "info");

            _ipcDirectRoutedProvider!.AddNotifyHandler(
                "ZongziTEK_Blackboard_Sticker_Connector.IsTimetableSyncEnabledChanged",
                OnIsTimetableSyncEnabledChanged);
            ConsoleHelper.WriteLog("订阅 IsTimetableSyncEnabledChanged 事件", "info");
        }

        public async Task StartAsync(CancellationToken _)
        {
            _ipcProvider = new IpcProvider("ZongziTEK_Blackboard_Sticker", new IpcConfiguration { AutoReconnectPeers = true });
            _ipcDirectRoutedProvider = new JsonIpcDirectRoutedProvider(_ipcProvider);

            // add notify handler
            RegisterNotificationHandlers();

            // connect
            _ipcDirectRoutedProvider.StartServer();
            ConsoleHelper.WriteLog("启动 IPC 服务器", "info");

            ConsoleHelper.WriteLog("开始连接 ClassIsland 插件", "info");
            _peerProxy = await _ipcProvider.GetAndConnectToPeerAsync("ZongziTEK_Blackboard_Sticker_Connector");
            _connectService = _ipcProvider.CreateIpcProxy<IConnectService>(_peerProxy);
            ConsoleHelper.WriteLog("连接到 ClassIsland 成功", "info");

            // get initial value
            _isTimetableSyncEnabled = await _connectService.GetIsTimetableSyncEnabled();

            // call methods once
            UpdateMainWindowTimetable();
        }

        public async Task StopAsync(CancellationToken _)
        {
            if (_ipcDirectRoutedProvider != null)
            {
                _ipcDirectRoutedProvider.IpcProvider.Dispose();
                _ipcDirectRoutedProvider = null;
            }
        }

        private async void OnClassIslandTimetableUpdated()
        {
            ConsoleHelper.WriteLog("ClassIsland 课程表变化", "info");
            await UpdateMainWindowTimetable();
        }

        private async void OnIsTimetableSyncEnabledChanged()
        {
            ConsoleHelper.WriteLog("IsTimetableSyncEnabled 变化", "info");
            _isTimetableSyncEnabled = await _connectService.GetIsTimetableSyncEnabled();

            await UpdateMainWindowTimetable();
        }

        private async Task UpdateMainWindowTimetable()
        {
            _timetableShared = await _connectService.GetCurrentTimetable();

            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = App.Current.MainWindow as MainWindow;
                mainWindow.LoadTimetableOrCurriculum();
                ConsoleHelper.WriteLog("由 ClassIsland Connector 更新正在显示的课程表", "info");
            });
        }
    }
}
