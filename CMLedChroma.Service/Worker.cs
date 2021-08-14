using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChromaBroadcast;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sander0542.CMLedController.Abstractions;
using Sander0542.CMLedController.Extensions;

namespace CMLedChroma.Service
{
    public class Worker : BackgroundService
    {
        private static readonly Guid ChromeBroadcastGuid = Guid.Empty;

        private readonly ILogger<Worker> _logger;
        private readonly ILedControllerProvider _ledControllerProvider;

        private IEnumerable<ILedControllerDevice> _ledControllerDevices;

        public Worker(ILogger<Worker> logger, ILedControllerProvider ledControllerProvider)
        {
            _logger = logger;
            _ledControllerProvider = ledControllerProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var result = RzChromaBroadcastAPI.Init(ChromeBroadcastGuid);

            if (result == RzResult.Success)
            {
                RzChromaBroadcastAPI.RegisterEventNotification(OnChromaBroadcastEvent);
                _logger.LogInformation("Razer Chroma Broadcast API successfully initialized");
            }
            else
            {
                _logger.LogError($"Could not initialize Razer Chroma Broadcast API ({result})");
                return;
            }

            _ledControllerDevices = await _ledControllerProvider.GetControllersAsync(stoppingToken);

            if (!_ledControllerDevices.Any())
            {
                _logger.LogWarning("There are not RGB Led Controllers installed");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Unregistering Razer Chroma Broadcast API");
            RzChromaBroadcastAPI.UnRegisterEventNotification();
            RzChromaBroadcastAPI.UnInit();

            await base.StopAsync(cancellationToken);
        }

        private RzResult OnChromaBroadcastEvent(RzChromaBroadcastType type, RzChromaBroadcastStatus? status, RzChromaBroadcastEffect? effect)
        {
            switch (type)
            {
                case RzChromaBroadcastType.BroadcastEffect:
                    if (effect.HasValue)
                    {
                        foreach (var device in _ledControllerDevices)
                        {
                            device.SetMultipleColorAsync(effect.Value.ChromaLink1, effect.Value.ChromaLink2, effect.Value.ChromaLink3, effect.Value.ChromaLink4).Start();
                        }
                    }
                    break;
                case RzChromaBroadcastType.BroadcastStatus:
                    switch (status)
                    {
                        case RzChromaBroadcastStatus.Live:
                            _logger.LogInformation("Razer Chroma Broadcast API is live");
                            break;
                        case RzChromaBroadcastStatus.NotLive:
                            _logger.LogInformation("Razer Chroma Broadcast API is not live");
                            break;
                    }
                    break;
            }

            return RzResult.Success;
        }
    }
}
