using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class LogStatement : MacroStatement
    {
        private PostfixExpression _pe;

        public LogStatement(PostfixExpression pe)
        {
            _pe = pe;
        }

        public override string ToString()
        {
            return string.Format("{0};", _pe);
        }

        public override void Compile(Function function)
        {
            Expression postE = _pe.PostE;
            int logLevel = 0;
            Expression e = null;
            bool exflag = false;
            CommaExpression c=null;
            if (!(postE is CommaExpression))
            {
                e = postE;
                c = new CommaExpression(new Data("0"));
                c.Add(e);
            }
            else if (!exflag)
            {
                CommaExpression cma = postE as CommaExpression;
                int argCount;
                if (M.SetExflag((argCount = cma.Expressions.Count) != 2, ref exflag)) Dict.AddException(new InterpretException(Context
                           , "log语句参数个数不正确"));
                Expression e1 = cma.Expressions[0];
                e = cma.Expressions[1];
                if (M.SetExflag(!int.TryParse(e1.Value, out logLevel), ref exflag)) Dict.AddException(new InterpretException(Context
                     , "不是有效的数字类型"));
                c = cma;
            }

            if (!exflag)
            {
                _result = new SingleStatement(
                        new PostfixExpression(
                            new Data("$writelog"), Symbol.GetInstance("(", typeof(PostfixExpression)),
                             c));
                _result.Compile(function);
            }  
        }
    }
}
