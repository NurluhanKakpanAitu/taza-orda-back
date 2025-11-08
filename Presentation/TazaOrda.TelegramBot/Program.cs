using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TazaOrda.TelegramBot.Configuration;
using TazaOrda.TelegramBot.Handlers;
using TazaOrda.TelegramBot.Services;

namespace TazaOrda.TelegramBot;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                      .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                // Конфигурация
                var botConfig = new BotConfiguration();
                context.Configuration.GetSection(BotConfiguration.SectionName).Bind(botConfig);
                services.AddSingleton(botConfig);

                // Telegram Bot
                services.AddSingleton<ITelegramBotClient>(sp =>
                {
                    var config = sp.GetRequiredService<BotConfiguration>();
                    return new TelegramBotClient(config.Token);
                });

                // Сервисы
                services.AddSingleton<StateManager>();
                services.AddHttpClient<TazaOrdaApiClient>();

                // Обработчики
                services.AddSingleton<UpdateHandler>();

                // Background service для бота
                services.AddHostedService<TelegramBotHostedService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        await host.RunAsync();
    }
}

/// <summary>
/// Hosted service для запуска Telegram бота
/// </summary>
public class TelegramBotHostedService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler;
    private readonly ILogger<TelegramBotHostedService> _logger;
    private readonly StateManager _stateManager;

    public TelegramBotHostedService(
        ITelegramBotClient botClient,
        UpdateHandler updateHandler,
        ILogger<TelegramBotHostedService> logger,
        StateManager stateManager)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _logger = logger;
        _stateManager = stateManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("Telegram бот запущен: @{BotUsername}", me.Username);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<Telegram.Bot.Types.Enums.UpdateType>(),
            DropPendingUpdates = true
        };

        // Запуск очистки неактивных состояний каждый час
        var cleanupTimer = new Timer(
            _ => _stateManager.CleanupInactiveStates(),
            null,
            TimeSpan.FromHours(1),
            TimeSpan.FromHours(1));

        try
        {
            await _botClient.ReceiveAsync(
                _updateHandler,
                receiverOptions,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Бот остановлен");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка в работе бота");
        }
        finally
        {
            cleanupTimer.Dispose();
        }
    }
}