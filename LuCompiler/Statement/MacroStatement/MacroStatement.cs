using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class MacroStatement : Statement
    {
        protected Statement _result;

        public override string QTranslate()
        {
            return _result.QTranslate();
        }
    }
}
