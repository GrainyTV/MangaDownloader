using Colors;
using System;

namespace HelperMethods;
static class Helpers
{
    public static void WriteLineColor(Color color, params string[] args)
    {
        Console.WriteLine($"\x1B[38:5:{(Byte)color}m{string.Join('\n', args)}\x1B[m");
    }
}