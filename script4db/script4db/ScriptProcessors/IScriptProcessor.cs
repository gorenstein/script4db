using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db.ScriptProcessors
{
    interface IScriptProcessor
    {
        string TextRaw { get; set; }
        Blocks Blocks { get; }
        ArrayList LogMessages { get; }
        ArrayList ConnectionsStrings();
    }
}
