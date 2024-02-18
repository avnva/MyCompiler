using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler;

public class HTMLManager
{
    public static void OpenInBrowser(string filePath)
    {
        if (System.IO.File.Exists(filePath))
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(filePath) { UseShellExecute = true };
            p.Start();
        }
    }
}
