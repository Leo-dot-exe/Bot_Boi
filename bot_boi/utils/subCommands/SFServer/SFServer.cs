using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;

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
  public static SlashCommandOptionBuilder Server_Status_Command()
  {
    return new SlashCommandOptionBuilder()
      .WithName("server_status")
      .WithDescription("Check the status of the satisfactory server!")
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
      case "stop_server":
        _Logic.StopServer(command);
        break;
      case "server_status":
        _Logic.ServerStatus(command);
        break;
      default:
        await command.RespondAsync("This Stat Command is in development sorry");
        break;
    }
  }
}


public class SFServerLogic
{
  private readonly string ServerStartBatchFilePath = "C:/Users/leott/Desktop/SatisfactoryDedicatedServer/StartServer.bat";
  private readonly string ServerStopBatchFilePath = "C:/Users/leott/Desktop/SatisfactoryDedicatedServer/StopServer.bat";
  public bool StartServer(SocketSlashCommand command)
  {
    if (IsServerRunning())
    {
      command.RespondAsync("Server allready running");
      return false;
    }
    try
    {
      string paramiters = $"/k \"{ServerStartBatchFilePath}\"";
      Process.Start("cmd", paramiters);
    }
    catch
    {
      Console.WriteLine("ERROR WITH SERVER!!");
      command.RespondAsync("Somthing went wrong! (idk what)");
      return false;
    }

    command.RespondAsync("Server started successfuly!");
    return true;
  }

  public bool StopServer(SocketSlashCommand command)
  {
    if (!IsServerRunning())
    {
      command.RespondAsync("Server allready stopped");
      return true;
    }

    try
    {
      string paramiters = $"/k \"{ServerStopBatchFilePath}\"";
      Process.Start("cmd", paramiters);
    }
    catch
    {
      Console.WriteLine("ERROR WITH SERVER!!");
      command.RespondAsync("Somthing went wrong! (idk what)");
      return false;
    }

    command.RespondAsync("Server stopped successfuly!");
    return true;
  }

  public bool ServerStatus(SocketSlashCommand command)
  {
    if (IsServerRunning())
      command.RespondAsync(":green_circle: Server Running");
    else
      command.RespondAsync(":red_circle: Server off");

    return true;
  }

  private bool IsServerRunning()
  {
    Process[] pname = Process.GetProcessesByName("FactoryServer-Win64-Shipping-Cmd");
    if (pname.Length > 0)
    {
      return true;
    }
    else
    {
      return false;
    }
  }
}
