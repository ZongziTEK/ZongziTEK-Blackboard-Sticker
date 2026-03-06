using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZongziTEK.BlackboardSticker.Helpers;
using ZongziTEK.BlackboardSticker.Interfaces;

namespace ZongziTEK.BlackboardSticker.Services
{
    public class ServiceManager
    {
        private readonly Dictionary<Type, IManagedService> _services = new();
        private CancellationTokenSource? _cancellationTokenSource = new();

        #region public methods
        public void RegisterService<T>() where T : IManagedService, new()
        {
            var serviceType = typeof(T);
            if (_services.ContainsKey(serviceType))
            {
                ConsoleHelper.WriteLog($"注册服务，但服务已存在，不再注册。服务名称：{serviceType}", "warn");
                return;
            }

            var service = new T();
            _services[serviceType] = service;
            ConsoleHelper.WriteLog($"注册服务。服务名称：{serviceType}", "info");

            if (_cancellationTokenSource != null)
            {
                _ = StartService(service, _cancellationTokenSource.Token);
            }
        }

        public void RemoveService<T>() where T : IManagedService
        {
            var serviceType = typeof(T);
            if (_services.TryGetValue(serviceType, out var service))
            {
                _ = service.StopAsync();
                ConsoleHelper.WriteLog($"停止服务。服务名称：{serviceType}", "info");
                _services.Remove(serviceType);
                ConsoleHelper.WriteLog($"移除服务。服务名称：{serviceType}", "info");
            }
        }

        public async Task RemoveAllServicesAsync()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            foreach (var service in _services.Values)
            {
                await service.StopAsync();
            }

            _services.Clear();
        }

        public T GetService<T>() where T : IManagedService
        {
            var serviceType = typeof(T);

            if (_services.TryGetValue(serviceType, out var service))
            {
                return (T)service;
            }

            return default;
        }
        #endregion

        private async Task StartService(IManagedService service, CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource = new();
                ConsoleHelper.WriteLog($"启动服务开始，服务名称：{service.GetType()}", "info");
                await service.StartAsync(cancellationToken);
                ConsoleHelper.WriteLog($"服务启动完成。服务名称：{service.GetType()}", "info");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLog($"服务在启动时崩溃。服务名称：{service.GetType().FullName}", "error");
                Console.WriteLine("--- 错误信息 ---");
                Console.WriteLine(ex);
                Console.WriteLine("--- 错误信息末尾 ---");
            }
        }
    }
}
