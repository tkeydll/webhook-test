using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Text.Json;
using System.Text;

namespace GitHubEventHandler
{
    public class CreateSummaryByAoai
    {
        private readonly ILogger<CreateSummaryByAoai> _logger;

        private AzureOpenAIClient _client;
        private string _endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");
        private string _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");


        public CreateSummaryByAoai(ILogger<CreateSummaryByAoai> logger)
        {
            _logger = logger;
            _client = new AzureOpenAIClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
        }

        [Function("CreateSummaryByAoai")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Create Summary By Aoai function processed a request.");

            ChatClient chatClient = _client.GetChatClient("gpt-4o-mini");

            // 入力の json をシリアライズして最小化
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // _logger.LogInformation($"Request Body: {requestBody}");

            // JSONを最小化
            string minimizedRequestBody = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(requestBody), new JsonSerializerOptions
            {
                WriteIndented = false
            });
            _logger.LogInformation($"Minimized Request Body: {minimizedRequestBody}");

            ChatCompletion completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage("あなたはプロジェクトマネジメントのエキスパートです。GitHub のイベントからプロジェクトの進捗を把握し、問題点を見つけ出します。"),
                    new UserChatMessage("以下の GitHub イベントを Issue 単位で要約してください。プルリクエストやプッシュは Issue に関連付けてください。"),
                    new UserChatMessage(minimizedRequestBody),
                ]
            );

            _logger.LogInformation($"Chat Completion: {completion.Content.FirstOrDefault().Text}");

            return new OkObjectResult(completion);
        }

    }
}
