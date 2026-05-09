namespace Suwayami.Service
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class MyBackgroundService : BackgroundService
    {
        private readonly FileLogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        public MyBackgroundService(FileLogger logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _logger.LogInfoAsync("ExecuteAsync ЗАПУЩЕН! " + DateTime.Now);
            await _logger.LogInfoAsync("ExecuteAsync START");

            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (!await timer.WaitForNextTickAsync(stoppingToken))
                            break;

                        await _logger.LogInfoAsync("ЗАДАЧА ВЫПОЛНЯЕТСЯ! " + DateTime.Now);
                        await _logger.LogInfoAsync("TASK RUN");

                        await Task.Delay(1000, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        await _logger.LogInfoAsync("Задача отменена токеном");
                        break;
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync("ОШИБКА В ЗАДАЧЕЕ", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("КРИТИЧЕСКАЯ ОШИБКА ExecuteAsync", ex);
            }

            await _logger.LogInfoAsync("ExecuteAsync ЗАВЕРШЕН");
        }
    }
}
