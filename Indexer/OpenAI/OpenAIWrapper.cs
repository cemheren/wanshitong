using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Images;
using OpenAI_API.Models;
using static OpenAI_API.Chat.ChatMessage;

public class OpenAIWrapper
{
    private readonly OpenAIAPI openAiApi;

    public OpenAIWrapper(OpenAIAPI openAiApi)
    {
        this.openAiApi = openAiApi;
    }

    public async Task Test()
    {
        var result = await this.openAiApi.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.GPT4_Turbo,
            Temperature = 0.1,
            MaxTokens = 50,
            Messages = new ChatMessage[] {
                new ChatMessage(ChatMessageRole.User, "Hello!")
            }
        });

        System.Console.WriteLine(result);
    }

    public async Task TestVision(byte[] imageData)
    {
        var chat = this.openAiApi.Chat.CreateConversation();
        chat.Model = Model.GPT4_Vision;
        chat.RequestParameters.MaxTokens = 250;
        chat.AppendSystemMessage("You are a desktop assistant that examines operating system applications.");
        chat.AppendUserInput("What is the application running in this image", ImageInput.FromImageBytes(imageData));
        string response = await chat.GetResponseFromChatbotAsync();
        Console.WriteLine(response); 

        chat.AppendUserInput("Please classify the text in the program into one category: Task, Document");
        chat.RequestParameters.ResponseFormat = ChatRequest.ResponseFormats.JsonObject;
        response = await chat.GetResponseFromChatbotAsync();
        Console.WriteLine(response); 
        
return;

        chat.AppendUserInput("What are the primary non-white colors in this logo?", ImageInput.FromImageUrl("https://rogerpincombe.com/templates/rp/center-aligned-no-shadow-small.png"));
        response = await chat.GetResponseFromChatbotAsync();
        Console.WriteLine(response); // "Blue, red, and yellow"

        // or when manually creating the ChatMessage
        var messageWithImage = new ChatMessage(ChatMessageRole.User, "What colors do these logos have in common?");
        messageWithImage.Images.Add(ImageInput.FromFile("path/to/logo.png"));
        messageWithImage.Images.Add(ImageInput.FromImageUrl("https://rogerpincombe.com/templates/rp/center-aligned-no-shadow-small.png"));

        // you can specify multiple images at once
        chat.AppendUserInput("What colors do these logos have in common?", ImageInput.FromFile("path/to/logo.png"), ImageInput.FromImageUrl("https://rogerpincombe.com/templates/rp/center-aligned-no-shadow-small.png"));
    }
}