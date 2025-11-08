namespace TazaOrda.TelegramBot.Configuration;

public class BotConfiguration
{
    public const string SectionName = "TelegramBot";

    public string Token { get; set; } = string.Empty;
    public long AdminChatId { get; set; } = 125691222;
    public HashSet<long> AdminIds { get; set; } = [125691222];
    public string ApiBaseUrl { get; set; } = "http://localhost:8080";
}