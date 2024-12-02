using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

using bot_boi.utils.subCommands.TheProgramCommands;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using Newtonsoft.Json;
using System.Text;

namespace bot_boi.utils.subCommands.TheProgramCommands;

public class TheProgramCommandBuilder
{
  public static SlashCommandOptionBuilder Website_Status_Command()
  {
    return new SlashCommandOptionBuilder()
      .WithName("website_status")
      .WithDescription("Check the status of the program website")
      .WithType(ApplicationCommandOptionType.SubCommand);
  }

  public static SlashCommandOptionBuilder Start_F_MK3()
  {
    return new SlashCommandOptionBuilder()
    .WithName("start_f_mk3")
    .WithDescription("a")
    .WithType(ApplicationCommandOptionType.SubCommand);
  }

  public static SlashCommandOptionBuilder Start_M_MK2()
  {
    return new SlashCommandOptionBuilder()
    .WithName("start_m_mk2")
    .WithDescription("a")
    .WithType(ApplicationCommandOptionType.SubCommand);
  }

  public static SlashCommandOptionBuilder Check_Result()
  {
    return new SlashCommandOptionBuilder()
    .WithName("check_result")
    .WithDescription("a")
    .WithType(ApplicationCommandOptionType.SubCommand)
    .AddOption("name", ApplicationCommandOptionType.String, "Your program username", isRequired: true);
  }
}

public class TheProgramCommandHandler
{
  private readonly DiscordSocketClient _Client;
  private readonly CommandService _Commands;
  private readonly TheProgramLogic _Logic;
  public TheProgramCommandHandler(DiscordSocketClient client, CommandService command)
  {
    _Client = client;
    _Commands = command;
    _Logic = new TheProgramLogic();
  }

  public async void TheProgramCommands(SocketSlashCommand command)
  {
    string name = command.Data.Options.First().Name;

    switch (name)
    {
      case "website_status":
        _Logic.website_status(command);
        break;
      case "start_f_mk3":
        _Logic.start_f_mk3(command);
        break;
      case "start_m_mk2":
        _Logic.start_m_mk2(command);
        break;
      case "check_result":
        _Logic.check_result(command);
        break;
    }
  }

  public class TheProgramLogic
  {
    public void website_status(SocketSlashCommand command)
    {
      return;
    }

    public void start_f_mk3(SocketSlashCommand command)
    {
      return;
    }

    public void start_m_mk2(SocketSlashCommand command)
    {
      return;
    }

    public async void check_result(SocketSlashCommand command)
    {
      //get username from command input
      var subCommand = command.Data.Options.FirstOrDefault(option => option.Name == "check_result");
      if (subCommand == null) { await command.RespondAsync("ERROR WITH SUB COMMAND"); return; }
      var nameOption = subCommand.Options.FirstOrDefault(option => option.Name == "name");
      if (nameOption == null) { await command.RespondAsync("ERROR WITH NAME OPTION"); return; }

      string CharacterName = nameOption.Value.ToString() ?? string.Empty;

      var http = new HttpService();
      string programData = http.Get("http://localhost:3000/api/db_query/leo");

      //get processed results
      JObject programDataJson = JObject.Parse(programData);

      http.PostProcessResults("http://localhost:3000/api/process_values_F", programDataJson);

      await command.RespondAsync(programData);
    }
  }
}

public class HttpService
{
  private readonly HttpClient Client;

  public HttpService()
  {
    HttpClientHandler handler = new HttpClientHandler
    {
      AutomaticDecompression = System.Net.DecompressionMethods.All
    };

    Client = new HttpClient();
  }

  public string Get(string url)
  {
    var endPoint = new Uri(url);
    try
    {
      var responce = Client.GetAsync(endPoint).Result;
      var responceJson = responce.Content.ReadAsStringAsync().Result;
      Console.WriteLine($"Responce: {responceJson}");
      return responceJson;
    }
    catch (AggregateException)
    {
      Console.WriteLine("Can not connect to api");
      return "Can not connect to api";
    }
  }

  public string PostProcessResults(string url, JObject userData)
  {
    var endPoint = new Uri(url);

    try
    {
      var postJson = JsonConvert.SerializeObject(userData);
      var payload = new StringContent(postJson, Encoding.UTF8, "apllication/json");

      var responce = Client.PostAsync(endPoint, payload).Result;
      var responceJson = responce.Content.ReadAsStringAsync().Result;
      System.Console.WriteLine(responceJson);

      //temp
      return "a";
    }
    catch (AggregateException)
    {
      Console.WriteLine("Can not connect to api");
      return "Can not connect to api";
    }
  }

  private void CloseClient() { Client.Dispose(); }
}