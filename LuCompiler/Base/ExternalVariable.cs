using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class ExternalVariable : Variable
    {
        private string _finalName;

        public override string FinalName
        {
            get
            {
                return _finalName;
            }
        }

        public ExternalVariable(string name, Context context)
            : base("super", name, context)
        {
            Name = Name.TrimStart('$');
            _finalName = G.KernalNames.Contains(Name) ? M.EVar(Name) : Name;
        }
    }
}
