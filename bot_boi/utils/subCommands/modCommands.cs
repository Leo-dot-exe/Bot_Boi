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
    private readonly IConfiguration _settings;
    public ModCommandsHandler(DiscordSocketClient client, CommandService command)
    {
      _client = client;
      _commands = command;

      var _builder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(path: "bot_settings.json");
      _settings = _builder.Build();
    }
  }
}
