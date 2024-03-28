using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.States;

public class WhitespaceState : IState
{
    private List<ParserError> errors;
    private StringHelper stringHelper;
    private Dictionary<StatesType, IState> StateMap;

    public WhitespaceState(List<ParserError> errors, StringHelper stringHelper, Dictionary<StatesType, IState> StateMap)
    {
        this.errors = errors;
        this.stringHelper = stringHelper;
        this.StateMap = StateMap;
    }

    public bool Handle()
    {
        //stringHelper.SkipSpaces();
        char currentSymbol;
        bool IsWhitespaceMet = false;

        ParserError error = new ParserError("Ожидался пробел или двоеточие", stringHelper.Index + 1, stringHelper.Index + 1);
        while (true)
        {
            if (!stringHelper.CanGetNext)
            {
                if (error.Value != string.Empty)
                    errors.Add(error);
                errors.Add(new ParserError("Обнаружено незаконченное выражение", stringHelper.Index, stringHelper.Index, ErrorType.UnfinishedExpression));
                return false;
            }

            currentSymbol = stringHelper.Current;

            if (currentSymbol == ' ')
            {
                IsWhitespaceMet = true;
                if (error.Value != string.Empty)
                    errors.Add(error);
                stringHelper.SkipSpaces();
                error = new ParserError("Ожидался аргумент или двоеточие", stringHelper.Index + 2, stringHelper.Index + 1);
            }
            else if (currentSymbol == ':')
            {
                IsWhitespaceMet = false;
                if (error.Value != string.Empty)
                    errors.Add(error);
                if (stringHelper.CanGetNext)
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
            StateMap[StatesType.Identifier].Handle();
            //StateMap[LexemeType.FirstOperatorNameState].Handle();
        return true;
    }
}
