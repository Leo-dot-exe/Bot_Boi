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
    public static SlashCommandOptionBuilder Battle_Command()
    {
      return new SlashCommandOptionBuilder()
        .WithName("battle")
        .WithDescription("battle your character against another character")
        .WithType(ApplicationCommandOptionType.SubCommand)
        .AddOption("your_character", ApplicationCommandOptionType.String, "your characters name", isRequired: true)
        .AddOption("opponent", ApplicationCommandOptionType.String, "your opponents name", isRequired: true);
    }
    public static SlashCommandOptionBuilder Stat_Info_Command()
    {
      return new SlashCommandOptionBuilder()
        .WithName("info")
        .WithDescription("information about how stat commands work or about a specific character")
        .WithType(ApplicationCommandOptionType.SubCommand)
        .AddOption("character", ApplicationCommandOptionType.String, "character name", isRequired: false);
    }
    public static SlashCommandOptionBuilder Get_all_Characters_Command()
    {
      return new SlashCommandOptionBuilder()
        .WithName("get_all_characters")
        .WithDescription("gets all the created characters")
        .WithType(ApplicationCommandOptionType.SubCommand);
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
          .AddChoice("warrior", "warrior")
          .AddChoice("mage", "mage")
          .AddChoice("rogue", "rogue")
          .AddChoice("sentinel", "sentinel")
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
          string name = command.Data.Options.First().Name;

          // find sub command
          switch (name)
          {
            case "create_character":
              StatCommandLogic.CreateCharacter(command);
              break;
            case "info":
              StatCommandLogic.Info(command);
              break;
            case "get_all_characters":
              StatCommandLogic.GetAllCharacters(command);
              break;
            case "battle":
              StatCommandLogic.Battle(command);
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