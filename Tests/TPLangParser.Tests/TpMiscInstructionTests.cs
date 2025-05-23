using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.Tests;

public class TpMiscInstructionTests
{
    [Theory]
    [InlineData("RSR[1] ENABLE", 1, true)]
    [InlineData("RSR[10] DISABLE", 10, false)]
    public void Parse_RsrInstruction_ParsesCorrectly(string input, int expectedAccess, bool expectedEnable)
    {
        var result = TpMiscInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpRsrInstruction>(result);

        var rsrInst = (TpRsrInstruction)result;
        Assert.Equal(expectedAccess, ((TpAccessDirect)rsrInst.Access).Number);
        Assert.Equal(expectedEnable, rsrInst.Enable);
    }

    [Theory]
    [InlineData("UALM[1]", 1)]
    [InlineData("UALM[100]", 100)]
    public void Parse_UserAlarmInstruction_ParsesCorrectly(string input, int expectedAccess)
    {
        var result = TpMiscInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpUserAlarmInstruction>(result);

        var ualmInst = (TpUserAlarmInstruction)result;
        Assert.Equal(expectedAccess, ((TpAccessDirect)ualmInst.Access).Number);
    }

    [Theory]
    [InlineData("TIMER[1]=START", 1, TpTimerAction.Start)]
    [InlineData("TIMER[5]=STOP", 5, TpTimerAction.Stop)]
    [InlineData("TIMER[10]=RESET", 10, TpTimerAction.Reset)]
    public void Parse_TimerInstruction_ParsesCorrectly(string input, int expectedAccess, TpTimerAction expectedAction)
    {
        var result = TpMiscInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpTimerInstruction>(result);

        var timerInst = (TpTimerInstruction)result;
        Assert.Equal(expectedAccess, ((TpAccessDirect)timerInst.Access).Number);
        Assert.Equal(expectedAction, timerInst.Action);
    }

    [Theory]
    [InlineData("OVERRIDE=50%", 50)]
    [InlineData("OVERRIDE=100%", 100)]
    [InlineData("OVERRIDE=25%", 25)]
    public void Parse_OverrideDirectInstruction_ParsesCorrectly(string input, int expectedValue)
    {
        var result = TpMiscInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var inst = Assert.IsType<TpOverrideDirect>(result);

        Assert.Equal(expectedValue, inst.Value);
    }

    [Theory]
    [InlineData("OVERRIDE=R[187:OverrideMemory]", typeof(TpRegister))]
    [InlineData("OVERRIDE=AR[1]", typeof(TpArgumentRegister))]
    public void Parse_OverrideIndirectInstruction_ParsesCorrectly(string input, Type expectedRegisterType)
    {
        var result = TpMiscInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var inst = Assert.IsType<TpOverrideIndirect>(result);
        Assert.IsType(expectedRegisterType, inst.Register.Register);

    }

    [Theory]
    [InlineData("MESSAGE[Test Message]", "Test Message")]
    [InlineData("MESSAGE[Hello World]", "Hello World")]
    [InlineData("MESSAGE[Error: Code 123]", "Error: Code 123")]
    [InlineData("MESSAGE[Something R[1]]", "Something R[1]")]
    [InlineData("MESSAGE[TCP_CALIB 1&9 Failed]", "TCP_CALIB 1&9 Failed")]
    public void Parse_MessageInstruction_ParsesCorrectly(string input, string expectedMessage)
    {
        var result = TpMiscInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMessageInstruction>(result);

        var messageInst = (TpMessageInstruction)result;
        Assert.Equal(expectedMessage, messageInst.Message);
    }

    [Theory]
    [InlineData("JOINT_MAX_SPEED[1]=100", 1)]
    [InlineData("JOINT_MAX_SPEED[1]=R[1]", 1)]
    [InlineData("JOINT_MAX_SPEED[1]=1.5", 1)]
    public void Parse_JointMaxSpeedInstruction_ParsesCorrectly(string input, int expectedAccess)
    {
        var result = TpMiscInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpJointMaxSpeedInstruction>(result);

        var speedInst = (TpJointMaxSpeedInstruction)result;
        Assert.Equal(expectedAccess, ((TpAccessDirect)speedInst.Access).Number);
        Assert.NotNull(speedInst.Value);
    }

    [Theory]
    [InlineData("LINEAR_MAX_SPEED[1]=100", 1)]
    [InlineData("LINEAR_MAX_SPEED[1]=R[1]", 1)]
    [InlineData("LINEAR_MAX_SPEED[1]=1.5", 1)]
    public void Parse_LinearMaxSpeedInstruction_ParsesCorrectly(string input, int expectedAccess)
    {
        var result = TpMiscInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpLinearMaxSpeedInstruction>(result);

        var speedInst = (TpLinearMaxSpeedInstruction)result;
        Assert.Equal(expectedAccess, ((TpAccessDirect)speedInst.Access).Number);
        Assert.NotNull(speedInst.Value);
    }

    [Theory]
    [InlineData("RSR[-1] ENABLE")] // Negative access number
    [InlineData("RSR[1] UNKNOWN")] // Invalid state
    [InlineData("TIMER[1]=PAUSE")] // Invalid timer action
    [InlineData("OVERRIDE=-1%")] // Negative override
    [InlineData("JOINT_MAX_SPEED[1]=ABC")] // Invalid speed value
    public void Parse_InvalidMiscInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpMiscInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("RSR[1]  ENABLE")] // Extra space
    [InlineData("TIMER[1] = START")] // Spaces around equals
    [InlineData("OVERRIDE  =  50%")] // Multiple spaces
    [InlineData("MESSAGE[  Test  ]")] // Spaces in message
    public void Parse_MiscInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpMiscInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("rsr[1] ENABLE")] // Lowercase command
    [InlineData("Timer[1]=START")] // Mixed case
    [InlineData("OVERRIDE=50%")] // Correct case
    [InlineData("Message[Test]")] // Mixed case
    public void Parse_MiscInstruction_CaseInsensitive_ThrowsParseException(string input)
    {
        if (input.ToUpper() != input)
        {
            Assert.Throws<ParseException>(() => TpMiscInstruction.GetParser().Parse(input));
        }
        else
        {
            var result = TpMiscInstruction.GetParser().Parse(input);
            Assert.NotNull(result);
        }
    }

    [Theory]
    [InlineData("$PARAM[1]=100")] // Parameter write
    [InlineData("R[1]=$PARAM[2]")] // Parameter read
    public void Parse_ParameterInstruction_ParsesCorrectly(string input)
    {
        var result = TpMiscInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.True(result is TpParameterWriteInstruction || result is TpParameterReadInstruction);
    }
}
