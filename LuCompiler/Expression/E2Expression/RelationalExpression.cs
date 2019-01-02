using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class RelationalExpression : E2Expression
    {
        public RelationalExpression(Expression e1, Expression e2, Symbol symbol)
            : base(e1,e2,symbol)
        {
        }

        public override void Compile(Function function)
        {
            base.Compile(function);
            if ((E1.ValueType & E2.ValueType) == ValueType.None) throw new InterpretException(Context,"只有两个相同类型的表达式可以比较！");
            ValueType = ValueType.Boolean;
        }
    }
}
