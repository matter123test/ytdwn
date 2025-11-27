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
        Config? config = parser.GetConfig(args);

        if (config != null)
        {
            config.PrintOut();
            Runner.RunWithConfigAsync(config).GetAwaiter().GetResult();
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