using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.States;

public class SemicolonState : IState
{
    private List<ParserError> errors;
    private StringHelper stringHelper;
    private Dictionary<StatesType, IState> StateMap;

    public SemicolonState(List<ParserError> errors, StringHelper stringHelper, Dictionary<StatesType, IState> StateMap)
    {
        this.errors = errors;
        this.stringHelper = stringHelper;
        this.StateMap = StateMap;
    }

    public bool Handle()
    {
        if (stringHelper.CanGetNext)
        {
            _ = stringHelper.Next;
            StateMap[StatesType.Lambda].Handle();
        }
        else
        {
            return false;
        }
        return true;
    }
}
