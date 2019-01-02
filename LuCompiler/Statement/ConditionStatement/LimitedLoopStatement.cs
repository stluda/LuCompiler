using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class LimitedLoopStatement : LoopStatement
    {
        private string _q_exitType = "Function";
        private Function _function;
        private bool _isDo = false;

        public bool IsDo
        {
            get { return _isDo; }
            set { _isDo = value; }
        }
        private Expression _checkExpression;
        private Expression _checkEveryCount;
        public LimitedLoopStatement(Expression condition, Expression checkExpression, Expression checkEveryCount, Statement statement)
            : base("limit", condition, statement)
        {
            _checkExpression = checkExpression;
            _checkEveryCount = checkEveryCount;
        }
        public LimitedLoopStatement(Expression condition, Expression checkExpression, Expression checkEveryCount, Statement statement, bool isDo)
            : this(condition, checkExpression, checkEveryCount, statement)
        {
            if (isDo)
            {
                _keyword = "dolimit";
                _isDo = true;
            }
        }

        public override string ToString()
        {
            switch (_keyword)
            {
                case "limit":
                    return string.Format("limit({1};{2};{3}){0}", _statement, _condition, _checkExpression, _checkEveryCount);
                case "dolimit":
                    return string.Format("do {0} limit({1};{2};{3});", _statement, _condition,_checkExpression,_checkEveryCount);
                default:
                    throw new Exception("...........................");
            }

        }

        public override void Compile(Function function)
        {
            _function = function;         
            function.IsLimited = true;
            base.Compile(function);
            try
            {
                _checkExpression.Compile(function);
            }
            catch (InterpretException ie) { Dict.AddException(ie); }
            try
            {
                _checkEveryCount.Compile(function);
            }
            catch (InterpretException ie) { Dict.AddException(ie); }            
            if ((ValueType.Numeric & _checkEveryCount.ValueType) != ValueType.Numeric) Dict.AddException(new InterpretException(Context,"limit循环的限定次数表达式必须是数字类型"),this);
        }

        public string _QTranslate_do()
        {
            return "";
        }

        public override string QTranslate()
        {
            if (_function.Type == ValueType.None) _q_exitType = "Sub";
            string checkEveryCountStr;
            string prefixBeforeWhileStatement = "";
            string prefix, postfix,value;
            value = Condition.QTranslate(out prefix,out postfix);
            string prefix2, postfix2, value2;
            value2 = _checkExpression.QTranslate(out prefix2, out postfix2);
            string prefix3, postfix3, value3;
            value3 = _checkEveryCount.QTranslate(out prefix3, out postfix3);
            if (_checkEveryCount is Data)
            {
                Data d = _checkEveryCount as Data;
                string partialPrefix, partialPostfix, partialValue;
                partialValue = d.QTranslate(out partialPrefix, out partialPostfix);
                if (d.ExpressionAttributes.HasFlag(ExpressionAttrs.Variable))
                {
                    if (d.RefVar is Function)
                    {
                        checkEveryCountStr = string.Format("sys_{0}_loop{1}_limit_uBound", Function.FinalName, LoopId);
                        prefixBeforeWhileStatement = string.Format("Dim {0} : {0} = {1} : {2}", checkEveryCountStr, partialValue, partialPostfix);
                    }
                    else checkEveryCountStr = partialValue;
                }
                else checkEveryCountStr = partialValue;
            }
            else
            {
                checkEveryCountStr = string.Format("sys_{0}_loop{1}_limit_uBound", Function.FinalName, LoopId);
                prefixBeforeWhileStatement = string.Format("{2} : Dim {0} : {0} = {1} : {3} :", checkEveryCountStr,value3,prefix3,postfix3);
            }

            string limitIndexStr = string.Format("sys_{0}_loop{1}_limitIndex", Function.FinalName, LoopId);
            prefixBeforeWhileStatement += string.Format("Dim {0} : {0} = 0 : \r\n", limitIndexStr);

            string postfixAtTheEndOfWhileStatement;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            sb.AppendLine(string.Format("{0} = {0} + 1 : ", limitIndexStr));
            sb.AppendLine(string.Format("If {0} >= {1} Then : ", limitIndexStr, checkEveryCountStr));
            sb2.AppendLine(string.Format("{0} = 0 : ", limitIndexStr))
                .AppendLine(string.Format("Call {0} : ", M.EVar("common_check")));
                /*暂时屏蔽
                .AppendLine(string.Format("If g_flag_exitLimit Then : Exit {0} : Else :",_q_exitType))
                .AppendLine(M.Q_CreateIf(prefix2, value2, postfix2,
                    string.Format("g_flag_exitLimit = True : g_resetFlag = True : Exit {0} : ",_q_exitType), null,"",false))
                .AppendLine("End If : ");
                 */
            sb.Append(M.Q_AddTab(sb2.ToString()))
                .AppendLine("End If : ");
            postfixAtTheEndOfWhileStatement = M.Q_AddTab(sb.ToString());
            if (IsDo)
            {
                return string.Format("{0}{1}", prefixBeforeWhileStatement,
                M.Q_CreateDoWhile(Function.FinalName, LoopId, prefix, value, postfix,
                S.QTranslate(), HasBreakStatement,HasContinueStatement, postfixAtTheEndOfWhileStatement,true));
            }
            else return string.Format("{0}{1}", prefixBeforeWhileStatement,
                M.Q_CreateWhile(Function.FinalName, LoopId, prefix, value, postfix,
                S.QTranslate(), HasBreakStatement, postfixAtTheEndOfWhileStatement,true));
        }
    }
}
