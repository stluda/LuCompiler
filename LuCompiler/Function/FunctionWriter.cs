using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public enum QFunctionType
    {
        Sub,
        Function
    }

    public class QFunctionWriter
    {
        private List<string> _args = new List<string>();
        private StringBuilder sb = new StringBuilder();
        private StringBuilder sb2 = new StringBuilder();
        private QFunctionType _type;
        private string _name;

        public QFunctionWriter(QFunctionType type, string name)
        {
            _type = type;
            _name = name;
        }

        public QFunctionWriter AddArg(string arg)
        {
            _args.Add(arg);
            return this;
        }

        public QFunctionWriter AddLine(string statement)
        {
            sb2.AppendLine("\t"+statement);
            return this;
        }

        public QFunctionWriter Add(string statement)
        {
            sb2.Append("\t"+statement);
            return this;
        }

        public override string ToString()
        {
            string type = _type == QFunctionType.Function ? "Function" : "Sub";
            sb.Append(string.Format("{0} {1}", type, _name));
            if (_args.Count == 0) sb.AppendLine();
            else
            {
                sb.AppendLine(string.Format("({0})", string.Join(",", _args)));
            }
            sb.Append(sb2.ToString());
            sb.AppendLine(string.Format("End {0}", type));
            return sb.ToString();
        }
    }
}
