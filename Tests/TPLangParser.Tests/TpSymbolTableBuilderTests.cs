using System.Text;

using TPLangParser.TPLang;
using TPLangParser.TPLang.SymbolTable;

namespace TPLangParser.Tests;

public class TpSymbolTableBuilderTests
{
    // Wraps instruction lines in a minimal valid TP program, parses it, and
    // returns the symbol table built during parsing.
    private static TpSymbolTable BuildTable(params string[] instructionLines)
    {
        var buffer = new StringBuilder();
        buffer.Append("/PROG TEST\n/ATTR\n/MN\n");
        for (var i = 0; i < instructionLines.Length; ++i)
        {
            buffer.Append($"  {i + 1}:  {instructionLines[i]} ;\n");
        }
        buffer.Append("/END\n");

        var result = TpProgram.ProcessAndParse(buffer.ToString());
        Assert.True(result.WasSuccessful, result.ToString());
        return result.Value.SymTable;
    }

    private static TpSymbolReference SingleUsage(TpSymbolTable table, string name)
    {
        var symbol = table.GetSymbol(name);
        Assert.NotNull(symbol);
        return Assert.Single(symbol!.Usages);
    }

    [Fact]
    public void RegisterAssignment_RecordsWriteTargetAndReadSource()
    {
        var table = BuildTable("R[1]=R[2]");

        Assert.Equal(TpSymbolRefKind.Write, SingleUsage(table, "R[1]").Kind);
        Assert.Equal(TpSymbolRefKind.Read, SingleUsage(table, "R[2]").Kind);
    }

    [Fact]
    public void RegisterAssignment_ClassifiesNumericRegisterKind()
    {
        var table = BuildTable("R[5]=1");

        Assert.Equal(TpSymbolKind.NumReg, table.GetSymbol("R[5]")!.Kind);
    }

    [Fact]
    public void IOInstruction_RecordsOutputWriteAndInputRead()
    {
        var table = BuildTable("DO[1]=DI[2]");

        Assert.Equal(TpSymbolRefKind.Write, SingleUsage(table, "DO[1]").Kind);
        Assert.Equal(TpSymbolKind.DigitalIO, table.GetSymbol("DO[1]")!.Kind);
        Assert.Equal(TpSymbolRefKind.Read, SingleUsage(table, "DI[2]").Kind);
    }

    [Fact]
    public void IndirectAccess_RecordsOnlyTheInnerIndexRegister()
    {
        var table = BuildTable("R[R[2]]=5");

        // The outer register's target can't be inferred statically, so it is
        // not recorded — only its inner index register (always a read) is.
        Assert.Null(table.GetSymbol("R[R[2]]"));
        Assert.Equal(TpSymbolRefKind.Read, SingleUsage(table, "R[2]").Kind);
    }

    [Fact]
    public void PositionRegister_ElementAndWholeAccessShareIdentity()
    {
        var table = BuildTable("PR[1]=PR[2]", "PR[1,1]=R[3]");

        var pr1 = table.GetSymbol("PR[1]");
        Assert.NotNull(pr1);
        Assert.Equal(TpSymbolKind.PosReg, pr1!.Kind);
        // Both the whole-register and the element assignment write PR[1].
        Assert.Equal(2, pr1.Writes.Count());
        Assert.Equal(TpSymbolRefKind.Read, SingleUsage(table, "PR[2]").Kind);
    }

    [Fact]
    public void Conditions_RecordEveryOperandAsRead()
    {
        var table = BuildTable("IF R[1]<R[2],JMP LBL[1]");

        Assert.Equal(TpSymbolRefKind.Read, SingleUsage(table, "R[1]").Kind);
        Assert.Equal(TpSymbolRefKind.Read, SingleUsage(table, "R[2]").Kind);
    }

    [Fact]
    public void MultipleUsages_AreGroupedUnderOneSymbol()
    {
        var table = BuildTable("R[1]=1", "R[1]=R[1]+1");

        var r1 = table.GetSymbol("R[1]");
        Assert.NotNull(r1);
        Assert.Equal(2, r1!.Writes.Count());
        Assert.Single(r1.Reads);
    }

    [Fact]
    public void ArgumentAndStringRegisters_AreClassifiedDistinctly()
    {
        var table = BuildTable("SR[1]=AR[2]");

        Assert.Equal(TpSymbolKind.StrReg, table.GetSymbol("SR[1]")!.Kind);
        Assert.Equal(TpSymbolKind.ArgReg, table.GetSymbol("AR[2]")!.Kind);
    }

    [Fact]
    public void GroupIO_IsRecordedWithGroupQualifiedName()
    {
        var table = BuildTable("GO[1]=GI[2]");

        Assert.Equal(TpSymbolKind.GroupIO, table.GetSymbol("GO[1]")!.Kind);
        Assert.Equal(TpSymbolKind.GroupIO, table.GetSymbol("GI[2]")!.Kind);
    }

    [Fact]
    public void Flag_RecordsWriteTargetAndReadSource()
    {
        var table = BuildTable("F[1]=(F[2])");

        Assert.Equal(TpSymbolRefKind.Write, SingleUsage(table, "F[1]").Kind);
        Assert.Equal(TpSymbolKind.Flag, table.GetSymbol("F[1]")!.Kind);
        Assert.Equal(TpSymbolRefKind.Read, SingleUsage(table, "F[2]").Kind);
    }

    [Fact]
    public void Flag_ReadIntoRegister_IsRecorded()
    {
        var table = BuildTable("R[1]=F[3]");

        Assert.Equal(TpSymbolKind.Flag, table.GetSymbol("F[3]")!.Kind);
        Assert.Equal(TpSymbolRefKind.Read, SingleUsage(table, "F[3]").Kind);
    }

    [Fact]
    public void IndirectFlag_RecordsOnlyTheInnerIndexRegister()
    {
        var table = BuildTable("R[1]=F[R[2]]");

        // An indirectly indexed flag can't be resolved statically, so only its
        // inner index register (always a read) is recorded.
        Assert.Null(table.GetSymbol("F[R[2]]"));
        Assert.Equal(TpSymbolRefKind.Read, SingleUsage(table, "R[2]").Kind);
    }

    [Fact]
    public void SystemVariable_ReadIntoRegister_IsRecorded()
    {
        var table = BuildTable("R[1]=$GROUP[1].$CURRENT_ANG[2]");

        var sysVar = table.GetSymbol("$GROUP[1].$CURRENT_ANG[2]");
        Assert.NotNull(sysVar);
        Assert.Equal(TpSymbolKind.SysVar, sysVar!.Kind);
        Assert.Equal(TpSymbolRefKind.Read, Assert.Single(sysVar.Usages).Kind);
    }

    [Fact]
    public void SystemVariable_AssignmentTarget_IsRecordedAsWrite()
    {
        var table = BuildTable("$SHELL_CONFIG=1");

        var sysVar = table.GetSymbol("$SHELL_CONFIG");
        Assert.NotNull(sysVar);
        Assert.Equal(TpSymbolKind.SysVar, sysVar!.Kind);
        Assert.Equal(TpSymbolRefKind.Write, Assert.Single(sysVar.Usages).Kind);
    }

    [Fact]
    public void KarelVariable_IsRecordedWithProgramQualifiedName()
    {
        var table = BuildTable("R[1]=$[AGT]CONF.EXT_AXIS.RAIL.ENB");

        var karelVar = table.GetSymbol("$[AGT]CONF.EXT_AXIS.RAIL.ENB");
        Assert.NotNull(karelVar);
        Assert.Equal(TpSymbolKind.KarelVar, karelVar!.Kind);
        Assert.Equal(TpSymbolRefKind.Read, Assert.Single(karelVar.Usages).Kind);
    }

    [Fact]
    public void Variable_CarriesItsSourcePosition()
    {
        var table = BuildTable("R[1]=$ERROR");

        var usage = Assert.Single(table.GetSymbol("$ERROR")!.Usages);
        // Source lines: /PROG (0), /ATTR (1), /MN (2), instruction (3). The
        // reported line must be the 0-based source line, i.e. 3.
        Assert.Equal(3, usage.Position.Line);
    }
}
