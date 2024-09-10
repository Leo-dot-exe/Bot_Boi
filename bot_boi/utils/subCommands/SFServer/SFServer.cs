using System;
using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.Interactions;
using Discord.Commands.Builders;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Newtonsoft.Json;

namespace bot_boi.utils.subCommands.SFServer;

public class SFServerCommandBuilder
{
  public static SlashCommandOptionBuilder Start_Server_Command()
  {
    return new SlashCommandOptionBuilder()
      .WithName("start_server")
      .WithDescription("Start the Satisfactory server up!")
      .WithType(ApplicationCommandOptionType.SubCommand);
  }
  public static SlashCommandOptionBuilder Stop_Server_Command()
  {
    return new SlashCommandOptionBuilder()
      .WithName("stop_server")
      .WithDescription("Stop the Satisfactory server.")
      .WithType(ApplicationCommandOptionType.SubCommand);
  }
}

public class SFServerCommandHandler
{
  private readonly DiscordSocketClient _Client;
  private readonly CommandService _Commands;
  private readonly SFServerLogic _Logic;
  public SFServerCommandHandler(DiscordSocketClient client, CommandService command)
  {
    _Client = client;
    _Commands = command;
    _Logic = new SFServerLogic();
  }

  public async void SFServerCommands(SocketSlashCommand command)
  {
    string name = command.Data.Options.First().Name;
    // find sub command
    switch (name)
    {
      case "start_server":
        _Logic.StartServer(command);
        break;
      default:
        await command.RespondAsync("This Stat Command is in development sorry");
        break;
    }
  }
}


public class SFServerLogic
{
  public bool StartServer(SocketSlashCommand command)
  {
    Console.WriteLine("START SERVER TEMP");
    return true;
  }
}
