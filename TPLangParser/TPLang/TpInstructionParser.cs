using Sprache;

namespace TPLangParser.TPLang;

internal interface ITpParser<out TInstructionType>
{
    public static abstract Parser<TInstructionType> GetParser();
}
