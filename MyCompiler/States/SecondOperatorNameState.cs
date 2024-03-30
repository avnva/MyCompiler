using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.States;

public class SecondOperatorNameState : IState
{
    private List<ParserError> errors;
    private StringHelper stringHelper;
    private Dictionary<StatesType, IState> StateMap;

    public SecondOperatorNameState(List<ParserError> errors, StringHelper stringHelper, Dictionary<StatesType, IState> StateMap)
    {
        this.errors = errors;
        this.stringHelper = stringHelper;
        this.StateMap = StateMap;
    }

    public bool Handle()
    {
        stringHelper.SkipSpaces();

        if (!stringHelper.CanGetCurrent)
        {
            errors.Add(new ParserError("Обнаружено незаконченное выражение", stringHelper.Index, stringHelper.Index, ErrorType.UnfinishedExpression));
            return false;
        }
        bool IsNotFirstSymbol = false;

        while (stringHelper.Current != ';')
        {
            ParserError error = new ParserError("Ожидался второй оператор или оператор конца выражения \";\"", stringHelper.Index + 1, stringHelper.Index + 1);
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

                if ((char.IsLetter(currentSymbol) || (char.IsDigit(currentSymbol) || currentSymbol == '_') && IsNotFirstSymbol) || currentSymbol == ';')
                {
                    IsNotFirstSymbol = true;
                    if (error.Value != string.Empty)
                        errors.Add(error);
                    if (currentSymbol != ';')
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

        StateMap[StatesType.Semicolon].Handle();
        return true;
    }
}
