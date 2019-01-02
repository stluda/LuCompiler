using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class DebugStatement : Statement
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

        private Statement _statement;
        private Statement _elseStatement = null;

        public Statement ElseStatement
        {
            get { return _elseStatement; }
            set { _elseStatement = value; }
        }
        private int _debugLevel;

        public DebugStatement(int debugLevel, Statement statement)
        {
            _statement = statement;
            _debugLevel = debugLevel;
        }

        public override void Compile(Function function)
        {
            if (G.IsDebugMode && G.DebugLevel >= _debugLevel) _statement.Compile(function);
            else if(_elseStatement!=null)_elseStatement.Compile(function);
        }

        public override string QTranslate()
        {
            return G.IsDebugMode && G.DebugLevel >= _debugLevel ? _statement.QTranslate() :
                _elseStatement==null ? "" : _elseStatement.QTranslate();
        }

        public override string ToString()
        {
            return string.Format("debug({0}){1}{2}",_debugLevel,_statement.ToString(),
                _elseStatement==null ? "" : string.Format("else {0}",_elseStatement.ToString()));
        }
    }
}
