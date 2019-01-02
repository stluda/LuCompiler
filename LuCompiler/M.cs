using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;

namespace LuCompiler
{
    public static class M
    {
        public static string GetFindPicDelay()
        {
            Variable var = G.GlobalVariables["FIND_PIC_DELAY"];
            if (var == null) return "50";
            else return var.Name;
        }

        public static string GetFindPicDelayValue()
        {
            Variable var = G.GlobalVariables["FIND_PIC_DELAY"];
            if (var == null) return "50";
            else return var.FinalName;
        }

        public static Expression GetFindPicDelayExpression()
        {
            return new Data(GetFindPicDelay());
        }

        public static void ForEachFileDo(string path, string postfix, Action<string> action)
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (file.EndsWith(postfix))
                {
                    action(file);
                }
            }

        }

        public static string ReadFile(string fileName)
        {
            string result = null;
            using (StreamReader sr = new StreamReader(fileName,Encoding.GetEncoding(0)))
            {
                result = sr.ReadToEnd();
                sr.Close();
            }
            return result;
        }

        public static string GetRelativeFileName(string name)
        {
            int index = name.LastIndexOf('\\') + 1;
            if (index < 0) return name;
            else return name.Substring(index, name.Length - index);
        }

        public static string GetFileDirectory(string name)
        {
            int index = name.LastIndexOf('\\');
            if (index <= 0) return "";
            else return name.Substring(0, index);
        }

        public static string GetNewPath(string oldPath, string newFileName)
        {
            string dir = GetFileDirectory(oldPath);
            if (dir != "") dir += @"\";
            return string.Format(@"{0}{1}",dir, newFileName);

        }

        public static void MyEncode(string srcFile, string dstFile)
        {
            int length = 1024 * 1024 * 10;
            byte[] buffer = new byte[length];
            byte[] newBuffer;
            using (FileStream fs = new FileStream(srcFile, FileMode.Open))
            {
                fs.Position = 0;
                int count = fs.Read(buffer, 0, length);
                newBuffer = new byte[count];
                for (int i = count - 1; i >= 0; i--)
                {
                    byte value = buffer[i];
                    byte newHigh =(byte)((value & 0x0F) << 4);
                    byte newLow = (byte)(value >> 4);
                    byte newValue =(byte)(newHigh | newLow);
                    newBuffer[i] = newValue;
                }
                fs.Close();
            }
            using (FileStream fs = new FileStream(dstFile, FileMode.Create))
            {
                fs.Write(newBuffer, 0, newBuffer.Length);
                fs.Close();
            }

        }


        private static int _indexOfTemporailyVar = 0;
        private static Regex r_matchline = new Regex(".+");

        //public static string Q_CreateLimit(string func_name, int loop_id,int limit_uBound,Expression c1,Expression c2, string statements,bool hasBreakStatement,bool hasContinueStatement)
        public static string Q_CreateWhile(string func_name, int loop_id, string prefix, string condition, string postfix, string statements, bool hasBreakStatement)
        {
            return Q_CreateWhile(func_name, loop_id, prefix, condition, postfix, statements, hasBreakStatement, "",true);
        }
        public static string Q_CreateWhile(string func_name, int loop_id, string prefix, string condition, string postfix, string statements, bool hasBreakStatement, string beforeEndStatement,bool ifOrNot)
        {
            string remStart = string.Format("sys_{0}_loop{1}_start", func_name, loop_id);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Rem {0} : ",remStart))
                .Append(Q_CreateIf(prefix,condition,postfix,statements,null,
                beforeEndStatement + string.Format("Goto {0} : ", remStart), ifOrNot));
            if (hasBreakStatement) sb.Append(string.Format("Rem sys_{0}_loop{1}_end : ", func_name, loop_id));
            return sb.ToString();
        }
        public static string Q_CreateDoWhile(string func_name, int loop_id, string prefix, string condition, string postfix, string statements, bool hasBreakStatement,bool hasContinueStatement, string beforeEndStatement,bool ifOrNot)
        {
            string remStart = string.Format("sys_{0}_loop{1}_start", func_name, loop_id);
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            sb.AppendLine(string.Format("Rem {0} : ", remStart));
            sb2.Append(statements);
            if (hasContinueStatement) sb2.AppendLine(string.Format("\tRem sys_{0}_loop{1}_next : ", func_name, loop_id));
            sb2.Append(beforeEndStatement);
            sb.Append(M.Q_AddTab(sb2.ToString()))
                .Append(Q_CreateIf(prefix, condition, postfix, string.Format("Goto {0} : ", remStart), null, "", ifOrNot));
            if (hasBreakStatement) sb.Append(string.Format("Rem sys_{0}_loop{1}_end : ", func_name, loop_id));
            return sb.ToString();
        }
        public static string Q_CreateIf(string prefix, string condition, string postfix, string statements1, string statements2)
        {
            return Q_CreateIf(prefix, condition, postfix, statements1, statements2,"",true);
        }
        public static void Q_ConditionMix(out string rCond, out string rPrefix, string prefix, string condition, string postfix)
        {
            rCond = condition;
            if (postfix != "")
            {
                postfix = string.Format("{0} = {1} : ",M.EVar("g_temp"), condition) + postfix;
                rCond = M.EVar("g_temp");
            }
            rPrefix = prefix + postfix;
            if (rPrefix != "") rPrefix += "\r\n";
        }
        public static string Q_CreateIf(string prefix,string condition,string postfix, string statements1, string statements2,string beforeEndStatement,bool ifOrNot)
        {
            StringBuilder sb = new StringBuilder();
            string rPrefix,rCond;
            Q_ConditionMix(out rCond, out rPrefix, prefix, condition, postfix);
            sb.Append(rPrefix)
                .AppendLine(string.Format("If {0} Then : ",ifOrNot?rCond:"NOT("+rCond+")"))
                .Append(Q_AddTab(statements1));
            if (statements2 != null)
            {
                sb.AppendLine("Else : ");
                sb.Append(Q_AddTab(statements2));
            }
            sb.Append(beforeEndStatement);
            sb.Append(" : End If : ");
            return sb.ToString();
        }

        public static string Q_AddTab(string statements)
        {
            return r_matchline.Replace(statements, "\t$0");
        }

        public static string Q_AddTab(string statements,int count)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('\t', count).Append("$0");
            return r_matchline.Replace(statements, sb.ToString());
        }

        private static Random random = new Random();
        public static string GetRandomString(int length)
        {            
            StringBuilder sb = new StringBuilder();
            sb.Append((char)random.Next('A', 'Z'));
            for (int i = length - 1; i > 0; i--)
            {
                switch (random.Next(0,2))
                {
                    case 0:
                        sb.Append((char)random.Next('A', 'Z'));
                        break;
                    default:
                        sb.Append((char)random.Next('0', '9'));
                        break;
                }
            }
            return sb.ToString();
        }

        public static string GetMixcode()
        {
            string mixcode;
            do
            {
                mixcode = GetRandomString(10);
            } while (G.Mixcodes.Contains(mixcode));
            G.Mixcodes.Add(mixcode);
            return mixcode;
        }
        public static string EVar(string var)
        {
            return G.IsDebugMode ? var : MixcodeOfExternalVar(var.ToLower());
        }
        public static string MixcodeOfExternalVar(string var)
        {
            string mixcode;
            if (G.MixcodeOfExternalVariable.ContainsKey(var)) return G.MixcodeOfExternalVariable[var];
            else
            {
                mixcode = GetMixcode();
                G.MixcodeOfExternalVariable[var] = mixcode;
                return mixcode;
            }
        }
        public static string MixcodeOfVar(Variable var)
        {
            string mixcode;
            if (G.MixCodeOfVariable.ContainsKey(var)) return G.MixCodeOfVariable[var];
            else
            {
                mixcode = GetMixcode();
                G.MixCodeOfVariable[var] = mixcode;
                return mixcode;
            }
        }
        public static Rectangle ToRectangle(string str)
        {
            string[] args = str.Split(',');
            return new Rectangle(
                int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3]));
        }
        public static Point ToPoint(string str)
        {
            string[] args = str.Split(',');
            return new Point(int.Parse(args[0]), int.Parse(args[1]));
        }
        public static string ValueIfDebug(string value,int debugLevel)
        {
            return G.IsDebugMode && G.DebugLevel >= debugLevel ? value : "";
        }
        public static string GetDebugLog(string logContent,int debugLevel,params object[] args)
        {
            string caseDebug = string.Format("Traceprint \"[d{1}]\"&{0} : ", string.Format(logContent, args), debugLevel);
            if (G.DoesWriteTraceprintToLog) caseDebug += string.Format("writelog 0,\"[d{1}]\"&{0} : ", string.Format(logContent, args), debugLevel);
            return ValueIfDebug(caseDebug,debugLevel);
        }
        public static string GetCondtionDebugLog(string condition, int debugLevel, string logContent1, string logContent2, params object[] args)
        {
            Func<string,string> caseDebug = (string logContent) =>
            {
                string debugContent = string.Format("Traceprint \"[d{1}]\"&{0} : ", string.Format(logContent, args), debugLevel);
                if (G.DoesWriteTraceprintToLog) debugContent += string.Format("writelog 0,\"[d{1}]\"&{0} : ", string.Format(logContent, args), debugLevel);
                return debugContent;
            };
            return ValueIfDebug(
                string.Format("If {0} Then : {1} : Else : {2} : End If : ",
                condition, caseDebug(logContent1), caseDebug(logContent2))
                , debugLevel);
        }
        public static readonly string QCommaConnect = "&\",\"&";
        public static string Quote(string str)
        {
            return '"' + str + '"';
        }
        public static string Group(object str)
        {
            return "(" + str + ")";
        }
        public static string QFormat(params object[] args)
        {
            StringBuilder sb = new StringBuilder(args[0].ToString());
            for (int i = 1, length = args.Length; i < length; i++)
            {
                sb.Append("&").Append(args[i].ToString());
            }
            return sb.ToString();
        }
        public static string QCommaFormat1(params object[] args)
        {
            StringBuilder sb = new StringBuilder("(" + args[0].ToString()+")");
            for (int i = 1, length = args.Length; i < length; i++)
            {
                sb.Append(QCommaConnect).Append("("+args[i].ToString()+")");
            }
            return sb.ToString();
        }
        public static string QCommaFormat2(params object[] args)
        {
            StringBuilder sb = new StringBuilder('"' + args[0].ToString() + '"');
            for (int i = 1, length = args.Length; i < length; i++)
            {
                sb.Append(QCommaConnect).Append('"' + args[i].ToString() + '"');
            }
            return sb.ToString();
        }

        public static string GetATempVar()
        {
            string value = "g_temp" + _indexOfTemporailyVar++;
            if (!G.IsDebugMode) value = MixcodeOfExternalVar(value);
            return value;
        }

        public static bool SetExflag(bool condition, ref bool exFlag)
        {
            exFlag = exFlag || condition;
            return condition;
        }

        public static void AddRefers(Context a, Context b)
        {
            string aStr=a.Value, bStr=b.Value;
            List<string> list;
            if (G.References.ContainsKey(aStr)) list = G.References[aStr];
            else
            {
                G.References[aStr] = list = new List<string>();
            }
            if (!list.Contains(bStr)) list.Add(bStr);
            if (!G.ReferencesContext.ContainsKey(aStr)) G.ReferencesContext[aStr] = a;
            if (!G.ReferencesContext.ContainsKey(bStr)) G.ReferencesContext[bStr] = b;
        }

        public static string ParseStaticVars()
        {
            if (G.StaticVars.Count == 0) return "";
            StringBuilder sb = new StringBuilder("DimEnv ");
            foreach (Variable var in G.StaticVars)
            {
                sb.Append(var.FinalName).Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static ValueType ToValueType(string type)
        {
            switch (type.ToLower())
            {
                case "none": return ValueType.None;
                case "array": return ValueType.Array;
                case "bool": return ValueType.Boolean;
                case "funcref": return ValueType.FunctionReference;
                case "map": return ValueType.Map;
                case "num": return ValueType.Numeric;
                case "string": return ValueType.String;
                case "state": return ValueType.State;
                case "super": return ValueType.Super;
                case "unknown":
                case "var": 
                    return ValueType.Unknown;                    
                default: throw new NotImplementedException();
            }
        }

        public static string GetTraceString(string src,int level,Function function)
        {
            CommaExpression cma = new CommaExpression(new Data(level.ToString()));
            cma.Add(new Data('"' + src + '"'));
            //cma.Add(new AdditiveExpression(new Data(string.Format("\"[d{0}]\"",level))
            //    ,new Data('"'+src+'"'),Symbol.GetInstance("+",typeof(AdditiveExpression))));
            TraceStatement ts = new TraceStatement(
                new PostfixExpression(new Data("trace"), "(", cma));
            ts.Compile(function);
            return ts.QTranslate();
        }

        private static Regex _regex_integer = new Regex("^[0-9]+$");
        private static Regex _regex_decimal = new Regex(@"^[0-9]+\.[0-9]+$");
        private static Regex _regex_string = new Regex("^\"[^\"]*\"$");
        private static Regex _regex_variable = new Regex(@"^[a-zA-Z\u4e00-\u9fa5]\w*$");
        public static ExpressionAttrs GetExpressionAttribute(string value)
        {
            if (_regex_integer.IsMatch(value)) return ExpressionAttrs.IntegerConst;
            else if (_regex_decimal.IsMatch(value)) return ExpressionAttrs.DecimalConst;
            else if(_regex_string.IsMatch(value))return ExpressionAttrs.StringConst;
            else if(_regex_variable.IsMatch(value))return ExpressionAttrs.Variable;
            else return ExpressionAttrs.Unknown;
        }
        public static ExpressionAttrs ToExpressionAttribute(ValueType type)
        {
            switch (type)
            {
                case ValueType.Array: return ExpressionAttrs.Array;
                case ValueType.Boolean: return ExpressionAttrs.Boolean;
                case ValueType.FunctionReference: return ExpressionAttrs.FunctionReference;
                case ValueType.Map: return ExpressionAttrs.Map;
                case ValueType.None: return ExpressionAttrs.None;
                case ValueType.Numeric: return ExpressionAttrs.Numeric;
                case ValueType.State: throw new NotImplementedException();
                case ValueType.String: return ExpressionAttrs.String;
                case ValueType.Super: return ExpressionAttrs.Super;
                case ValueType.Unknown: return ExpressionAttrs.Unknown;
                default: throw new NotImplementedException();
            }
        }


        public static string CombineIntOrStr(object v1, object v2)
        {
            string value1 = v1.ToString(), value2 = v2.ToString();
            if (_regex_integer.IsMatch(value1) && _regex_integer.IsMatch(value2))
            {
                return (int.Parse(value1) + int.Parse(value2)).ToString();
            }
            else return string.Format("{0}+{1}", value1, value2);
        }
    }
}
