using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.States;

public class FirstOperatorNameState : IState
{
    private List<ParserError> errors;
    private StringHelper stringHelper;
    private Dictionary<StatesType, IState> StateMap;

    public FirstOperatorNameState(List<ParserError> errors, StringHelper stringHelper, Dictionary<StatesType, IState> StateMap)
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

        char currentSymbol = stringHelper.Current;
        bool IsNotFirstSymbol = false;

        ParserError error = new ParserError("Ожидался оператор", stringHelper.Index + 1, stringHelper.Index + 1);
        while (!stringHelper.isSpace(currentSymbol))
        {
            if (!stringHelper.CanGetNext)
            {
                if (error.Value != string.Empty)
                    errors.Add(error);
                errors.Add(new ParserError("Обнаружено незаконченное выражение", stringHelper.Index, stringHelper.Index, ErrorType.UnfinishedExpression));
                return false;
            }

            currentSymbol = stringHelper.Current;

            if (char.IsLetter(currentSymbol) || (char.IsDigit(currentSymbol) || currentSymbol == '_') && IsNotFirstSymbol)
            {
                IsNotFirstSymbol = true;
                if (error.Value != string.Empty)
                    errors.Add(error);
                error = new ParserError("Ожидался оператор", stringHelper.Index + 1, stringHelper.Index + 1);
            }
            else
            {
                error.Value += currentSymbol;
                error.EndIndex = stringHelper.Index + 1;
            }
            currentSymbol = stringHelper.Next;
        }

        StateMap[StatesType.ArithmeticOperator].Handle();
        return true;
    }
}
