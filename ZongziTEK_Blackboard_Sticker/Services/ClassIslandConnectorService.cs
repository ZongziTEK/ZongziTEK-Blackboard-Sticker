using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using dotnetCampus.Ipc.IpcRouteds.DirectRouteds;
using dotnetCampus.Ipc.Pipes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dotnetCampus.Ipc.Context;
using ZongziTEK.BlackboardSticker.Models;
using ZongziTEK_Blackboard_Sticker.Helpers;
using ZongziTEK_Blackboard_Sticker.Interfaces;
using ZongziTEK.BlackboardSticker.Shared.IPC;

namespace ZongziTEK_Blackboard_Sticker.Services
{
    public class ClassIslandConnectorService : IManagedService
    {
        public bool IsConnected => _isConnected;
        public bool IsTimetableSyncEnabled => _isTimetableSyncEnabled;
        public List<Lesson> TimetableShared => _timetableShared;

        private IpcProvider? _ipcProvider;
        private PeerProxy? _peerProxy;
        private IConnectService? _connectService;
        private JsonIpcDirectRoutedProvider? _ipcDirectRoutedProvider;

        private const double DefaultIslandLineSpacing = 5d;

        private bool _isConnected;
        private bool _isTimetableSyncEnabled;
        private bool _isConnecting;
        private List<Lesson> _timetableShared = new();

        private void RegisterNotificationHandlers()
        {
            _ipcDirectRoutedProvider!.AddNotifyHandler(
                "ZongziTEK_Blackboard_Sticker_Connector.ServiceStarted",
                OnClassIslandPluginConnectionStarted);
            ConsoleHelper.WriteLog("订阅 ClassIsland 插件 ConnectService 启动完毕事件", "info");

            _ipcDirectRoutedProvider!.AddNotifyHandler(
                "ZongziTEK_Blackboard_Sticker_Connector.ServiceStopped",
                OnClassIslandPluginConnectionStopped);
            ConsoleHelper.WriteLog("订阅 ClassIsland 插件 ConnectService 停止事件", "info");

            _ipcDirectRoutedProvider!.AddNotifyHandler(
                "ZongziTEK_Blackboard_Sticker_Connector.TimetableUpdated",
                OnClassIslandTimetableUpdated);
            ConsoleHelper.WriteLog("订阅 ClassIsland 课程表变化事件", "info");

            _ipcDirectRoutedProvider!.AddNotifyHandler(
                "ZongziTEK_Blackboard_Sticker_Connector.IsTimetableSyncEnabledChanged",
                OnIsTimetableSyncEnabledChanged);
            ConsoleHelper.WriteLog("订阅 IsTimetableSyncEnabledChanged 事件", "info");

            _ipcDirectRoutedProvider!.AddNotifyHandler(
                "ZongziTEK_Blackboard_Sticker_Connector.IslandTerritoryChanged",
                OnIslandTerritoryChanged);
            ConsoleHelper.WriteLog("订阅 IslandTerritoryChanged 事件", "info");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _ipcProvider = new IpcProvider("ZongziTEK_Blackboard_Sticker", new IpcConfiguration { AutoReconnectPeers = true });
            _ipcDirectRoutedProvider = new JsonIpcDirectRoutedProvider(_ipcProvider);

            RegisterNotificationHandlers();

            _ipcDirectRoutedProvider.StartServer();
            ConsoleHelper.WriteLog("启动 IPC 服务器", "info");

            if (cancellationToken.IsCancellationRequested) return;

            if (await TryConnectAsync())
            {
                await RefreshClassIslandStateAndUi();
            }
            else
            {
                RestoreMainWindowToLocalState();
            }
        }

        public async Task StopAsync(CancellationToken _)
        {
            OnClassIslandPluginConnectionStopped();

            if (_ipcDirectRoutedProvider != null)
            {
                _ipcDirectRoutedProvider.IpcProvider.Dispose();
                _ipcDirectRoutedProvider = null;
            }
        }

        private async Task OnClassIslandPluginConnectionStarted() // 接收到通知时触发
        {
            ConsoleHelper.WriteLog("ClassIsland 插件 ConnectService 启动完毕事件触发", "info");

            if (_isConnecting)
            {
                return;
            }

            if (await TryConnectAsync())
            {
                await RefreshClassIslandStateAndUi();
            }
            else
            {
                RestoreMainWindowToLocalState();
            }
        }

        private void OnClassIslandPluginConnectionStopped() // 接收到通知时触发
        {
            ConsoleHelper.WriteLog("ClassIsland 插件 ConnectService 停止事件触发", "info");
            ResetConnectionState();
            RestoreMainWindowToLocalState();
        }

        private void RestoreMainWindowToLocalState()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = App.Current.MainWindow as MainWindow;

                if (mainWindow == null) return;

                mainWindow.LoadTimetableOrCurriculum();
                ConsoleHelper.WriteLog("由 ClassIsland Connector 还原为本地课程表", "info");

                mainWindow.Creep(0, true);
                ConsoleHelper.WriteLog("取消避让 ClassIsland 主界面", "info");
            });
        }

        private async void OnClassIslandTimetableUpdated()
        {
            ConsoleHelper.WriteLog("ClassIsland 课程表变化", "info");
            await UpdateMainWindowTimetable();
        }

        private async void OnIsTimetableSyncEnabledChanged()
        {
            ConsoleHelper.WriteLog("IsTimetableSyncEnabled 变化", "info");
            _isTimetableSyncEnabled = await InvokeConnectService(
                service => service.GetIsTimetableSyncEnabled(),
                false,
                nameof(IConnectService.GetIsTimetableSyncEnabled));

            await UpdateMainWindowTimetable();
        }

        private async Task OnIslandTerritoryChanged()
        {
            var islandTerritoryHeight = await InvokeConnectService(
                service => service.GetIslandTerritoryHeight(),
                0d,
                nameof(IConnectService.GetIslandTerritoryHeight));
            var islandDockingLocation = await InvokeConnectService(
                service => service.GetIslandDockingLocation(),
                0,
                nameof(IConnectService.GetIslandDockingLocation));
            var islandLineSpacing = await InvokeOptionalConnectService(
                service => service.GetIslandLineSpacing(),
                DefaultIslandLineSpacing,
                nameof(IConnectService.GetIslandLineSpacing));
            double avoidance = _isConnected
                ? NormalizeAvoidanceValue(islandTerritoryHeight) + NormalizeAvoidanceValue(islandLineSpacing)
                : 0d;
            bool isTop = islandDockingLocation <= 2;

            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = App.Current.MainWindow as MainWindow;
                if (mainWindow == null) return;

                mainWindow.Creep(avoidance, isTop);
            });

            ConsoleHelper.WriteLog("黑板贴避让 ClassIsland 主界面", "info");
        }

        public Task RefreshIslandTerritory()
        {
            return OnIslandTerritoryChanged();
        }

        private async Task<bool> TryConnectAsync()
        {
            if (_ipcProvider == null)
            {
                ResetConnectionState();
                return false;
            }

            if (_isConnecting)
            {
                return false;
            }

            try
            {
                _isConnecting = true;
                ConsoleHelper.WriteLog("开始连接 ClassIsland 插件", "info");
                Task<PeerProxy> connectTask = _ipcProvider.GetAndConnectToPeerAsync("ZongziTEK_Blackboard_Sticker_Connector");
                Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
                Task completedTask = await Task.WhenAny(connectTask, timeoutTask);
                if (completedTask != connectTask)
                {
                    _ = connectTask.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
                    throw new TimeoutException("连接 ClassIsland 插件超时");
                }

                _peerProxy = await connectTask;
                _connectService = _ipcProvider.CreateIpcProxy<IConnectService>(_peerProxy);
                _isConnected = true;
                ConsoleHelper.WriteLog("连接到 ClassIsland 成功", "info");
                return true;
            }
            catch (Exception ex)
            {
                ResetConnectionState();
                ConsoleHelper.WriteLog("连接 ClassIsland 插件失败，等待插件启动通知后重试", "warn");
                Console.WriteLine("--- 错误信息 ---");
                Console.WriteLine(ex);
                Console.WriteLine("--- 错误信息末尾 ---");
                return false;
            }
            finally
            {
                _isConnecting = false;
            }
        }

        private async Task UpdateMainWindowTimetable()
        {
            _timetableShared = await InvokeConnectService(
                service => service.GetCurrentTimetable(),
                _timetableShared,
                nameof(IConnectService.GetCurrentTimetable));

            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = App.Current.MainWindow as MainWindow;
                if (mainWindow == null) return;

                mainWindow.LoadTimetableOrCurriculum();
                ConsoleHelper.WriteLog("由 ClassIsland Connector 更新正在显示的课程表", "info");
            });
        }

        private async Task RefreshClassIslandState()
        {
            _isTimetableSyncEnabled = await InvokeConnectService(
                service => service.GetIsTimetableSyncEnabled(),
                false,
                nameof(IConnectService.GetIsTimetableSyncEnabled));
        }

        private async Task RefreshClassIslandStateAndUi()
        {
            await RefreshClassIslandState();
            await UpdateMainWindowTimetable();
            await OnIslandTerritoryChanged();
        }

        private void ResetConnectionState()
        {
            _isConnected = false;
            _isTimetableSyncEnabled = false;
            _isConnecting = false;
            _connectService = null;
            _peerProxy = null;
        }

        private async Task<T> InvokeConnectService<T>(
            Func<IConnectService, Task<T>> invocation,
            T fallbackValue,
            string operationName)
        {
            if (_connectService == null)
            {
                return fallbackValue;
            }

            try
            {
                var value = await invocation(_connectService);
                _isConnected = true;
                return value;
            }
            catch (Exception ex)
            {
                ResetConnectionState();
                ConsoleHelper.WriteLog($"ClassIsland Connector IPC 调用失败：{operationName}", "warn");
                Console.WriteLine("--- 错误信息 ---");
                Console.WriteLine(ex);
                Console.WriteLine("--- 错误信息末尾 ---");
                return fallbackValue;
            }
        }

        private async Task<T> InvokeOptionalConnectService<T>(
            Func<IConnectService, Task<T>> invocation,
            T fallbackValue,
            string operationName)
        {
            if (_connectService == null)
            {
                return fallbackValue;
            }

            try
            {
                return await invocation(_connectService);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLog($"ClassIsland Connector 可选 IPC 调用失败，使用默认值：{operationName}", "warn");
                Console.WriteLine("--- 错误信息 ---");
                Console.WriteLine(ex);
                Console.WriteLine("--- 错误信息末尾 ---");
                return fallbackValue;
            }
        }

        private static double NormalizeAvoidanceValue(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0) return 0d;
            return value;
        }
    }
}
