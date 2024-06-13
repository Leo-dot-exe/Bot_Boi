using System;
using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace cSharpBot
{
  class Program
  {
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;

    public static async Task Main(string[] args)
    {
      await new Program().MainAsync();
    }

    public async Task MainAsync(string[] args)
    {

    }

    public Program() //CONSTRUCTOR
    {
      _client = new DiscordSocketClient
      (
        new DiscordSocketConfig
        {
          GatewayIntents = GatewayIntents.All
        }
      );

      _client.Log += Log; //Hook into the client log event
      _client.Ready += Ready; //Hook into the client ready event
      _client.MessageReceived += MessageReceivedAsync; //Hook into message received event

      var _builder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(path: "config.json");
      _config = _builder.Build();
    }

    public async Task MainAsync()
    {
      Console.WriteLine("Logging in...");
      await _client.LoginAsync(TokenType.Bot, _config["TOKEN"]);
      Console.WriteLine("Starting...");
      await _client.StartAsync();

      await Task.Delay(-1); //Block program until task is closed
    }

    private Task Log(LogMessage log)
    {
      Console.WriteLine(log.ToString());
      return Task.CompletedTask;
    }

    private Task Ready()
    {
      Console.WriteLine($"Connected! :D");
      return Task.CompletedTask;
    }

    //temp MESSAGE HANDLER
    private async Task MessageReceivedAsync(SocketMessage message)
    {
      //SHOW ALL PROPERTIES
      // foreach (var prop in message.GetType().GetProperties())
      // {
      //   try
      //   {
      //     var propName = prop.Name;
      //     var propValue = prop.GetValue(message, null) ?? "null";
      //     Console.WriteLine($"{propName}: {propValue}");
      //   }
      //   catch (Exception ex)
      //   {
      //     Console.WriteLine($"{prop.Name}: Exception - {ex.Message}");
      //   }
      // }


      if (string.IsNullOrEmpty(message.Content))
      {
        Console.WriteLine("Received a message with empty or null content.");
      }
      else
      {
        Console.WriteLine($"Message received: {message.Content}");
      }

      // Stop the bot from replying to itself
      if (message.Author.Id == _client.CurrentUser.Id)
        return;

      if (message.Content == ".hello")
      {
        await message.Channel.SendMessageAsync("world!");
      }

      //if luc wrote message react with monke
      if (message.Author.Id.Equals("1122481591592169573"))
      {
        IEmote monke = Emote.Parse("<:Monke:1112676381457924096>");
        await message.AddReactionAsync(monke);
      }
    }
  }
}