using FanucLsp.Lsp.State;
using FanucLSP.Util;
using KarelParser;
using ParserUtils;

namespace FanucLsp.Lsp.Util;

internal static class KarelProgramUtils
{
    public static string GetTokenAt(string content, ContentPosition position)
    {
        var lines = content.Split('\n');
        if (position.Line < 0 || position.Line >= lines.Length)
        {
            return string.Empty;
        }

        var line = lines[position.Line];
        if (position.Character < 0 || position.Character >= line.Length)
        {
            return string.Empty;
        }

        // Find the start of the identifier
        var start = position.Character;
        while (start > 0 && IsIdentifierChar(line[start - 1]))
        {
            start--;
        }

        // Find the end of the identifier
        var end = position.Character;
        while (end < line.Length && IsIdentifierChar(line[end]))
        {
            end++;
        }

        // Extract the identifier
        if (start < end && IsIdentifierStart(line[start]))
        {
            return line.Substring(start, end - start);
        }

        return string.Empty;
    }

    private static bool IsIdentifierStart(char c) => char.IsLetter(c) || c == '_';

    private static bool IsIdentifierChar(char c) => char.IsLetterOrDigit(c) || c == '_';
}

internal static class KarelProgramExtensions
{
    public static TextDocumentLocation? GetDeclarationPosition(
        this KarelProgram program,
        ContentPosition position
    )
    {
        /*
         * TODO:
         * - traverse program to find scope of current position
         * - if within a declaration, search type declaration if position is on type
         * - if within a function body, search its declaration as well as top level decls
         * - if within the main body, only search top level decls
         * - need to be able to search for variables, routines, struct fields
         */
        var node = program.GetObjectAtPosition(position);
        return null;
    }

    private static object? GetObjectAtPosition(this KarelProgram program, ContentPosition position)
    {
        foreach (var decl in program.Declarations)
        {
            if (decl.Start.Line <= position.Line && decl.End.Line >= position.Line)
            {
                return GetObjectInDeclaration(decl, position);
            }
        }
        foreach (var routine in program.Routines)
        {
            if (routine.Start.Line <= position.Line && routine.End.Line >= position.Line)
            {
                return GetObjectInRoutine(routine, position);
            }
        }
        foreach (var stmt in program.Statements)
        {
            if (stmt.Start.Line <= position.Line && stmt.End.Line >= position.Line)
            {
                return GetObjectInStatement(stmt, position);
            }
        }
        return null;
    }

    private static object? GetObjectInDeclaration(
        KarelDeclaration decl,
        ContentPosition position
    ) =>
        decl switch
        {
            KarelTypeDeclaration typeDecl => GetObjectInTypeDecl(typeDecl, position),
            KarelVariableDeclaration varDecl => GetObjectInVarDecl(varDecl, position),
            KarelConstantDeclaration constDecl => GetObjectInConstDecl(constDecl, position),
            _ => null,
        };

    private static object? GetObjectInTypeDecl(
        KarelTypeDeclaration typeDecl,
        ContentPosition position
    )
    {
        return null;
    }

    private static object? GetObjectInVarDecl(
        KarelVariableDeclaration varDecl,
        ContentPosition position
    )
    {
        return null;
    }

    private static object? GetObjectInConstDecl(
        KarelConstantDeclaration constDecl,
        ContentPosition position
    )
    {
        return null;
    }

    private static object? GetObjectInRoutine(KarelRoutine decl, ContentPosition position)
    {
        return null;
    }

    private static object? GetObjectInStatement(KarelStatement decl, ContentPosition position)
    {
        return null;
    }
}
