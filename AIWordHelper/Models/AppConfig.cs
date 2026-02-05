using System.Text.Json.Serialization;

namespace AIWordHelper.Models;

public class AppConfig
{
    [JsonPropertyName("apiUrl")]
    public string ApiUrl { get; set; } = "https://api.openai.com/v1";

    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = "";

    [JsonPropertyName("selectedModel")]
    public string SelectedModel { get; set; } = "gpt-3.5-turbo";

    [JsonPropertyName("waitTimeSeconds")]
    public double WaitTimeSeconds { get; set; } = 2.0;

    [JsonPropertyName("prompts")]
    public List<PromptTemplate> Prompts { get; set; } = new()
    {
        new PromptTemplate
        {
            Name = "翻译成中文",
            Content = "请将以下内容翻译成中文：\n\n",
            IsEnabled = true
        },
        new PromptTemplate
        {
            Name = "解释代码",
            Content = "请解释以下代码的功能：\n\n",
            IsEnabled = false
        }
    };
}
