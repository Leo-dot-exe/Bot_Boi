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



namespace bot_boi.utils.StatCommands
{
  public class StatCommands
  {
    public static SlashCommandOptionBuilder Stat_Battle_Command()
    {
      return new SlashCommandOptionBuilder()
        .WithName("battle")
        .WithDescription("start stat battle")
        .WithType(ApplicationCommandOptionType.SubCommand)
        .AddOption("user", ApplicationCommandOptionType.User, "set user here", isRequired: true);
    }

    public static SlashCommandOptionBuilder Stat_Character_Create_Command()
    {
      return new SlashCommandOptionBuilder()
        .WithName("create_character")
        .WithDescription("create a randomly generated stat character")
        .WithType(ApplicationCommandOptionType.SubCommand)
        .AddOption("name", ApplicationCommandOptionType.String, "name your character", isRequired: true);
    }
  }

  public class StatCommandHandler
  {
    private readonly DiscordSocketClient _client;
    public StatCommandHandler(DiscordSocketClient client)
    {
      _client = client;
    }


    public async void ModCommands(SocketSlashCommand command)
    {
      {
        Console.WriteLine(command.Data.Name);
        string name = command.Data.Options.First().Name;

        // find sub command
        switch (name)
        {
          case "create_character":
            await command.RespondAsync("SETTINGS HERE");
            break;
          default:
            await command.RespondAsync("This Command is in development sorry");
            break;
        }
      }
    }
  }
}