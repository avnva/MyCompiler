using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.States;

public class ArgState : IState
{
    private List<ParserError> errors;
    private StringHelper stringHelper;
    private Dictionary<StatesType, IState> StateMap;

    public ArgState(List<ParserError> errors, StringHelper stringHelper, Dictionary<StatesType, IState> StateMap)
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
        bool IsArgExists = false;
        bool IsCommaMet = false;

        ParserError error = new ParserError("Ожидался аргумент", stringHelper.Index + 1, stringHelper.Index + 1);
        while (!stringHelper.isSpace(currentSymbol) || currentSymbol != ',')
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
                error = new ParserError("Ожидался аргумент", stringHelper.Index + 1, stringHelper.Index + 1);
            }
            else
            {
                error.Value += currentSymbol;
                error.EndIndex = stringHelper.Index + 1;
            }
            currentSymbol = stringHelper.Next;
        }
        
        if (currentSymbol == ',')
        {
            if (!stringHelper.CanGetNext)
            {
                if (error.Value != string.Empty)
                    errors.Add(error);
                errors.Add(new ParserError("Обнаружено незаконченное выражение", stringHelper.Index, stringHelper.Index, ErrorType.UnfinishedExpression));
                return false;
            }
            stringHelper.SkipSpaces();
            while (!stringHelper.isSpace(currentSymbol) || currentSymbol != ',')
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
                    error = new ParserError("Ожидался аргумент", stringHelper.Index + 1, stringHelper.Index + 1);
                }
                else
                {
                    error.Value += currentSymbol;
                    error.EndIndex = stringHelper.Index + 1;
                }
                currentSymbol = stringHelper.Next;
            }
        }

        StateMap[StatesType.FirstOperator].Handle();
        return true;
    }
}
