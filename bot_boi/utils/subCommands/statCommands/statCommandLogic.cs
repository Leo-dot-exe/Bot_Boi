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

//db
using Dapper;
using Microsoft.Data.Sqlite;


namespace bot_boi.utils.StatCommands.Logic
{
  //DB STRUCTURE THING
  public class StatUsers
  {
    public int id { get; set; }
    public int user_id { get; set; }
    public string username { get; set; }
  }
  //STATS STRUCTURE THING
  public class StatObject
  {
    public string Name { get; set; }
    public int Speed { get; set; } //to dodge attackes
    public int Strength { get; set; } //attack strength
    public int Durability { get; set; } //dammage resistance
    public int Hp { get; set; } //Health points (random from 60 - 100)
    public int Intelligence { get; set; } //More likely to Hit attacks
  }

  public class StatCommandLogic
  {
    private const string DbPath = "Stat.db";
    public static async Task InitializeDatabase()
    {
      using var connection = new SqliteConnection($"Data Source={DbPath}");
      await connection.OpenAsync();

      var createStatUsersTable = @"
      CREATE TABLE IF NOT EXISTS StatUsers (
      id INTEGER PRIMARY KEY,
      user_id INTEGER NOT NULL,
      username TEXT NOT NULL
      );";

      var createStatCharactersTable = @"
        CREATE TABLE IF NOT EXISTS StatCharacters (
        Id INTEGER PRIMARY KEY,
        OwnerId INTEGER,
        Name TEXT NOT NULL,
        Hp INTEGER NOT NULL,
        Speed INTEGER NOT NULL,
        Strength INTEGER NOT NULL,
        Durability INTEGER NOT NULL,
        Intelligence INTEGER NOT NULL,
        FOREIGN KEY (OwnerId) REFERENCES StatUsers (Id)
        );";

      await connection.ExecuteAsync(createStatUsersTable);
      await connection.ExecuteAsync(createStatCharactersTable);
    }
    public static async void CreateCharacter(SocketSlashCommand command)
    {
      Random rand = new();

      //get character and class name
      var subCommand = command.Data.Options.FirstOrDefault(option => option.Name == "create_character");
      var statClass = subCommand.Options.FirstOrDefault(option => option.Name == "class");
      var nameOption = subCommand.Options.FirstOrDefault(option => option.Name == "name");
      string CharacterName = nameOption.Value.ToString();
      string ClassName = statClass.Value.ToString();

      //null checks
      if (CharacterName == null)
        return;

      //Define proficency modifiers
      int SpeedMod = 0;
      int StrengthMod = 0;
      int DurabilityMod = 0;
      int IntellegenceMod = 0;

      //Define proficency minimum values
      int SpeedMin = 20;
      int StrengthMin = 20;
      int durabilityMin = 20;
      int IntellegenceMin = 20;

      //find characters proficency
      switch (ClassName)
      {
        case "strength":
          StrengthMod = rand.Next(10, 50);
          StrengthMin = 60;
          break;
        case "intellegence":
          IntellegenceMod = rand.Next(10, 50);
          IntellegenceMin = 60;
          break;
        case "speed":
          SpeedMod = rand.Next(10, 50);
          SpeedMin = 60;
          break;
        case "durability":
          DurabilityMod = rand.Next(10, 50);
          durabilityMin = 60;
          break;
        default:
          await command.RespondAsync("PLEASE ENTER A CLASS");
          return;
      }

      //set random stats
      await command.RespondAsync("stat command");
      var NewCharacter = new StatObject
      {
        Name = CharacterName,
        Hp = rand.Next(60, 100),
        Speed = rand.Next(SpeedMin, 100) + SpeedMod,
        Strength = rand.Next(StrengthMin, 100) + StrengthMod,
        Durability = rand.Next(durabilityMin, 100) + DurabilityMod,
        Intelligence = rand.Next(IntellegenceMin, 100) + IntellegenceMod
      };

      //temp
      Console.WriteLine($"Name: {NewCharacter.Name}");
      Console.WriteLine($"HP: {NewCharacter.Hp}");
      Console.WriteLine($"Speed: {NewCharacter.Speed}");
      Console.WriteLine($"Strength: {NewCharacter.Strength}");
      Console.WriteLine($"Durability: {NewCharacter.Durability}");
      Console.WriteLine($"Intellegence: {NewCharacter.Intelligence}");

      await CreateCharacterDB();

    }


    private static async Task CreateCharacterDB()
    {
      //conect to db
      await using var connection = new SqliteConnection($"Data Source={DbPath}");
      await connection.OpenAsync();

      var query = "SELECT * FROM StatUsers";

      var results = await connection.QueryAsync<StatUsers>(query);
      Console.WriteLine(results);
      foreach (var item in results)
      {
        Console.WriteLine(item.user_id);
      }
    }
  }

}