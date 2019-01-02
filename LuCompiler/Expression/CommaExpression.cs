using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class CommaExpression : Expression
    {
        List<Expression> _expressions = new List<Expression>();

        public List<Expression> Expressions
        {
            get { return _expressions; }
        }
        public string ValueWithQuote()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Expression e in _expressions)
            {
                sb.Append("\"").Append(e).Append("\",");
            }
            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        public void Add(Expression e)
        {
            _expressions.Add(e);
            e.Parent = this;
            Value += "," + e.Value;
        }

        public CommaExpression(Expression e):base()
        {            
            _expressions.Add(e);
            Value = e.Value;
            e.Parent = this;
        }

        public static CommaExpression Parse(Context context,params string[] args)
        {
            CommaExpression ret = new CommaExpression(new Data(args[0]) { Context = context });
            for (int i = 1, length = args.Length; i < length; i++) ret.Add(new Data(args[i]) {Context = context });
            return ret;
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            string prefix,postfix;
            bool flag;
            interValFlag = false;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            foreach (Expression e in _expressions)
            {
                sb.Append(e.QTranslate(out prefix,out flag,out postfix)).Append(",");
                sb2.Append(prefix);
                sb3.Append(postfix);
                interValFlag = interValFlag || flag;
            }
            sb.Remove(sb.Length - 1, 1);
            prefixStatement = sb2.ToString();
            postfixStatement = sb3.ToString();
            return sb.ToString();
        }

        public override void Compile(Function function)
        {
            foreach (Expression e in _expressions) e.Compile(function);
            ValueType = ValueType.None;
            _isCompiled = true;
        }

        public override void RecursiveDo(Func<Expression, bool> act, int depth)
        {
            if (!act(this) || depth == 0) return;
            foreach (Expression e in _expressions)
            {
                e.RecursiveDo(act, depth - 1);
            }
        }
    }
}
