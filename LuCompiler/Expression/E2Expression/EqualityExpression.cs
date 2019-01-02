using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class EqualityExpression : E2Expression
    {
        public EqualityExpression(Expression e1, Expression e2, Symbol symbol)
            : base(e1,e2,symbol)
        {
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag,out string postfixStatement)
        {
            string statement1, statement2;
            string pStatement1, pStatement2;
            bool flag1, flag2;
            string value = string.Format("{1}{0}{2}",Symbol.Equals("==")?"=":"<>",
                E1.QTranslate(out statement1, out flag1, out pStatement1), E2.QTranslate(out statement2, out flag2, out pStatement2));
            interValFlag = flag1 || flag2;
            prefixStatement = statement1 + statement2;
            postfixStatement = pStatement1 + pStatement2;
            return value;
        }

        public override void Compile(Function function)
        {
            base.Compile(function);
            if ((E1.ValueType & E2.ValueType) == ValueType.None) throw new InterpretException(Context,"只有两个相同类型的表达式可以比较！");
            ValueType = ValueType.Boolean;
        }
    }
}
