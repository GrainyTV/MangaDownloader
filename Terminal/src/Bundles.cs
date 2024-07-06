using Optional;
using System.Collections.Generic;

public class ImageUrlBundle
{
    public RequestInfo Metadata { get; init; } = null!;
    public Option<IEnumerable<string>> Images { get; init; }
}

public class PathBundle
{
    public RequestInfo Metadata { get; init; } = null!;
    public List<string> Paths { get; init; } = null!;
}