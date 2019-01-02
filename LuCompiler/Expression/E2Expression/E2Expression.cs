using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class E2Expression : Expression
    {
        private Expression _e1;
        private Expression _e2;
        private Symbol _symbol;

        public virtual Expression E1
        {
            get { return _e1; }
            set { _e1 = value; }
        }
        public virtual Expression E2
        {
            get { return _e2; }
            set { _e2 = value; }
        }
        public Symbol Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        public virtual string getCombinedValue(string value1, string value2)
        {
            return string.Format("{0}{1}{2}", value1, Symbol, value2);
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            string statement1, statement2;
            string pStatement1, pStatement2;
            bool flag1,flag2;
            string value = getCombinedValue(E1.QTranslate(out statement1, out flag1, out pStatement1), E2.QTranslate(out statement2, out flag2, out pStatement2));
            interValFlag = flag1 || flag2;
            prefixStatement = statement1 + statement2;
            postfixStatement = pStatement1 + pStatement2;
            return value;
        }

        public E2Expression(Expression e1, Expression e2,Symbol symbol) : base()
        {
            E1 = e1;
            E2 = e2;
            E1.Parent = this;
            E2.Parent = this;
            Symbol = symbol;
            Value = getCombinedValue(e1.Value, e2.Value);
        }

        public override void Compile(Function function)
        {
            _e1.Compile(function);
            _e2.Compile(function);
            IsCompiled = true;            
        }

        public override void RecursiveDo(Func<Expression, bool> act, int depth)
        {
            if (!act(this) || depth == 0) return;
            E1.RecursiveDo(act, depth - 1);
            E2.RecursiveDo(act, depth - 1);
        }
    }
}
