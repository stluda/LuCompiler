using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class State : Variable
    {
        private Hashtable<string, Expression> _expressionOfStates = new Hashtable<string, Expression>();

        public State(string name,Context context) : base(ValueType.State,name,context)
        {
        }

        public Hashtable<string, Expression> ExpressionOfStates
        {
            get { return _expressionOfStates; }
        }

        public void Add(string key, Expression value)
        {
            _expressionOfStates[key] = value;
        }

        public void Compile(Function function)
        {
            if (function.LocalVariables.ContainsKey(Name)) Dict.AddException(new InterpretException(Context,"重复定义!"),this);
            function.LocalVariables[Name] = this;
            foreach (Expression exp in _expressionOfStates.Values)
            {
                try
                {
                    exp.Compile(function);
                }
                catch (InterpretException ie)
                {
                    Dict.AddException(ie);
                }
                if ((ValueType.Boolean & exp.ValueType) != ValueType.Boolean)
                    Dict.AddException(new InterpretException(Context, "State类型的每个子类型都必须是布尔类型"),this);
            }
        }
    }
}
