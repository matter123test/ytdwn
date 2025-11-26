using System.CommandLine;

class ArgParse()
{

    private readonly RootCommand rootCommand = new("YTDWN searchs videos on youtube and downloads it.");

    private readonly Option<string> csvFilePathOption = new("--path", "-p")
    {
        Description = "Input CSV file path"
    };

    private readonly Option<bool> convertToMp3Option = new("--convert", "-c")
    {
        Description = "Confirm audio file conversion to mp3"
    };

    private readonly Option<string> outputFolderPathOption = new("--output", "-o")
    {
        Description = "Download location"
    };

    private readonly Option<string> trackFieldOption = new("--trackfield", "-at")
    {
        Description = "Track name field in the csv file"
    };

    private readonly Option<string> authorFieldOption = new("--authorfield", "-af")
    {
        Description = "Author name field in the csv file"
    };

    public Config? GetConfigFromArgs(string[] args)
    {
        List<Option> options = [csvFilePathOption, convertToMp3Option, outputFolderPathOption, trackFieldOption, authorFieldOption];
        options.ForEach(rootCommand.Options.Add);

        string? csvFilePath = null;
        bool convertToMp3 = false;
        string? outputFolderPath = null;
        string? trackField = null;
        string? authorField = null;

        bool shouldReturnNull = false;

        rootCommand.Validators.Add(result =>
        {
            csvFilePath = result.GetValue(csvFilePathOption);
            outputFolderPath = result.GetValue(outputFolderPathOption);

            if (csvFilePath != null && outputFolderPath == null)
            {
                result.AddError("--output is required when --path is provided");
                shouldReturnNull = true;
            }

            trackField = result.GetValue(trackFieldOption);
            authorField = result.GetValue(authorFieldOption);

            if (csvFilePath != null && trackField == null || csvFilePath != null && authorField == null)
            {
                result.AddError("--trackfield and --authorfield is required when --path is provided");
                shouldReturnNull = true;
            }
        });

        rootCommand.Parse(args).Invoke();

        if (shouldReturnNull) return null;

        rootCommand.SetAction(parseResult =>
        {
            csvFilePath = parseResult.GetValue(this.csvFilePathOption);
            convertToMp3 = parseResult.GetValue(convertToMp3Option);
            outputFolderPath = parseResult.GetValue(outputFolderPathOption);
            trackField = parseResult.GetValue(trackFieldOption);
            authorField = parseResult.GetValue(authorFieldOption);
        });

        rootCommand.Parse(args).Invoke();

        return new Config(csvFilePath!, convertToMp3, outputFolderPath!, trackField!, authorField!);
    }
}