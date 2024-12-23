using System.Threading;

public record RequestInfo
{
    public required string Time { get; init; }
    public required string UniqueId { get; init; }
    public required CancellationTokenSource CancellationSource { get; init; }
    public required string Url { get; init; }
    public required string Title { get; init; }

    public override string ToString()
    {
        return $"RequestInfo:\n  => Title: {Title}\n  => Chapter: {UniqueId}\n  => Url: {Url}";
    }
}