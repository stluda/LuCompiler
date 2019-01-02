using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class ConditionalExpression : Expression
    {
        private Expression _e1;
        private Expression _e2;
        private Expression _e3;

        public Expression E1
        {
            get { return _e1; }
            set { _e1 = value; }
        }
        public Expression E2
        {
            get { return _e2; }
            set { _e2 = value; }
        }
        public Expression E3
        {
            get { return _e3; }
            set { _e3 = value; }
        }

        public ConditionalExpression(Expression e1, Expression e2, Expression e3)
            : base()
        {
            E1 = e1;
            E2 = e2;
            E3 = e3;
            e1.Parent = e2.Parent = e3.Parent = this;
            Value = string.Format("{0}?{1}:{2}", e1, e2, e3);
        }

        public override void RecursiveDo(Func<Expression, bool> act, int depth)
        {
            if (!act(this) || depth == 0) return;
            E1.RecursiveDo(act, depth - 1);
            E2.RecursiveDo(act, depth - 1);
            E3.RecursiveDo(act, depth - 1);
        }
    }
}
