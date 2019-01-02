using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class BreakStatement : InLoopStatement
    {
        private string _functionName;

        public override string ToString()
        {
            return "break;";
        }

        public override void Compile(Function function)
        {
            base.Compile(function);
            _functionName = function.FinalName;
            LoopStatement.HasBreakStatement = true;
        }

        public override string QTranslate()
        {
            switch (LoopType)
            {
                case "dowhile":
                case "while":
                case "limit":
                case "dolimit":
                    return string.Format("Goto sys_{0}_loop{1}_end : ", _functionName, LoopStatement.LoopId);
                case "for":
                    return "Exit For : ";
                default:
                    throw new InterpretException(Context,"尚不支持的类型！");
            }
        }
    }
}
