using Spectre.Console;

class Runner
{
    private static void CleanupUrls(List<string?> fullUrls)
    {
        fullUrls.RemoveAll(url => url == null);
        fullUrls.ForEach(url => AnsiConsole.Write(new Text($"{url}\n", Color.Blue)));
    }

    public static async Task RunWithoutConfigAsync()
    {
        AnsiConsole.Write(new Rule());

        string csvFilePath = AnsiConsole.Prompt(
            new TextPrompt<string>("CSV file path: ")
                .Validate(path =>
                {
                    if (File.Exists(path))
                    {
                        if (!Path.GetExtension(path).ToLower().Equals(".csv"))
                        {
                            return ValidationResult.Error("File is not a CSV file!");
                        }

                        return ValidationResult.Success();
                    }
                    else if (!File.Exists(path))
                    {
                        return ValidationResult.Error("File doesn't exist!");
                    }
                    else
                    {
                        return ValidationResult.Error();
                    }
                })
        );

        bool convertToMp3 = AnsiConsole.Prompt(
            new TextPrompt<bool>("Convert audio to mp3? ")
                .AddChoice(true)
                .AddChoice(false)
                .DefaultValue(true)
                .WithConverter(choice => choice ? "y" : "n"));


        string outputFolder = AnsiConsole.Prompt(
            new TextPrompt<string>("Output folder: ")
                .DefaultValue("downloads")
        );

        AnsiConsole.Write(new Rule("CSV fields"));

        string trackField = AnsiConsole.Prompt(
            new TextPrompt<string>("Track name field: ")
                .DefaultValue("Track Name")
        );

        string authorField = AnsiConsole.Prompt(
            new TextPrompt<string>("Author name field:")
                .DefaultValue("Artist Name(s)")
        );

        AnsiConsole.Write(new Rule());

        await RunWithConfigAsync(new Config(csvFilePath, convertToMp3, outputFolder, trackField, authorField, null));
    }

    public static async Task RunWithConfigAsync(Config config)
    {
        var playlist = Extractor.GetPlaylistFromCSV(config.CsvFilePath, config.TrackField, config.AuthorField);
        if (playlist == null) return;

        var fullUrls = new List<string?>();

        AnsiConsole.Write(new Rule("Fetched urls"));
        await Extractor.ExtractUrlsAsync(playlist, fullUrls);
        CleanupUrls(fullUrls);
        AnsiConsole.Write(new Rule());

        if (!Directory.Exists(config.OutputFolderPath)) Directory.CreateDirectory(config.OutputFolderPath);

        await Downloader.DownloadAsyncParallel(fullUrls!, config.OutputFolderPath);

        if (config.ConvertToMp3)
        {
            AnsiConsole.Write(new Rule());
            await Downloader.ConvertToMp3ParallelAsync(Directory.GetFiles(config.OutputFolderPath));
        }
    }


    //TODO: FINISH SINGLE_DOWNLOAD FUNCTIONS
    public static async Task RunDownloadFromUrl(Config config)
    {
        // await Downloader.DownloadAsync()
    }
}