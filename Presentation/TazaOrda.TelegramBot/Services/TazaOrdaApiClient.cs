using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TazaOrda.Domain.DTOs.Auth;
using TazaOrda.Domain.DTOs.Events;
using TazaOrda.Domain.DTOs.Reports;
using TazaOrda.Domain.DTOs.Users;
using TazaOrda.TelegramBot.Configuration;

namespace TazaOrda.TelegramBot.Services;

/// <summary>
/// Клиент для взаимодействия с API TazaOrda
/// </summary>
public class TazaOrdaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly BotConfiguration _config;
    private readonly ILogger<TazaOrdaApiClient> _logger;
    private string? _accessToken;

    public TazaOrdaApiClient(HttpClient httpClient, BotConfiguration config, ILogger<TazaOrdaApiClient> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_config.ApiBaseUrl);
    }

    private void SetAuthToken(string token)
    {
        _accessToken = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    #region Auth

    public async Task<(bool Success, Guid? UserId, string? Token)> RegisterUserAsync(string firstName, string lastName, string phoneNumber)
    {
        try
        {
            var request = new RegisterRequest
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Password = GenerateRandomPassword()
            };

            var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Registration failed: {Error}", error);
                return (false, null, null);
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result != null)
            {
                SetAuthToken(result.AccessToken);
                return (true, result.User.Id, result.AccessToken);
            }

            return (false, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return (false, null, null);
        }
    }

    public async Task<(bool Success, Guid? UserId, string? Token)> LoginAsync(string phoneNumber)
    {
        try
        {
            var request = new LoginRequest
            {
                PhoneNumber = phoneNumber,
                Password = GenerateRandomPassword() // В реальном приложении нужна правильная аутентификация
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);

            if (!response.IsSuccessStatusCode)
                return (false, null, null);

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result != null)
            {
                SetAuthToken(result.AccessToken);
                return (true, result.User.Id, result.AccessToken);
            }

            return (false, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in");
            return (false, null, null);
        }
    }

    #endregion

    #region Reports

    public async Task<(bool Success, Guid? ReportId)> CreateReportAsync(CreateReportRequest request, string token)
    {
        try
        {
            SetAuthToken(token);
            var response = await _httpClient.PostAsJsonAsync("/api/reports", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Report creation failed: {Error}", error);
                return (false, null);
            }

            var result = await response.Content.ReadFromJsonAsync<CreateReportResponse>();
            return (true, result?.ReportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report");
            return (false, null);
        }
    }

    public async Task<List<ReportListDto>?> GetUserReportsAsync(Guid userId, string token)
    {
        try
        {
            SetAuthToken(token);
            var response = await _httpClient.GetAsync($"/api/reports/user/{userId}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<ReportListDto>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user reports");
            return null;
        }
    }

    #endregion

    #region Events

    public async Task<List<EventListDto>?> GetActiveEventsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/events/active");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<EventListDto>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active events");
            return null;
        }
    }

    public async Task<bool> SubscribeToEventAsync(Guid eventId, Guid userId, string token)
    {
        try
        {
            SetAuthToken(token);
            var response = await _httpClient.PostAsync($"/api/events/{eventId}/join", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to event");
            return false;
        }
    }

    public async Task<bool> UnsubscribeFromEventAsync(Guid eventId, Guid userId, string token)
    {
        try
        {
            SetAuthToken(token);
            var response = await _httpClient.PostAsync($"/api/events/{eventId}/leave", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from event");
            return false;
        }
    }

    public async Task<List<EventListDto>?> GetUserEventsAsync(string token)
    {
        try
        {
            SetAuthToken(token);
            var response = await _httpClient.GetAsync("/api/events/my");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<EventListDto>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user events");
            return null;
        }
    }

    #endregion

    #region Users

    public async Task<UserProfileDto?> GetUserByPhoneAsync(string phoneNumber)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/users/by-phone/{phoneNumber}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserProfileDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by phone");
            return null;
        }
    }

    #endregion

    private static string GenerateRandomPassword()
    {
        return Guid.NewGuid().ToString("N")[..16];
    }
}