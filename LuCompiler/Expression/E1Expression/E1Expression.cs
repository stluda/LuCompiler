using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class E1Expression : Expression
    {
        private Expression _e;
        private Symbol _symbol;

        public virtual Expression E
        {
            get { return _e; }
            set { _e = value; }
        }
        public Symbol Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        public E1Expression(Expression e,Symbol symbol) : base() 
        {
            E = e;
            Symbol = symbol;
            e.Parent = this;
        }

        public override void RecursiveDo(Func<Expression, bool> act, int depth)
        {
            if (!act(this)||depth == 0) return;
            E.RecursiveDo(act, depth - 1);
        }
    }
}
