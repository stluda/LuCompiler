using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class FunctionSwitcher : Function
    {
        public override string FinalName
        {
            get { return G.IsDebugMode ? "f" : Mixcode; }
        }

        public FunctionSwitcher(List<KernalFunction> kernals,List<Module> modules,List<Function> functions)
        {
            G.FunctionSwitcher = this;
            //kernals
            int i = -1;
            foreach (KernalFunction f in kernals)
            {
                i++;
                Dict.IndexOfFunction[f] = i;
                Dict.FunctionOFIndex[i] = f;
            }
            //modules
            foreach (Module f in modules)
            {
                i++;
                Dict.IndexOfFunction[f] = i;
                Dict.FunctionOFIndex[i] = f;
            }

            //AEFunctions
            foreach (AEFunction f in G.AEFunctions.Values)
            {
                i++;
                Dict.IndexOfFunction[f] = i;
                Dict.FunctionOFIndex[i] = f;
            }

            //Functions
            foreach (Function f in functions)
            {
                i++;
                Dict.IndexOfFunction[f] = i;
                Dict.FunctionOFIndex[i] = f;
            }
            functions.Add(this);
        }

        public override void Compile()
        {
            this.Compile(null);
        }

        public override void Compile(Function sender)
        {
        }

        public static string ArgNameOfIndex(int index)
        {
            return M.EVar(string.Format("sys_inarg{0}", index));
        }

        public static string ToSysInArg(Expression exp)
        {           
            if (exp == null) return "";
            if (exp is CommaExpression)
            {
                StringBuilder sb = new StringBuilder();
                CommaExpression c = exp as CommaExpression;
                int argCount = c.Expressions.Count;
                sb.Append(ArgNameOfIndex(1));
                for (int i = 1; i < argCount; i++) sb.Append(',').Append(ArgNameOfIndex(i + 1));
                return sb.ToString();
            }
            else return ArgNameOfIndex(1);
        }

        public static string ToSetSysInArg(Expression exp)
        {
            if (exp == null) return "";
            if (exp is CommaExpression)
            {
                StringBuilder sb = new StringBuilder();
                CommaExpression c = exp as CommaExpression;
                int i = 1;
                foreach (Expression e in c.Expressions)
                {
                    sb.Append(string.Format("{0} = {1} : ", ArgNameOfIndex(i++), e.QTranslate()));
                }
                return sb.ToString();
            }
            else return string.Format("{0} = {1} : ", ArgNameOfIndex(1), exp.QTranslate());
        }

        public override string QTranslate()
        {//sys_inarg0
            string inarg=M.EVar("FUNCTION_NO");
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            Action<string> appendCase = (string value) =>
            {
                if (sb2.Length < 1024)
                {
                    sb2.Append(value);
                }
                else
                {
                    sb.AppendLine(sb2.ToString());
                    sb2.Clear();
                    sb2.Append(value);
                }
            };
            /*Action<string> appendCase = (string value)=>
            {
                sb2.AppendLine(value);
            };*/

            Action appendCaseEnd = () => { if (sb2.Length > 0)sb.AppendLine(sb2.ToString()); };

            sb.AppendLine(string.Format("Select Case {0}", inarg));
            for (int i = 0, length = Dict.FunctionOFIndex.Keys.Count; i < length; i++)
            {
                string arg2 = "";
                Function func = Dict.FunctionOFIndex[i];
                if (func is AEFunction)
                {
                    if (func is AEFunctionSwitcher)
                    {
                        if(func.Type==ValueType.None)
                            arg2 = string.Format("{0} {1}", func.FinalName, ArgNameOfIndex(1));
                        else
                            switch (func.Name.ToLower())
                            {
                                case "find":
                                case "finde":
                                case "finda":
                                case "fclicke":
                                case "fdbclicke":
                                    arg2 = string.Format("{0} = {1}({2},{3})", FinalName, func.FinalName, ArgNameOfIndex(1), ArgNameOfIndex(2));
                                    break;
                                default:
                                    arg2 = string.Format("{0} = {1}({2})", FinalName, func.FinalName, ArgNameOfIndex(1));
                                    break;
                            }
                            
                    }
                    else
                    {
                        if (func is AEFindFunction) arg2 = string.Format("{0} = {1}({2})", FinalName, func.FinalName,M.GetFindPicDelayValue());
                        else arg2 = func.FinalName;
                    }
                }
                else if (func is Module) arg2 = func.FinalName;
                else
                {
                    bool hasRetValue = func.Type != ValueType.None;
                    if (func.Type != ValueType.None) arg2 = string.Format("{0} = ",FinalName);
                    string ret = ToSysInArg(func.InArgs);
                    if (hasRetValue) ret = "(" + ret + ")";
                    else ret = " "+ret;
                    arg2 += func.FinalName + ret;
                }
                //sb.AppendLine(string.Format("Case {0} : {1}", i, arg2));
                appendCase(string.Format("Case {0}:{1}:", i, arg2));
            }
            appendCaseEnd();
            sb.AppendLine(string.Format("Case Else : writelog 0,\"警告：如果你看到这句话，说明执行脚本的过程中发生异常，请联系作者，异常码：INVALID_FUNC_CALL\" : {0}", G.IsDebugMode ? string.Format("Traceprint \"函数的非法调用:\"&{0} : ExitScript",inarg) : ""));
            sb.AppendLine("End Select");
            return string.Format("Function {0}({1})\r\n{2}End Function",FinalName, inarg, M.Q_AddTab(sb.ToString()));
        }
    }
}
