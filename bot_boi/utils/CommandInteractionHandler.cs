using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace bot_boi.InteractionsHandler
{
  public class CommandInteractionHandler
  {
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly CommandService _commands;

    public CommandInteractionHandler(DiscordSocketClient client, CommandService commands)
    {
      _commands = commands;
      _client = client;
      var _builder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile(path: Directory.GetCurrentDirectory() + "/data/json/config.json");
      _config = _builder.Build();
    }


    public async void Rolls(SocketSlashCommand command)
    {
      var guildUser = (SocketGuildUser)command.Data.Options.First().Value;

      var roleList = string.Join(",\n", guildUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

      var embedBuiler = new EmbedBuilder()
        .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
        .WithTitle("Roles")
        .WithDescription(roleList)
        .WithColor(Color.Green)
        .WithCurrentTimestamp();

      await command.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
    }

    public async void Temp(SocketSlashCommand command)
    {
      await command.RespondAsync("Temp");
    }
  }
}
