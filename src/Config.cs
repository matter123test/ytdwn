using Spectre.Console;

class Config(string? csvFilePath, bool convertToMp3, string? outputFolderPath, string? trackField, string? authorField, string? urlToDownload)
{
    public readonly string? CsvFilePath = csvFilePath;
    public readonly bool ConvertToMp3 = convertToMp3;
    public readonly string? OutputFolderPath = outputFolderPath;
    public readonly string? TrackField = trackField;
    public readonly string? AuthorField = authorField;
    public readonly string? UrlToDownload = urlToDownload;

    public void PrintOut()
    {
        AnsiConsole.Write(new Rule("Config"));

        AnsiConsole.WriteLine($"CSV file path: {CsvFilePath}");
        AnsiConsole.WriteLine($"Convert to mp3: {ConvertToMp3}");
        AnsiConsole.WriteLine($"Download folder: {OutputFolderPath}");
        AnsiConsole.WriteLine($"Track name field: {TrackField}");
        AnsiConsole.WriteLine($"Author name field: {AuthorField}");

        AnsiConsole.WriteLine($"Url to download: {UrlToDownload}");

        AnsiConsole.Write(new Rule());
    }
}