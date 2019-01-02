using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class IfStatement : ConditionStatement
    {
        public override Statement Parent
        {
            get
            {
                return base.Parent;
            }
            set
            {
                base.Parent = value;
                if (_elseStatement != null) _elseStatement.Parent = value;
            }
        }

        private Statement _elseStatement;

        public Statement ElseStatement
        {
            get { return _elseStatement; }
            set { _elseStatement = value;  }
        }

        public IfStatement(Expression condition, Statement statement)
            : base("if", condition, statement)
        {
        }

        public IfStatement(Expression condition, Statement statement, Statement elseStatement)
            : this(condition, statement)
        {
            _elseStatement = elseStatement;
        }

        public override string ToString()
        {
            return _elseStatement == null ?
                base.ToString() : string.Format("if({0}){1}else {2}", Condition, S, _elseStatement);
        }

        public override void Compile(Function function)
        {
            base.Compile(function);
            if (!_condition.ValueType.HasFlag(ValueType.Boolean))
                Dict.AddException(new InterpretException(_condition.Context, "If语句里面的表达式必须是布尔类型"),this);
            if (_elseStatement != null) _elseStatement.Compile(function);
        }

        public override string QTranslate()
        {
            string prefix, value, postfix;
            value = Condition.QTranslate(out prefix, out postfix);
            return M.Q_CreateIf(prefix, value, postfix, S.QTranslate(), _elseStatement == null ? null : _elseStatement.QTranslate());
        }
    }
}
