using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.IO;

namespace MyCompiler
{
    internal class XshdReader : XshdSyntaxDefinition
    {
        private Stream stream;

        public XshdReader(Stream stream)
        {
            this.stream = stream;
        }
    }
}