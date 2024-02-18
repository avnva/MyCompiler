using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler;

public class MessageEventArgs
{
    public string Message { get; private set; }

    public MessageEventArgs(string message)
    {
        Message = message;
    }
}
