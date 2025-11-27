using System.Globalization;
using CsvHelper;
using Spectre.Console;
using YoutubeExplode;

class PlaylistItem(string trackName, string artistName)
{
    public string TrackName { get; set; } = trackName;
    public string ArtistName { get; set; } = artistName;
}

class Extractor()
{
    public static async Task ExtractUrlsAsync(List<PlaylistItem> playlist, List<string?> fullUrls)
    {
        (var urls, var failedList) = await Extractor.GetUrlsFromPlaylistParallel(playlist);

        fullUrls.AddRange(urls);

        if (failedList.Count > 0)
        {
            AnsiConsole.WriteLine("⏳ Retrying failed urls");
            await ExtractUrlsAsync(failedList, fullUrls);
        }
    }


    public static List<PlaylistItem>? GetPlaylistFromCSV(string path, string trackNameField, string artistNameField)
    {
        var playlist = new List<PlaylistItem>();

        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                try
                {
                    string trackName = csv.GetField<string>(trackNameField)!;
                    string artistName = csv.GetField<string>(artistNameField)!;


                    playlist.Add(new PlaylistItem(trackName, artistName));

                }
                catch (CsvHelper.MissingFieldException)
                {
                    AnsiConsole.WriteLine($"❌ Fields: \"{trackNameField}\" or \"{artistNameField}\" do not exist!");
                    return null;
                } 
            }
        }

        return playlist;
    }

    private static async Task<string?> GetVideoIdFromQuery(string search)
    {
        using var youtube = new YoutubeClient();

        await foreach (var video in youtube.Search.GetVideosAsync(search))
        {
            return video.Id;
        }

        return null;

    }

    private static string? ExtractUrl(PlaylistItem item)
    {
        string query = $"{item.TrackName} {item.ArtistName}";

        string? id = GetVideoIdFromQuery(query).Result;

        if (id == null)
        {
            AnsiConsole.WriteLine($"⚠️ No results for {item.TrackName} {item.ArtistName}");
            return null;
        }

        string url = $"https://www.youtube.com/watch?v={id}";

        return url;
    }

    public static void GetUrlsFromPlaylist(ref List<PlaylistItem> playlist)
    {
        foreach (var item in playlist)
        {
            if (item == null)
            {
                continue;
            }
        }
    }

    public static async Task<(List<string?>, List<PlaylistItem> failedList)> GetUrlsFromPlaylistParallel(List<PlaylistItem> playlist)
    {
        int expectedUrlsLength = playlist.Count;
        List<string?> urls = [];
        List<PlaylistItem> failedList = [];

        await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("[green]Fetching urls[/]", maxValue: expectedUrlsLength);


            await Parallel.ForEachAsync(
               playlist,
               new ParallelOptions { MaxDegreeOfParallelism = 10 },
               async (item, ct) =>
               {
                   string? url = null;
                   try
                   {
                       url = ExtractUrl(item);
                   }
                   catch (Exception)
                   {
                       AnsiConsole.WriteLine($"❌ Error on {item.ArtistName} {item.TrackName}");
                       failedList.Add(item);
                   }

                   urls.Add(url);
                   task.Value = urls.Count;

               });


            task.Value = expectedUrlsLength;
        });

        return (urls, failedList);
    }
}