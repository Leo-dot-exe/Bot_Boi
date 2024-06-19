using System;
using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

using bot_boi.Command;

namespace cSharpBot
{
  class SwearJson
  {
    public string[] WORDS;
  }

  class Program
  {
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly CommandService _commands;
    private readonly CommandHandler _handler;

    public static async Task Main(string[] args)
    {
      await new Program().MainAsync();
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
        .AddJsonFile(path: "./json/config.json");
      _config = _builder.Build();

      _commands = new CommandService();
      _handler = new CommandHandler(_client, _commands);
    }

    public async Task MainAsync()
    {
      Console.WriteLine("Logging in...");
      await _client.LoginAsync(TokenType.Bot, _config["TOKEN"]);
      Console.WriteLine("Starting...");
      await _client.StartAsync();

      await _handler.MainAsync();

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

    //SIMPLE MESSAGE HANDLER
    private async Task MessageReceivedAsync(SocketMessage message)
    {
      if (message.Author.Id == _client.CurrentUser.Id)
        return;

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
        Console.WriteLine($"Message received: {message.Content} USER_ID: {message.Author.Id}");
      }

      // hello world
      if (message.Content == ".hello")
      {
        await message.Channel.SendMessageAsync("world!");
      }

      //if luc wrote message react with monke
      if (message.Author.Id == 1122481591592169573)
      {
        IEmote monke = Emote.Parse("<:Monke:1112676381457924096>");
        await message.AddReactionAsync(monke);
      }

      //reply with cheese
      if (message.Content.ToUpper().Contains("CHEESE"))
      {
        await message.Channel.SendMessageAsync("https://tenor.com/lpag1q8ndU0.gif");
      }

      //swear checking
      //json stuff
      string jsonSwearFile = File.ReadAllText("./bin/Debug/net8.0/json/DirtyWords.json");
      SwearJson swears = JsonConvert.DeserializeObject<SwearJson>(jsonSwearFile);

      if (Array.Exists(swears.WORDS, element => element == message.Content))
      {
        await message.Channel.SendMessageAsync("Naughty! We dont fucking swear in our christian minecraft server you little shit.");
      }

    }
  }
}