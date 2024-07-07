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
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace bot_boi.utils.SubCommands
{
  //setup mod sub commands
  public class ModCommands
  {
    public static SlashCommandOptionBuilder Bot_Settings_Command()
    {
      //bot settings:
      return new SlashCommandOptionBuilder()
        .WithName("bot_settings")
        .WithDescription("some settings for bot_boi")
        .WithType(ApplicationCommandOptionType.SubCommandGroup)
        .AddOption(new SlashCommandOptionBuilder()
          .WithName("1")
          .WithDescription("description1")
          .WithType(ApplicationCommandOptionType.SubCommand)
          .AddOption("user", ApplicationCommandOptionType.User, "set user here", isRequired: true)
        ).AddOption(new SlashCommandOptionBuilder()
          .WithName("2")
          .WithDescription("description2")
          .WithType(ApplicationCommandOptionType.SubCommand)
          .AddOption("user2", ApplicationCommandOptionType.User, "set user here2", isRequired: false)
        );
    }

    public static SlashCommandOptionBuilder subCommand2()
    {
      //???
      return new SlashCommandOptionBuilder()
        .WithName("sub2")
        .WithDescription("temp2")
        .WithType(ApplicationCommandOptionType.SubCommandGroup)
        .AddOption(new SlashCommandOptionBuilder()
          .WithName("11")
          .WithDescription("description11")
          .WithType(ApplicationCommandOptionType.SubCommand)
          .AddOption("user", ApplicationCommandOptionType.User, "set user here", isRequired: true)
        ).AddOption(new SlashCommandOptionBuilder()
          .WithName("22")
          .WithDescription("description22")
          .WithType(ApplicationCommandOptionType.SubCommand)
          .AddOption("user2", ApplicationCommandOptionType.User, "set user here2", isRequired: false)
        );
    }


  }

  //handle the mod sub commands
  public class ModCommandsHandler
  {
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly SocketGuild _guild;
    private readonly IConfiguration _settings;
    private readonly IConfiguration _config;
    public ModCommandsHandler(DiscordSocketClient client, CommandService commands)
    {
      _client = client;
      _commands = commands;

      var _builder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(path: "/json/config.json");
      _config = _builder.Build();
      var _settingsBuilder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(path: "/json/bot_settings.json");
      _settings = _settingsBuilder.Build();

      _guild = _client.GetGuild(ulong.Parse(_config["SERVER_ID"]));
    }

    public async void ModCommands(SocketSlashCommand command)
    {
      {
        Console.WriteLine(command.Data.Name);
        string name = command.Data.Options.First().Name;

        // check if user has correct permitions
        ulong serverId;
        if (!ulong.TryParse(_config["SERVER_ID"], out serverId))
        {
          Console.WriteLine("CANT GET SERVER ID");
        }
        SocketGuild guildTest;
        guildTest = _client.GetGuild(serverId);
        var commandUser = guildTest.GetUser(command.User.Id);
        if (commandUser == null)
        {
          Console.WriteLine("error");
        }
        bool valid = false;
        foreach (var item in commandUser.Roles)
        {
          if (commandUser.Roles.ToString() == "mod")
          {
            valid = true;
          }
        }
        if (valid == false)
        {
          await command.RespondAsync("You don't have the correct permitions to run this command.");
          return;
        }

        // find sub command
        switch (name)
        {
          case "bot_settings":
            await command.RespondAsync("SETTINGS HERE");
            break;
          default:
            await command.RespondAsync("This Mod Command is in development sorry");
            break;
        }
      }
    }
  }
}
