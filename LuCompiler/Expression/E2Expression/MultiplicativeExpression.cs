using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class MultiplicativeExpression : E2Expression
    {
        public override string getCombinedValue(string value1, string value2)
        {
            if (E1 is AdditiveExpression) value1 = string.Format("({0})", value1);
            if (E2 is AdditiveExpression) value2 = string.Format("({0})", value2);
            return base.getCombinedValue(value1, value2);
        }

        public MultiplicativeExpression(Expression e1, Expression e2, Symbol symbol) 
            : base(e1,e2,symbol)
        {        
        }

        public override void Compile(Function function)
        {
            base.Compile(function);

            if ((E1.ValueType & ValueType.Numeric) == ValueType.Numeric
                && (E2.ValueType & ValueType.Numeric) == ValueType.Numeric)
            {
                ValueType = ValueType.Numeric;
            }
            else throw new InterpretException(Context,"乘除号的两端必须是数字类型！");
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            if (!Symbol.Equals("%"))
                return base.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
            else
            {
                string statement1, statement2;
                string pStatement1, pStatement2;
                bool flag1, flag2;
                string value1 = E1.QTranslate(out statement1, out flag1, out pStatement1);
                string value2 = E2.QTranslate(out statement2, out flag2, out pStatement2);
                string value = string.Format("({0}) mod {1}", value1, value2);
                interValFlag = flag1 || flag2;
                prefixStatement = statement1 + statement2;
                postfixStatement = pStatement1 + pStatement2;
                return value;
            }
        }
    }
}
