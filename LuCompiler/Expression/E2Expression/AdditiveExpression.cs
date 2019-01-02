using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class AdditiveExpression : E2Expression
    {
        private bool _isStringConcat = false;
        public AdditiveExpression(Expression e1, Expression e2, Symbol symbol)
            : base(e1,e2,symbol)
        {
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            if (!_isStringConcat)
                return base.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
            string statement1, statement2;
            string pStatement1, pStatement2;
            bool flag1, flag2;
            string value = string.Format("{0} & {1}", E1.QTranslate(out statement1, out flag1, out pStatement1), E2.QTranslate(out statement2, out flag2, out pStatement2));
            interValFlag = flag1 || flag2;
            prefixStatement = statement1 + statement2;
            postfixStatement = pStatement1 + pStatement2;
            return value;
        }

        public override void Compile(Function function)
        {
            base.Compile(function);
            if (_isStringConcat = Symbol.Equals("+") && E1.ValueType == ValueType.String 
                || E2.ValueType==ValueType.String)
            {
                ValueType = ValueType.String;
                return;
            }

            if ((E1.ValueType & ValueType.Numeric) == ValueType.Numeric
                && (E2.ValueType & ValueType.Numeric) == ValueType.Numeric)
            {                
                ValueType = ValueType.Numeric;
            }
            else throw new InterpretException(Context,"加减号的两端必须是数字类型！");
        }
    }
}
