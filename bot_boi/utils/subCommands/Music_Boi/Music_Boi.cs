using System;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace bot_boi.utils.subCommands.Music_Boi;

// TODO 
// Add Skiping
// Add youtube 
public class Music_Boi_CommandBuilder
{
  public static SlashCommandOptionBuilder Play_Command()
  {
    return new SlashCommandOptionBuilder()
      .WithName("add")
      .WithDescription("Add a youtube video to the queue!")
      .WithType(ApplicationCommandOptionType.SubCommand)
      .AddOption("link", ApplicationCommandOptionType.String, "youtube link", isRequired: true);
  }
  public static SlashCommandOptionBuilder Disconect_Command()
  {
    return new SlashCommandOptionBuilder()
      .WithName("disconect")
      .WithDescription("Disconect Bot Boi from the vc")
      .WithType(ApplicationCommandOptionType.SubCommand);
  }
  public static SlashCommandOptionBuilder Pause_Command()
  {
    return new SlashCommandOptionBuilder()
      .WithName("toggle_pause")
      .WithDescription("pauses / plays the current audio")
      .WithType(ApplicationCommandOptionType.SubCommand);
  }
  public static SlashCommandOptionBuilder Skip_Command()
  {
    return new SlashCommandOptionBuilder()
      .WithName("skip")
      .WithDescription("skips the current playing video")
      .WithType(ApplicationCommandOptionType.SubCommand);
  }
}

public class Music_Boi_CommandHandler
{
  private readonly DiscordSocketClient _Client;
  private readonly CommandService _Commands;
  private readonly Music_BoiLogic _Logic;
  public Music_Boi_CommandHandler(DiscordSocketClient client, CommandService command)
  {
    _Client = client;
    _Commands = command;
    _Logic = new Music_BoiLogic();
  }

  public void Music_Boi_Commands(SocketSlashCommand command)
  {
    string name = command.Data.Options.First().Name;

    switch (name)
    {
      case "add":
        _Logic.play(command, _Client);
        break;
      case "disconect":
        _Logic.Disconect(command);
        break;
      case "toggle_pause":
        _Logic.Pause(command);
        break;
    }
  }
}

public class Music_BoiLogic
{
  private enum PlaybackState
  {
    Playing,
    Paused,
    Stopped
  }
  private PlaybackState CurrentPlaybackState = PlaybackState.Stopped;
  private ManualResetEventSlim PauseEvent = new ManualResetEventSlim(true);

  private ConcurrentQueue<string> SongQueue = new ConcurrentQueue<string>();
  private IAudioClient? AudioCLient = null;

  public async void play(SocketSlashCommand command, DiscordSocketClient client)
  {
    //get url
    var subCommand = command.Data.Options.FirstOrDefault(option => option.Name == "add"); //name of sub command
    if (subCommand == null) { await command.RespondAsync("ERROR WITH SUB COMMAND"); return; }
    var nameOption = subCommand.Options.FirstOrDefault(option => option.Name == "link"); //Name of Option
    if (nameOption == null) { await command.RespondAsync("ERROR WITH NAME OPTION"); return; }

    string url = nameOption.Value.ToString() ?? string.Empty;

    //check if url is valid
    const string pattern = @"^(?:https?:\/\/)?(?:www\.)?(?:youtube\.com|youtu\.be)\/(?:watch\?v=|embed\/|v\/|u\/\w+\/|shorts\/)([\w-]{11})";
    Regex regex = new(pattern);

    if (!regex.IsMatch(url))
    {
      await command.RespondAsync("Url not valid please provide a valid youtube url.");
      return;
    }

    //get Socket Guild
    ulong? guildId = command.GuildId;
    if (guildId == null)
    {
      await command.RespondAsync("ERROR: Cant get GuildId @Music_Boi.cs");
      return;
    }
    SocketGuild? guild = client.GetGuild(guildId.Value);
    if (guild == null)
    {
      await command.RespondAsync("ERROR: Guild not found @Music_Boi.cs");
      return;
    }


    // ulong vcId;
    SocketGuildUser guildUser = guild.GetUser(command.User.Id);
    ulong vcId;

    //If user is in a vc get the id
    if (guildUser.VoiceChannel == null)
    {
      vcId = 906940817187405865;
    }
    else
    {
      vcId = guildUser.VoiceChannel.Id;
    }

    //Connect
    if (this.AudioCLient == null || this.AudioCLient.ConnectionState == ConnectionState.Disconnected || this.AudioCLient.ConnectionState == ConnectionState.Disconnecting)
    {
      ConnectVc(guild, vcId);
    }

    try
    {
      await EnsureAudioClientReadyAsync(TimeSpan.FromSeconds(10)); // Wait for up to 10 seconds
      if (SongQueue.Count() > 1)
      {
        await command.RespondAsync($"Added to the queue {url} curren queue {SongQueue.ToArray()}");
      }
      else
      {
        await command.RespondAsync($"Added to the queue {url}");
      }
      SongQueue.Enqueue(url);
      if (CurrentPlaybackState == PlaybackState.Stopped)
      {
        await AddToQueue(AudioCLient!);
      }
    }
    catch (TimeoutException ex)
    {
      System.Console.WriteLine("Timeout: " + ex.Message);
      await command.RespondAsync("AudioClient is not ready. Please try again later.");
    }
  }


  private async Task EnsureAudioClientReadyAsync(TimeSpan timeout)
  {
    var startTime = DateTime.UtcNow;

    while (this.AudioCLient == null)
    {
      System.Console.WriteLine("Waiting for AudioClient to be ready...");

      await Task.Delay(100);
      if (DateTime.UtcNow - startTime > timeout)
      {
        throw new TimeoutException("AudioClient did not initialize");
      }
    }
  }


  private async void ConnectVc(SocketGuild guild, ulong vcId)
  {
    //Make a SocketVoiceChannel instance and join
    SocketVoiceChannel VC = guild.GetVoiceChannel(vcId);

    IAudioClient audioClient = await VC.ConnectAsync();
    AudioCLient = audioClient;
    return;
  }

  private async Task AddToQueue(IAudioClient client)
  {
    while (true)
    {
      // Try to dequeue a song
      if (!SongQueue.TryDequeue(out string? currentSong))
      {
        Console.WriteLine("Queue is empty. Stopping playback.");
        CurrentPlaybackState = PlaybackState.Stopped;
        break; // Exit the loop if the queue is empty
      }

      //YT STUFF
      string sourcePath;

      if (Uri.IsWellFormedUriString(currentSong, UriKind.Absolute) && currentSong.Contains("youtube.com"))
      {
        try
        {
          sourcePath = await GetYtAudioUrl(currentSong);
        }
        catch (Exception e)
        {
          System.Console.WriteLine($"Cant fetch youtube url {e}");
          continue;
        }
      }
      else
      {
        sourcePath = currentSong;
      }


      var ffmpeg = CreateStream(sourcePath); //CHANGE TO URL
      ffmpeg.Start();

      CurrentPlaybackState = PlaybackState.Playing;

      var output = ffmpeg.StandardOutput.BaseStream;
      var discord = client.CreatePCMStream(AudioApplication.Mixed);

      try
      {
        Console.WriteLine("Playing");

        while (true)
        {
          PauseEvent.Wait(); //wait if paused
          byte[] buffer = new byte[3840];
          int byteRead = await output.ReadAsync(buffer, 0, buffer.Length);

          if (byteRead == 0)
            break;

          await discord.WriteAsync(buffer, 0, byteRead);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine($"Error during playback: {e.Message}");
      }
      finally
      {
        await discord.FlushAsync(); // Wait for the buffer to clear
        ffmpeg.Dispose();
        discord.Dispose();
        Console.WriteLine("Finished playing");
      }
    }

    CurrentPlaybackState = PlaybackState.Stopped;
  }

  private Process CreateStream(string path)
  {
    //create audio stream
    return new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = "ffmpeg",
        Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 -bufsize 512k pipe:1",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      }
    };
  }

  private async Task<string> GetYtAudioUrl(string Url)
  {
    var processInfo = new ProcessStartInfo
    {
      FileName = "C:/Users/leott/Documents/bot_boi/bot_boi/data/yt-dlp.exe",
      Arguments = $"-f bestaudio --get-url \"{Url}\"",
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    using (var process = Process.Start(processInfo))
    {
      if (process == null)
        throw new InvalidOperationException("yt-dlp no workie");

      string audioUrl = await process.StandardOutput.ReadToEndAsync();
      process.WaitForExit();

      if (string.IsNullOrWhiteSpace(audioUrl))
        throw new Exception("canot retreve url");

      return audioUrl.Trim();
    }
  }

  public void Disconect(SocketSlashCommand command)
  {
    if (AudioCLient != null && AudioCLient.ConnectionState == ConnectionState.Connected)
    {
      SongQueue.Clear();
      PauseEvent.Set();
      CurrentPlaybackState = PlaybackState.Stopped;
      AudioCLient.Dispose();
      AudioCLient = null;
    }

    command.RespondAsync("Disconecting...");
  }

  public async void Pause(SocketSlashCommand command)
  {
    if (CurrentPlaybackState == PlaybackState.Playing)
    {
      PauseEvent.Reset();
      CurrentPlaybackState = PlaybackState.Paused;
      Console.WriteLine("Playback Paused");
      await command.RespondAsync("Paused");
    }
    else if (CurrentPlaybackState == PlaybackState.Paused)
    {
      PauseEvent.Set();
      CurrentPlaybackState = PlaybackState.Playing;
      await command.RespondAsync("Playback Unpaused");
      Console.WriteLine("Unpaused");
    }
    else
    {
      Console.WriteLine("Canot Pause");
    }
  }
}
