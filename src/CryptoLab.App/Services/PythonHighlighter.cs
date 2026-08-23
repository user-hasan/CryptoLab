using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CryptoLab.App.Services;

internal static class PythonHighlighter
{
    private static readonly Brush DefaultBrush = Brush("#D4D4D4");
    private static readonly Brush KeywordBrush = Brush("#569CD6");
    private static readonly Brush StringBrush = Brush("#CE9178");
    private static readonly Brush CommentBrush = Brush("#6A9955");
    private static readonly Brush NumberBrush = Brush("#B5CEA8");
    private static readonly Brush FunctionBrush = Brush("#DCDCAA");
    private static readonly Brush BuiltinBrush = Brush("#4EC9B0");

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "def", "return", "if", "elif", "else", "for", "while", "in", "not", "and", "or",
        "is", "import", "from", "as", "with", "try", "except", "raise", "lambda",
        "global", "nonlocal", "pass", "break", "continue", "assert", "class", "del",
        "yield", "async", "await"
    };

    private static readonly HashSet<string> Constants = new(StringComparer.Ordinal)
    {
        "True", "False", "None"
    };

    private static readonly HashSet<string> Builtins = new(StringComparer.Ordinal)
    {
        "print", "len", "range", "zip", "int", "bytes", "bytearray", "str", "list",
        "format", "enumerate", "abs", "all", "ord", "chr", "round", "min", "max",
        "sum", "hex", "repr"
    };

    private static readonly Regex NumberRegex = new(@"0[xX][0-9a-fA-F]+|\d+\.?\d*(?:[eE][+-]?\d+)?", RegexOptions.Compiled);

    public static RichTextBox Build(string code)
    {
        var paragraph = new Paragraph
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12.5,
            Foreground = DefaultBrush,
            Margin = new Thickness(0)
        };
        Fill(paragraph, code);
        return new RichTextBox
        {
            Document = new FlowDocument(paragraph)
            {
                PageWidth = 100000,
                PagePadding = new Thickness(6)
            },
            IsReadOnly = true,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12.5,
            Foreground = DefaultBrush,
            Background = Brush("#0c1522"),
            MinHeight = 260,
            MaxHeight = 420,
            FlowDirection = FlowDirection.LeftToRight,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
    }

    private static void Fill(Paragraph paragraph, string code)
    {
        var lines = code.Split('\n');
        bool inDocstring = false;
        string docDelimiter = "\"\"\"";
        string previousWord = string.Empty;

        for (int li = 0; li < lines.Length; li++)
        {
            string line = lines[li].TrimEnd('\r');
            int i = 0;

            if (inDocstring)
            {
                int close = line.IndexOf(docDelimiter, StringComparison.Ordinal);
                if (close < 0)
                {
                    paragraph.Inlines.Add(Run(line, StringBrush));
                    i = line.Length;
                }
                else
                {
                    paragraph.Inlines.Add(Run(line.Substring(0, close + 3), StringBrush));
                    i = close + 3;
                    inDocstring = false;
                }
            }

            while (i < line.Length)
            {
                char c = line[i];
                if (c == ' ' || c == '\t')
                {
                    int j = i;
                    while (j < line.Length && (line[j] == ' ' || line[j] == '\t')) j++;
                    paragraph.Inlines.Add(Run(line.Substring(i, j - i), DefaultBrush));
                    i = j;
                    continue;
                }
                if (c == '#')
                {
                    paragraph.Inlines.Add(Run(line.Substring(i), CommentBrush));
                    i = line.Length;
                    continue;
                }
                if (c == '"' || c == '\'')
                {
                    string delimiter = c == '"' ? "\"\"\"" : "'''";
                    if (line.Substring(i).StartsWith(delimiter, StringComparison.Ordinal))
                    {
                        int close = line.IndexOf(delimiter, i + 3, StringComparison.Ordinal);
                        if (close >= 0)
                        {
                            paragraph.Inlines.Add(Run(line.Substring(i, close + 3 - i), StringBrush));
                            i = close + 3;
                        }
                        else
                        {
                            paragraph.Inlines.Add(Run(line.Substring(i), StringBrush));
                            i = line.Length;
                            inDocstring = true;
                            docDelimiter = delimiter;
                        }
                        continue;
                    }
                    int end = i + 1;
                    while (end < line.Length)
                    {
                        if (line[end] == '\\') { end += 2; continue; }
                        if (line[end] == c) { end++; break; }
                        end++;
                    }
                    if (end > line.Length) end = line.Length;
                    paragraph.Inlines.Add(Run(line.Substring(i, end - i), StringBrush));
                    i = end;
                    continue;
                }
                if (char.IsDigit(c) || (c == '.' && i + 1 < line.Length && char.IsDigit(line[i + 1])))
                {
                    var num = NumberRegex.Match(line, i);
                    if (num.Success && num.Index == i)
                    {
                        paragraph.Inlines.Add(Run(num.Value, NumberBrush));
                        i += num.Length;
                        continue;
                    }
                }
                if (c == '_' || char.IsLetter(c))
                {
                    int j = i;
                    while (j < line.Length && (char.IsLetterOrDigit(line[j]) || line[j] == '_')) j++;
                    string word = line.Substring(i, j - i);
                    Brush brush = DefaultBrush;
                    if (Keywords.Contains(word) || Constants.Contains(word)) brush = KeywordBrush;
                    else if (Builtins.Contains(word)) brush = BuiltinBrush;
                    else if (previousWord == "def" || previousWord == "class") brush = FunctionBrush;
                    paragraph.Inlines.Add(Run(word, brush));
                    previousWord = word;
                    i = j;
                    continue;
                }
                paragraph.Inlines.Add(Run(line.Substring(i, 1), DefaultBrush));
                i++;
            }

            if (li < lines.Length - 1) paragraph.Inlines.Add(new LineBreak());
        }
    }

    private static Run Run(string text, Brush brush) => new(text) { Foreground = brush };

    private static Brush Brush(string hex) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
}