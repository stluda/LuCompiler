using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class StateExpression : Expression
    {
        private State _state;
        private Expression _expression;
        private string _varName;
        private string _stateType;

        public State State
        {
            get { return _state; }
        }

        public StateExpression(string arg) : base()
        {
            string[] args = arg.Split('.');
            _varName = args[0];
            _stateType = args[1];
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            return _expression.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
        }

        public override void Compile(Function function)
        {
            base.Compile(function);
            Variable var;
            if (function.LocalVariables.ContainsKey(_varName))
            {
                var = function.LocalVariables[_varName];
            }
            else if (G.GlobalVariables.ContainsKey(_varName))
            {
                var = G.GlobalVariables[_varName];
            }
            else throw new InterpretException(Context,"不存在这个变量");
            if (!(var is State)) throw new InterpretException(Context,"这个变量不是state类型");
            _state = var as State;
            if (!_state.ExpressionOfStates.ContainsKey(_stateType))
                throw new InterpretException(Context, string.Format("state类型{0}不存在{1}子类型", _varName, _stateType));
            _expression = _state.ExpressionOfStates[_stateType];
            ValueType = ValueType.Boolean;
        }
    }
}
