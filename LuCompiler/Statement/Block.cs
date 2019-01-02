using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Block : Statement
    {
        private string _wrapType = "unknown";
        private ConditionStatement _outsideStatement;

        public ConditionStatement OutsideStatement
        {
            get { return _outsideStatement; }
            set { _outsideStatement = value; }
        }
        public string WrapType
        {
            get { return _wrapType; }
            set { _wrapType = value; }
        }
        private List<Statement> _statements = new List<Statement>();

        public void Add(Statement statement)
        {
            _statements.Add(statement);
            statement.Parent = this;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (Statement statement in _statements)
            {
                sb.Append(("\r\n" + statement.ToString()).Replace("\r\n", "\r\n\t"));
            }
            sb.Append("\r\n}");
            return sb.ToString();
        }

        public override string QTranslate()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Statement statement in _statements)
            {
                string result = statement.QTranslate();
                if (result != "") result += "\r\n";
                sb.Append(result);
            }
            return sb.ToString();
        }

        public override void Compile(Function function)
        {
            foreach (Statement statement in _statements) statement.Compile(function);
        }
    }
}
