using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp;

public class LspMethods
{
    // Requests
    public const string Initialize = "initialize";
    public const string Shutdown = "shutdown";

    public const string TextDocumentDidHover = "textDocument/hover";
    public const string TextDocumentDefinition = "textDocument/definition";
    public const string TextDocumentCodeAction = "textDocument/codeAction";
    public const string TextDocumentCompletion = "textDocument/completion";

    // Notifications
    public const string Initialized = "initialized";

    public const string TextDocumentDidOpen = "textDocument/didOpen";
    public const string TextDocumentDidClose = "textDocument/didClose";
    public const string TextDocumentDidChange = "textDocument/didChange";
    public const string TextDocumentDidSave = "textDocument/didSave";
    public const string TextDocumentPublishDiagnostics = "textDocument/publishDiagnostics";
}

public class LspUtils
{
    private const string HeaderCommentDelimiter = "********************************";

    // This is actually already pretty good at extracting arguments
    public static string ExtractDocComment(TpProgram? program)
        => program switch
        {
            not null => program.Main.Instructions
                 .TakeWhile(instr => instr is TpInstructionComment)
                 .Select(instr => (instr as TpInstructionComment)!.Comment)
                 .ToList() switch
            {
                { Count: > 4 } headerComment =>
                    headerComment.RemoveAll(cmt => cmt.StartsWith(HeaderCommentDelimiter)) switch
                    {
                        4 => headerComment.Aggregate((acc, cmt) => acc + "\n" + cmt).Replace("[", "\\[").Replace("]", "\\]"),
                        _ => string.Empty
                    },
                _ => string.Empty
            },
            _ => string.Empty
        };
}
