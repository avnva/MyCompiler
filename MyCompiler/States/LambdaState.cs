using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.States;

public class LambdaState : IState
{
    private List<ParserError> errors;
    private StringHelper stringHelper;
    private Dictionary<StatesType, IState> stateMap;

    public LambdaState(List<ParserError> errors, StringHelper stringHelper, Dictionary<StatesType, IState> StateMap)
    {
        this.errors = errors;
        this.stringHelper = stringHelper;
        stateMap = StateMap;
    }

    public bool Handle()
    {
        stringHelper.SkipSpaces();

        if (!stringHelper.CanGetNext)
            return true;

        foreach (char c in "lambda")
        {
            ParserError error = new ParserError("Ожидалось ключевое слово \"lambda\"", stringHelper.Index + 1, stringHelper.Index + 1);
            while (true)
            {
                if (!stringHelper.CanGetNext)
                {
                    if (error.Value != string.Empty)
                        errors.Add(error);
                    errors.Add(new ParserError("Обнаружено незаконченное выражение", stringHelper.Index, stringHelper.Index, ErrorType.UnfinishedExpression));
                    return false;
                }
                char currentSymbol = stringHelper.Current;

                if (currentSymbol == c)
                {
                    if (error.Value != string.Empty)
                        errors.Add(error);
                    currentSymbol = stringHelper.Next;
                    break;
                }
                else
                {
                    error.Value += currentSymbol;
                    error.EndIndex = stringHelper.Index + 1;
                }
                currentSymbol = stringHelper.Next;
            }
        }

        stateMap[StatesType.FirstArgument].Handle();
        return true;
    }
}