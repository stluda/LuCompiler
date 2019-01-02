using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class AEFunctionSwitcher : AEFunction
    {
        private new string _type;
        private List<int> _indexList;
        public override string FinalName
        {
            get { return G.IsDebugMode ? "sys_" + _type : Mixcode; }
        }

        public AEFunctionSwitcher(string type,List<int> list)
        {
            Name = _type = type;
            _indexList = list;
            _register();
            G.AEFunctions[type.ToLower()] = this;
            Compile();
        }

        protected override void _register()
        {
            if (!G.IsDebugMode) _s_functionName = Mixcode;
            else _s_functionName = string.Format("sys_{0}", _type);
        }

        public override void Compile()
        {
            this.Compile(null);
        }

        public override void Compile(Function sender)
        {
            switch (_type.ToLower())
            {
                case "find":
                case "fclicke":
                case "fdbclicke":
                    Type = ValueType.Boolean;
                    break;
                case "clicke":
                case "dbclicke":
                    Type = ValueType.None;
                    break;
            }
        }

        public override string QTranslate()
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            string aeObjNo = M.EVar("AEOBJECT_NO");
            string delayTime = M.EVar("t");

            Action<string> appendCase = (string value)=>
            {
                if (sb3.Length < 1024)
                {
                    sb3.Append(value);
                }
                else
                {
                    sb2.AppendLine(sb3.ToString());
                    sb3.Clear();
                    sb3.Append(value);
                }
            };

            Action appendCaseEnd = () => { if (sb3.Length > 0)sb2.AppendLine(sb3.ToString()); };
            string op;
            switch (op=_type.ToLower())
            {
                case "find":
                case "fclicke":
                case "fdbclicke":
                    sb.AppendLine(string.Format("Function {0}({1},{2}): ", _s_functionName, aeObjNo,delayTime));
                    sb2.AppendLine(string.Format("Select Case {0}", aeObjNo));
                    foreach (int i in _indexList)
                    {
                        AEObject obj = Dict.AEObjectOfIndex[i];
                        AEFunction function;
                        switch (op)
                        {
                            case "find":
                                function = AEFunction.GetInstance(Context.Empty, 
                                    (obj is Area)?"finda":"finde"
                                    , obj);
                                appendCase(string.Format("Case {0}:{1}={2}({3}):", i, _s_functionName, function.S_FunctionName, delayTime));
                                break;
                            case "fclicke":
                            case "fdbclicke":
                                function = AEFunction.GetInstance(Context.Empty, op, obj);
                                appendCase(string.Format("Case {0}:{1}={2}({3}):", i, _s_functionName, function.S_FunctionName, delayTime));
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        if (function == null) throw new Exception("........");                     
                    }
                    appendCaseEnd();
                    sb2.AppendLine(string.Format("Case Else : writelog 0,\"警告：如果你看到这句话，说明执行脚本的过程中发生异常，请联系作者，异常码：INVALID_AEFUNC_CALL\" : {0}", G.IsDebugMode ? "Traceprint \"AE函数的非法调用\" : ExitScript" : ""));
                    sb2.AppendLine("End Select");
                    sb.Append(M.Q_AddTab(sb2.ToString()));
                    sb.AppendLine("End Function");
                    break;
                case "clicke":
                case "dbclicke":
                    string action = _type.ToLower().TrimEnd('e');
                    sb.AppendLine(string.Format("Sub {0}({1}): ", _s_functionName, aeObjNo));
                    sb2.AppendLine(string.Format("Select Case {0}", aeObjNo));
                    foreach (int i in _indexList)
                    {
                        AEObject obj = Dict.AEObjectOfIndex[i];
                        appendCase(string.Format("Case {0}:{1} {2},{3}:", i, action, obj.S_x, obj.S_y));
                    }
                    appendCaseEnd();
                    sb2.AppendLine(string.Format("Case Else : writelog 0,\"警告：如果你看到这句话，说明执行脚本的过程中发生异常，请联系作者，异常码：INVALID_AEFUNC_CALL\" : Traceprint \"AE函数的非法调用\" : {0} :  ", G.IsDebugMode ? "ExitScript" : ""));
                    sb2.AppendLine("End Select");
                    sb.Append(M.Q_AddTab(sb2.ToString()));
                    sb.AppendLine("End Sub");
                    break;
            }
            return sb.ToString();
        }

    }
}
