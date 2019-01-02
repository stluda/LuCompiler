using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class LoopStatement : ConditionStatement
    {
        private int _loopId;
        private bool _hasContinueStatement;
        private bool _hasBreakStatement;


        public LoopStatement(string keyword, Expression condition, Statement statement)
            : base(keyword, condition, statement)
        {
        }

        public bool HasContinueStatement
        {
            get { return _hasContinueStatement; }
            set { _hasContinueStatement = value; }
        }

        public bool HasBreakStatement
        {
            get { return _hasBreakStatement; }
            set { _hasBreakStatement = value; }
        }

        public int LoopId
        {
            get { return _loopId; }
            set { _loopId = value; }
        }

        public override void Compile(Function function)
        {
            base.Compile(function);
            bool exflag = false;
            try
            {
                _condition.Compile(function);
            }
            catch(InterpretException ie)
            {
                Dict.AddException(ie); exflag = true;
            }
            if (!exflag&&(_condition.ValueType & ValueType.Boolean) != ValueType.Boolean) Dict.AddException(new InterpretException(Context, "表达式必须是布尔类型"),this);
            _loopId = function.LoopCount++;
        }

        public override string ToString()
        {
            switch (_keyword)
            {
                case "dowhile":
                    return string.Format("do {0} while({1});", _statement, _condition);
                default:
                    return base.ToString();
            }
        }

        public override string QTranslate()
        {
            string prefix, value, postfix;
            value = Condition.QTranslate(out prefix, out postfix);
            switch (Keyword)
            {
                case "limit":
                    break;
                case "while":
                    return M.Q_CreateWhile(Function.FinalName, LoopId, prefix, value, postfix, S.QTranslate(), _hasBreakStatement);
                case "dowhile":
                    return M.Q_CreateDoWhile(Function.FinalName, LoopId, prefix, value, postfix, S.QTranslate(), _hasBreakStatement, _hasContinueStatement, "", true);
                default:
                    throw new InterpretException(Context,"...........");
            }
            return "";
        }
    }
}
