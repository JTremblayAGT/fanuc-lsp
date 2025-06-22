namespace FanucTpLsp.Lsp.Format;

public interface IFormatter
{
    public string Format(string content, FormattingOptions options);
}
