using Spectre.Console;


class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            TitleScreen(clear: true);
            Runner.RunWithoutConfigAsync().GetAwaiter().GetResult();
            return;
        }

        TitleScreen();

        ArgParse parser = new();
        (ArgParseResult result, Config? config) = parser.GetConfig(args);

        if (result == ArgParseResult.ERROR) return;
        else if (result == ArgParseResult.PLAYLIST_DOWNLOAD)
        {
            config!.PrintOut();
            Runner.RunWithConfigAsync(config).GetAwaiter().GetResult();
        }
        else if (result == ArgParseResult.SINGLE_DOWNLOAD)
        {
            config!.PrintOut();
            Runner.RunDownloadFromUrl(config).GetAwaiter().GetResult();
        }
    }

    static void TitleScreen(bool clear = false)
    {
        if (clear) AnsiConsole.Clear();

        AnsiConsole.Write(
            new FigletText("YTDWN")
                .Centered()
                .Color(Color.Yellow));
    }

}