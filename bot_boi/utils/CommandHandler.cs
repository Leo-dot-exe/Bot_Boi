using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using bot_boi.InteractionsHandler;
using bot_boi.utils.SubCommands;
using bot_boi.utils.StatCommands.Commands;
using static bot_boi.utils.StatCommands.Commands.StatCommands;
using bot_boi.utils.subCommands.SFServer;
using bot_boi.utils.subCommands.TheProgramCommands;
using bot_boi.utils.subCommands.Music_Boi;

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
    private readonly SFServerCommandHandler _SFServerCommandInteractions;
    private readonly TheProgramCommandHandler _TheProgramCommandInteractions;
    private readonly Music_Boi_CommandHandler _Music_Boi_CommandInteractions;

    public CommandHandler(DiscordSocketClient client, CommandService commands)
    {
      _commands = commands;
      _client = client;
      var _builder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(path: "./data/json/config.json");
      _config = _builder.Build();

      _commandInteractions = new CommandInteractionHandler(_client, _commands);

      //different sub command handlers
      _ModCommandInteractions = new ModCommandsHandler(_client, _commands);
      _StatCommandInteractions = new StatCommandHandler(_client, _commands);
      _SFServerCommandInteractions = new SFServerCommandHandler(_client, _commands);
      _TheProgramCommandInteractions = new TheProgramCommandHandler(_client, _commands);
      _Music_Boi_CommandInteractions = new Music_Boi_CommandHandler(_client, _commands);
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
        .AddOption(StatCommands.Battle_Command())
        .AddOption(StatCommands.Delete_Command());
      CreateCommand(stat_commands, guild);

      var sfserver_commands = new SlashCommandBuilder()
        .WithName("satisfactory")
        .WithDescription("Commands for the Satisfactory Server!")
        .AddOption(SFServerCommandBuilder.Start_Server_Command())
        .AddOption(SFServerCommandBuilder.Stop_Server_Command())
        .AddOption(SFServerCommandBuilder.Server_Status_Command());
      CreateCommand(sfserver_commands, guild);

      var TheProgram_commands = new SlashCommandBuilder()
        .WithName("the-program")
        .WithDescription("Dicord version of the program")
        .AddOption(TheProgramCommandBuilder.Website_Status_Command())
        .AddOption(TheProgramCommandBuilder.Start_F_MK3())
        .AddOption(TheProgramCommandBuilder.Start_M_MK2())
        .AddOption(TheProgramCommandBuilder.Check_Result());
      CreateCommand(TheProgram_commands, guild);

      var Music_Boi_Commands = new SlashCommandBuilder()
        .WithName("music_boi")
        .WithDescription("Play music through youtube onto discord")
        .AddOption(Music_Boi_CommandBuilder.Play_Command())
        .AddOption(Music_Boi_CommandBuilder.Disconect_Command())
        .AddOption(Music_Boi_CommandBuilder.Pause_Command());
      CreateCommand(Music_Boi_Commands, guild);
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
        case "satisfactory":
          _SFServerCommandInteractions.SFServerCommands(command);
          break;
        case "the-program":
          _TheProgramCommandInteractions.TheProgramCommands(command);
          break;
        case "music_boi":
          _Music_Boi_CommandInteractions.Music_Boi_Commands(command);
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
