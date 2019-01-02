using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class ConditionStatement : Statement
    {
        protected string _keyword;
        protected Expression _condition;
        protected Statement _statement;
        private Function _function;

        public Function Function
        {
            get { return _function; }
            set { _function = value; }
        }

        public string Keyword
        {
            get { return _keyword; }
        }

        public Statement S
        {
            get { return _statement; }
            set { _statement = value; }
        }

        public Expression Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }

        public ConditionStatement(string keyword, Expression condition, Statement statement)
        {
            _keyword = keyword;
            _condition = condition;
            _statement = statement;
            _statement.Parent = this;
        }

        public override string ToString()
        {
            return string.Format("{0}({1}){2}", _keyword, _condition, _statement);
        }

        public override string QTranslate()
        {
            string prefix, value,postfix;
            value = _condition.QTranslate(out prefix,out postfix);
            return "";
        }

        public override void Compile(Function function)
        {
            bool exFlag = false;
            try
            {
                _condition.Compile(function);
            }
            catch(InterpretException ie)
            {
                Dict.AddException(ie);
                exFlag = true;
            }            
            _statement.Compile(function);
            _isCompiled = true;
            _function = function;
        }

    }
}
