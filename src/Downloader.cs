using Spectre.Console;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using FFMpegCore;


class Downloader
{
    private static async Task ConvertToMp3Async(string filePath)
    {
        if (!Path.GetExtension(filePath).ToLower().Equals(".mp3"))
        {
            AnsiConsole.WriteLine($"{filePath} --> .mp3");

            string newPath = Path.ChangeExtension(filePath, ".mp3");

            await FFMpegArguments
                .FromFileInput(filePath)
                .OutputToFile(newPath)
                .ProcessAsynchronously();

            File.Delete(filePath);
        }

    }


    public static async Task ConvertToMp3ParallelAsync(string[] filePaths)
    {
        int current = 0;

        await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("[blue]Converting audio to mp3 [/]", maxValue: filePaths.Length);

            await Parallel.ForEachAsync(
                filePaths,
                new ParallelOptions { MaxDegreeOfParallelism = 5 },
                async (filePath, ct) =>
                {
                    try
                    {
                        await ConvertToMp3Async(filePath);
                    }
                    catch (Exception e)
                    {
                        AnsiConsole.WriteException(e);
                    }

                    current += 1;
                    task.Value = current;
                }

            );


            task.Description = "[green]Conversion to mp3 complete [/]";
            task.Value = filePaths.Length;
        });
    }

    private static string GetPath(string outputFolder, string title, string author, IStreamInfo streamInfo)
    {
        List<char> invalidChars = ['"', '\\', .. Path.GetInvalidFileNameChars()];

        // Sanitize
        foreach (var c in invalidChars)
        {
            title = title.Replace(c, '_');
            author = author.Replace(c, '_');
        }

        string path = Path.Join(outputFolder, $"{title}-{author}.{streamInfo.Container}");

        return path.Trim();
    }

    public static async Task DownloadAsync(string url, string outputFolder)
    {
        var youtubeClient = new YoutubeClient();
        var video = await youtubeClient.Videos.GetAsync(url);
        string title = video.Title;
        string author = video.Author.ChannelTitle;

        var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(url);
        var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();


        string fullPath = GetPath(outputFolder, title, author, streamInfo);
        AnsiConsole.WriteLine(fullPath);
        await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, fullPath);

        youtubeClient.Dispose();
    }

    public static async Task DownloadAsyncParallel(List<string> urls, string outputFolder)
    {
        int current = 0;


        await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask($"[blue]Downloading audio [/]", maxValue: urls.Count);

            await Parallel.ForEachAsync(
                urls,
                new ParallelOptions { MaxDegreeOfParallelism = 10 },
                async (url, ct) =>
                {
                    task.Description = $"[blue]Downloading {url} audio [/]";

                    try
                    {
                        await DownloadAsync(url, outputFolder);
                    }
                    catch (Exception e)
                    {
                        AnsiConsole.WriteException(e);
                    }

                    current += 1;
                    task.Value = current;
                }

            );

            task.Description = "[green]Download complete [/]";
            task.Value = urls.Count;
        });
    }
}