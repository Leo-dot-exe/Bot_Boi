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
using System.Net.Sockets;


namespace bot_boi.utils.StatCommands.Logic
{
  //DB STRUCTURE THING
  public class StatUsers
  {
    public int id { get; set; }
    public string user_id { get; set; }
    public string username { get; set; }
  }
  //USER DB STRUCTURE
  public class StatCharacters
  {
    public int id { get; set; }
    public int owner_id { get; set; }
    public string name { get; set; }
    public string character_class { get; set; }
    public int hp { get; set; }
    public int speed { get; set; }
    public int strength { get; set; }
    public int durability { get; set; }
    public int intelligence { get; set; }
  }



  //STATS DATA TYPE
  public class StatObject
  {
    public string Name { get; set; }
    public int Hp { get; set; } //Health points (random from 60 - 100)
    public int Speed { get; set; } //to dodge attackes
    public int Strength { get; set; } //attack strength
    public int Durability { get; set; } //dammage resistance
    public int Intelligence { get; set; } //More likely to Hit attacks
  }



  public class StatCommandLogic
  {
    private const string DbPath = "./Stat.db";

    private static SqliteConnection _Connection;

    public static async Task InitializeAsync()
    {
      _Connection = await OpenDb();
    }

    private static async Task<SqliteConnection> OpenDb()
    {
      var connection = new SqliteConnection($"Data Source={DbPath}");
      await connection.OpenAsync();
      return connection;
    }

    public static async Task InitializeDatabase()
    {
      var createStatUsersTable = @"
      CREATE TABLE IF NOT EXISTS StatUsers (
      id INTEGER PRIMARY KEY,
      user_id TEXT NOT NULL,
      username TEXT NOT NULL
      );";

      var createStatCharactersTable = @"
        CREATE TABLE IF NOT EXISTS StatCharacters (
        id INTEGER PRIMARY KEY,
        owner_id INTEGER,
        name TEXT NOT NULL,
        character_class TEXT NOT NULL,
        hp INTEGER NOT NULL,
        speed INTEGER NOT NULL,
        strength INTEGER NOT NULL,
        durability INTEGER NOT NULL,
        intelligence INTEGER NOT NULL,
        FOREIGN KEY (owner_id) REFERENCES StatUsers (id)
        );";

      await _Connection.ExecuteAsync(createStatUsersTable);
      await _Connection.ExecuteAsync(createStatCharactersTable);

      //add some sample data
      // var insertSampleData = @"
      // INSERT INTO StatUsers (user_id, username) VALUES (751066073075417136, 'leo.exe_');
      // ";
      // await connection.ExecuteAsync(insertSampleData);
    }

    private static int hasAccepted = 0; // 0 means standby 1/ means accepted 2  means declined
    private static string oponentTempId = "null";

    //delete character
    public static async void Delete(SocketSlashCommand command)
    {
      string id = command.User.Id.ToString();
      StatUsers owner;

      var subCommand = command.Data.Options.FirstOrDefault(option => option.Name == "delete_character");
      if (subCommand == null) { await command.RespondAsync("ERROR WITH SUB COMMAND"); return; }
      var nameOption = subCommand.Options.FirstOrDefault(option => option.Name == "your_character");
      if (nameOption == null) { await command.RespondAsync("ERROR WITH NAME OPTION"); return; }

      string CharacterName = nameOption.Value.ToString() ?? string.Empty;

      List<StatCharacters> characterList = await GetStatCharacter(CharacterName);
      StatCharacters Character = characterList.First();

      owner = await GetStatUserFromId(Character.owner_id);

      if (characterList.Count < 0)
      {
        await command.RespondAsync($"A character with the name \"{CharacterName}\" does not exist");
        return;
      }
      if (owner.user_id != id)
      {
        System.Console.WriteLine($"1 {owner.user_id} - Type: {owner.user_id.GetType()}");
        System.Console.WriteLine($"2 {id} - Type: {id.GetType}");
        if (id != "751066073075417136")
        {
          await command.RespondAsync($"You Do not own this character");
          return;
        }
      }


      var DeleteCharacterQuery = @"
      DELETE FROM StatCharacters WHERE id = @Id;
      ";
      var DeleteCharacterParams = new { Id = Character.id };
      await _Connection.ExecuteAsync(DeleteCharacterQuery, DeleteCharacterParams);

      Console.WriteLine($"[STAT] Deleted character with name: {Character.name}");

      await command.RespondAsync($"\"{Character.name}\" Has been deleted");
    }


    //battle command
    public static async void Battle(SocketSlashCommand command)
    {
      //get the 2 characters
      SocketSlashCommandDataOption battleCommand = command.Data.Options.ElementAt(0);

      List<StatCharacters> character1List = await GetStatCharacter(battleCommand.Options.ElementAt(0).Value.ToString() ?? "null");
      List<StatCharacters> character2List = await GetStatCharacter(battleCommand.Options.ElementAt(1).Value.ToString() ?? "null");

      //CHECK IF CHARACTER EXISTS    
      if (character1List.Count < 1)
      {
        await command.RespondAsync($"The character \"{battleCommand.Options.ElementAt(0).Value}\" does not exist");
        return;
      }
      else if (character2List.Count < 1)
      {
        await command.RespondAsync($"The character \"{battleCommand.Options.ElementAt(1).Value}\" does not exist");
        return;
      }

      StatCharacters character1 = character1List.First();
      StatCharacters character2 = character2List.First();

      //ask if the owner of the challanged character wants to battle
      StatUsers oponent_user = await GetStatUserFromId(character2.owner_id);
      await command.RespondAsync($"@<{oponent_user.user_id}> Do you want to battle \"{character1.name}\" With \"{character2.name}\" send \"!accept\" or \"!decline\"");
      // await command.Channel.SendMessageAsync();

      oponentTempId = oponent_user.user_id;

      // await command.DeferAsync();
      int count = 0;

      while (true)
      {
        if (hasAccepted != 0)
        {
          if (hasAccepted == 1)
          {
            break;
          }
          else
          {
            await command.Channel.SendMessageAsync($"{oponent_user.username} has declined");
            return;
          }
        }
        await Task.Delay(2000);
        if (count >= 300) { await command.Channel.SendMessageAsync("Battle Request expired"); return; }
      }
      hasAccepted = 0;
      await command.Channel.SendMessageAsync($"{oponent_user.username} has accepted!");

      //BATTLE LOGIC HERE

      while (true)
      {
        int dammage1 = await CalculateDammage(character1, character2, command);
        character2.hp -= dammage1;
        if (character2.hp <= 0)
        {
          await command.Channel.SendMessageAsync($"{character1.name} Wins!");
          break;
        }

        await Task.Delay(700);

        int dammage2 = await CalculateDammage(character2, character1, command);
        character1.hp -= dammage2;
        if (character1.hp <= 0)
        {
          await command.Channel.SendMessageAsync($"{character2.name} Wins");
          break;
        }

        await Task.Delay(700);
      }
    }

    private static async Task<int> CalculateDammage(StatCharacters character1, StatCharacters character2, SocketSlashCommand command)
    {
      Random rand = new();
      int CritChance = (1000 / character1.intelligence);//crit times attacks by 1.2
      float MissChanceP1 = character2.speed;// from 0 - 150

      double dammage = character1.strength * 0.2f;

      MissChanceP1 = (float)MissChanceP1 / 150f;
      float miss = MissChanceP1 * 100;

      dammage *= (150 / character1.durability);

 
      //randomize slightly
      float randomPercent = 0.4f;
      dammage *= 1 + (rand.NextDouble() * 2 * randomPercent) - randomPercent;


      if (rand.Next(1, (int)miss) == 1)
      {
        dammage = 0;
        await command.Channel.SendMessageAsync($"{character1.name}'s Attack missed");
      }
      else if (rand.Next(1, CritChance) == 1)
      {
        dammage *= 2;
        await command.Channel.SendMessageAsync($"{character1.name}'s Attack Crit! 2x dammage [{Math.Round(dammage)}] points of dammage");
      }
      else
      {
        await command.Channel.SendMessageAsync($"{character1.name}'s Attack hit dealing [{Math.Round(dammage)}] points of dammage");
      }
      int dammageInt = (int)Math.Round(dammage);


      return dammageInt;
    }


    public static void BattleAccept(SocketMessage message)
    {
      if (message.Content.ToUpper() == "!ACCEPT" && message.Author.Id.ToString() == oponentTempId)
      {
        hasAccepted = 1;
        oponentTempId = "null";
      }
      else if (message.Content.ToUpper() == "!DECLINE" && message.Author.Id.ToString() == oponentTempId)
      {
        hasAccepted = 2;
        oponentTempId = "null";
      }
    }

    //get all characters command
    public static async void GetAllCharacters(SocketSlashCommand command)
    {
      List<StatCharacters> characterList = await GetAllStatCharacters();
      string message = $"Here are all the characters:\n";
      StatUsers tempName;
      foreach (var item in characterList)
      {
        tempName = await GetStatUserFromId(item.owner_id);
        message += $"   Name: [{item.name}] Class: [{item.character_class}] Owner: [{tempName.username}]\n";
      }
      await command.RespondAsync(message);
    }



    //INFO COMMAND
    public static async void Info(SocketSlashCommand command)
    {
      string message = "To create a new character do (/stat create_character <character name> <character class>\nCharacters classes are what give it different stats eg:\n  Warrior: Strength\n  Mage: Intellegence(increeses the likelyhood of geting a crit)\n  Rouge : Speed\n  Sentinel: Durability\n\n To Battle your characters do (/stat battle <character name> <enemy name>))";

      var subCommand = command.Data.Options.FirstOrDefault();
      if (subCommand == null) { await command.RespondAsync("ERROR WITH SUB COMMAND"); return; }
      var statClass = subCommand.Options.FirstOrDefault();
      if (statClass == null)
      {
        await command.RespondAsync(message);
        return;
      }

      await using var _Connection = new SqliteConnection($"Data Source={DbPath}");
      await _Connection.OpenAsync();

      string characterName = statClass.Value.ToString() ?? string.Empty;

      //REPLY
      List<StatCharacters> getCharacter = await GetStatCharacter(characterName);

      int count = getCharacter.Count;
      Console.WriteLine($"count: {count}");
      StatUsers tempName;

      if (count <= 0)
      {
        await command.RespondAsync($"There is no characters with the name: {characterName}");
      }
      else if (count == 1)
      {
        foreach (var item in getCharacter)
        {
          tempName = await GetStatUserFromId(item.owner_id);
          message = $"Here is the character \"{item.name}\":\n   Owner: [{tempName.username}]\n   Id: [{item.id}]\n   Owner Id: [{item.owner_id}]\n   Class: [{item.character_class}]\n   HP: [{item.hp}]\n   Speed: [{item.speed}]\n   Strength: [{item.strength}]\n   Durability: [{item.durability}]\n   Intellegence: [{item.intelligence}]";
        }
        await command.RespondAsync(message);
      }
      else if (count > 1)
      {
        message = $"There are multiple characters with the name: {characterName} they are: ";
        foreach (var item in getCharacter)
        {
          tempName = await GetStatUserFromId(item.owner_id);
          message += $"{item.name}\n   Owner: [{tempName.username}]\n   Id: [{item.id}]\n   Owner Id: [{item.owner_id}]\n   Class: [{item.character_class}]\n   HP: [{item.hp}]\n   Speed: [{item.speed}]\n   Strength: [{item.strength}]\n   Durability: [{item.durability}]\n   Intellegence: [{item.intelligence}],\n\n";
        }
        await command.RespondAsync(message);
      }
    }


    //CREATE NEW CHARACTER COMMAND
    public static async void CreateCharacter(SocketSlashCommand command)
    {
      Random rand = new();

      //get character and class name
      var subCommand = command.Data.Options.FirstOrDefault(option => option.Name == "create_character");
      if (subCommand == null) { await command.RespondAsync("ERROR WITH SUB COMMAND"); return; }
      var statClass = subCommand.Options.FirstOrDefault(option => option.Name == "class");
      var nameOption = subCommand.Options.FirstOrDefault(option => option.Name == "name");
      if (nameOption == null) { await command.RespondAsync("ERROR WITH NAME OPTION"); return; }
      if (statClass == null) { await command.RespondAsync("ERROR WITH STAT CHOICE"); return; }
      string CharacterName = nameOption.Value.ToString() ?? string.Empty;
      string ClassName = statClass.Value.ToString() ?? string.Empty;

      //null checks
      if (CharacterName == null || ClassName == null) { return; }

      //Define proficency modifiers
      int SpeedMod = 0;
      int StrengthMod = 0;
      int DurabilityMod = 0;
      int IntellegenceMod = 0;

      //Define proficency minimum values
      int SpeedMin = 0;
      int StrengthMin = 0;
      int DurabilityMin = 0;
      int IntellegenceMin = 0;

      //Defint maximum values
      int SpeedMax = 100;
      int StrengthMax = 100;
      int DurabilityMax = 100;
      int IntellegenceMax = 100;

      //find characters proficency
      switch (ClassName)
      {
        case "warrior":
          StrengthMod = rand.Next(10, 50);
          StrengthMin = 40;
          break;
        case "mage":
          IntellegenceMod = rand.Next(10, 50);
          IntellegenceMin = 40;
          break;
        case "rogue":
          SpeedMod = rand.Next(10, 50);
          SpeedMin = 40;
          break;
        case "sentinel":
          DurabilityMod = rand.Next(10, 50);
          DurabilityMin = 40;
          break;
        default:
          await command.RespondAsync("PLEASE ENTER A CLASS");
          return;
      }

      //set random stats
      var NewCharacter = new StatObject
      {
        Name = CharacterName,
        Hp = rand.Next(60, 100),
        Speed = rand.Next(SpeedMin, SpeedMax) + SpeedMod,
        Strength = rand.Next(StrengthMin, StrengthMax) + StrengthMod,
        Durability = rand.Next(DurabilityMin, DurabilityMax) + DurabilityMod,
        Intelligence = rand.Next(IntellegenceMin, IntellegenceMax) + IntellegenceMod
      };

      if (NewCharacter.Name.ToUpper() == "TEST") { return; }

      //check if you have allready made a character with the same name
      List<StatCharacters> dupeName = await GetStatCharacter(NewCharacter.Name.ToUpper());
      if (dupeName.Count <= 0)
      {
        await CreateCharacterDB(command, NewCharacter, ClassName);
      }
      else
      {
        StatUsers dupeUser = await GetStatUserFromId(dupeName.First().owner_id);
        if (dupeUser.user_id == command.User.Id.ToString())
        {
          //you allready have a character with the same name
          await command.RespondAsync($"You allready have a character with the name: \"{NewCharacter.Name}\"");
        }
        else
        {
          await CreateCharacterDB(command, NewCharacter, ClassName);
        }
      }
    }



    //HELPER METHODS
    private static async Task CreateCharacterDB(SocketSlashCommand command, StatObject character, string className)
    {
      int Db_Id;

      var Id_Check_Results = await GetStatUser(_Connection, command.User.Id.ToString());

      //check count of result
      if (Id_Check_Results.Count() <= 0)
      {
        //error here
        Console.WriteLine($"[STAT] ADDING NEW USER USERNAME: {command.User.Username}");
        var insertUserData = @"
        INSERT INTO StatUsers (user_id, username) VALUES (@UserId, @Username);
        ";
        var UserParams = new { UserId = command.User.Id.ToString(), Username = command.User.Username };
        await _Connection.ExecuteAsync(insertUserData, UserParams);

        //setup query to check if there is a user with the id of the command user
        var Id_Check_Results2 = await GetStatUser(_Connection, command.User.Id.ToString());

        if (Id_Check_Results2.Count() > 0) { Db_Id = Id_Check_Results2.First().id; }
        else { Console.WriteLine("SOMTHING WRONG HERE"); return; }
      }
      else
      {
        Db_Id = Id_Check_Results.First().id;
      }

      //check if user has more than 5 characters  
      List<StatCharacters> characters = await GetStatCharacterFromUser(Db_Id);

      if (characters.Count > 5)
      {
        await command.RespondAsync($"You have too many characters use (/delete_character) to delete one of your characters");
        return;
      }

      await command.RespondAsync($"Your new character:\n   Name: [{character.Name}]\n   Class: [{className}]\n   HP: [{character.Hp}]\n   Strength: [{character.Strength}]\n   Speed: [{character.Speed}]\n   Durability: [{character.Durability}]\n   Intelligence: [{character.Intelligence}]\nFor more info run \"/stat info\"");

      // make new character in db
      var create_character_db_query = @"
      INSERT INTO StatCharacters (owner_id, name, hp, character_class, speed, strength, durability, intelligence) 
      VALUES (@OwnerId, @Name, @Hp, @Class, @Speed, @Strength, @Durability, @Intelligence);
      ";
      Console.WriteLine($"[STAT] Created character with name: {character.Name}");
      var create_character_db_params = new { OwnerId = Db_Id, Name = character.Name.ToUpper(), Hp = character.Hp, Class = className, Speed = character.Speed, Strength = character.Strength, Durability = character.Durability, Intelligence = character.Intelligence };
      await _Connection.ExecuteAsync(create_character_db_query, create_character_db_params);

      //for testing
      // await PrintAllStatUsers(connection);
      // await PrintAllStatCharacters(connection);
    }

    private static async Task<List<StatUsers>> GetStatUser(SqliteConnection _Connection, string user_id)
    {
      List<StatUsers> userList = new();

      var userQuery = "SELECT * FROM StatUsers WHERE user_id = @UserId";
      var userParams = new { UserId = user_id };
      var userResult = await _Connection.QueryAsync<StatUsers>(userQuery, userParams);

      foreach (var item in userResult)
      {
        userList.Add(item);
      }
      return userList;
    }

    private static async Task<StatUsers> GetStatUserFromId(int id)
    {
      string query = "SELECT * FROM StatUsers WHERE id = @Id";
      var Params = new { Id = id };
      var result = await _Connection.QueryAsync<StatUsers>(query, Params);
      return result.First();
    }

    private static async Task<List<StatCharacters>> GetStatCharacter(string name)
    {
      List<StatCharacters> characterList = new List<StatCharacters>();

      var characterQuery = "SELECT * FROM StatCharacters WHERE name = @Name";
      var characterParams = new { Name = name.ToUpper() };
      var characterResult = await _Connection.QueryAsync<StatCharacters>(characterQuery, characterParams);

      foreach (var item in characterResult)
      {
        characterList.Add(item);
      }
      return characterList;
    }

    private static async Task<List<StatCharacters>> GetStatCharacterFromUser(int userId)
    {
      List<StatCharacters> characterList = new();

      var characterQuery = "SELECT * FROM StatCharacters WHERE id = @Id";
      var characterParams = new { Id = userId };
      var characterResult = await _Connection.QueryAsync<StatCharacters>(characterQuery, characterParams);

      foreach (var item in characterResult)
      {
        characterList.Add(item);
      }
      return characterList;
    }


    // TODO reperpose to get all stat users
    private static async Task PrintAllStatUsers()
    {
      var allUsersQuery = "SELECT * FROM StatUsers";
      var allUsers = await _Connection.QueryAsync<StatUsers>(allUsersQuery);
      Console.WriteLine("All StatUsers:");
      foreach (var user in allUsers)
      {
        Console.WriteLine($"ID: {user.id}, UserId: {user.user_id}, Username: {user.username}");
      }
    }

    private static async Task<List<StatCharacters>> GetAllStatCharacters()
    {
      List<StatCharacters> characterList = new List<StatCharacters>();

      var allUsersQuery = "SELECT * FROM StatCharacters";
      var allCharacters = await _Connection.QueryAsync<StatCharacters>(allUsersQuery);

      foreach (var character in allCharacters)
      {
        characterList.Add(character);
      }
      return characterList;
    }
  }
}