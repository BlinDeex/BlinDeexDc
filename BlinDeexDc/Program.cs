// See https://aka.ms/new-console-template for more information

using BlinDeexDc;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

await Initialize(Array.Empty<string>());
return;

static async Task Initialize(string[] args)
{
    var discord = Database.DiscordClient;
    
    discord.MessageCreated += async (sender, eventArgs) =>
    {
        if (eventArgs.Message.Content.StartsWith("ping", StringComparison.CurrentCultureIgnoreCase))
        {
            await eventArgs.Message.RespondAsync("Pong!");
        }
    };

    var commands = discord.UseSlashCommands();
    commands.RegisterCommands<SlashCommands>(Database.GUILD_ID);
    
    Database.Play = new VoicePlay();
    Database.Play.RunTicks();
    
    Database.Commands = new SlashCommands();

    discord.ComponentInteractionCreated += async (sender, eventArgs) =>
    {
        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, Database.GetUI());

        switch (eventArgs.Id)
        {
            case Database.PREVIOUS_BUTTON:
                await Database.Play.RollBackSongs(1);
                break;
            case Database.PAUSE_BUTTON:
                break;
            case Database.SKIP_BUTTON:
                await Database.Play.SkipSongs(1);
                break;
        }
    };

    await discord.ConnectAsync();
    await Task.Delay(-1);
    
}