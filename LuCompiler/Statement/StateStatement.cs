using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class StateStatement : DeclarationStatement
    {
        private State _state;

        public StateStatement(State state)
        {
            _state = state;
        }

        public override void Compile(Function function)
        {
            _state.Compile(function);
        }

        public override string QTranslate()
        {
            return "";
        }
    }
}
