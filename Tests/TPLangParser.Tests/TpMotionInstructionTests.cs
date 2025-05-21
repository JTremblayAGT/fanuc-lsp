using AGT.TPLangParser.TPLang;
using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.Tests;

public class TpMotionInstructionTests
{
    [Theory]
    [InlineData("L P[1] 100% FINE", TpMotionType.Linear)]
    [InlineData("J P[2] 100% FINE", TpMotionType.Joint)]
    [InlineData("C P[3] P[4] 100% FINE", TpMotionType.Circular)]
    [InlineData("A P[5] P[6] 100% FINE", TpMotionType.CircularArc)]
    [InlineData("S P[7] 100% FINE", TpMotionType.Spline)]
    public void ParseMotionInstruction_ValidMotionTypes_ReturnsCorrectType(string input, TpMotionType expectedType)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Equal(expectedType, result.MotionType);
    }

    [Theory]
    [InlineData("X P[1] 100% FINE")]
    [InlineData("B P[1] 100% FINE")]
    [InlineData("H P[1] 100% FINE")]
    [InlineData("P P[1] 100% FINE")]
    public void ParseMotionInstruction_InvalidMotionType_ThrowsParseException(string input) =>
        Assert.Throws<ParseException>(() => TpMotionInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("L P[1] 10% FINE", TpSpeedUnit.Percentage, 10)]
    [InlineData("L P[1] 50% FINE", TpSpeedUnit.Percentage, 50)]
    [InlineData("L P[1] 100% FINE", TpSpeedUnit.Percentage, 100)]
    public void ParseMotionInstruction_PercentageSpeed_ParsesCorrectly(string input, TpSpeedUnit expectedType, double expectedValue)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMotionSpeedLiteral>(result.Speed);
        var speed = (TpMotionSpeedLiteral)result.Speed;
        Assert.Equal(expectedType, speed.Unit);
        Assert.Equal(expectedValue, speed.Value);
    }

    [Theory]
    [InlineData("L P[1] 10mm/sec FINE", TpSpeedUnit.MmPerSec, 10)]
    [InlineData("L P[1] 100mm/sec FINE", TpSpeedUnit.MmPerSec, 100)]
    [InlineData("L P[1] 500mm/sec FINE", TpSpeedUnit.MmPerSec, 500)]
    public void ParseMotionInstruction_MillimeterSpeed_ParsesCorrectly(string input, TpSpeedUnit expectedType, double expectedValue)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMotionSpeedLiteral>(result.Speed);
        var speed = (TpMotionSpeedLiteral)result.Speed;
        Assert.Equal(expectedType, speed.Unit);
        Assert.Equal(expectedValue, speed.Value);
    }

    [Theory]
    [InlineData("L P[1] 10cm/min FINE", TpSpeedUnit.CmPerMin, 10)]
    [InlineData("L P[1] 100cm/min FINE", TpSpeedUnit.CmPerMin, 100)]
    [InlineData("L P[1] 1000cm/min FINE", TpSpeedUnit.CmPerMin, 1000)]
    public void ParseMotionInstruction_CentimeterSpeed_ParsesCorrectly(string input, TpSpeedUnit expectedType, double expectedValue)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMotionSpeedLiteral>(result.Speed);
        var speed = (TpMotionSpeedLiteral)result.Speed;
        Assert.Equal(expectedType, speed.Unit);
        Assert.Equal(expectedValue, speed.Value);
    }

    [Theory]
    [InlineData("L P[1] 0.1deg/sec FINE", TpSpeedUnit.DegPerSec, 0.1)]
    [InlineData("L P[1] 10deg/sec FINE", TpSpeedUnit.DegPerSec, 10)]
    [InlineData("L P[1] 180deg/sec FINE", TpSpeedUnit.DegPerSec, 180)]
    public void ParseMotionInstruction_DegreeSpeed_ParsesCorrectly(string input, TpSpeedUnit expectedType, double expectedValue)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMotionSpeedLiteral>(result.Speed);
        var speed = (TpMotionSpeedLiteral)result.Speed;
        Assert.Equal(expectedType, speed.Unit);
        Assert.Equal(expectedValue, speed.Value);
    }

    [Theory]
    [InlineData("L P[1] 5sec FINE", TpSpeedUnit.Seconds, 5)]
    [InlineData("L P[1] 10sec FINE", TpSpeedUnit.Seconds, 10)]
    [InlineData("L P[1] 60sec FINE", TpSpeedUnit.Seconds, 60)]
    public void ParseMotionInstruction_TimeSpeed_ParsesCorrectly(string input, TpSpeedUnit expectedType, double expectedValue)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMotionSpeedLiteral>(result.Speed);
        var speed = (TpMotionSpeedLiteral)result.Speed;
        Assert.Equal(expectedType, speed.Unit);
        Assert.Equal(expectedValue, speed.Value);
    }

    [Theory]
    [InlineData("L P[1] R[1] FINE")]
    [InlineData("L P[1] AR[2] FINE")]
    [InlineData("L P[1] R[AR[3]] FINE")]
    [InlineData("L PR[62:JPOS[]] R[81:JOG L mm/s Speed]mm/sec FINE Skip,LBL[1100] ACC60")]
    public void ParseMotionInstruction_RegisterSpeed_ParsesCorrectly(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMotionSpeedIndirect>(result.Speed);
    }

    [Theory]
    [InlineData("L P[1] WELD_SPEED FINE")]
    public void ParseMotionInstruction_WeldSpeed_ParsesCorrectly(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMotionSpeedWeld>(result.Speed);
    }

    // Tests for the FINE, CNT, and CD termination types

    [Theory]
    [InlineData("L P[1] 100% FINE", TpTerminationType.Fine)]
    public void ParseMotionInstruction_FineTermination_ParsesCorrectly(string input, TpTerminationType expectedTerminationType)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Equal(result.Termination.Type, expectedTerminationType);
    }

    [Theory]
    [InlineData("L P[1] 100% CNT0", TpTerminationType.Cnt, 0)]
    [InlineData("L P[1] 100% CNT1", TpTerminationType.Cnt, 1)]
    [InlineData("L P[1] 100% CNT50", TpTerminationType.Cnt, 50)]
    [InlineData("L P[1] 100% CNT100", TpTerminationType.Cnt, 100)]
    [InlineData("L P[1] 100% CD0", TpTerminationType.Cd, 0)]
    [InlineData("L P[1] 100% CD1", TpTerminationType.Cd, 1)]
    [InlineData("L P[1] 100% CD10", TpTerminationType.Cd, 10)]
    [InlineData("L P[1] 100% CD100", TpTerminationType.Cd, 100)]
    public void ParseMotionInstruction_CntOrCdTermination_ParsesCorrectly(string input, TpTerminationType expectedTerminationType, int expectedValue)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Equal(result.Termination.Type, expectedTerminationType);
        Assert.NotNull(result.Termination.Value);
        Assert.Equal(result.Termination.Value, expectedValue);
    }

    // Tests for TpPosition in motion instructions

    [Theory]
    [InlineData("L P[1] 100% FINE", 1)]
    [InlineData("J P[42] 100% FINE", 42)]
    [InlineData("L P[999] 100% FINE", 999)]
    public void ParseMotionInstruction_DirectPositionRegister_ParsesCorrectly(string input, int expectedRegisterNumber)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpPosition>(result.PrimaryPosition);
        var position = result.PrimaryPosition;
        Assert.IsType<TpAccessDirect>(position.Access);
        var access = position.Access as TpAccessDirect;
        Assert.NotNull(access);
        Assert.Equal(expectedRegisterNumber, access.Number);
    }

    [Theory]
    [InlineData("L P[R[1]] 100% FINE")]
    [InlineData("J P[R[10]] 100% FINE")]
    [InlineData("L P[R[AR[5]]] 100% FINE")]
    public void ParseMotionInstruction_IndirectPositionRegister_ParsesCorrectly(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpPosition>(result.PrimaryPosition);
        var position = result.PrimaryPosition;
        Assert.IsType<TpAccessIndirect>(position.Access);
    }

    [Theory]
    [InlineData("L PR[1] 100% FINE", 1)]
    [InlineData("J PR[25] 100% FINE", 25)]
    public void ParseMotionInstruction_PositionRegister_ParsesCorrectly(string input, int expectedRegisterNumber)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpPositionRegister>(result.PrimaryPosition);
        var position = (TpPositionRegister)result.PrimaryPosition;
        Assert.IsType<TpAccessDirect>(position.Access);
        var access = position.Access as TpAccessDirect;
        Assert.NotNull(access);
        Assert.Equal(expectedRegisterNumber, access.Number);
    }

    [Theory]
    [InlineData("L PR[R[1]] 100% FINE")]
    [InlineData("J PR[AR[12]] 100% FINE")]
    public void ParseMotionInstruction_IndirectPositionRegisterPR_ParsesCorrectly(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpPositionRegister>(result.PrimaryPosition);
        var position = (TpPositionRegister)result.PrimaryPosition;
        Assert.IsType<TpAccessIndirect>(position.Access);
    }

    [Theory]
    [InlineData("C P[1] P[2] 100% FINE", 1, 2)]
    [InlineData("A P[10] P[20] 100% FINE", 10, 20)]
    public void ParseMotionInstruction_TwoPositions_ParsesCorrectly(string input, int expectedFirstRegister, int expectedSecondRegister)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        // Check primary position
        Assert.IsType<TpPosition>(result.PrimaryPosition);
        var primary = result.PrimaryPosition;
        Assert.IsType<TpAccessDirect>(primary.Access);
        var primaryAccess = (TpAccessDirect)primary.Access;
        Assert.Equal(expectedFirstRegister, primaryAccess.Number);

        // Check secondary position
        Assert.NotNull(result.SecondaryPosition);
        Assert.IsType<TpPosition>(result.SecondaryPosition);
        var secondary = result.SecondaryPosition;
        Assert.IsType<TpAccessDirect>(secondary.Access);
        var secondaryAccess = (TpAccessDirect)secondary.Access;
        Assert.Equal(expectedSecondRegister, secondaryAccess.Number);
    }

    [Theory]
    [InlineData("C PR[1] PR[2] 100% FINE")]
    [InlineData("A PR[10] P[20] 100% FINE")]
    [InlineData("C P[1] PR[20] 100% FINE")]
    public void ParseMotionInstruction_MixedPositionTypes_ParsesCorrectly(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        // Both positions should be present
        Assert.NotNull(result.PrimaryPosition);
        Assert.NotNull(result.SecondaryPosition);
    }

    [Theory]
    [InlineData("L P[1:Home Position] 100% FINE", "Home Position")]
    public void ParseMotionInstruction_WithPositionComment_ParsesCorrectly(string input, string expectedComment)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpPosition>(result.PrimaryPosition);
        var position = result.PrimaryPosition;
        Assert.IsType<TpAccessDirect>(position.Access);
        var access = (TpAccessDirect)position.Access;
        Assert.Equal(1, access.Number);
        Assert.Equal(expectedComment, access.Comment);
    }

    [Theory]
    [InlineData("L P[1] 100% FINE")]
    [InlineData("J P[2] 100% FINE")]
    [InlineData("S P[3] 100% FINE")]
    public void SecondaryPosition_NonCircularMotion_ThrowsIllegalOperationException(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Throws<InvalidOperationException>(() =>
        {
            var position = result.SecondaryPosition;
        });
    }

    [Theory]
    [InlineData("L P[1] 100% FINE Wjnt")]
    [InlineData("J P[2] 100% FINE Wjnt")]
    [InlineData("C P[3] P[4] 100% FINE Wjnt")]
    [InlineData("A P[5] P[6] 100% FINE Wjnt")]
    [InlineData("S P[7] 100% FINE Wjnt")]
    public void ParseMotionInstruction_WithWristJointOption_AddsOptionToList(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpWristJointOption);
        Assert.Single(result.Options.OfType<TpWristJointOption>());
    }

    [Theory]
    [InlineData("L P[1] 100% FINE ACC100", 100)]
    [InlineData("J P[2] 100% FINE ACC50", 50)]
    [InlineData("C P[3] P[4] 100% FINE ACC25", 25)]
    [InlineData("A P[5] P[6] 100% FINE ACC1", 1)]
    [InlineData("S P[7] 100% FINE ACC40", 40)]
    [InlineData("J P[1:LocalHome 1] 100% CNT0 ACC60", 60)]
    public void ParseMotionInstruction_AccOption_AddsOptionToList(string input, int expectedValue)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpAccOption accOption && accOption.Value == expectedValue);
        Assert.Single(result.Options.OfType<TpAccOption>());
    }

    [Theory]
    [InlineData("L P[1] 100% CNT100 PTH")]
    [InlineData("J P[2] 100% CNT100 PTH")]
    [InlineData("C P[3] P[4] 100% CNT100 PTH")]
    [InlineData("A P[5] P[6] 100% CNT100 PTH")]
    [InlineData("S P[7] 100% CNT100 PTH")]
    public void ParseMotionInstruction_WithPathOption_AddsOptionToList(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpPathOption);
        Assert.Single(result.Options.OfType<TpPathOption>());
    }

    [Theory]
    [InlineData("L P[1] 100% CNT100 AP_LD10", TpLinearDistanceType.Approach, 10)]
    [InlineData("L P[1] 100% CNT100 AP_LD25", TpLinearDistanceType.Approach, 25)]
    [InlineData("L P[1] 100% CNT100 AP_LD50", TpLinearDistanceType.Approach, 50)]
    [InlineData("J P[2] 100% CNT100 AP_LD75", TpLinearDistanceType.Approach, 75)]
    [InlineData("L P[1] 100% CNT100 RT_LD10", TpLinearDistanceType.Retract, 10)]
    [InlineData("L P[1] 100% CNT100 RT_LD25", TpLinearDistanceType.Retract, 25)]
    [InlineData("L P[1] 100% CNT100 RT_LD50", TpLinearDistanceType.Retract, 50)]
    [InlineData("J P[2] 100% CNT100 RT_LD75", TpLinearDistanceType.Retract, 75)]
    public void ParseMotionInstruction_WithLinearDistanceOption_AddsOptionToList(string input, TpLinearDistanceType expectedType, int expectedValue)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpLinearDistanceOptionLiteral);

        var distanceOption = result.Options.OfType<TpLinearDistanceOptionLiteral>().FirstOrDefault();
        Assert.NotNull(distanceOption);
        Assert.Equal(expectedType, distanceOption.Type);
        Assert.Equal(expectedValue, distanceOption.Distance);
        Assert.Single(result.Options.OfType<TpLinearDistanceOption>());
    }

    [Theory]
    [InlineData("L P[1] 100% CNT100 AP_LDR[1]", TpLinearDistanceType.Approach)]
    [InlineData("L P[1] 100% CNT100 RT_LDR[42]", TpLinearDistanceType.Retract)]
    public void ParseMotionInstruction_WithLinearDistanceOptionRegister_AddsOptionToList(string input, TpLinearDistanceType expectedType)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpLinearDistanceOptionRegister);

        var distanceOption = result.Options.OfType<TpLinearDistanceOptionRegister>().FirstOrDefault();
        Assert.NotNull(distanceOption);
        Assert.Equal(expectedType, distanceOption.Type);
        Assert.Single(result.Options.OfType<TpLinearDistanceOption>());
    }

    [Theory]
    [InlineData("L P[1] 100% FINE BREAK")]
    [InlineData("J P[2] 100% CNT100 BREAK")]
    [InlineData("C P[3] P[4] 100% FINE BREAK")]
    [InlineData("A P[5] P[6] 100% FINE BREAK")]
    [InlineData("S P[7] 100% CNT50 BREAK")]
    public void ParseMotionInstruction_WithBREAKOption_AddsOptionToList(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpBreakOption);
        Assert.Single(result.Options.OfType<TpBreakOption>());
    }
    [Theory]
    [InlineData("L P[1] 100% FINE Offset", false, null)]
    [InlineData("J P[2] 100% CNT100 Offset", false, null)]
    [InlineData("C P[3] P[4] 100% FINE Offset", false, null)]
    [InlineData("A P[5] P[6] 100% FINE Offset", false, null)]
    [InlineData("S P[7] 100% CNT50 Offset", false, null)]
    [InlineData("L P[1] 100% FINE Offset, PR[1]", true, 1)]
    [InlineData("J P[2] 100% CNT100 Offset, PR[5]", true, 5)]
    [InlineData("C P[3] P[4] 100% FINE Offset, PR[10]", true, 10)]
    [InlineData("A P[5] P[6] 100% FINE Offset, PR[42]", true, 42)]
    [InlineData("S P[7] 100% CNT50 Offset, PR[100]", true, 100)]
    public void ParseMotionInstruction_WithOffsetOption_AddsOptionToList(
        string input,
        bool hasPositionRegister,
        int? expectedRegisterNumber)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpOffsetOption);
        Assert.Single(result.Options.OfType<TpOffsetOption>());

        var offsetOption = result.Options.OfType<TpOffsetOption>().FirstOrDefault();
        Assert.NotNull(offsetOption);

        if (hasPositionRegister)
        {
            Assert.NotNull(offsetOption.PositionRegister);
            Assert.IsType<TpPositionRegister>(offsetOption.PositionRegister);

            var positionRegister = offsetOption.PositionRegister;
            Assert.IsType<TpAccessDirect>(positionRegister.Access);

            var access = positionRegister.Access as TpAccessDirect;
            Assert.NotNull(access);
            Assert.Equal(expectedRegisterNumber, access.Number);
        }
        else
        {
            Assert.Null(offsetOption.PositionRegister);
        }
    }

    [Theory]
    [InlineData("L P[1] 100% FINE Offset, PR[R[1]]")]
    [InlineData("J P[2] 100% CNT100 Offset, PR[AR[5]]")]
    public void ParseMotionInstruction_WithOffsetOption_IndirectRegister_ParsesCorrectly(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpOffsetOption);

        var offsetOption = result.Options.OfType<TpOffsetOption>().FirstOrDefault();
        Assert.NotNull(offsetOption);
        Assert.NotNull(offsetOption.PositionRegister);
        Assert.IsType<TpPositionRegister>(offsetOption.PositionRegister);

        var positionRegister = offsetOption.PositionRegister;
        Assert.IsType<TpAccessIndirect>(positionRegister.Access);
    }
    [Theory]
    [InlineData("L P[1] 100% FINE Tool_Offset", false, null)]
    [InlineData("J P[2] 100% CNT100 Tool_Offset", false, null)]
    [InlineData("C P[3] P[4] 100% FINE Tool_Offset", false, null)]
    [InlineData("A P[5] P[6] 100% FINE Tool_Offset", false, null)]
    [InlineData("S P[7] 100% CNT50 Tool_Offset", false, null)]
    [InlineData("L P[1] 100% FINE Tool_Offset, PR[1]", true, 1)]
    [InlineData("J P[2] 100% CNT100 Tool_Offset, PR[5]", true, 5)]
    [InlineData("C P[3] P[4] 100% FINE Tool_Offset, PR[10]", true, 10)]
    [InlineData("A P[5] P[6] 100% FINE Tool_Offset, PR[42]", true, 42)]
    [InlineData("S P[7] 100% CNT50 Tool_Offset, PR[100]", true, 100)]
    public void ParseMotionInstruction_WithToolOffsetOption_AddsOptionToList(
        string input,
        bool hasPositionRegister,
        int? expectedRegisterNumber)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpToolOffsetOption);
        Assert.Single(result.Options.OfType<TpToolOffsetOption>());

        var toolOffsetOption = result.Options.OfType<TpToolOffsetOption>().FirstOrDefault();
        Assert.NotNull(toolOffsetOption);

        if (hasPositionRegister)
        {
            Assert.NotNull(toolOffsetOption.PositionRegister);
            Assert.IsType<TpPositionRegister>(toolOffsetOption.PositionRegister);

            var positionRegister = toolOffsetOption.PositionRegister;
            Assert.IsType<TpAccessDirect>(positionRegister.Access);

            var access = positionRegister.Access as TpAccessDirect;
            Assert.NotNull(access);
            Assert.Equal(expectedRegisterNumber, access.Number);
        }
        else
        {
            Assert.Null(toolOffsetOption.PositionRegister);
        }
    }

    [Theory]
    [InlineData("L P[1] 100% FINE Tool_Offset, PR[R[1]]")]
    [InlineData("J P[2] 100% CNT100 Tool_Offset, PR[AR[5]]")]
    public void ParseMotionInstruction_WithToolOffsetOption_IndirectRegister_ParsesCorrectly(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpToolOffsetOption);

        var toolOffsetOption = result.Options.OfType<TpToolOffsetOption>().FirstOrDefault();
        Assert.NotNull(toolOffsetOption);
        Assert.NotNull(toolOffsetOption.PositionRegister);
        Assert.IsType<TpPositionRegister>(toolOffsetOption.PositionRegister);

        var positionRegister = toolOffsetOption.PositionRegister;
        Assert.IsType<TpAccessIndirect>(positionRegister.Access);
    }

    [Theory]
    [InlineData("L P[1] 100% FINE ORNT_BASE",TpOrntBaseRefFrame.WorldFrame, new object[]{0, 'z'})]
    [InlineData("J P[2] 100% CNT100 ORNT_BASE",TpOrntBaseRefFrame.WorldFrame, new object[]{0, 'z'})]
    [InlineData("C P[3] P[4] 100% FINE ORNT_BASE", TpOrntBaseRefFrame.WorldFrame, new object[]{0, 'z'})]
    [InlineData("A P[5] P[6] 100% FINE ORNT_BASE", TpOrntBaseRefFrame.WorldFrame, new object[] {0, 'z'})]
    [InlineData("S P[7] 100% CNT50 ORNT_BASE", TpOrntBaseRefFrame.WorldFrame, new object[] { 0, 'z' })]
    [InlineData("L P[1] 100% FINE ORNT_BASE UF[1,x]",TpOrntBaseRefFrame.UserFrame, new object[] { 1, 'x' })]
    [InlineData("J P[2] 100% CNT100 ORNT_BASE UF[3,y]", TpOrntBaseRefFrame.UserFrame, new object[] { 3, 'y' })]
    [InlineData("L P[1] 100% FINE ORNT_BASE LDR[5,z]", TpOrntBaseRefFrame.LeaderReferenceFrame, new object[] { 5, 'z' })]
    [InlineData("J P[2] 100% CNT100 ORNT_BASE LDR[7,x]", TpOrntBaseRefFrame.LeaderReferenceFrame, new object[] { 7, 'x' })]
    public void ParseMotionInstruction_WithOrntBaseOption_AddsOptionToList(
        string input,
        TpOrntBaseRefFrame expectedFrameType,
        object[] expectedValues)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpOrntBaseOption);
        Assert.Single(result.Options.OfType<TpOrntBaseOption>());

        var orntBaseOption = result.Options.OfType<TpOrntBaseOption>().FirstOrDefault();
        Assert.NotNull(orntBaseOption);
        Assert.Equal(expectedFrameType, orntBaseOption.ReferenceFrame);
        Assert.Equal((int)expectedValues[0], orntBaseOption.FrameIndex);
        Assert.Equal((char)expectedValues[1], orntBaseOption.DirectionIndex);
    }

    [Theory]
    [InlineData("L P[1] 100% FINE RTCP")]
    [InlineData("J P[2] 100% CNT100 RTCP")]
    [InlineData("C P[3] P[4] 100% FINE RTCP")]
    [InlineData("A P[5] P[6] 100% FINE RTCP")]
    [InlineData("S P[7] 100% CNT50 RTCP")]
    public void ParseMotionInstruction_WithRemoteTcpOption_AddsOptionToList(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpRemoteTcpOption);
        Assert.Single(result.Options.OfType<TpRemoteTcpOption>());
    }

    [Theory]
    [InlineData("L P[1] 100% FINE Skip,LBL[1]", 1)]
    [InlineData("J P[2] 100% CNT100 Skip,LBL[5]", 5)]
    [InlineData("C P[3] P[4] 100% FINE Skip,LBL[10]", 10)]
    [InlineData("A P[5] P[6] 100% FINE Skip,LBL[42]", 42)]
    [InlineData("S P[7] 100% CNT50 Skip,LBL[100]", 100)]
    public void ParseMotionInstruction_WithSkipOption_AddsOptionToList(string input, int expectedLabelNumber)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpSkipOption);
        Assert.Single(result.Options.OfType<TpSkipOption>());

        var skipOption = result.Options.OfType<TpSkipOption>().FirstOrDefault();
        Assert.NotNull(skipOption);
        Assert.NotNull(skipOption.Label);
        var access = Assert.IsType<TpAccessDirect>(skipOption.Label.LabelNumber);
        Assert.Equal(expectedLabelNumber, access.Number);
    }

    [Theory]
    [InlineData("L P[1] 100% FINE SkipJump,LBL[1]", 1)]
    [InlineData("J P[2] 100% CNT100 SkipJump,LBL[5]", 5)]
    [InlineData("C P[3] P[4] 100% FINE SkipJump,LBL[10]", 10)]
    [InlineData("A P[5] P[6] 100% FINE SkipJump,LBL[42]", 42)]
    [InlineData("S P[7] 100% CNT50 SkipJump,LBL[100]", 100)]
    public void ParseMotionInstruction_WithSkipJumpOption_AddsOptionToList(string input, int expectedLabelNumber)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpSkipJumpOption);
        Assert.Single(result.Options.OfType<TpSkipJumpOption>());

        var skipOption = result.Options.OfType<TpSkipJumpOption>().FirstOrDefault();
        Assert.NotNull(skipOption);
        Assert.NotNull(skipOption.Label);
        var access = Assert.IsType<TpAccessDirect>(skipOption.Label.LabelNumber);
        Assert.Equal(expectedLabelNumber, access.Number);
    }

    [Theory]
    [InlineData("L P[1] 100% FINE Arc Start[1,R[2]]", TpArcWeldingOptionType.Start, 1, true, 1, false, 2)]
    [InlineData("J P[2] 100% FINE Arc End[5,R[10]]", TpArcWeldingOptionType.End, 1, true, 5, false, 10)]
    [InlineData("L P[1] 100% FINE Arc Start[R[1],R[2]]", TpArcWeldingOptionType.Start, 1, false, 1, false, 2)]
    [InlineData("J P[2] 100% FINE Arc End[R[5],R[10]]", TpArcWeldingOptionType.End, 1, false, 5, false, 10)]
    [InlineData("L P[1] 100% FINE Arc Start E2[R[1],R[2]]", TpArcWeldingOptionType.Start, 2, false, 1, false, 2)]
    [InlineData("J P[2] 100% FINE Arc End E3[R[5],R[10]]", TpArcWeldingOptionType.End, 3, false, 5, false, 10)]
    public void ParseMotionInstruction_WithWeldOption_AddsOptionToList(
        string input,
        TpArcWeldingOptionType expectedType,
        int expectedEquipment,
        bool procedureIsDirect,
        int expectedProcedureValue,
        bool scheduleIsDirect,
        int expectedScheduleValue)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpWeldOption);
        Assert.Single(result.Options.OfType<TpWeldOption>());

        var weldOption = result.Options.OfType<TpWeldOption>().FirstOrDefault();
        Assert.NotNull(weldOption);
        Assert.Equal(expectedType, weldOption.Type);
        Assert.Equal(expectedEquipment, weldOption.WeldEquipment);

        // Verify that the arguments are of type TpWeldOptionProcedures
        Assert.IsType<TpWeldOptionProcedures>(weldOption.Args);
        var procedures = (TpWeldOptionProcedures)weldOption.Args;

        // Check procedure
        if (procedureIsDirect)
        {
            Assert.IsType<TpWeldOptionScheduleArg>(procedures.Procedure);
            var procArg = (TpWeldOptionScheduleArg)procedures.Procedure;
            Assert.Equal(expectedProcedureValue, procArg.ScheduleNumber);
        }
        else
        {
            Assert.IsType<TpWeldOptionRegisterArg>(procedures.Procedure);
            var procArg = (TpWeldOptionRegisterArg)procedures.Procedure;
            Assert.IsType<TpRegister>(procArg.Register);

            var register = procArg.Register;
            Assert.IsType<TpAccessDirect>(register.Access);
            var access = (TpAccessDirect)register.Access;
            Assert.Equal(expectedProcedureValue, access.Number);
        }

        // Check schedule
        if (scheduleIsDirect)
        {
            Assert.IsType<TpWeldOptionScheduleArg>(procedures.Schedule);
            var schedArg = (TpWeldOptionScheduleArg)procedures.Schedule;
            Assert.Equal(expectedScheduleValue, schedArg.ScheduleNumber);
        }
        else
        {
            Assert.IsType<TpWeldOptionRegisterArg>(procedures.Schedule);
            var schedArg = (TpWeldOptionRegisterArg)procedures.Schedule;
            Assert.IsType<TpRegister>(schedArg.Register);

            var register = schedArg.Register;
            Assert.IsType<TpAccessDirect>(register.Access);
            var access = (TpAccessDirect)register.Access;
            Assert.Equal(expectedScheduleValue, access.Number);
        }
    }

    [Theory]
    [InlineData("L P[1] 100% FINE Arc Start[1.5,2.0,3.2]")]
    [InlineData("J P[2] 100% FINE Arc End[0.5,1.0,1.5,2.0]")]
    [InlineData("L P[1] 100% FINE Arc StartE2[1.5,2.0,3.2]")]
    [InlineData("L P[1] 100% FINE Arc StartE2[0,0.0IPM,0.000,0.00,0.0s]")]
    public void ParseMotionInstruction_WithWeldOptionParameters_ParsesCorrectly(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpWeldOption);

        var weldOption = result.Options.OfType<TpWeldOption>().FirstOrDefault();
        Assert.NotNull(weldOption);

        Assert.IsType<TpWeldOptionParameters>(weldOption.Args);
        var parameters = (TpWeldOptionParameters)weldOption.Args;

        Assert.NotNull(parameters.Parameters);
        Assert.NotEmpty(parameters.Parameters);
    }

    [Theory]
    [InlineData("L P[1] 100% FINE TA_REF", false, null)]
    [InlineData("J P[2] 100% CNT100 TA_REF", false, null)]
    [InlineData("C P[3] P[4] 100% FINE TA_REF", false, null)]
    [InlineData("A P[5] P[6] 100% FINE TA_REF", false, null)]
    [InlineData("S P[7] 100% CNT50 TA_REF", false, null)]
    [InlineData("L P[1] 100% FINE TA_REF PR[1]", true, 1)]
    [InlineData("J P[2] 100% CNT100 TA_REF PR[5]", true, 5)]
    [InlineData("C P[3] P[4] 100% FINE TA_REF PR[10]", true, 10)]
    [InlineData("A P[5] P[6] 100% FINE TA_REF PR[42]", true, 42)]
    [InlineData("S P[7] 100% CNT50 TA_REF PR[100]", true, 100)]
    public void ParseMotionInstruction_WithTorchAngleOption_AddsOptionToList(
        string input,
        bool hasPositionRegister,
        int? expectedRegisterNumber)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpTorchAngleOption);
        Assert.Single(result.Options.OfType<TpTorchAngleOption>());

        var torchAngleOption = result.Options.OfType<TpTorchAngleOption>().FirstOrDefault();
        Assert.NotNull(torchAngleOption);

        if (hasPositionRegister)
        {
            Assert.NotNull(torchAngleOption.PositionRegister);
            Assert.IsType<TpPositionRegister>(torchAngleOption.PositionRegister);

            var positionRegister = torchAngleOption.PositionRegister;
            Assert.IsType<TpAccessDirect>(positionRegister.Access);

            var access = positionRegister.Access as TpAccessDirect;
            Assert.NotNull(access);
            Assert.Equal(expectedRegisterNumber, access.Number);
        }
        else
        {
            Assert.Null(torchAngleOption.PositionRegister);
        }
    }

    [Theory]
    [InlineData("L P[1] 100% FINE TA_REF PR[R[1]]")]
    [InlineData("J P[2] 100% CNT100 TA_REF PR[AR[5]]")]
    public void ParseMotionInstruction_WithTorchAngleOption_IndirectRegister_ParsesCorrectly(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpTorchAngleOption);

        var torchAngleOption = result.Options.OfType<TpTorchAngleOption>().FirstOrDefault();
        Assert.NotNull(torchAngleOption);
        Assert.NotNull(torchAngleOption.PositionRegister);
        Assert.IsType<TpPositionRegister>(torchAngleOption.PositionRegister);

        var positionRegister = torchAngleOption.PositionRegister;
        Assert.IsType<TpAccessIndirect>(positionRegister.Access);
    }

    [Theory]
    [InlineData("L P[1] 100% FINE COORD")]
    [InlineData("J P[2] 100% CNT100 COORD")]
    [InlineData("C P[3] P[4] 100% FINE COORD")]
    [InlineData("A P[5] P[6] 100% FINE COORD")]
    [InlineData("S P[7] 100% CNT50 COORD")]
    public void ParseMotionInstruction_WithCoordMotionOption_AddsOptionToList(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpCoordMotionOption);
        Assert.Single(result.Options.OfType<TpCoordMotionOption>());
    }

    [Theory]
    [InlineData("L P[1] 100% FINE EV50%", 50, false)]
    [InlineData("J P[2] 100% CNT100 EV75%", 75, false)]
    [InlineData("C P[3] P[4] 100% FINE EV100%", 100, false)]
    [InlineData("A P[5] P[6] 100% FINE EV25%", 25, false)]
    [InlineData("S P[7] 100% CNT50 EV10%", 10, false)]
    [InlineData("L P[1] 100% FINE Ind.EV50%", 50, true)]
    [InlineData("J P[2] 100% CNT100 Ind.EV75%", 75, true)]
    [InlineData("C P[3] P[4] 100% FINE Ind.EV100%", 100, true)]
    [InlineData("A P[5] P[6] 100% FINE Ind.EV25%", 25, true)]
    [InlineData("S P[7] 100% CNT50 Ind.EV10%", 10, true)]
    public void ParseMotionInstruction_WithExtendedVelocityOption_AddsOptionToList(
        string input,
        int expectedValue,
        bool expectedIsIndependent)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpExtendedVelocityOption);
        Assert.Single(result.Options.OfType<TpExtendedVelocityOption>());

        var evOption = result.Options.OfType<TpExtendedVelocityOption>().FirstOrDefault();
        Assert.NotNull(evOption);
        Assert.Equal(expectedValue, evOption.Value);
        Assert.Equal(expectedIsIndependent, evOption.IsIndependent);
    }

    [Theory]
    [InlineData("L P[1] 100% FINE FPLIN")]
    [InlineData("J P[2] 100% CNT100 FPLIN")]
    [InlineData("C P[3] P[4] 100% FINE FPLIN")]
    [InlineData("A P[5] P[6] 100% FINE FPLIN")]
    [InlineData("S P[7] 100% CNT50 FPLIN")]
    public void ParseMotionInstruction_WithFaceplateLinearOption_AddsOptionToList(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpFaceplateLinearOption);
        Assert.Single(result.Options.OfType<TpFaceplateLinearOption>());
    }

    [Theory]
    [InlineData("L P[1] 100% FINE INC")]
    [InlineData("J P[2] 100% CNT100 INC")]
    [InlineData("C P[3] P[4] 100% FINE INC")]
    [InlineData("A P[5] P[6] 100% FINE INC")]
    [InlineData("S P[7] 100% CNT50 INC")]
    public void ParseMotionInstruction_WithIncrementalOption_AddsOptionToList(string input)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Contains(result.Options, o => o is TpIncrementalOption);
        Assert.Single(result.Options.OfType<TpIncrementalOption>());
    }

    [Theory]
    [InlineData("L P[1] 100% FINE INC RTCP", typeof(TpIncrementalOption), typeof(TpRemoteTcpOption))]
    [InlineData("J P[2] 100% CNT100 PTH BREAK", typeof(TpPathOption), typeof(TpBreakOption))]
    [InlineData("C P[3] P[4] 100% FINE Wjnt Offset", typeof(TpWristJointOption), typeof(TpOffsetOption))]
    [InlineData("A P[5] P[6] 100% FINE ACC50 FPLIN", typeof(TpAccOption), typeof(TpFaceplateLinearOption))]
    [InlineData("L P[1] 100% FINE Tool_Offset COORD", typeof(TpToolOffsetOption), typeof(TpCoordMotionOption))]
    [InlineData("J P[2] 100% CNT100 EV75% RTCP Wjnt", typeof(TpExtendedVelocityOption), typeof(TpRemoteTcpOption),
        typeof(TpWristJointOption))]
    [InlineData("L P[1] 100% FINE TA_REF INC BREAK", typeof(TpTorchAngleOption), typeof(TpIncrementalOption),
        typeof(TpBreakOption))]
    [InlineData("C P[3] P[4] 100% FINE AP_LD10 COORD FPLIN", typeof(TpLinearDistanceOptionLiteral),
        typeof(TpCoordMotionOption), typeof(TpFaceplateLinearOption))]
    [InlineData("L P[1] 100% FINE Skip,LBL[5] RTCP", typeof(TpSkipOption), typeof(TpRemoteTcpOption))]
    [InlineData("A P[5] P[6] 100% CNT100 SkipJump,LBL[10] BREAK", typeof(TpSkipJumpOption), typeof(TpBreakOption))]
    [InlineData("L P[1] 100% FINE ORNT_BASE UF[1,x] RTCP", typeof(TpOrntBaseOption), typeof(TpRemoteTcpOption))]
    [InlineData("L P[1] 100% FINE Tool_Offset, PR[5] INC", typeof(TpToolOffsetOption), typeof(TpIncrementalOption))]
    [InlineData("J P[2] 100% CNT100 ORNT_BASE RTCP ACC90 PTH",
        typeof(TpOrntBaseOption), typeof(TpRemoteTcpOption), typeof(TpAccOption), typeof(TpPathOption))]
    [InlineData("L P[1] 100% FINE Offset RT_LD25 BREAK INC",
        typeof(TpOffsetOption), typeof(TpLinearDistanceOptionLiteral), typeof(TpBreakOption),
        typeof(TpIncrementalOption))]
    [InlineData("C P[3] P[4] 100% FINE Arc Start[1,2] FPLIN", typeof(TpWeldOption), typeof(TpFaceplateLinearOption))]
    [InlineData("L P[1] 100% FINE TA_REF PR[10] RTCP Ind.EV50%", typeof(TpTorchAngleOption), typeof(TpRemoteTcpOption),
        typeof(TpExtendedVelocityOption))]
    [InlineData("J P[2] 100% CNT100 Offset, PR[1] Tool_Offset, PR[2] COORD",
        typeof(TpOffsetOption), typeof(TpToolOffsetOption), typeof(TpCoordMotionOption))]
    [InlineData("L P[1] 100% FINE AP_LDR[1] SkipJump,LBL[1] ORNT_BASE UF[2,y]",
        typeof(TpLinearDistanceOptionRegister), typeof(TpSkipJumpOption), typeof(TpOrntBaseOption))]
    [InlineData("L P[1] 100% FINE INC RTCP ACC100 BREAK COORD",
        typeof(TpIncrementalOption), typeof(TpRemoteTcpOption), typeof(TpAccOption), typeof(TpBreakOption),
        typeof(TpCoordMotionOption))]
    [InlineData("L P[1:WELD START] 100.0inch/min FINE Offset,PR[101:Reserved-Welding] Arc Start[996]", typeof(TpOffsetOption), typeof(TpWeldOption))]
    public void ParseMotionInstruction_WithMultipleOptions_AddsAllOptionsToList(string input,
        params Type[] expectedOptionTypes)
    {
        var result = TpMotionInstruction.GetParser().Parse(input);

        Assert.NotNull(result);

        // Check that we have the correct number of options
        Assert.Equal(expectedOptionTypes.Length, result.Options.Count);

        // Check that each expected option type is present exactly once
        foreach (var matchingOptions in expectedOptionTypes.Select(optionType =>
                     result.Options.Where(optionType.IsInstanceOfType).ToList()))
        {
            Assert.Single(matchingOptions);
        }

        // Check that the options are in the correct order as they appear in the input
        for (var i = 0; i < expectedOptionTypes.Length; i++)
        {
            Assert.IsType(expectedOptionTypes[i], result.Options[i]);
        }
    }
}
