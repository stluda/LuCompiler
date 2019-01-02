using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Main : Function
    {
        public Main(Context context) : base("main", null,context) { }
        public override Hashtable<string, Variable> LocalVariables
        {
            get
            {
                return G.GlobalVariables;
            }
        }
    }
}
