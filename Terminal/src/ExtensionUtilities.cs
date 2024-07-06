using Optional;
using Optional.Unsafe;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;

namespace ExtensionMethods;

public static class ExtensionUtilities
{
    public static string Flatten(this IEnumerable<string> elements)
    {
        return string.Join(' ', elements);
    }

    public static T Unwrap<T>(this Option<T> maybe)
    {
        return maybe.ValueOrFailure();
    }

    public static void AddPages(this PdfDocument document, Int32 amount)
    {
        if (amount == 0)
            return;

        document.AddPage();
        document.AddPages(amount - 1);
    }

    public static void ForEach<T>(this IEnumerable<T> sequence, Action<T, int> action)
    {
        int i = 0;
        
        foreach (T item in sequence)
        {
            action.Invoke(item, i);
            ++i;
        }
    }
}