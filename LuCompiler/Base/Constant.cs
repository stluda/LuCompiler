using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class ConstantVariable : Variable
    {
        private string _value;

        public override string FinalName
        {
            get
            {
                return _value;
            }
        }

        public ConstantVariable(string type, string name, Context context,string value) : base(type,name,context)
        {
            _value = value;
        }
    }
}
