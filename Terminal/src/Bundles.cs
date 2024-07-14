using System.Collections.Generic;

public class ImageUrlBundle
{
    public RequestInfo Metadata { get; init; } = null!;
    public IEnumerable<string> Images { get; init; } = null!;
}

public class PathBundle
{
    public RequestInfo Metadata { get; init; } = null!;
    public List<string> Paths { get; init; } = null!;
}