using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp.Definition;

internal interface IDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(TpProgram program, ContentRange range, TextDocumentItem document);
}
