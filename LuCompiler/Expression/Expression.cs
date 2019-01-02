using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public abstract class Expression : SyntaxElement
    {
        public bool IsAssignable
        {
            get
            {
                return this is Data || this is PostfixExpression;
            }
        }
        private string _value;
        protected bool _isCompiled;
        private Expression _parent;
        private Context _context;
        protected Variable _refVar;
        protected ExpressionAttrs _expressionAttributes = ExpressionAttrs.None;

        public ExpressionAttrs ExpressionAttributes
        {
            get { return _expressionAttributes; }
            set { _expressionAttributes = value; }
        }
        public Context Context
        {
            get { return _context; }
            set { _context = value; }
        }
        public Variable RefVar
        {
            get { return _refVar; }
            set { _refVar = value; }
        }
        public Expression Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public bool IsCompiled
        {
            get { return _isCompiled; }
            set { _isCompiled = value; }
        }

        public int RowIndex
        {
            get { return _context.RowIndex; }
        }
                

        public Expression()
        {
            _context = G.TokenTaker.CurrentContext;                       
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private ValueType _valueType = ValueType.Unknown;

        public ValueType ValueType
        {
            get { return _valueType; }
            set { _valueType = value; }
        }

        public override string ToString()
        {
            return Value;
        }

        public virtual string QTranslate(out string prefixStatement,out bool interValFlag,out string postfixStatement)
        {
            prefixStatement = "";
            postfixStatement = "";
            interValFlag = false;
            return Value;
        }
        public string QTranslate(out string prefixStatement,out string postfixStatement)
        {
            bool flag;
            return QTranslate(out prefixStatement, out flag,out postfixStatement);
        }
        public string QTranslate()
        {
            bool flag;
            string prefixStatement,postfixStatement;
            return QTranslate(out prefixStatement, out flag, out postfixStatement);
        }

        public virtual void Compile(Function function)
        {
            IsCompiled = true;
        }

        public virtual void ForEachExpression(Action<Expression[]> act)
        {
            act(new Expression[] { this });
        }

        public virtual void RecursiveDo(Func<Expression, bool> act, int depth)
        {
            act(this);
        }

        public void RecursiveDo(Func<Expression, bool> act)
        {
            RecursiveDo(act, -1);
        }

        public bool HasAttr(ExpressionAttrs attr)
        {
            return _expressionAttributes.HasFlag(attr);
        }
    }
}
