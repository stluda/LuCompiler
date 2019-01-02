using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class EveryStatement : ConditionStatement
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

        private IfStatement _result;
        private Statement _elseStatement;
        private Function _function;
        private string _s_everyIndex;
        public EveryStatement(Expression condition, Statement statement)
            : base("every", condition, statement) { }
        public EveryStatement(Expression condition, Statement statement, Statement elseStatement) : this(condition,statement)
        {
            _elseStatement = elseStatement;
        }

        public override void Compile(Function function)
        {            
            _function = function;
            _s_everyIndex = string.Format("{0}_{1}_everyIndex", function is Module ? "sys" : "lu", function.Name);
            _s_everyIndex = M.EVar(_s_everyIndex);
            _result = new IfStatement(new EqualityExpression(
                new MultiplicativeExpression(new Data("$"+_s_everyIndex), _condition,Symbol.GetInstance("%",typeof(MultiplicativeExpression))),
                new Data("0"),
                Symbol.GetInstance("==",typeof(EqualityExpression))), _statement,_elseStatement);
            _result.Compile(function);
            if ((_condition.ValueType & ValueType.Numeric) != ValueType.Numeric) throw new InterpretException(Context, "表达式必须是数字类型");
            function._s_everyIndex = _s_everyIndex;            
        }

        public override string QTranslate()
        {
            return _result.QTranslate();
        }
    }
}
