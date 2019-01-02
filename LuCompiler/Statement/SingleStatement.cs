using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class SingleStatement : Statement
    {
        private Expression _assignmentExpression;
        private bool _isFunctionCall = false;

        public SingleStatement(Expression exp)
        {
            _assignmentExpression = exp;
        }

        public override string ToString()
        {
            return _assignmentExpression + ";";
        }

        public override string QTranslate()
        {
            bool flag;
            string prefix="", value,postfix="";
            if (_assignmentExpression is PostfixExpression)
            {                
                PostfixExpression pe = _assignmentExpression as PostfixExpression;
                if (pe.IsMacro) return pe.QTranslate();
                //string partialPrefix, partialPostfix, a, b;
                switch (pe.Symbol.Value)
                {
                    case "(":
                        bool temp;
                        string ret = pe.QTranslate(out prefix, out temp, out postfix, true);
                        return prefix + ret + ":" + postfix+" : ";
                    case "<":
                        return pe.QTranslate() + " : "; 
                    case "++":
                    case "--":
                        return string.Format("{0}={0}{1}1 : ", pe.E.QTranslate(), pe.Symbol.Value[0]);
                    default:
                        throw new InterpretException(Context, "无法识别的后置表达式{0}",pe);
                }
            }

            value = _assignmentExpression.QTranslate(out prefix, out flag, out postfix);
            if (_assignmentExpression is AssignmentExpression)
            {          
                return prefix+postfix;
            }
            else if (_assignmentExpression is UnaryExpression)
            {
                UnaryExpression unary = _assignmentExpression as UnaryExpression;
                return string.Format("{0}={0}{1}1 : ", unary.E.QTranslate(), unary.Symbol.Value[0]);
            }
            else if (_assignmentExpression is Data)
            {
                return string.Format("{0} : {1}", value.TrimEnd("()".ToCharArray()), postfix);
            }
            else if (_assignmentExpression is DotExpression)
            {
                bool temp;
                DotExpression de = _assignmentExpression as DotExpression;
                string ret = de.QTranslate(out prefix, out temp, out postfix,true);
                return prefix + ret + ":" + postfix + " : ";
            }
            throw new NotSupportedException();
        }

        public override void Compile(Function function)
        {
            InterpretException ie=null;
            try
            {
                _assignmentExpression.Compile(function);
            }
            catch (InterpretException e)
            {
                Dict.AddException(e);
                return;
            }
            Expression checkExpression;
            checkExpression = (_assignmentExpression is DotExpression) ?
                (_assignmentExpression as DotExpression).GetLastMember() : _assignmentExpression;
            if (checkExpression is UnaryExpression)
            {
                UnaryExpression unary = checkExpression as UnaryExpression;
                if (!unary.Symbol.Equals("++") && !unary.Symbol.Equals("--"))ie=new InterpretException(Context,"只有函数调用表达式，自增表达式，赋值表达式可以作为语句！");
            }
            else if (checkExpression is Data)
            {
                ie = new InterpretException(Context, string.Format("{0}缺少'()'",checkExpression));
                /*
                Data data = checkExpression as Data;
                _isFunctionCall = data.RefVar is Function;
                ExpressionAttributes flags = data.ExpressionAttributes;
                if (!flags.HasFlag(ExpressionAttributes.Function)&&!flags.HasFlag(ExpressionAttributes.External))
                {
                    ie = new InterpretException(Context, "只有函数调用表达式，自增表达式，赋值表达式可以作为语句！");
                }*/
            }
            else if (checkExpression is PostfixExpression)
            {
                PostfixExpression pe = checkExpression as PostfixExpression;
                if (pe.Symbol.Equals("++") || pe.Symbol.Equals("--") || pe.Symbol.Equals("(") || pe.Symbol.Equals("<"))
                {

                }
                else ie = new InterpretException(Context, "只有函数调用表达式，自增表达式，赋值表达式可以作为语句！");                
            }
            else if (!(checkExpression is AssignmentExpression)) ie = new InterpretException(Context, "只有函数调用表达式，自增表达式，赋值表达式可以作为语句！");
            if (ie != null) Dict.AddException(ie);
        }
    }
}
