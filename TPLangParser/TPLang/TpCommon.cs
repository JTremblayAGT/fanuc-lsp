using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.TPLang;

public enum TpUnit
{
    Mm,
    Deg
}

public struct TpCommon
{
    public static readonly Parser<int> LineNumber =
        from lineNumber in Parse.Number.Select(int.Parse).Token()
        from colon in Parse.Char(':')
        select lineNumber;

    public static readonly Parser<IEnumerable<char>> LineEnd =
        Parse.WhiteSpace.Many().Then(_ => Parse.Char(';').Then(_ => Parse.AnyChar.Until(Parse.LineTerminator)));

    public static readonly Parser<string> Identifier =
        Parse.Identifier(Parse.LetterOrDigit, Parse.LetterOrDigit.Or(Parse.Char('_'))).Token();

    public static readonly Parser<string> ProgramName = Identifier;

    public static readonly Parser<TpUnit> Unit =
        Keyword("mm").Return(TpUnit.Mm)
            .Or(Keyword("deg").Return(TpUnit.Deg));

    public static Parser<string> Keyword(string keyword)
        => Parse.String(keyword).Text().Token();

    public static Parser<TParsed> Fail<TParsed>(string message)
        => input => Result.Failure<TParsed>(input, message, []);
}

public static class TpParserExtensions
{
    public static Parser<TParsedType> BetweenParen<TParsedType>(this Parser<TParsedType> parser)
        => parser.Contained(TpCommon.Keyword("("), TpCommon.Keyword(")"));

    public static Parser<TParsedType> BetweenBrackets<TParsedType>(this Parser<TParsedType> parser)
        => parser.Contained(TpCommon.Keyword("["), TpCommon.Keyword("]"));

    public static Parser<TParsedType> BetweenBraces<TParsedType>(this Parser<TParsedType> parser)
        => parser.Contained(TpCommon.Keyword("{"), TpCommon.Keyword("}"));

}

public enum TpArcWeldingOptionType
{
    Start,
    End
}

internal struct TpArcWeldingOptionTypeParser
{
    public static readonly Parser<TpArcWeldingOptionType> Parser =
        TpCommon.Keyword("Start").Return(TpArcWeldingOptionType.Start)
            .Or(TpCommon.Keyword("End").Return(TpArcWeldingOptionType.End));
}

public abstract record TpAccess : ITpParser<TpAccess>
{
    protected static readonly Parser<string> CommentParser =
        from leading in TpCommon.Keyword(":")
        from comment in InnerComment
        select comment;

    protected static readonly Parser<string> InnerComment =
        Parse.CharExcept("\n\r").Until(CommentParserWithLookahead).Text();

    protected static Parser<char> CommentParserWithLookahead
        => input =>
        {
            var result = Parse.Char(']').Preview()(input);
            if (!result.Value.IsDefined)
            {
                return Result.Failure<char>(input, string.Empty, []);
            }

            var next = result.Remainder.Advance();

            // Try parsing anything that can come after a register comment
            var lookAhead = TpValue.GetParser().Return(0)
                .Or(TpCommon.LineEnd.Return(0))
                .Or(TpMotionSpeed.SpeedUnitParser.Return(0))
                .Or(TpMotionSpeed.GetParser().Return(0))
                .Or(TpMotionOption.GetParser().Return(0))
                .Or(TpArithmeticOperatorParser.Parser.Return(0))
                .Or(TpComparisonOperatorParser.Parser.Return(0))
                .Or(TpLogicalOperatorParser.Parser.Return(0))
                .Or(TpCommon.Keyword(")")
                .Or(TpCommon.Keyword("("))
                .Or(TpCommon.Keyword(","))
                .Or(TpCommon.Keyword("TO"))
                .Or(TpCommon.Keyword("DOWNTO")).Return(0))
                .Token().Preview()(next);

            return lookAhead.Value.IsDefined || next.AtEnd ? Result.Success(']', input) : Result.Failure<char>(input, string.Empty, []);
        };

    public static Parser<TpAccess> GetParser()
        => TpAccessDirect.GetParser()
            .Or(TpAccessIndirect.GetParser())
            .Or(TpAccessMultiple.GetParser());
}

public sealed record TpAccessDirect(int Number, string? Comment, int? Group)
    : TpAccess, ITpParser<TpAccess>
{
    public new static Parser<TpAccess> GetParser()
        => (from grp in
                    (from kw in TpCommon.Keyword("GP")
                     from num in Parse.Number.Select(int.Parse)
                     from sep in TpCommon.Keyword(":")
                     select num).Optional()
            from registerNumber in Parse.Number.Select(int.Parse)
            from comment in CommentParser.Optional()
            select new TpAccessDirect(registerNumber, comment.GetOrDefault(), grp.GetOrDefault())
            ).BetweenBrackets();
}

public sealed record TpAccessIndirect(TpRegister Register, int? Group)
    : TpAccess, ITpParser<TpAccess>
{
    public new static Parser<TpAccess> GetParser()
        => (from grp in
                    (from kw in TpCommon.Keyword("GP")
                     from num in Parse.Number.Select(int.Parse)
                     from sep in TpCommon.Keyword(":")
                     select num).Optional()
            from register in TpRegister.GetParser().Or(TpArgumentRegister.GetParser())
            select new TpAccessIndirect(register, grp.GetOrDefault())
            ).BetweenBrackets();
}

public sealed record TpAccessMultiple(TpValue Number, TpValue Item, string? Comment, int? Group)
    : TpAccess, ITpParser<TpAccess>
{
    public new static Parser<TpAccess> GetParser()
        => (from grp in
                    (from kw in TpCommon.Keyword("GP")
                     from num in Parse.Number.Select(int.Parse)
                     from sep in TpCommon.Keyword(":")
                     select num).Optional()
            from num in TpValueIntegerConstant.GetParser().Or(TpValueRegister.GetParser())
            from sep in TpCommon.Keyword(",")
            from item in TpValueIntegerConstant.GetParser().Or(TpValueRegister.GetParser())
            from comment in CommentParser.Optional()
            select new TpAccessMultiple(num, item, comment.GetOrDefault(), grp.GetOrDefault())
            ).BetweenBrackets();
}

public abstract record TpGenericRegister(TpAccess Access) : ITpParser<TpGenericRegister>
{
    public static Parser<TpGenericRegister> GetParser()
        => TpRegister.GetParser().Select(TpGenericRegister (reg) => reg)
        .Or(TpPositionRegister.GetParser().Select(TpGenericRegister (reg) => reg))
        .Or(TpArgumentRegister.GetParser().Select(TpGenericRegister (reg) => reg))
        .Or(TpStringRegister.GetParser().Select(TpGenericRegister (reg) => reg));
}

// This type is not included in the generic register parser as it is only valid
// in motion instructions
public record TpPosition(TpAccess Access)
    : TpGenericRegister(Access), ITpParser<TpPosition>
{
    private static readonly Parser<TpPosition> Parser =
        TpCommon.Keyword("P").Then(_ => TpAccess.GetParser())
            .Select(access => new TpPosition(access));

    public new static Parser<TpPosition> GetParser()
        => TpPositionRegister.GetParser().Or(Parser);
}

public record TpRegister(TpAccess Access)
    : TpGenericRegister(Access), ITpParser<TpRegister>
{
    public new static Parser<TpRegister> GetParser()
        => TpCommon.Keyword("R").Then(_ => TpAccess.GetParser())
            .Select(access => new TpRegister(access));
}

// A PR can also be used as a position for motion instruction
public sealed record TpPositionRegister(TpAccess Access)
    : TpPosition(Access), ITpParser<TpPositionRegister>
{
    public static readonly Parser<TpPositionRegister> Element
        = TpCommon.Keyword("PR").Then(_ => TpAccessMultiple.GetParser())
            .Select(access => new TpPositionRegister(access));

    public new static Parser<TpPositionRegister> GetParser()
        => TpCommon.Keyword("PR").Then(_ => TpAccess.GetParser())
            .Select(access => new TpPositionRegister(access));
}

public sealed record TpArgumentRegister(TpAccess Access)
    : TpRegister(Access), ITpParser<TpArgumentRegister>
{
    public new static Parser<TpArgumentRegister> GetParser()
        => TpCommon.Keyword("AR").Then(_ => TpAccess.GetParser())
            .Select(access => new TpArgumentRegister(access));
}

public sealed record TpStringRegister(TpAccess Access)
    : TpGenericRegister(Access), ITpParser<TpStringRegister>
{
    public new static Parser<TpStringRegister> GetParser()
        => TpCommon.Keyword("SR").Then(_ => TpAccess.GetParser())
            .Select(access => new TpStringRegister(access));
}

public sealed record TpFlag(TpAccess Access) : ITpParser<TpFlag>
{
    public static Parser<TpFlag> GetParser()
        => TpCommon.Keyword("F").Then(_ => TpAccess.GetParser())
            .Select(access => new TpFlag(access));
}

public enum TpIOType
{
    Input,
    Output
}

public struct TpIOTypeParser
{
    public static readonly Parser<TpIOType> Parser =
        Parse.Char('I').Return(TpIOType.Input)
            .Or(Parse.Char('O').Return(TpIOType.Output));

    public static Parser<TpIOType> GetParserFor(TpIOType symbol)
        => symbol switch
        {
            TpIOType.Input => Parse.Char('I').Return(TpIOType.Input),
            TpIOType.Output => Parse.Char('O').Return(TpIOType.Output),
            _ => TpCommon.Fail<TpIOType>($"Invalid IO type [{symbol}]")
        };
}

public interface IIOPort
{
    public static abstract char Prefix();
}

public abstract record TpIOPort(TpIOType Type, TpAccess PortNumber) : ITpParser<TpIOPort>, IIOPort
{
    protected static Parser<TpIOPort> MakeParser(char symbol, Func<TpIOType, TpAccess, TpIOPort> builder)
        => from keyword in Parse.Char(symbol)
           from ioType in TpIOTypeParser.Parser
           from portNum in TpAccess.GetParser()
           select builder(ioType, portNum);

    public static Parser<TpIOPort> MakeParser(char symbol, TpIOType type, Func<TpAccess, TpIOPort> builder)
        => from keyword in Parse.Char(symbol)
           from ioType in TpIOTypeParser.GetParserFor(type)
           from portNum in TpAccess.GetParser()
           select builder(portNum);

    public static Parser<TpIOPort> GetParser()
        => TpOnOffIOPort.GetParser()
            .Or(TpNumericalIOPort.GetParser()).Token();

    public static char Prefix() => default;
}

public abstract record TpOnOffIOPort(TpIOType Type, TpAccess PortNumber) : TpIOPort(Type, PortNumber), ITpParser<TpIOPort>
{
    public new static Parser<TpIOPort> GetParser()
        => MakeParser(TpDigitalIOPort.Prefix(), (type, i) => new TpDigitalIOPort(type, i))
            .Or(MakeParser(TpRobotIOPort.Prefix(), (type, i) => new TpRobotIOPort(type, i)))
            .Or(MakeParser(TpUopIOPort.Prefix(), (type, i) => new TpUopIOPort(type, i)))
            .Or(MakeParser(TpSopIOPort.Prefix(), (type, i) => new TpSopIOPort(type, i)));
}

public abstract record TpNumericalIOPort(TpIOType Type, TpAccess PortNumber)
    : TpIOPort(Type, PortNumber), ITpParser<TpIOPort>
{
    public new static Parser<TpIOPort> GetParser()
        => MakeParser(TpAnalogIOPort.Prefix(), (type, i) => new TpAnalogIOPort(type, i))
        .Or(MakeParser(TpGroupIOPort.Prefix(), (type, i) => new TpGroupIOPort(type, i)))
        .Or(MakeParser(TpWeldingIOPort.Prefix(), (type, i) => new TpWeldingIOPort(type, i)))
        .Token();
}

public sealed record TpDigitalIOPort(TpIOType Type, TpAccess PortNumber) : TpOnOffIOPort(Type, PortNumber)
{
    public TpDigitalIOPort() : this(TpIOType.Input, null!)
    {
    }

    public new static char Prefix() => 'D';
}

public sealed record TpRobotIOPort(TpIOType Type, TpAccess PortNumber) : TpOnOffIOPort(Type, PortNumber)
{
    public TpRobotIOPort() : this(TpIOType.Input, null!)
    {
    }

    public new static char Prefix() => 'R';
}

public sealed record TpSopIOPort(TpIOType Type, TpAccess PortNumber) : TpOnOffIOPort(Type, PortNumber)
{
    public TpSopIOPort() : this(TpIOType.Input, null!)
    {
    }

    public new static char Prefix() => 'S';
}
public sealed record TpUopIOPort(TpIOType Type, TpAccess PortNumber) : TpOnOffIOPort(Type, PortNumber)
{
    public TpUopIOPort() : this(TpIOType.Input, null!)
    {
    }

    public new static char Prefix() => 'U';
}

public sealed record TpAnalogIOPort(TpIOType Type, TpAccess PortNumber) : TpNumericalIOPort(Type, PortNumber)
{
    public TpAnalogIOPort() : this(TpIOType.Input, null!)
    {
    }

    public new static char Prefix() => 'A';
}

public sealed record TpGroupIOPort(TpIOType Type, TpAccess PortNumber) : TpNumericalIOPort(Type, PortNumber)
{
    public TpGroupIOPort() : this(TpIOType.Input, null!)
    {
    }

    public new static char Prefix() => 'G';
}

public sealed record TpWeldingIOPort(TpIOType Type, TpAccess PortNumber) : TpNumericalIOPort(Type, PortNumber)
{
    public TpWeldingIOPort() : this(TpIOType.Input, null!)
    {
    }

    public new static char Prefix() => 'W';
}

public sealed record TpLabel(TpAccess LabelNumber) : ITpParser<TpLabel>
{
    public static Parser<TpLabel> GetParser()
        => from keyword in TpCommon.Keyword("LBL")
           from labelNumber in TpAccessDirect.GetParser().Or(TpAccessIndirect.GetParser())
           select new TpLabel(labelNumber);

}
public abstract record TpWeldInstructionArgs : ITpParser<TpWeldInstructionArgs>
{
    public static Parser<TpWeldInstructionArgs> GetParser()
        => TpWeldInstructionWeldSchedule.GetParser()
            .Or(TpWeldInstructionRegister.GetParser())
            .Or(TpWeldInstructionParameters.GetParser());

}

public sealed record TpWeldInstructionWeldSchedule(TpAccessDirect Access)
    : TpWeldInstructionArgs, ITpParser<TpWeldInstructionArgs>
{
    public new static Parser<TpWeldInstructionArgs> GetParser()
        => TpAccessDirect.GetParser()
            .Select(access => new TpWeldInstructionWeldSchedule((access as TpAccessDirect)!));
}

public record TpWeldInstructionRegister(TpRegister Register)
    : TpWeldInstructionArgs, ITpParser<TpWeldInstructionArgs>
{
    public new static Parser<TpWeldInstructionArgs> GetParser()
        => from register in TpArgumentRegister.GetParser().Or(TpRegister.GetParser()).BetweenBrackets()
           select new TpWeldInstructionRegister(register);
}

public record TpWeldInstructionParameters(List<string> Parameters)
    : TpWeldInstructionArgs, ITpParser<TpWeldInstructionArgs>
{
    public new static Parser<TpWeldInstructionArgs> GetParser()
        => from arguments in
               Parse.LetterOrDigit.Or(Parse.Char('.')).AtLeastOnce().Text()
                   .DelimitedBy(Parse.Char(',')).BetweenBrackets()
           select new TpWeldInstructionParameters(arguments.ToList());

}

public enum TpComparisonOperator
{
    Equal,
    NotEqual,
    LesserEqual,
    Lesser,
    GreaterEqual,
    Greater
}

public struct TpComparisonOperatorParser
{
    public static readonly Parser<TpComparisonOperator> Parser =
        TpCommon.Keyword("=").Return(TpComparisonOperator.Equal)
            .Or(TpCommon.Keyword("<>").Return(TpComparisonOperator.NotEqual))
            .Or(TpCommon.Keyword("<=").Return(TpComparisonOperator.LesserEqual))
            .Or(TpCommon.Keyword(">=").Return(TpComparisonOperator.GreaterEqual))
            .Or(TpCommon.Keyword("<").Return(TpComparisonOperator.Lesser))
            .Or(TpCommon.Keyword(">").Return(TpComparisonOperator.Greater));
}

public enum TpLogicalOperator
{
    And,
    Or,
}

public struct TpLogicalOperatorParser
{
    public static readonly Parser<TpLogicalOperator> Parser =
        TpCommon.Keyword("AND").Return(TpLogicalOperator.And)
            .Or(TpCommon.Keyword("OR").Return(TpLogicalOperator.Or));
}
public enum TpArithmeticOperator
{
    Plus,
    Minus,
    Times,
    Div,
    Mod,
    IntegerDiv
}

public struct TpArithmeticOperatorParser
{
    public static readonly Parser<TpArithmeticOperator> Plus
        = TpCommon.Keyword("+").Return(TpArithmeticOperator.Plus);

    public static readonly Parser<TpArithmeticOperator> Minus
        = TpCommon.Keyword("-").Return(TpArithmeticOperator.Minus);

    public static readonly Parser<TpArithmeticOperator> Times
        = TpCommon.Keyword("*").Return(TpArithmeticOperator.Times);

    public static readonly Parser<TpArithmeticOperator> Div
        = TpCommon.Keyword("/").Return(TpArithmeticOperator.Div);

    public static readonly Parser<TpArithmeticOperator> Mod
        = TpCommon.Keyword("MOD").Return(TpArithmeticOperator.Mod);

    public static readonly Parser<TpArithmeticOperator> IntDiv
        = TpCommon.Keyword("DIV").Return(TpArithmeticOperator.IntegerDiv);

    public static readonly Parser<TpArithmeticOperator> Parser =
        Plus
            .Or(Minus)
            .Or(Times)
            .Or(Div)
            .Or(Mod)
            .Or(IntDiv);

}

public record TpMathExpression : ITpParser<TpMathExpression>
{
    protected static readonly Parser<TpRegister> Argument
        = TpRegister.GetParser().Or(TpArgumentRegister.GetParser());

    protected static Parser<TpMathExpression> MakeParser(
        string keyword,
        Func<TpRegister, TpMathExpression> builder
    )
        => from kw in TpCommon.Keyword(keyword)
           from value in Argument.BetweenBrackets()
           select builder(value);

    public static Parser<TpMathExpression> GetParser()
        => TpSqrtExpression.GetParser()
            .Or(TpSinExpression.GetParser())
            .Or(TpCosExpression.GetParser())
            .Or(TpTanExpression.GetParser())
            .Or(TpAsinExpression.GetParser())
            .Or(TpAcosExpression.GetParser())
            .Or(TpAtanExpression.GetParser())
            .Or(TpAtan2Expression.GetParser())
            .Or(TpLnExpression.GetParser())
            .Or(TpExpExpression.GetParser())
            .Or(TpAbsExpression.GetParser())
            .Or(TpTruncExpression.GetParser())
            .Or(TpRoundExpression.GetParser());
}

public record TpSqrtExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("SQRT", reg => new TpSqrtExpression(reg));
}

public record TpSinExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("SIN", reg => new TpSinExpression(reg));
}

public record TpCosExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("COS", reg => new TpCosExpression(reg));
}

public record TpTanExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("TAN", reg => new TpTanExpression(reg));
}

public record TpAsinExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("ASIN", reg => new TpAsinExpression(reg));
}

public record TpAcosExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("ACOS", reg => new TpAcosExpression(reg));
}

public record TpAtanExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("ATAN", reg => new TpAtanExpression(reg));
}

public record TpAtan2Expression(TpRegister Value1, TpRegister Value2) : TpMathExpression, ITpParser<TpMathExpression>
{
    // Special case with two arguments
    public new static Parser<TpMathExpression> GetParser()
        => from kw in TpCommon.Keyword("ATAN2")
           from kvp in
               (from value1 in Argument
                from sep in Parse.Char(',')
                from value2 in Argument
                select (value1, value2)).BetweenBrackets()
           select new TpAtan2Expression(kvp.value1, kvp.value2);
}

public record TpLnExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("LN", reg => new TpLnExpression(reg));
}

public record TpExpExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("EXP", reg => new TpExpExpression(reg));
}

public record TpAbsExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("ABS", reg => new TpAbsExpression(reg));
}

public record TpTruncExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("TRUNC", reg => new TpTruncExpression(reg));
}

public record TpRoundExpression(TpRegister Value) : TpMathExpression, ITpParser<TpMathExpression>
{
    public new static Parser<TpMathExpression> GetParser()
        => MakeParser("ROUND", reg => new TpRoundExpression(reg));
}

public record TpValue : ITpParser<TpValue>
{
    public static Parser<TpValue> GetParser()
        => TpValueFloatingPointConstant.GetParser()
            .Or(TpValueIntegerConstant.GetParser())
            .Or(TpValueFlag.GetParser())
            .Or(TpValueIOPort.GetParser())
            .Or(TpValueIOState.GetParser())
            .Or(TpValueRegister.GetParser())
            .Or(TpValueParameter.GetParser())
            .Or(TpValueString.GetParser())
            .Or(TpValuePulse.GetParser())
            .Or(TpValueMathExpr.GetParser())
            .Or(TpValueTimer.GetParser())
            .Or(TpValueErrorNum.GetParser());

    public static readonly Parser<TpValue> Assignable
        = TpValueFlag.GetParser()
            .Or(TpValueIOPort.GetParser())
            .Or(TpValueRegister.GetParser())
            .Or(TpValueParameter.GetParser())
            .Or(TpValueTimer.GetParser());

    public static readonly Parser<TpValue> Position
        = TpValuePosition.GetParser()
            .Or(TpValueLpos.GetParser())
            .Or(TpValueJpos.GetParser())
            .Or(TpValueUFrame.GetParser())
            .Or(TpValueUTool.GetParser());
}

public record TpValueErrorNum : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => TpCommon.Keyword("ERR_NUM").Return(new TpValueErrorNum());
}

public record TpValuePosition(TpPosition Pos) : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => TpPosition.GetParser().Select(pos => new TpValuePosition(pos));
}

public record TpValueLpos : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => TpCommon.Keyword("LPOS").Return(new TpValueLpos());
}

public record TpValueJpos : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => TpCommon.Keyword("JPOS").Return(new TpValueJpos());
}

public record TpValueUFrame(TpUserFrame UserFrame) : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => TpUserFrame.GetParser().Select(frm => new TpValueUFrame(frm));
}

public record TpValueUTool(TpUserTool UserFrame) : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => TpUserTool.GetParser().Select(frm => new TpValueUTool(frm));
}

public record TpValueTimer(TpAccess Access) : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => from keyword in TpCommon.Keyword("TIMER")
           from access in TpAccess.GetParser()
           select new TpValueTimer(access);
}

public record TpValuePulse(double Width) : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => from keyword in TpCommon.Keyword("PULSE")
           from width in (
               from sep in TpCommon.Keyword(",")
               from width in Parse.DecimalInvariant.Select(double.Parse)
               from tok in TpCommon.Keyword("sec")
               select width
           ).Optional()
           select new TpValuePulse(width.GetOrDefault());
}

public record TpValueIntegerConstant(int Value) : TpValue, ITpParser<TpValue>
{
    private static readonly Parser<TpValueIntegerConstant> PositiveConstant =
        Parse.Number.Select(int.Parse).Select(val => new TpValueIntegerConstant(val));

    private static readonly Parser<TpValueIntegerConstant> NegativeConstant =
        Parse.Char('-').Then(_ => PositiveConstant)
            .Select(intConst => intConst with { Value = -intConst.Value }).BetweenParen();

    public new static Parser<TpValue> GetParser()
        => PositiveConstant.Or(NegativeConstant).Token();
}

public record TpValueFloatingPointConstant(double Value) : TpValue, ITpParser<TpValue>
{
    private static readonly Parser<TpValueFloatingPointConstant> PositiveConstant =
        from number in Parse.Number.Optional()
        from dot in Parse.Char('.')
        from dec in Parse.Number
        select new TpValueFloatingPointConstant(double.Parse($"{number.GetOrElse('0'.ToString())}.{dec}"));

    private static readonly Parser<TpValueFloatingPointConstant> NegativeConstant =
        Parse.Char('-').Then(_ => PositiveConstant)
            .Select(floatConst => floatConst with { Value = -floatConst.Value }).BetweenParen();

    public new static Parser<TpValue> GetParser()
        => PositiveConstant.Or(NegativeConstant).Token();
}

public record TpValueRegister(TpGenericRegister Register) : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => TpGenericRegister.GetParser().Select(reg => new TpValueRegister(reg));

    public static readonly Parser<TpValueRegister> NumericRegister
        = TpRegister.GetParser().Select(reg => new TpValueRegister(reg));

    public static readonly Parser<TpValueRegister> ArgumentRegister
        = TpArgumentRegister.GetParser().Select(reg => new TpValueRegister(reg));

    public static readonly Parser<TpValueRegister> PositionRegister
        = TpPositionRegister.GetParser().Select(reg => new TpValueRegister(reg));

    public static readonly Parser<TpValueRegister> StringRegister
        = TpStringRegister.GetParser().Select(reg => new TpValueRegister(reg));
}

public record TpValueIOPort(TpIOPort IOPort) : TpValue, ITpParser<TpValue>
{
    public static Parser<TpValueIOPort> MakeParser<TIOType>(char symbol, TpIOType type) where TIOType : TpIOPort, new()
        => TpIOPort.MakeParser(symbol, type, idx => new TIOType { Type = type, PortNumber = idx })
            .Select(ioPort => new TpValueIOPort(ioPort));

    public new static Parser<TpValue> GetParser()
        => TpIOPort.GetParser().Select(ioPort => new TpValueIOPort(ioPort));
}

public record TpValueOnOffIOPort(TpOnOffIOPort IOPort) : TpValue, ITpParser<TpValueOnOffIOPort>
{
    public new static Parser<TpValueOnOffIOPort> GetParser()
        => TpOnOffIOPort.GetParser().Select(ioPort => new TpValueOnOffIOPort((TpOnOffIOPort)ioPort));
}

public record TpValueNumericalIOPort(TpNumericalIOPort IOPort) : TpValue, ITpParser<TpValueNumericalIOPort>
{
    public new static Parser<TpValueNumericalIOPort> GetParser()
        => TpNumericalIOPort.GetParser().Select(ioPort => new TpValueNumericalIOPort((TpNumericalIOPort)ioPort));
}

public record TpValueFlag(TpFlag Flag) : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => TpFlag.GetParser().Select(flag => new TpValueFlag(flag));
}

// TODO: implement markers

public enum TpOnOffState
{
    On,
    Off
}

public struct TpOnOffStateParser
{
    public static readonly Parser<TpOnOffState> Parser =
        TpCommon.Keyword("ON").Return(TpOnOffState.On)
            .Or(TpCommon.Keyword("OFF").Return(TpOnOffState.Off));
}

public record TpValueIOState(TpOnOffState State) : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => TpOnOffStateParser.Parser.Select(state => new TpValueIOState(state));
}

public abstract record TpValueParameter : TpValue
{
    protected static readonly Parser<string> Identifier =
        Parse.Identifier(Parse.Letter, Parse.LetterOrDigit.Or(Parse.Char('_')));

    protected static readonly Parser<string> Tag =
        from variable in Identifier
        from accessor in Parse.Number.BetweenBrackets().Optional()
        select $"{variable}" + (accessor.IsDefined ? $"[{accessor.Get()}]" : string.Empty);

    protected static Parser<string> AccumulateTag(Parser<string> tagParser)
        => tagParser.DelimitedBy(Parse.Char('.'), 1, null)
            .Select(tags => tags.Aggregate((acc, tag) => string.IsNullOrWhiteSpace(acc) ? tag : $"{acc}.{tag}"));

    public new static Parser<TpValueParameter> GetParser()
        => TpValueKarelVariable.GetParser()
            .Or(TpValueSystemVariable.GetParser());
}

public sealed record TpValueSystemVariable(string Variable)
    : TpValueParameter, ITpParser<TpValueParameter>
{
    private static readonly Parser<string> VariableTag =
        from prefix in Parse.Char('$')
        from tag in Tag
        select $"${tag}";

    public new static Parser<TpValueParameter> GetParser()
        => from variable in AccumulateTag(VariableTag)
           select new TpValueSystemVariable(variable);
}

public sealed record TpValueKarelVariable(string Program, string Variable)
    : TpValueParameter, ITpParser<TpValueParameter>
{
    private static readonly Parser<string> ProgramName =
        Identifier.BetweenBrackets();

    public new static Parser<TpValueParameter> GetParser()
        => from prefix in Parse.Char('$')
           from progName in ProgramName
           from sep in TpCommon.Keyword(".").Optional()
           from var in AccumulateTag(Tag)
           select new TpValueKarelVariable(progName, var);
}

public record TpValueString(string Value) : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => from start in Parse.Char('\'')
           from value in Parse.AnyChar.Until(Parse.Char('\'')).Text()
           select new TpValueString(value);
}

public record TpValueMathExpr(TpMathExpression Expression) : TpValue, ITpParser<TpValue>
{
    public new static Parser<TpValue> GetParser()
        => TpMathExpression.GetParser().Select(expr => new TpValueMathExpr(expr));
}

public record TpUserFrame(TpAccess Access) : ITpParser<TpUserFrame>
{
    public static Parser<TpUserFrame> GetParser()
        => from keyword in TpCommon.Keyword("UFRAME")
           from access in TpAccess.GetParser()
           select new TpUserFrame(access);
}

public record TpUserTool(TpAccess Access) : ITpParser<TpUserTool>
{
    public static Parser<TpUserTool> GetParser()
        => from keyword in TpCommon.Keyword("UTOOL")
           from access in TpAccess.GetParser()
           select new TpUserTool(access);
}

