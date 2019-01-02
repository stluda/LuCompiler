using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class ReturnStatement : Statement
    {
        private string _q_exitType = "Function";
        private Expression _returnExpression;
        private Function _function;

        public Expression ReturnExpression
        {
            get { return _returnExpression; }
            set { _returnExpression = value; }
        }

        public ReturnStatement()
        {
        }

        public ReturnStatement(Expression returnExpression)
        {
            _returnExpression = returnExpression;
        }

        public override void Compile(Function function)
        {
            _function = function;
            if (_returnExpression != null)
            {
                _returnExpression.Compile(function);
                function.Type = _returnExpression.ValueType;
            }
        }

        public override string QTranslate()
        {
            if(_function.Type == ValueType.None)_q_exitType = "Sub";
            string prefix = "", postfix = "";
            string value="";
            if (_returnExpression != null)
            {
                value = _returnExpression.QTranslate(out prefix, out postfix);
                if(G.IsDebugMode)
                    value = string.Format("lu_{0} = {1} :", _function.Name, value);
                else
                    value = string.Format("{0} = {1} :", _function.Mixcode, value);
            }
            if (prefix != "") prefix += "\r\n";
            if (postfix != "") postfix = "\r\n" + postfix;            
            return prefix + value + postfix + _function.GetDebugInfo_EndFunction +  string.Format("Exit {0} : ",_q_exitType);
        }
    }
}
