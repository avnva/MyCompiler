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
    Сolon = 4,
    FirstArgument = 5,
    Comma = 6,
    SecondArgument = 8,
    FirstOperator = 9,
    ArithmeticOperator =10,
    SecondOperator = 11,
    Semicolon = 12
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
        StateMap.Add(StatesType.FirstArgument, new FirstArgState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.Comma, new CommaState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.SecondArgument, new SecondArgState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.Сolon, new WhitespaceState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.FirstOperator, new FirstOperatorNameState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.ArithmeticOperator, new ArithmeticOperatorState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.SecondOperator, new SecondOperatorNameState(Errors, StringHelper, StateMap));
        StateMap.Add(StatesType.Semicolon, new SemicolonState(Errors, StringHelper, StateMap));

    }
}