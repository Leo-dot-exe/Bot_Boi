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

using bot_boi.InteractionsHandler;
using bot_boi.utils.SubCommands;
using bot_boi.utils.StatCommands.Commands;
using static bot_boi.utils.StatCommands.Commands.StatCommands;

namespace bot_boi.Command
{
  public class CommandHandler
  {
    private readonly CommandInteractionHandler _commandInteractions;
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly CommandService _commands;

    //sub command handlers
    private readonly ModCommandsHandler _ModCommandInteractions;
    private readonly StatCommandHandler _StatCommandInteractions;

    public CommandHandler(DiscordSocketClient client, CommandService commands)
    {
      _commands = commands;
      _client = client;
      var _builder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(path: "./json/config.json");
      _config = _builder.Build();

      _commandInteractions = new CommandInteractionHandler(_client, _commands);

      //different sub command handlers
      _ModCommandInteractions = new ModCommandsHandler(_client, _commands);
      _StatCommandInteractions = new StatCommandHandler(_client, _commands);
    }

    public async Task MainAsync()
    {
      //hook the client ready command
      _client.Ready += ClientReady;
      _client.SlashCommandExecuted += SlashCommandInteractions;
      Console.WriteLine("COMMANDS HANDELED");
    }

    public async Task ClientReady()
    {
      ulong guildid = ulong.Parse(_config["SERVER_ID"]);
      SocketGuild guild = _client.GetGuild(guildid);

      string Clear = _config["CLEAR_COMMANDS"];
      if (Clear == "True")
      {
        ClearCommands(guild);
      }

      Console.WriteLine("Client Ready!");

      //misc commands:
      var rolls_command = new SlashCommandBuilder()
        .WithName("rolls")
        .WithDescription("Shows the rolls of a certain user!")
        .AddOption("user", ApplicationCommandOptionType.User, "The users whos roles you want to be listed", isRequired: true);
      CreateCommand(rolls_command, guild);

      var temp_command = new SlashCommandBuilder()
        .WithName("temp")
        .WithDescription("temp")
        .AddOption("name", ApplicationCommandOptionType.Boolean, "description");
      CreateCommand(temp_command, guild);

      //SUB COMMANDS:
      var mod_commands = new SlashCommandBuilder()
        .WithName("mod")
        .WithDescription("commands for moderators only")
        .AddOption(ModCommands.Bot_Settings_Command())
        .AddOption(ModCommands.subCommand2());
      CreateCommand(mod_commands, guild);

      var stat_commands = new SlashCommandBuilder()
        .WithName("stat")
        .WithDescription("stat_commands")
        .AddOption(StatCommands.Stat_Character_Create_Command())
        .AddOption(StatCommands.Stat_Info_Command())
        .AddOption(StatCommands.Get_all_Characters_Command())
        .AddOption(StatCommands.Battle_Command());
      CreateCommand(stat_commands, guild);

    }

    private async static void CreateCommand(SlashCommandBuilder command, SocketGuild guild)
    {
      try
      {
        await guild.CreateApplicationCommandAsync(command.Build());
      }
      catch (HttpException exception)
      {
        var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
        Console.WriteLine(json);
      }
    }

    private async Task SlashCommandInteractions(SocketSlashCommand command)
    {
      switch (command.Data.Name)
      {
        case "rolls":
          _commandInteractions.Rolls(command);
          break;
        case "mod":
          _ModCommandInteractions.ModCommands(command);
          break;
        case "stat":
          _StatCommandInteractions.StatCommands(command);
          break;
        case "temp":
          _commandInteractions.Temp(command);
          break;
        default:
          await command.RespondAsync("This Command is in development sorry");
          break;
      }
    }


    private void ClearCommands(SocketGuild guild)
    {
      Console.WriteLine("DELETEING COMANDS");
      _client.Rest.DeleteAllGlobalCommandsAsync();
      Console.WriteLine($"Deleted global commands");
      guild.DeleteApplicationCommandsAsync();
      Console.WriteLine($"Deleted guild commands");
    }
  }
}
