using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace BlinDeexDc;

public class SlashCommands : ApplicationCommandModule
{
    
    [SlashCommand("Play", "Plays a song")]
    public async Task PlayCommand(InteractionContext ctx, [Option("Url", "Song URL")] string url)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (ctx.Member?.VoiceState?.Channel! == null!)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must be in a voice channel to use this command!"));
            return;
        }
        
        await Database.Play.JoinChannelAsync(ctx.Member.VoiceState.Channel);
        Database.SongsQueue.Add(url);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Playing song!"));
    }

    [SlashCommand("Skip", "Skips songs")]
    public async Task SkipCommand(InteractionContext ctx, [Option("Amount", "Number of songs to skip")] long amount = 1)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (amount <= 0)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Please specify a positive number of songs to skip."));
            return;
        }

        await Database.Play.SkipSongs((int)amount);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Skipped {amount} songs!"));
    }
    
    [SlashCommand("Previous", "goes back")]
    public async Task PreviousCommand(InteractionContext ctx, [Option("Amount", "Number of songs to skip")] long amount = 1)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (amount <= 0)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Please specify a positive number of songs to skip."));
            return;
        }

        await Database.Play.RollBackSongs((int)amount);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Rolled back {amount} songs!"));
    }
    
    [SlashCommand("Queue", "Skips songs")]
    public async Task QueueSize(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent($"There are {Database.SongsQueue} songs in the list!"));
    }
    
    [SlashCommand("GetUI", "Opens UI")]
    public async Task UI(InteractionContext ctx)
    {
        var builder = Database.GetUI();
        
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
    }
    
    [SlashCommand("Index", "Tells index of currently playing song, useful to know how much to skip/return")]
    public async Task CurrentSongIndex(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent($"Currently playing song index: {Database.CurrentSong}"));
    }

    [SlashCommand("Search", "Search youtube and plays first result")]
    public async Task SearchCommand(InteractionContext ctx, [Option("Sentence", "What to search for")] string sentence)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Searching..."));
        
        if (ctx.Member?.VoiceState?.Channel == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must be in a voice channel to use this command!"));
            return;
        }
        
        await Database.Play.JoinChannelAsync(ctx.Member.VoiceState.Channel);
        
        string result = await Database.Play.SearchBySentence(sentence);
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(result));
    }

    [SlashCommand("PlayList", "Loads full playlist into queue")]
    public async Task PlayListCommand(InteractionContext ctx, [Option("Url", "Song URL")] string url)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (ctx.Member?.VoiceState?.Channel == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must be in a voice channel to use this command!"));
            return;
        }

        await Database.Play.JoinChannelAsync(ctx.Member.VoiceState.Channel);

        await Database.Play.NewPlaylist(url, 0);

        DiscordInteractionResponseBuilder builder = new();

        await ctx.DeleteResponseAsync();
    }

    [SlashCommand("TopGezai", "Displays top gezus")]
    public async Task TopGezaiCommand(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        StringBuilder gezuListSb = new();
        var topGezai = Database.Duhai.OrderByDescending(pair => pair.Value)
                                      .Take(15)
                                      .ToDictionary(pair => pair.Key, pair => pair.Value);

        if (topGezai.Count == 0)
        {
            gezuListSb.Append("Nera gezu! (kolkas)");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(gezuListSb.ToString()));
            return;
        }

        foreach (var topGezas in topGezai)
        {
            gezuListSb.AppendLine($"{topGezas.Key} == {topGezas.Value}");
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(gezuListSb.ToString()));
    }

    [SlashCommand("VoteGezas", "Who is gezas??")]
    public async Task VoteGezas(InteractionContext ctx, [Option("Gezas", "Which user is gezas?")] DiscordUser targetUser)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        string user = ctx.Member.Username;
        string target = targetUser.Username;

        if (!Database.Duhai.TryAdd(target, 1)) Database.Duhai[target] += 1;

        if (user == target)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{user} pasigezino save lmao"));
            return;
        }
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"@{ctx.Member.Mention} galvoja, jog @{targetUser.Mention} yra gezas!"));
        
    }
}
