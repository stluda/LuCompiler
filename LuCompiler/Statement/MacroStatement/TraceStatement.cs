using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class TraceStatement : MacroStatement
    {
        private PostfixExpression _pe;       

        public TraceStatement(PostfixExpression pe)
        {
            _pe = pe;
        }

        public override string ToString()
        {
            return string.Format("{0};",_pe);
        }        

        public override void Compile(Function function)
        {
            Expression postE = _pe.PostE;
            bool exflag = false;
            int debugLevel = 0;
            Expression e = null;
            if (!(postE is CommaExpression))
            {
                e = postE;
            }
            else if (!exflag)
            {
                CommaExpression cma = postE as CommaExpression;
                int argCount;
                if (M.SetExflag((argCount = cma.Expressions.Count) != 2, ref exflag)) Dict.AddException(new InterpretException(Context
                           , "trace语句参数个数不正确"));
                Expression e1 = cma.Expressions[0];
                e = cma.Expressions[1];
                if (M.SetExflag(!int.TryParse(e1.Value, out debugLevel), ref exflag)) Dict.AddException(new InterpretException(Context
                     , "不是有效的数字类型"));
            }

            if (!exflag)
            {
                AdditiveExpression add1 = new AdditiveExpression(new Data(string.Format("\"[d{0}]\"", debugLevel))
                , e, Symbol.GetInstance("+", typeof(AdditiveExpression)));
                AdditiveExpression add2 = new AdditiveExpression(new Data(string.Format("\"[d{0}]\"", debugLevel))
                , e, Symbol.GetInstance("+", typeof(AdditiveExpression)));


                Block block = new Block();
                block.Add(new SingleStatement(
                        new PostfixExpression(
                            new Data("$Traceprint"), Symbol.GetInstance("(", typeof(PostfixExpression)),
                            add1/*e*/)));
                CommaExpression c = new CommaExpression(new Data("0"));
                c.Add(add2/*e*/);
                if(G.DoesWriteTraceprintToLog)
                    block.Add(new SingleStatement(
                        new PostfixExpression(
                            new Data("$writelog"), Symbol.GetInstance("(", typeof(PostfixExpression)),
                            c)));

                _result = new DebugStatement(debugLevel, block);
                _result.Compile(function);
            }  
        }

    }
}
