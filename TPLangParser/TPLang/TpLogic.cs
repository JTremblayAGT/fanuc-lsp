using Sprache;

namespace AGT.TPLangParser.TPLang;
public abstract record TpComparisonExpression(TpComparisonOperator Operator, TpValue Lhs, TpValue Rhs)
    : ITpParser<TpComparisonExpression>
{
    public static Parser<TpComparisonExpression> GetParser()
        => TpRegisterComparisonExpression.GetParser()
            .Or(TpDigitalIOComparisonExpression.GetParser())
            .Or(TpAnalogIOComparisonExpression.GetParser())
            .Or(TpParameterComparisonExpression.GetParser());
}

public sealed record TpRegisterComparisonExpression(TpComparisonOperator Operator, TpValue Lhs, TpValue Rhs)
    : TpComparisonExpression(Operator, Lhs, Rhs), ITpParser<TpComparisonExpression>
{
    private static readonly Parser<TpValue> AllowedValues =
        TpValueIntegerConstant.GetParser()
            .Or(TpValueFloatingPointConstant.GetParser())
            .Or(TpValueRegister.GetParser());

    public new static Parser<TpComparisonExpression> GetParser()
        => from register in TpValueRegister.GetParser()
            from op in TpComparisonOperatorParser.Parser
            from value in AllowedValues
            select new TpRegisterComparisonExpression(op, register, value);
}

public sealed record TpDigitalIOComparisonExpression(TpComparisonOperator Operator, TpValue Lhs, TpValue Rhs)
    : TpComparisonExpression(Operator, Lhs, Rhs), ITpParser<TpComparisonExpression>
{
    private static readonly Parser<TpValue> AllowedValues =
        TpValueIOPort.GetParser()
            .Or(TpValueIOState.GetParser())
            .Or(TpValueRegister.GetParser());

    public new static Parser<TpComparisonExpression> GetParser()
        => from port in TpOnOffIOPort.GetParser().Select(port => new TpValueIOPort(port))
            from op in TpComparisonOperatorParser.Parser
            from value in AllowedValues
            select new TpDigitalIOComparisonExpression(op, port, value);
}

public sealed record TpAnalogIOComparisonExpression(TpComparisonOperator Operator, TpValue Lhs, TpValue Rhs)
    : TpComparisonExpression(Operator, Lhs, Rhs), ITpParser<TpComparisonExpression>
{
    private static readonly Parser<TpValue> AllowedValues =
        TpValueIntegerConstant.GetParser()
            .Or(TpValueFloatingPointConstant.GetParser())
            .Or(TpValueRegister.GetParser());

    public new static Parser<TpComparisonExpression> GetParser()
        => from port in TpNumericalIOPort.GetParser().Select(port => new TpValueIOPort(port))
            from op in TpComparisonOperatorParser.Parser
            from value in AllowedValues
            select new TpAnalogIOComparisonExpression(op, port, value);
}

public sealed record TpParameterComparisonExpression(TpComparisonOperator Operator, TpValue Lhs, TpValue Rhs)
    : TpComparisonExpression(Operator, Lhs, Rhs), ITpParser<TpComparisonExpression>
{
    private static readonly Parser<TpValue> AllowedValues =
        TpValueIntegerConstant.GetParser()
            .Or(TpValueFloatingPointConstant.GetParser())
            .Or(TpValueRegister.GetParser());

    public new static Parser<TpComparisonExpression> GetParser()
        => from port in TpValueParameter.GetParser()
            from op in TpComparisonOperatorParser.Parser
            from value in AllowedValues
            select new TpParameterComparisonExpression(op, port, value);
}

public abstract record TpLogicExpression : ITpParser<TpLogicExpression>
{
    public static Parser<TpLogicExpression> GetParser()
        => TpLogicExpressionOr.GetParser()
            .Or(TpLogicExpressionAnd.GetParser())
            .Or(TpLogicExpressionSingle.GetParser());
}

public sealed record TpLogicExpressionSingle(TpComparisonExpression Expression) 
    : TpLogicExpression, ITpParser<TpLogicExpression>
{
    public new static Parser<TpLogicExpression> GetParser() 
        => TpComparisonExpression.GetParser()
            .Select(comp => new TpLogicExpressionSingle(comp));
}
public sealed record TpLogicExpressionAnd(List<TpComparisonExpression> Expression) 
    : TpLogicExpression, ITpParser<TpLogicExpression>
{
    public new static Parser<TpLogicExpression> GetParser()
        => TpComparisonExpression.GetParser()
            .DelimitedBy(TpCommon.Keyword("AND"), 2, 5)
            .Select(exprs => new TpLogicExpressionAnd(exprs.ToList()));
}
public sealed record TpLogicExpressionOr(List<TpComparisonExpression> Expression) 
    : TpLogicExpression, ITpParser<TpLogicExpression>
{
    public new static Parser<TpLogicExpression> GetParser()
        => TpComparisonExpression.GetParser()
            .DelimitedBy(TpCommon.Keyword("OR"), 2, 5)
            .Select(exprs => new TpLogicExpressionOr(exprs.ToList()));
}
