# TazaOrda Telegram Bot

Telegram бот для платформы TazaOrda - системы управления чистотой города.

## Функциональность

### Для пользователей:
- **Регистрация** (`/register`) - создание аккаунта в системе
- **Создание обращений** (`/report`) - отправка заявок о проблемах с чистотой:
  - Выбор категории проблемы
  - Описание проблемы
  - Отправка геолокации
  - Прикрепление фото (опционально)
- **Просмотр обращений** (`/myreports`) - список всех обращений пользователя
- **События** (`/events`) - просмотр активных субботников и акций
- **Подписка на события** - регистрация на участие в событиях
- **Мои события** (`/myevents`) - список событий, в которых участвует пользователь

### Для администраторов:
- **Админ-панель** (`/admin`) - доступна только для ADMIN_IDS
- Уведомления о новых обращениях
- Быстрые действия (принять/отклонить обращение)
- Статистика системы

## Настройка

### 1. Создание Telegram бота

1. Найдите [@BotFather](https://t.me/BotFather) в Telegram
2. Отправьте команду `/newbot`
3. Следуйте инструкциям для создания бота
4. Сохраните полученный токен

### 2. Конфигурация

Отредактируйте `appsettings.json`:

```json
{
  "TelegramBot": {
    "Token": "ваш_токен_бота",
    "AdminChatId": 125691222,
    "AdminIds": [125691222],
    "ApiBaseUrl": "http://localhost:8080"
  }
}
```

Или используйте переменные окружения:
```bash
export TelegramBot__Token="ваш_токен_бота"
export TelegramBot__AdminChatId=125691222
export TelegramBot__ApiBaseUrl="http://localhost:8080"
```

### 3. Запуск

#### Локальный запуск:

```bash
cd Presentation/TazaOrda.TelegramBot
dotnet restore
dotnet run
```

#### Docker:

```bash
docker build -t tazaorda-bot -f Dockerfile.bot .
docker run -d --name tazaorda-bot \
  -e TelegramBot__Token="ваш_токен" \
  -e TelegramBot__AdminChatId=125691222 \
  -e TelegramBot__ApiBaseUrl="http://api:8080" \
  --network tazaorda-network \
  tazaorda-bot
```

## Команды бота

### Основные команды:
- `/start` - Начать работу с ботом
- `/register` - Регистрация в системе
- `/report` - Создать обращение о проблеме
- `/myreports` - Мои обращения
- `/events` - Список активных событий
- `/myevents` - Мои события
- `/help` - Справка по командам
- `/cancel` - Отменить текущую операцию

### Админ-команды:
- `/admin` - Панель администратора (только для админов)

## Архитектура

```
TazaOrda.TelegramBot/
├── Configuration/
│   └── BotConfiguration.cs      # Настройки бота
├── Handlers/
│   └── UpdateHandler.cs         # Обработчик сообщений и команд
├── Models/
│   └── UserState.cs             # Модели состояний диалога
├── Services/
│   ├── StateManager.cs          # Управление состояниями пользователей
│   └── TazaOrdaApiClient.cs     # Клиент для API TazaOrda
├── Program.cs                   # Точка входа
└── appsettings.json            # Конфигурация
```

## Интеграция с API

Бот взаимодействует с основным API TazaOrda через HTTP клиент:

- **Регистрация**: `POST /api/auth/register`
- **Создание обращения**: `POST /api/reports`
- **Получение событий**: `GET /api/events/active`
- **Подписка на событие**: `POST /api/events/{id}/join`
- **Отписка от события**: `POST /api/events/{id}/leave`

## Управление состоянием

Бот использует `StateManager` для отслеживания состояния диалога с каждым пользователем:

- `None` - нет активного диалога
- `AwaitingRegistrationName` - ожидание имени при регистрации
- `AwaitingRegistrationPhone` - ожидание номера телефона
- `AwaitingReportCategory` - ожидание выбора категории обращения
- `AwaitingReportDescription` - ожидание описания проблемы
- `AwaitingReportLocation` - ожидание геолокации
- `AwaitingReportPhoto` - ожидание фото

## Администраторы

Администраторы определяются в конфигурации:

```json
{
  "TelegramBot": {
    "AdminChatId": 125691222,
    "AdminIds": [125691222]
  }
}
```

- `AdminChatId` - чат, куда отправляются уведомления о новых обращениях
- `AdminIds` - список Telegram ID администраторов, имеющих доступ к `/admin`

## Безопасность

- Токены пользователей хранятся в памяти (StateManager)
- Не используется для production без дополнительной защиты
- Рекомендуется использовать Redis для хранения состояний в production
- Неактивные состояния очищаются автоматически через 24 часа

## Зависимости

- `Telegram.Bot` v22.0.0 - Telegram Bot API
- `Microsoft.Extensions.*` - DI, конфигурация, логирование
- `TazaOrda.Domain` - доменные модели
- `TazaOrda.Infrastructure` - инфраструктурный слой

## Логирование

Бот использует стандартное логирование .NET:
- Консольный вывод
- Уровни: Information, Warning, Error
- Логирование всех входящих сообщений и ошибок

## Мониторинг

Для production рекомендуется добавить:
- Application Insights / Serilog
- Health checks
- Metrics (Prometheus)
- Distributed tracing

## TODO / Улучшения

- [ ] Интеграция с Redis для хранения состояний
- [ ] Уведомления о смене статуса обращения
- [ ] Напоминания о предстоящих событиях
- [ ] Геймификация (рейтинг, достижения)
- [ ] Локализация (поддержка казахского языка)
- [ ] Rate limiting
- [ ] Webhook вместо long polling
- [ ] Inline-режим для быстрого создания обращений

## Лицензия

Proprietary