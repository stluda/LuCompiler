using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class ExternalExpression : Expression
    {

        public ExternalExpression(string expressionString)
        {
            Context = Context.Empty;
            Value = expressionString;
        }

        public override void Compile(Function function)
        {
            this.ValueType = ValueType.Super;
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            base.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
            return Value;
        }


    }
}
