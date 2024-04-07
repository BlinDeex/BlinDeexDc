using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using YoutubeExplode;

namespace BlinDeexDc;

public static class Database
{
    public static readonly Dictionary<string, int> Duhai = new();

    public static readonly List<string> SongsQueue = new(128);

    public static DiscordChannel? TargetChannel = null;

    public static int CurrentSong { get; set; } = -1;
    
    public const string PREVIOUS_BUTTON = "previous";
    public const string PAUSE_BUTTON = "pause"; 
    public const string SKIP_BUTTON = "skip";

    public static string CurrentPlayingUrl { get; set; } = "";
    public static string CurrentPlayingName { get; set; } = "";

    public static string CurrentPlayingImageUrl { get; set; } = "";

    public const ulong GUILD_ID = 548225482475175936;

    public static readonly DiscordClient DiscordClient = new(new DiscordConfiguration()
    {
        Token = "MTIyNTA5ODQ0ODI1MzIyMjk1Mg.GICyI-.TLvAVcpf3EJeC36qYE7dr7jLj3wnBNFupmoZBI",
        TokenType = TokenType.Bot,
        Intents = DiscordIntents.All
    });

    public static VoicePlay Play { get; set; } = null!;
    public static SlashCommands Commands { get; set; } = null!;
    
    public static DiscordInteractionResponseBuilder GetUI()
    {
        var embed = new DiscordEmbedBuilder();
        embed.Title = CurrentPlayingName;
        embed.Color = DiscordColor.Blue;
        embed.ImageUrl = CurrentPlayingUrl;
        
        var builder = new DiscordInteractionResponseBuilder().AddComponents([

            new DiscordButtonComponent(ButtonStyle.Primary, PREVIOUS_BUTTON, "\u25c4"),
            new DiscordButtonComponent(ButtonStyle.Primary, PAUSE_BUTTON, "\u220e", true),
            new DiscordButtonComponent(ButtonStyle.Primary, SKIP_BUTTON, "\u25ba")
        ]);

        builder.AddEmbed(embed);

        return builder;
    }
}