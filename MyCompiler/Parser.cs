using Microsoft.VisualBasic;
using MyCompiler.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace MyCompiler;

public enum StatesType
{
    Identifier = 1,
    AssignmentOperator = 2,
    Lambda = 3,
    Whitespace = 4,
    Arguments = 5,
    FirstOperator = 6,
    ArithmeticOperator =7,
    SecondOperator = 8,
    Semicolon = 9
}
public class Parser
{
    public Dictionary<StatesType, IState> StateMap;
    public StringHelper StringHelper { get; set; }
    public List<ParserError> Errors { get; set; }

    public List<ParserError> Parse(string text = "")
    {
        Errors.Clear();
        StringHelper.Source = text;

        StateMap[StatesType.Identifier].Handle();
        return Errors;
    }

    public Parser(string text)
    {
        Errors = new List<ParserError>();
        StringHelper = new StringHelper(text);
        StateMap = new Dictionary<StatesType, IState>();

        StateMap.Add(StatesType.Identifier, new IdState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.AssignmentOperator, new AssignmentOperatorState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.Lambda, new LambdaState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.Whitespace, new WhitespaceState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.Arguments, new ArgState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.FirstOperator, new FirstOperatorNameState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.ArithmeticOperator, new ArithmeticOperatorState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.SecondOperator, new SecondOperatorNameState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.Semicolon, new SemicolonState(Errors, StringHelper, StateMap));

    }
}