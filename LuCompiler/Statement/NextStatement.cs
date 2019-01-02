using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class NextStatement : Statement
    {
        private string _moduleName;
        private int _refIndex;
        public NextStatement(string moduleName)
        {
            _moduleName = moduleName;            
        }

        public override void Compile(Function function)
        {
            base.Compile(function);
            if (!(function is Module)) throw new InterpretException(Context,"只有模块里可以使用next语句");
            if (!G.ModuleIndexOfName.ContainsKey(_moduleName)) throw new InterpretException(Context,string.Format("不存在名为'{0}'的模块", _moduleName));
            _refIndex = G.ModuleIndexOfName[_moduleName];
        }

        public override string QTranslate()
        {
            return string.Format("{0} = {1} : ", M.EVar("g_cpu_iPtr"), _refIndex - 1);
        }
    }
}
