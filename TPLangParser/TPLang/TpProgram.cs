using Sprache;

namespace TPLangParser.TPLang;

public sealed record TpHeaderAttribute(string Name, string Value) : TpInstruction(0), ITpParser<TpHeaderAttribute>
{
    private static readonly Parser<TpHeaderAttribute> Inner =
        from key in TpCommon.Identifier
        from separator in Parse.Chars("=:")
        from value in Parse.AnyChar.Until(Parse.Char(';')).Text()
        select new TpHeaderAttribute(key, value);

    public new static Parser<TpHeaderAttribute> GetParser()
        => from kvp in Inner.WithLineNumber()
           select kvp.Value with { LineNumber = kvp.LineNumber };
}

public sealed record TpProgramAttributes(Dictionary<string, TpHeaderAttribute> Attributes)
    : ITpParser<TpProgramAttributes>
{

    public static Parser<TpProgramAttributes> GetParser()
        => from attrTag in Parse.String("/ATTR").Token()
           from attributes in TpHeaderAttribute.GetParser().Many()
           select new TpProgramAttributes(attributes.ToDictionary(attr => attr.Name));
}

public sealed record TpProgramAppl(Dictionary<string, string> Attributes) : ITpParser<TpProgramAppl>
{
    private static readonly Parser<(string, string)> Attribute =
        from key in Parse.CharExcept('/').Until(Parse.Char(':')).Text()
        from value in Parse.CharExcept('/').Until(Parse.Char(';')).Text()
        select (key, value);

    private static readonly Parser<(string, string)> SingleAttribute =
        from attr in TpCommon.Identifier.Token()
        from tail in Parse.Char(';').Token()
        select (attr, string.Empty);

    public static Parser<TpProgramAppl> GetParser()
        => from attrTag in Parse.String("/APPL").Token()
           from attributes in Attribute.Or(SingleAttribute).Many()
           select new TpProgramAppl(attributes.ToDictionary());
}

public enum TpProgramSubtype
{
    None,
    Collection,
    Macro,
    ConditionMonitor
}

public struct TpProgramSubtypeParser
{
    // TODO: validate the representation of all subtypes
    public static readonly Parser<TpProgramSubtype> Parser =
        from optType in
            Parse.String("Collection").Return(TpProgramSubtype.Collection)
                .Or(Parse.String("Macro").Return(TpProgramSubtype.Macro))
                .Or(Parse.String("Cond").Return(TpProgramSubtype.ConditionMonitor))
                .Token().Optional()
        select optType.GetOrElse(TpProgramSubtype.None);
}

public sealed record TpProgramHeader(
    string ProgramName,
    TpProgramSubtype SubType,
    TpProgramAttributes Attributes,
    TpProgramAppl Appl) : ITpParser<TpProgramHeader>
{
    public static Parser<TpProgramHeader> GetParser()
        => from progTag in Parse.String("/PROG").Token()
           from progName in TpCommon.ProgramName
           from subType in TpProgramSubtypeParser.Parser
           from attributes in TpProgramAttributes.GetParser()
           from appl in TpProgramAppl.GetParser().Optional()
           select new TpProgramHeader(progName, subType, attributes, appl.GetOrDefault());
}

public sealed record TpProgramMain(List<TpInstruction> Instructions) : ITpParser<TpProgramMain>
{
    public static Parser<TpProgramMain> GetParser()
        => from mainTag in Parse.String("/MN").Token()
           from instructions in TpInstruction.GetParser().XMany()
           select new TpProgramMain(instructions.ToList());
}

public sealed record TpProgramPositionExternalAxis(int Index, double Value, TpUnit Unit)
    : ITpParser<TpProgramPositionExternalAxis>
{
    private static readonly Parser<double> Double =
        from negation in Parse.Char('-').Optional()
        from number in Parse.Number.Optional()
        from dot in Parse.Char('.')
        from dec in Parse.Number
        select double.Parse($"{number.GetOrElse('0'.ToString())}.{dec}") * (negation.IsDefined ? -1 : 1);

    public static Parser<TpProgramPositionExternalAxis> GetParser()
        => from index in Parse.Char('E').Token().Then(_ => Parse.Number.Select(int.Parse))
           from separator in Parse.Char('=').Token()
           from value in Double.Or(Parse.Char('*').Many().Return(double.NaN))
           from unit in TpCommon.Unit.Token()
           select new TpProgramPositionExternalAxis(index, value, unit);
}

public sealed record TpProgramPosition(
    int Index,
    string Comment,
    int Group,
    int UserFrame,
    int UserTool,
    string Config,
    double X,
    double Y,
    double Z,
    double W,
    double P,
    double R,
    List<TpProgramPositionExternalAxis> ExtAxies) : ITpParser<TpProgramPosition>
{
    public bool IsJointPosition => string.IsNullOrWhiteSpace(Config);

    public double J1 => X;
    public double J2 => Y;
    public double J3 => Z;
    public double J4 => W;
    public double J5 => P;
    public double J6 => R;

    //TODO: make sure GP2 is handled properly, it'll come up
    private static readonly Parser<(int Index, string Comment)> IdxAndComment =
        from idx in Parse.Number.Select(int.Parse)
        from comment in Parse.Char(':')
            .Then(_ => Parse.CharExcept(']').Many().Text()).Optional()
        select (idx, comment.GetOrDefault());

    private static readonly Parser<int> GroupNumber =
        from keyword in Parse.String("GP")
        from groupNumber in Parse.Number.Select(int.Parse)
        from tail in Parse.Char(':')
        select groupNumber;

    private static readonly Parser<int> UFrame =
        from keyword in Parse.String("UF")
        from sep in Parse.Char(':').Token()
        from frameNumber in Parse.Number.Select(int.Parse)
            .Or(Parse.Char('F').Return(-1))
        from tail in Parse.Char(',')
        select frameNumber;

    private static readonly Parser<int> UTool =
        from keyword in Parse.String("UT")
        from sep in Parse.Char(':').Token()
        from toolNumber in Parse.Number.Select(int.Parse)
            .Or(Parse.Char('F').Return(-1))
        from tail in Parse.Char(',')
        select toolNumber;

    private static readonly Parser<string> Conf =
        from keyword in Parse.String("CONFIG")
        from sep in Parse.Char(':').Token()
        from config in TpValueString.GetParser().Token()
            .Select(strVal => ((TpValueString)strVal).Value)
        from tail in Parse.Char(',')
        select config;

    private static readonly Parser<char> Joint =
        Parse.Char('J').Then(_ => Parse.Chars("123456"));

    private static readonly Parser<double> Double =
        from negation in Parse.Char('-').Optional()
        from number in Parse.Number.Optional()
        from dot in Parse.Char('.')
        from dec in Parse.Number
        select double.Parse($"{number.GetOrElse('0'.ToString())}.{dec}") * (negation.IsDefined ? -1 : 1);

    private static readonly Parser<double> PosValue =
        from name in Parse.Chars("XYZWPR").Or(Joint)
        from sep in Parse.Char('=').Token()
        from value in Double.Or(Parse.Char('*').Many().Return(double.NaN))
        from tail in TpCommon.Unit.Then(_ => Parse.Char(',')).Token()
        select value;

    private static readonly Parser<TpProgramPosition> InnerParser =
        from groupNumber in GroupNumber.Token()
        from userFrame in UFrame.Token()
        from userTool in UTool.Token()
        from config in Conf.Token().Optional()
        from values in PosValue.Repeat(6)
            .Select(vals => vals.ToArray())
        from extAxies in TpProgramPositionExternalAxis.GetParser()
            .DelimitedBy(Parse.Char(','), 1, 3)
            .Select(axies => axies.ToList())
            .Optional()
        select new TpProgramPosition(0, string.Empty, groupNumber, userFrame, userTool,
            config.GetOrDefault(), values[0], values[1], values[2], values[3], values[4], values[5],
            extAxies.GetOrDefault());

    public static Parser<TpProgramPosition> GetParser()
        => from prefix in TpCommon.Keyword("P")
           from kvp in IdxAndComment.BetweenBrackets().Token()
           from pos in InnerParser.BetweenBraces().Token()
           from tail in Parse.Char(';').Token()
           select pos with { Index = kvp.Index, Comment = kvp.Comment };
}

public sealed record TpProgramPositions(List<TpProgramPosition> Positions) : ITpParser<TpProgramPositions>
{
    public static Parser<TpProgramPositions> GetParser()
        => from posTag in TpCommon.Keyword("/POS")
           from positions in TpProgramPosition.GetParser().XMany()
           select new TpProgramPositions(positions.ToList());
}

public sealed record TpProgram(TpProgramHeader Header, TpProgramMain Main, TpProgramPositions Positions)
    : ITpParser<TpProgram>
{
    public static IResult<TpProgram> ProcessAndParse(string buffer)
    {
        // Process buffer, removing any lines that start with a ':' token without a line number
        var lines = buffer.Split(['\n']);
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = lines[i];
            if (i == 0 || !line.TrimStart().StartsWith(':'))
            { continue; }

            lines[i - 1] = lines[i - 1].TrimEnd();
            lines[i - 1] += line[(line.IndexOf(':') + 1)..];
            lines[i] = "  1:  ;"; // replace with noop -> semantically identical
        }

        var processedBuffer = lines.Aggregate((acc, line) => acc + line + '\n');
        return GetParser().TryParse(processedBuffer);
    }

    public static Parser<TpProgram> GetParser()
        => from header in TpProgramHeader.GetParser()
           from main in TpProgramMain.GetParser()
           from positions in TpProgramPositions.GetParser()
           from endTag in TpCommon.Keyword("/END")
           select new TpProgram(header, main, positions);
}
