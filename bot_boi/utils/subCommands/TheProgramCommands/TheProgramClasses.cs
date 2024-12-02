using System;

namespace bot_boi.utils.subCommands.TheProgramCommands;

public class TheProgramUserData
{
  public int id { get; set; }
  public required string username { get; set; }
  public required int[] divValues { get; set; }
  public long timestamp { get; set; }
}