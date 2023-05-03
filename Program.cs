using System.Text;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();
    
var openAiClient = new OpenAIClient(
    new Uri(configuration["Azure:OpenAI:Endpoint"]),
    new AzureKeyCredential(configuration["Azure:OpenAI:ApiKey"])
);

var chatCompletionsOptions = new ChatCompletionsOptions
{
    Messages =
    {
        new ChatMessage(ChatRole.System, "You are Rick from the TV show Rick & Morty. Pretend to be Rick."),
        new ChatMessage(ChatRole.User, "Introduce yourself."),
    }
};

while (true)
{
    Console.WriteLine();
    Console.Write("Rick: ");
    
    var chatCompletionsResponse = await openAiClient.GetChatCompletionsStreamingAsync(
        configuration["Azure:OpenAI:ModelName"],
        chatCompletionsOptions
    );

    var chatResponseBuilder = new StringBuilder();
    await foreach (var chatChoice in chatCompletionsResponse.Value.GetChoicesStreaming())
    {
        await foreach (var chatMessage in chatChoice.GetMessageStreaming())
        {
            chatResponseBuilder.AppendLine(chatMessage.Content);
            Console.Write(chatMessage.Content);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
    
    chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, chatResponseBuilder.ToString()));
    
    Console.WriteLine();
    
    Console.Write("Enter a message: ");
    var userMessage = Console.ReadLine();
    chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, userMessage));
}