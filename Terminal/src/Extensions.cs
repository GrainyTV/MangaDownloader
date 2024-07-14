using HtmlAgilityPack;
using Optional;
using Optional.Collections;
using Optional.Unsafe;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtensionMethods;

public static class Extensions
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

    public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
    {
        foreach (T item in sequence)
        {
            action.Invoke(item);
        }
    }

    public static bool IsNonEmpty(this string anyString)
    {
        return anyString.Trim() != String.Empty;
    }

    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }

    public static Option<string> GetClassesOfFirstParentDiv(this HtmlNode node)
    {
        return node
            .Ancestors()
            .Where(node => node.OriginalName.Equals("div") && node.GetClasses().Any())
            .FirstOrNone()
            .Map(node => node.GetClasses().Flatten().Trim());
    }
}