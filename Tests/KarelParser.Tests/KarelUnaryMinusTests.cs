using Sprache;

namespace KarelParser.Tests;

public class KarelUnaryMinusTests
{
    [Theory]
    [InlineData("-axis_pos")]
    [InlineData("-axis_pos * G2sign")]
    [InlineData("-CNV_JPOS(x)")]
    [InlineData("5 - -axis_pos")]
    [InlineData("a - b")]
    [InlineData("-5 * G2sign")]
    public void Expression_Parses(string src)
    {
        var r = KarelExpression.GetParser().End().TryParse(src);
        Assert.True(r.WasSuccessful, $"{src} => {r.Message}");
    }

    [Fact]
    public void NegatedVariable_ProducesUnaryMinusNode()
        => Assert.IsType<KarelUnaryMinus>(
            KarelExpression.GetParser().End().Parse("-axis_pos"));

    [Fact]
    public void NegatedIntegerLiteral_StaysInteger()
        => Assert.IsType<KarelInteger>(
            KarelExpression.GetParser().End().Parse("-5"));

    // Regression: the whitespace-skipping minus operator must not reach across a
    // line break to swallow the leading '-' of a following '--' comment and parse
    // the comment body as "value - (-comment)".
    [Fact]
    public void LineComment_AfterStatement_IsNotConsumedAsMinus()
    {
        var program = "PROGRAM test\nBEGIN\n\tGr_no = 1\n\t--PR num to set\n\tGr_no = 2\nEND test\n";
        var r = KarelProgram.ProcessAndParse(program);
        Assert.True(r.WasSuccessful, $"{r.Message} @ {r.Remainder.Line}:{r.Remainder.Column}");
    }

    // Regression: 'x--y' is 'x' followed by a comment, never 'x - (-y)'.
    [Fact]
    public void DoubleDashWithoutSpace_IsComment()
    {
        var program = "PROGRAM test\nBEGIN\n\tx = 1--y\n\tx = 2\nEND test\n";
        var r = KarelProgram.ProcessAndParse(program);
        Assert.True(r.WasSuccessful, $"{r.Message} @ {r.Remainder.Line}:{r.Remainder.Column}");
    }
}