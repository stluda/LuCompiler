using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class ContinueStatement : InLoopStatement
    {
        private string _functionName;
        

        public override void Compile(Function function)
        {
            base.Compile(function);
            LoopStatement.HasContinueStatement = true;
            _functionName = function.FinalName;
        }

        public override string QTranslate()
        {            
            switch (LoopStatement.Keyword)
            {
                case "dowhile":
                case "dolimit":
                    return string.Format("Goto sys_{0}_loop{1}_next : ", _functionName, LoopStatement.LoopId);  
                case "while":
                    return string.Format("Goto sys_{0}_loop{1}_start : ", _functionName, LoopStatement.LoopId);   
                case "limit":
                    return string.Format("sys_{0}_loop{1}_limitIndex = sys_{0}_loop{1}_limitIndex + 1 : Goto sys_{0}_loop{1}_start : ", _functionName, LoopStatement.LoopId);                
                case "for":
                    return "Exit For : ";
                default:
                    throw new InterpretException(Context,"尚不支持的类型！");
            }
        }

    }
}
