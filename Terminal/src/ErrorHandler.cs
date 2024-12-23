using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public enum ErrorValue
{
    NetworkFailure,
    NoImageResourceFound,
    FailedToDownloadImage,
}

public static class ErrorMessages
{
    public static IReadOnlyDictionary<ErrorValue, (string Cause, string Suggestion)> Information =
                       new Dictionary<ErrorValue, (string Cause, string Suggestion)>
    {
        { ErrorValue.NetworkFailure, ("Network failure detected.", "Check your network connection and ensure the requested resource is available.") },
        { ErrorValue.NoImageResourceFound, ("The provided website does not contain any images.", "Check the website URL or try a different site.") },
        { ErrorValue.FailedToDownloadImage, ("The application was unable to download an image.", "Ensure the image is still available at the specified URL.") },
    };

    public static bool IsProperlyFilled { get => Enum.GetNames(typeof(ErrorValue)).Length == Information.Count; }

    public static void LogFailedProcess(RequestInfo requestInfo, ErrorValue errorValue)
    {
        CancellationTokenSource source = requestInfo.CancellationSource;
        source.Cancel();
        
        var errorInfo = ErrorMessages.Information[errorValue];
        using (var logFile = new StreamWriter(path: $"logfile-{requestInfo.Time}.log", append: true))
        {
            logFile.WriteLine("[ERROR]");
            logFile.WriteLine($"Cause: {errorInfo.Cause}");
            logFile.WriteLine($"Suggestion: {errorInfo.Suggestion}");
            logFile.WriteLine(requestInfo.ToString());
            logFile.WriteLine();
        }

        source.Token.ThrowIfCancellationRequested();
    }
}