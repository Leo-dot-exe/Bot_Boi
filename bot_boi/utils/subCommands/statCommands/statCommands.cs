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

using bot_boi.utils.StatCommands.Logic;

namespace bot_boi.utils.StatCommands.Commands
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
        .AddOption("name", ApplicationCommandOptionType.String, "name your character", isRequired: true)
        .AddOption(new SlashCommandOptionBuilder()
          .WithName("class")
          .WithDescription("Choose your character's class")
          .WithType(ApplicationCommandOptionType.String)
          .WithRequired(true)
          .AddChoice("warrior", "strength")
          .AddChoice("mage", "intellegence")
          .AddChoice("rogue", "speed")
          .AddChoice("sentinel", "durability")
          );
    }

    public class StatCommandHandler
    {
      private readonly DiscordSocketClient _client;
      private readonly CommandService _commands;
      public StatCommandHandler(DiscordSocketClient client, CommandService command)
      {
        _client = client;
        _commands = command;
      }
      public async void StatCommands(SocketSlashCommand command)
      {
        {
          Console.WriteLine(command.Data.Name);
          string name = command.Data.Options.First().Name;

          // find sub command
          switch (name)
          {
            case "create_character":
              StatCommandLogic.CreateCharacter(command);
              break;
            default:
              await command.RespondAsync("This Stat Command is in development sorry");
              break;
          }
        }
      }
    }
  }
}