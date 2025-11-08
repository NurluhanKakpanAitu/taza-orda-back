using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TazaOrda.Domain.DTOs.Reports;
using TazaOrda.Domain.Enums;
using TazaOrda.TelegramBot.Configuration;
using TazaOrda.TelegramBot.Models;
using TazaOrda.TelegramBot.Services;

namespace TazaOrda.TelegramBot.Handlers;

public class UpdateHandler : IUpdateHandler
{
    private readonly ILogger<UpdateHandler> _logger;
    private readonly BotConfiguration _config;
    private readonly StateManager _stateManager;
    private readonly TazaOrdaApiClient _apiClient;

    public UpdateHandler(
        ILogger<UpdateHandler> logger,
        BotConfiguration config,
        StateManager stateManager,
        TazaOrdaApiClient apiClient)
    {
        _logger = logger;
        _config = config;
        _stateManager = stateManager;
        _apiClient = apiClient;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var handler = update switch
            {
                { Message: { } message } => HandleMessageAsync(botClient, message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => HandleCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
                _ => Task.CompletedTask
            };

            await handler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, Telegram.Bot.Polling.HandleErrorSource source, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError("Polling error from {Source}: {ErrorMessage}", source, errorMessage);
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message.Text is not { } messageText)
        {
            // Обработка других типов сообщений (фото, локация и т.д.)
            await HandleNonTextMessageAsync(botClient, message, cancellationToken);
            return;
        }

        var chatId = message.Chat.Id;
        _logger.LogInformation("Received message from {ChatId}: {Message}", chatId, messageText);

        // Обработка команд
        if (messageText.StartsWith('/'))
        {
            await HandleCommandAsync(botClient, message, cancellationToken);
            return;
        }

        // Обработка текста в зависимости от состояния диалога
        var state = _stateManager.GetOrCreateState(chatId);
        await HandleConversationStateAsync(botClient, message, state, cancellationToken);
    }

    private async Task HandleCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var command = message.Text!.Split(' ')[0].ToLowerInvariant();

        switch (command)
        {
            case "/start":
                await HandleStartCommandAsync(botClient, chatId, cancellationToken);
                break;

            case "/register":
                await HandleRegisterCommandAsync(botClient, chatId, cancellationToken);
                break;

            case "/report":
                await HandleReportCommandAsync(botClient, chatId, cancellationToken);
                break;

            case "/events":
                await HandleEventsCommandAsync(botClient, chatId, cancellationToken);
                break;

            case "/myevents":
                await HandleMyEventsCommandAsync(botClient, chatId, cancellationToken);
                break;

            case "/myreports":
                await HandleMyReportsCommandAsync(botClient, chatId, cancellationToken);
                break;

            case "/cancel":
                await HandleCancelCommandAsync(botClient, chatId, cancellationToken);
                break;

            case "/help":
                await HandleHelpCommandAsync(botClient, chatId, cancellationToken);
                break;

            // Админ-команды
            case "/admin":
                if (IsAdmin(chatId))
                    await HandleAdminCommandAsync(botClient, chatId, cancellationToken);
                break;

            default:
                await botClient.SendTextMessageAsync(
                    chatId,
                    "Неизвестная команда. Используйте /help для списка доступных команд.",
                    cancellationToken: cancellationToken);
                break;
        }
    }

    private async Task HandleStartCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "📝 Создать обращение", "📋 Мои обращения" },
            new KeyboardButton[] { "🎉 События", "🎯 Мои события" },
            new KeyboardButton[] { "ℹ️ Помощь" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId,
            "🌟 Добро пожаловать в TazaOrda!\n\n" +
            "Я помогу вам:\n" +
            "• Сообщить о проблемах с чистотой в городе\n" +
            "• Участвовать в субботниках и акциях\n" +
            "• Получать награды за активность\n\n" +
            "Выберите действие или используйте команды:\n" +
            "/register - Регистрация\n" +
            "/report - Создать обращение\n" +
            "/events - Список событий\n" +
            "/help - Помощь",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleRegisterCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        _stateManager.UpdateState(chatId, state =>
        {
            state.State = ConversationState.AwaitingRegistrationName;
            state.Data["registration"] = new RegistrationData();
        });

        await botClient.SendTextMessageAsync(
            chatId,
            "Для регистрации введите ваше имя и фамилию через пробел.\n" +
            "Например: Иван Петров\n\n" +
            "Для отмены используйте /cancel",
            cancellationToken: cancellationToken);
    }

    private async Task HandleReportCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var state = _stateManager.GetOrCreateState(chatId);

        if (state.UserId == null)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Для создания обращения необходимо зарегистрироваться.\n" +
                "Используйте /register",
                cancellationToken: cancellationToken);
            return;
        }

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🗑️ Переполненный бак", "category_OverflowingBin"),
                InlineKeyboardButton.WithCallbackData("🚮 Свалка мусора", "category_IllegalDump")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🧹 Мусор на улице", "category_StreetLitter"),
                InlineKeyboardButton.WithCallbackData("💔 Поломанный контейнер", "category_DamagedContainer")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("❄️ Неубранный снег/лёд", "category_SnowIce"),
                InlineKeyboardButton.WithCallbackData("❓ Другое", "category_Other")
            }
        });

        _stateManager.UpdateState(chatId, s =>
        {
            s.State = ConversationState.AwaitingReportCategory;
            s.Data["report"] = new ReportData();
        });

        await botClient.SendTextMessageAsync(
            chatId,
            "📝 Создание обращения\n\n" +
            "Выберите категорию проблемы:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleEventsCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var events = await _apiClient.GetActiveEventsAsync();

        if (events == null || events.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "📅 Сейчас нет активных событий.\nСледите за обновлениями!",
                cancellationToken: cancellationToken);
            return;
        }

        var buttons = events.Select(e =>
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{e.Title} ({e.ParticipantsCount} участников)",
                    $"event_view_{e.Id}")
            }).ToArray();

        var keyboard = new InlineKeyboardMarkup(buttons);

        await botClient.SendTextMessageAsync(
            chatId,
            "🎉 Активные события:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleMyEventsCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var state = _stateManager.GetOrCreateState(chatId);
        if (state.UserId == null)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Для просмотра событий необходимо зарегистрироваться.\n" +
                "Используйте /register",
                cancellationToken: cancellationToken);
            return;
        }

        var userToken = state.Data.TryGetValue("token", out var token) ? token as string : null;
        if (userToken == null)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Ошибка авторизации. Пожалуйста, зарегистрируйтесь снова.",
                cancellationToken: cancellationToken);
            return;
        }

        var events = await _apiClient.GetUserEventsAsync(userToken);

        if (events == null || events.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "🎯 Вы пока не участвуете ни в одном событии.\n" +
                "Используйте /events для просмотра доступных событий.",
                cancellationToken: cancellationToken);
            return;
        }

        var message = "🎯 Ваши события:\n\n" +
                      string.Join("\n\n", events.Select(e =>
                          $"• {e.Title}\n" +
                          $"  📅 {e.StartAt:dd.MM.yyyy HH:mm}\n" +
                          $"  👥 {e.ParticipantsCount} участников\n" +
                          $"  💰 Награда: {e.CoinReward} монет"));

        await botClient.SendTextMessageAsync(
            chatId,
            message,
            cancellationToken: cancellationToken);
    }

    private async Task HandleMyReportsCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var state = _stateManager.GetOrCreateState(chatId);
        if (state.UserId == null)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Для просмотра обращений необходимо зарегистрироваться.\n" +
                "Используйте /register",
                cancellationToken: cancellationToken);
            return;
        }

        var userToken = state.Data.TryGetValue("token", out var token) ? token as string : null;
        if (userToken == null || state.UserId == null)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Ошибка авторизации. Пожалуйста, зарегистрируйтесь снова.",
                cancellationToken: cancellationToken);
            return;
        }

        var reports = await _apiClient.GetUserReportsAsync(state.UserId.Value, userToken);

        if (reports == null || reports.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "📋 У вас пока нет обращений.\n" +
                "Используйте /report для создания нового обращения.",
                cancellationToken: cancellationToken);
            return;
        }

        var message = "📋 Ваши обращения:\n\n" +
                      string.Join("\n\n", reports.Select(r =>
                          $"• {r.Category}\n" +
                          $"  📍 {r.Address}\n" +
                          $"  📊 Статус: {r.Status}\n" +
                          $"  📅 {r.CreatedAt:dd.MM.yyyy}"));

        await botClient.SendTextMessageAsync(
            chatId,
            message,
            cancellationToken: cancellationToken);
    }

    private async Task HandleCancelCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        _stateManager.ResetConversation(chatId);

        await botClient.SendTextMessageAsync(
            chatId,
            "❌ Операция отменена.\n" +
            "Используйте /start для возврата в главное меню.",
            cancellationToken: cancellationToken);
    }

    private async Task HandleHelpCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var helpText = "ℹ️ Справка по командам:\n\n" +
                       "📱 Основные команды:\n" +
                       "/start - Главное меню\n" +
                       "/register - Регистрация в системе\n" +
                       "/report - Создать обращение\n" +
                       "/myreports - Мои обращения\n\n" +
                       "🎉 События:\n" +
                       "/events - Список активных событий\n" +
                       "/myevents - Мои события\n\n" +
                       "🛠️ Другое:\n" +
                       "/cancel - Отменить текущую операцию\n" +
                       "/help - Показать эту справку\n\n" +
                       "Для получения дополнительной помощи свяжитесь с поддержкой.";

        await botClient.SendTextMessageAsync(
            chatId,
            helpText,
            cancellationToken: cancellationToken);
    }

    private async Task HandleAdminCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📊 Статистика", "admin_stats"),
                InlineKeyboardButton.WithCallbackData("📝 Новые обращения", "admin_new_reports")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🎉 Управление событиями", "admin_events"),
                InlineKeyboardButton.WithCallbackData("👥 Пользователи", "admin_users")
            }
        });

        await botClient.SendTextMessageAsync(
            chatId,
            "🔧 Панель администратора\n\nВыберите действие:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleConversationStateAsync(ITelegramBotClient botClient, Message message, UserState state, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var text = message.Text ?? string.Empty;

        switch (state.State)
        {
            case ConversationState.AwaitingRegistrationName:
                await HandleRegistrationNameAsync(botClient, chatId, text, cancellationToken);
                break;

            case ConversationState.AwaitingRegistrationPhone:
                await HandleRegistrationPhoneAsync(botClient, chatId, text, cancellationToken);
                break;

            case ConversationState.AwaitingReportDescription:
                await HandleReportDescriptionAsync(botClient, chatId, text, cancellationToken);
                break;

            default:
                await botClient.SendTextMessageAsync(
                    chatId,
                    "Используйте команды для взаимодействия с ботом.\n" +
                    "Введите /help для списка доступных команд.",
                    cancellationToken: cancellationToken);
                break;
        }
    }

    private async Task HandleRegistrationNameAsync(ITelegramBotClient botClient, long chatId, string text, CancellationToken cancellationToken)
    {
        var parts = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Пожалуйста, введите имя и фамилию через пробел.\n" +
                "Например: Иван Петров",
                cancellationToken: cancellationToken);
            return;
        }

        var firstName = parts[0];
        var lastName = string.Join(" ", parts.Skip(1));

        _stateManager.UpdateState(chatId, state =>
        {
            var regData = state.Data["registration"] as RegistrationData ?? new RegistrationData();
            regData.FirstName = firstName;
            regData.LastName = lastName;
            state.Data["registration"] = regData;
            state.State = ConversationState.AwaitingRegistrationPhone;
        });

        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            KeyboardButton.WithRequestContact("📱 Отправить номер телефона")
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId,
            $"Отлично, {firstName}!\n\n" +
            "Теперь отправьте ваш номер телефона, используя кнопку ниже, " +
            "или введите вручную в формате: +7XXXXXXXXXX",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleRegistrationPhoneAsync(ITelegramBotClient botClient, long chatId, string phoneNumber, CancellationToken cancellationToken)
    {
        var regData = _stateManager.GetData<RegistrationData>(chatId, "registration");
        if (regData?.FirstName == null || regData.LastName == null)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Ошибка регистрации. Пожалуйста, начните заново с /register",
                cancellationToken: cancellationToken);
            return;
        }

        phoneNumber = NormalizePhoneNumber(phoneNumber);

        var (success, userId, token) = await _apiClient.RegisterUserAsync(
            regData.FirstName,
            regData.LastName,
            phoneNumber);

        if (success && userId.HasValue && token != null)
        {
            _stateManager.UpdateState(chatId, state =>
            {
                state.UserId = userId.Value;
                state.Data["token"] = token;
                state.State = ConversationState.None;
                state.Data.Remove("registration");
            });

            await botClient.SendTextMessageAsync(
                chatId,
                $"✅ Регистрация успешно завершена!\n\n" +
                $"Добро пожаловать, {regData.FirstName} {regData.LastName}!\n\n" +
                "Теперь вы можете создавать обращения и участвовать в событиях.\n" +
                "Используйте /help для просмотра доступных команд.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "❌ Ошибка регистрации. Возможно, этот номер телефона уже зарегистрирован.\n" +
                "Попробуйте еще раз или свяжитесь с поддержкой.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleReportDescriptionAsync(ITelegramBotClient botClient, long chatId, string description, CancellationToken cancellationToken)
    {
        _stateManager.UpdateState(chatId, state =>
        {
            var reportData = state.Data["report"] as ReportData ?? new ReportData();
            reportData.Description = description;
            state.Data["report"] = reportData;
            state.State = ConversationState.AwaitingReportLocation;
        });

        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            KeyboardButton.WithRequestLocation("📍 Отправить локацию")
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId,
            "Отлично! Теперь отправьте геолокацию проблемы, используя кнопку ниже.",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleNonTextMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var state = _stateManager.GetOrCreateState(chatId);

        // Обработка контакта при регистрации
        if (message.Contact != null && state.State == ConversationState.AwaitingRegistrationPhone)
        {
            await HandleRegistrationPhoneAsync(botClient, chatId, message.Contact.PhoneNumber, cancellationToken);
            return;
        }

        // Обработка локации при создании обращения
        if (message.Location != null && state.State == ConversationState.AwaitingReportLocation)
        {
            await HandleReportLocationAsync(botClient, message, cancellationToken);
            return;
        }

        // Обработка фото при создании обращения
        if (message.Photo != null && state.State == ConversationState.AwaitingReportPhoto)
        {
            await HandleReportPhotoAsync(botClient, message, cancellationToken);
            return;
        }
    }

    private async Task HandleReportLocationAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message.Location == null)
        {
            _logger.LogWarning("Location message received but Location is null");
            return;
        }

        var chatId = message.Chat.Id;
        var location = message.Location;

        _logger.LogInformation("Received location from {ChatId}: Lat={Lat}, Lng={Lng}",
            chatId, location.Latitude, location.Longitude);

        _stateManager.UpdateState(chatId, state =>
        {
            var reportData = state.Data["report"] as ReportData ?? new ReportData();
            reportData.Latitude = location.Latitude;
            reportData.Longitude = location.Longitude;
            state.Data["report"] = reportData;
            state.State = ConversationState.AwaitingReportPhoto;
        });

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("⏭️ Пропустить фото", "report_skip_photo")
        });

        // Убираем клавиатуру с кнопкой геолокации
        await botClient.SendTextMessageAsync(
            chatId,
            $"✅ Геолокация получена!\n" +
            $"📍 {location.Latitude:F6}, {location.Longitude:F6}\n\n" +
            "📸 Отправьте фото проблемы или нажмите кнопку для пропуска.",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleReportPhotoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message.Photo == null || message.Photo.Length == 0) return;

        var chatId = message.Chat.Id;
        var photo = message.Photo.Last(); // Берем фото наилучшего качества

        // В реальном приложении здесь нужно загрузить фото в хранилище
        var photoUrl = photo.FileId; // Временно используем FileId

        await CompleteReportCreationAsync(botClient, chatId, photoUrl, cancellationToken);
    }

    private async Task CompleteReportCreationAsync(ITelegramBotClient botClient, long chatId, string? photoUrl, CancellationToken cancellationToken)
    {
        var state = _stateManager.GetOrCreateState(chatId);
        var reportData = state.Data["report"] as ReportData;
        var userToken = state.Data.TryGetValue("token", out var token) ? token as string : null;

        if (reportData?.Category == null || reportData.Description == null ||
            reportData.Latitude == null || reportData.Longitude == null ||
            userToken == null)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "❌ Ошибка создания обращения. Пожалуйста, попробуйте еще раз.",
                cancellationToken: cancellationToken);
            return;
        }

        var request = new CreateReportRequest
        {
            Category = reportData.Category.Value.ToString(),
            Description = reportData.Description,
            Lat = reportData.Latitude.Value,
            Lng = reportData.Longitude.Value,
            PhotoUrl = photoUrl ?? string.Empty
        };

        var (success, reportId) = await _apiClient.CreateReportAsync(request, userToken);

        if (success)
        {
            _stateManager.ResetConversation(chatId);

            await botClient.SendTextMessageAsync(
                chatId,
                "✅ Обращение успешно создано!\n\n" +
                $"📋 ID: {reportId}\n" +
                $"📍 Локация: {reportData.Latitude:F6}, {reportData.Longitude:F6}\n\n" +
                "Мы рассмотрим ваше обращение в ближайшее время.\n" +
                "Вы получите уведомление об изменении статуса.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);

            // Отправка уведомления админу
            if (_config.AdminChatId != 0)
            {
                await SendAdminNotificationAsync(botClient, reportId, reportData, cancellationToken);
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "❌ Ошибка при создании обращения. Пожалуйста, попробуйте еще раз позже.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task SendAdminNotificationAsync(ITelegramBotClient botClient, Guid? reportId, ReportData reportData, CancellationToken cancellationToken)
    {
        var message = "🔔 Новое обращение!\n\n" +
                      $"📋 ID: {reportId}\n" +
                      $"🏷️ Категория: {GetCategoryEmoji(reportData.Category!.Value)} {reportData.Category}\n" +
                      $"📝 Описание: {reportData.Description}\n" +
                      $"📍 Координаты: {reportData.Latitude:F6}, {reportData.Longitude:F6}";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Принять", $"admin_accept_{reportId}"),
                InlineKeyboardButton.WithCallbackData("❌ Отклонить", $"admin_reject_{reportId}")
            }
        });

        await botClient.SendTextMessageAsync(
            _config.AdminChatId,
            message,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Data == null) return;

        var chatId = callbackQuery.Message!.Chat.Id;
        var data = callbackQuery.Data;

        _logger.LogInformation("Received callback from {ChatId}: {Data}", chatId, data);

        if (data.StartsWith("category_"))
        {
            await HandleCategorySelectionAsync(botClient, callbackQuery, cancellationToken);
        }
        else if (data.StartsWith("event_"))
        {
            await HandleEventActionAsync(botClient, callbackQuery, cancellationToken);
        }
        else if (data.StartsWith("admin_"))
        {
            if (IsAdmin(chatId))
                await HandleAdminActionAsync(botClient, callbackQuery, cancellationToken);
        }
        else if (data == "report_skip_photo")
        {
            await CompleteReportCreationAsync(botClient, chatId, null, cancellationToken);
        }

        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
    }

    private async Task HandleCategorySelectionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var categoryStr = callbackQuery.Data!.Replace("category_", "");

        if (Enum.TryParse<ReportCategory>(categoryStr, out var category))
        {
            _stateManager.UpdateState(chatId, state =>
            {
                var reportData = state.Data["report"] as ReportData ?? new ReportData();
                reportData.Category = category;
                state.Data["report"] = reportData;
                state.State = ConversationState.AwaitingReportDescription;
            });

            await botClient.SendTextMessageAsync(
                chatId,
                $"Вы выбрали: {GetCategoryEmoji(category)} {category}\n\n" +
                "Опишите проблему подробно:",
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleEventActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var data = callbackQuery.Data!;

        if (data.StartsWith("event_view_"))
        {
            var eventIdStr = data.Replace("event_view_", "");
            if (Guid.TryParse(eventIdStr, out var eventId))
            {
                await ShowEventDetailsAsync(botClient, chatId, eventId, cancellationToken);
            }
        }
        else if (data.StartsWith("event_subscribe_"))
        {
            var eventIdStr = data.Replace("event_subscribe_", "");
            if (Guid.TryParse(eventIdStr, out var eventId))
            {
                await SubscribeToEventAsync(botClient, chatId, eventId, cancellationToken);
            }
        }
        else if (data.StartsWith("event_unsubscribe_"))
        {
            var eventIdStr = data.Replace("event_unsubscribe_", "");
            if (Guid.TryParse(eventIdStr, out var eventId))
            {
                await UnsubscribeFromEventAsync(botClient, chatId, eventId, cancellationToken);
            }
        }
    }

    private async Task ShowEventDetailsAsync(ITelegramBotClient botClient, long chatId, Guid eventId, CancellationToken cancellationToken)
    {
        var events = await _apiClient.GetActiveEventsAsync();
        var eventDetails = events?.FirstOrDefault(e => e.Id == eventId);

        if (eventDetails == null)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Событие не найдено.",
                cancellationToken: cancellationToken);
            return;
        }

        var message = $"🎉 {eventDetails.Title}\n\n" +
                      $"📝 {eventDetails.Description}\n\n" +
                      $"📅 Начало: {eventDetails.StartAt:dd.MM.yyyy HH:mm}\n" +
                      $"⏰ Окончание: {eventDetails.EndAt:dd.MM.yyyy HH:mm}\n" +
                      $"👥 Участников: {eventDetails.ParticipantsCount}\n" +
                      $"💰 Награда: {eventDetails.CoinReward} монет";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("✅ Подписаться", $"event_subscribe_{eventId}"),
            InlineKeyboardButton.WithCallbackData("❌ Отписаться", $"event_unsubscribe_{eventId}")
        });

        await botClient.SendTextMessageAsync(
            chatId,
            message,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task SubscribeToEventAsync(ITelegramBotClient botClient, long chatId, Guid eventId, CancellationToken cancellationToken)
    {
        var state = _stateManager.GetOrCreateState(chatId);
        if (state.UserId == null)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Для подписки на события необходимо зарегистрироваться.\nИспользуйте /register",
                cancellationToken: cancellationToken);
            return;
        }

        var userToken = state.Data.TryGetValue("token", out var token) ? token as string : null;
        if (userToken == null)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Ошибка авторизации. Пожалуйста, зарегистрируйтесь снова.",
                cancellationToken: cancellationToken);
            return;
        }

        var success = await _apiClient.SubscribeToEventAsync(eventId, state.UserId.Value, userToken);

        if (success)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "✅ Вы успешно подписались на событие!\n" +
                "Мы напомним вам о нем ближе к дате начала.",
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "❌ Ошибка при подписке. Возможно, вы уже подписаны на это событие.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task UnsubscribeFromEventAsync(ITelegramBotClient botClient, long chatId, Guid eventId, CancellationToken cancellationToken)
    {
        var state = _stateManager.GetOrCreateState(chatId);
        if (state.UserId == null) return;

        var userToken = state.Data.TryGetValue("token", out var token) ? token as string : null;
        if (userToken == null) return;

        var success = await _apiClient.UnsubscribeFromEventAsync(eventId, state.UserId.Value, userToken);

        if (success)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "✅ Вы успешно отписались от события.",
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "❌ Ошибка при отписке от события.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleAdminActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var data = callbackQuery.Data!;

        if (data == "admin_stats")
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "📊 Статистика системы:\n\n" +
                "Функционал в разработке...",
                cancellationToken: cancellationToken);
        }
        else if (data == "admin_new_reports")
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "📝 Новые обращения:\n\n" +
                "Функционал в разработке...",
                cancellationToken: cancellationToken);
        }
    }

    private bool IsAdmin(long chatId) => _config.AdminIds.Contains(chatId);

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("8") && digits.Length == 11)
            return "+7" + digits[1..];
        if (!digits.StartsWith("+"))
            return "+" + digits;
        return digits;
    }

    private static string GetCategoryEmoji(ReportCategory category) => category switch
    {
        ReportCategory.OverflowingBin => "🗑️",
        ReportCategory.IllegalDump => "🚮",
        ReportCategory.StreetLitter => "🧹",
        ReportCategory.DamagedContainer => "💔",
        ReportCategory.SnowIce => "❄️",
        ReportCategory.WaterPollution => "💧",
        ReportCategory.MissedCollection => "🚚",
        _ => "❓"
    };

    private static string GetStatusText(ReportStatus status) => status switch
    {
        ReportStatus.New => "🆕 Новое",
        ReportStatus.InProgress => "⏳ В работе",
        ReportStatus.Completed => "✅ Выполнено",
        ReportStatus.Rejected => "❌ Отклонено",
        ReportStatus.Closed => "🔒 Закрыто",
        _ => "❓ Неизвестно"
    };
}