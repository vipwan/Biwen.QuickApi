// <copyright file="EmitterBase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Gen.Shared;

using System.Collections.Generic;
using System.Text;

#if !SHARED_PROJECT
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
internal class EmitterBase
{
    private const int DefaultStringBuilderCapacity = 1024;
    private const int IndentChars = 4;

    private readonly StringBuilder sb = new(DefaultStringBuilderCapacity);
    private readonly string[] padding = new string[16];
    private int indent;

    public EmitterBase(bool emitPreamble = true)
    {
        var padding = this.padding;
        for (int i = 0; i < padding.Length; i++)
        {
            padding[i] = new string(' ', i * IndentChars);
        }

        if (emitPreamble)
        {
            Out(GeneratorUtilities.FilePreamble);
        }
    }

    protected void OutOpenBrace()
    {
        OutLn("{");
        Indent();
    }

    protected void OutCloseBrace()
    {
        Unindent();
        OutLn("}");
    }

    protected void OutCloseBraceWithExtra(string extra)
    {
        Unindent();
        OutIndent();
        Out("}");
        Out(extra);
        OutLn();
    }

    protected void OutIndent()
    {
        _ = sb.Append(padding[indent]);
    }

    protected string GetPaddingString(byte indent)
    {
        return padding[indent];
    }

    protected void OutLn()
    {
        _ = sb.AppendLine();
    }

    protected void OutLn(string line)
    {
        OutIndent();
        _ = sb.AppendLine(line);
    }

    protected void OutPP(string line)
    {
        _ = sb.AppendLine(line);
    }

    protected void OutEnumeration(IEnumerable<string> e)
    {
        bool first = true;
        foreach (var item in e)
        {
            if (!first)
            {
                Out(", ");
            }

            Out(item);
            first = false;
        }
    }

    protected void Out(string text) => _ = sb.Append(text);

    protected void Out(char ch) => _ = sb.Append(ch);

    protected void Indent() => indent++;

    protected void Unindent() => indent--;

    protected void OutGeneratedCodeAttribute() => OutLn($"[{GeneratorUtilities.GeneratedCodeAttribute}]");

    protected string Capture() => sb.ToString();
}
