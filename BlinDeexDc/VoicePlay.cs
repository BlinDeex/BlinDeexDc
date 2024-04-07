using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace BlinDeexDc;

public class VoicePlay
{

    private VoiceNextConnection? connection;

    private VoiceNextExtension voice = Database.DiscordClient.UseVoiceNext(new VoiceNextConfiguration()
    {
        AudioFormat = AudioFormat.Default
    });

    private static readonly YoutubeClient Youtube = new();

    private static readonly StreamClient StreamClient = new(new HttpClient());
    private static readonly TimeSpan updateInterval = TimeSpan.FromSeconds(0.5f);
    private Process? ffmpeg;
    
    public async Task JoinChannelAsync(DiscordChannel channel)
    {
        if (connection != null)
        {
            Console.WriteLine("user is already in channel, cant join again!");
            return;
        }

        if (channel == null!)
        {
            Console.WriteLine("user voice channel is null!");
            return;
        }

        connection = await channel.ConnectAsync();

    }

    public void LeaveChannelAsync()
    {
        if (connection == null)
        {
            Console.WriteLine("connection was already null! Cant disconnect");
            return;
        }
        
        connection.Disconnect();
        connection = null;
    }

    public async Task RollBackSongs(int amount)
    {
        amount++;
        await Database.Play.StopCurrentSong();

        Database.CurrentSong = Database.CurrentSong > amount ? Database.CurrentSong - amount : -1;
    }

    public async Task SkipSongs(int amount)
    {
        await Database.Play.StopCurrentSong();

        int sum = Database.CurrentSong + amount;
        
        Database.CurrentSong = sum > Database.SongsQueue.Count ? Database.SongsQueue.Count : sum;
    }

    public async void RunTicks()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    
                    await Tick();
                    
                    await Task.Delay(updateInterval);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during update loop: {ex}");
                }
            }
        });
    }

    private async Task Tick()
    {
        if (connection == null)
        {
            Console.WriteLine("no connection for ticks!");
            return;
        }

        bool isPlaying = connection.IsPlaying;
        
        Console.WriteLine(isPlaying);

        if (!isPlaying && Database.SongsQueue.Count > 0 && Database.CurrentSong < Database.SongsQueue.Count)
        {
            while (Database.CurrentSong < Database.SongsQueue.Count - 1)
            {
                Database.CurrentSong++;
                string nextUrl = Database.SongsQueue[Database.CurrentSong];
                var streamManifest = await Youtube.Videos.Streams.GetManifestAsync(nextUrl);
                if (streamManifest.Streams.Count == 0)
                {
                    Console.WriteLine("invalid url! skipping song");
                    continue;
                }
                
                Database.CurrentPlayingUrl = nextUrl;
                
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                //Video video = await Youtube.Videos.GetAsync(VideoId.Parse(nextUrl));
                //Database.CurrentPlayingImageUrl = "pAZWv8QQ8Co&t=982s";
                
                if(File.Exists("C:/song.mp3")) File.Delete("C:/song.mp3");
                await StreamClient.DownloadAsync(audioStreamInfo, "C:/song.mp3");

                await StartNewSong();

            }
        }
    }

    public async Task NewPlaylist(string url, long start)
    {
        var videos = await Youtube.Playlists.GetVideosAsync(url);

        for (int i = (int)start; i < videos.Count; i++)
        {
            Console.WriteLine(videos[i].Title);
            Database.SongsQueue.Add(videos[i].Url);
        }
    }

    public async Task<string> SearchBySentence(string sentence)
    {
        
        var foundVideos = await Youtube.Search.GetVideosAsync(sentence);

        if (foundVideos.Count == 0)
        {
            Console.WriteLine("no videos were found!");
            return "no search results!";

        }
        
        Database.SongsQueue.Add(foundVideos[0].Url);

        return "loaded new song: " + foundVideos[0].Title;

    }

    private async Task StartNewSong()
    {
        if (connection == null)
        {
            Console.WriteLine("StartNewSong conn null");
            return;
        }
        
        var transmit = connection.GetTransmitSink();


        await JoinChannelAsync(connection.TargetChannel);
        if(!connection.IsPlaying) await connection.SendSpeakingAsync();
        
        var pcm = ConvertAudioToPcm("C:/song.mp3");
        await pcm.CopyToAsync(transmit);
        
    }

    

    private Stream ConvertAudioToPcm(string cSongMp3)
    {
        ffmpeg = Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"""-i "{cSongMp3}" -ac 2 -f s16le -ar 48000 pipe:1""",
            RedirectStandardOutput = true,
            UseShellExecute = false
        });

        Stream pcm = ffmpeg!.StandardOutput.BaseStream;

        return pcm;
    }

    public async Task StopCurrentSong()
    {
        if (connection == null) return;
        if (ffmpeg == null) return;
        ffmpeg.Kill();
        ffmpeg.Dispose();
        ffmpeg = null;
        await connection.SendSpeakingAsync(false);
    }
}
