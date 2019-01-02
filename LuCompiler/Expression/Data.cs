using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace LuCompiler
{
    public class Data : Expression
    {
        private static Regex r_integer = new Regex(@"^\-?[0-9]+$");
        private static Regex r_decimal=new Regex(@"^\-?[0-9]+\.[0-9]+$");
        private static Regex r_variable = new Regex(@"[a-zA-Z\u4e00-\u9fa5]\w*");
        private static Regex r_string = new Regex("\"(\\\\\"|[^\"])*\"");
        private static Regex r_capture = new Regex(@"^\$[0-9]+$");
        private int _captureID = -1;
        private bool _isVariable = false;
        private bool _isResult = false;
        private bool _isCapture = false;

        private Function _function;

        public bool IsResult
        {
            get { return _isResult; }
            set { _isResult = value; }
        }

        [Obsolete("IsVariable已过时，用ExpressionType.HasFlag(ExpressionType.Variable)来代替", true)]
        public bool IsVariable
        {
            get { return _isVariable; }
            set { _isVariable = value; }
        }

        public Data(string value) : base()
        {
            this.Value = value;
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            if (!IsCompiled) throw new Exception("尚未编译");
            prefixStatement = "";
            postfixStatement = "";
            interValFlag = false;
            switch (Value)
            {
                case "result":
                    return _function.FinalName;
                default:
                    /*if (_expressionAttributes.HasFlag(ExpressionAttrs.ObjectMember))
                    {
                        return Value;
                    }*/
                    if (_expressionAttributes.HasFlag(ExpressionAttrs.External))
                    {
                        string name = Value.TrimStart('$');
                        if (G.KernalNames.Contains(name.ToLower())) name = M.EVar(name);
                        return name;
                    }
                    else if (_expressionAttributes.HasFlag(ExpressionAttrs.Const))
                    {
                        string ret = Value;
                        if (_expressionAttributes.HasFlag(ExpressionAttrs.String))
                        {
                            ret = ret.Replace(@"\\", @"{#slash}").Replace("\\\"", "\"\"").Replace(@"{#slash}","\\");
                        }
                        return ret;
                    }
                    else if (_expressionAttributes.HasFlag(ExpressionAttrs.Function))
                    {
                        Function func = _refVar as Function;
                        return Dict.IndexOfFunction[func].ToString();
                    }
                    else if (_expressionAttributes.HasFlag(ExpressionAttrs.Variable))
                    {
                        return _refVar.FinalName;
                    }
                    throw new NotImplementedException();
            }         
        }

        public override void Compile(Function function)
        {
            _function = function;
            Action<ValueType, ExpressionAttrs> set = (ValueType type,ExpressionAttrs attr) =>
            {
                ValueType = type;
                _expressionAttributes |= attr;
                _isCompiled = true;
            };
            /*if (ExpressionAttributes.HasFlag(ExpressionAttrs.ObjectMember))
            {
                ValueType = ValueType.Super;
                _isCompiled = true;
                return;
            }*/
            if (r_integer.IsMatch(Value))
            {
                set(ValueType.Numeric, ExpressionAttrs.IntegerConst);
                return;
            }
            else if (r_decimal.IsMatch(Value))
            {
                set(ValueType.Numeric, ExpressionAttrs.DecimalConst);
                return;
            }
            else if (r_string.IsMatch(Value))
            {
                set(ValueType.String, ExpressionAttrs.StringConst);
                return;
            }
            else if (r_variable.IsMatch(Value))
            {
                switch (Value.ToLower())
                {
                    case "map":

                        break;
                    case "result":
                        _isResult = true;
                        break;
                    case "true":
                    case "false":
                        set(ValueType.Boolean, ExpressionAttrs.BooleanConst);
                        return;
                    default:
                        switch (Value.ToLower())
                        {
                            case "mainform":
                                Value = "$" + Value;
                                break;
                        }
                        _expressionAttributes |= ExpressionAttrs.Variable;
                        if(Value.StartsWith("$"))_expressionAttributes |= ExpressionAttrs.External;
                        if (_expressionAttributes.HasFlag(ExpressionAttrs.External))
                        {
                            if (!Value.StartsWith("$")) Value = "$" + Value;
                            if (r_capture.IsMatch(Value))
                            {
                                throw new NotImplementedException();
                                _isCapture = true;
                                _captureID = int.Parse(Value.TrimStart('$'));
                                return;
                            }
                            if (!G.GlobalVariables.ContainsKey(Value))
                            {
                                Variable var = new ExternalVariable(Value, Context);
                                //var.IsExternal = true;
                                _refVar = G.GlobalVariables[Value] = var;
                            }
                        }
                        switch (Value.ToLower())
                        {
                            case "finde":
                            case "finda":
                            case "find":
                                _refVar = G.AEFunctions["find"];
                                break;
                            case "fclicke":
                            case "fdbclicke":
                            case "clicke":
                            case "dbclicke":
                                _refVar = G.AEFunctions[Value.ToLower()];
                                break;
                            default:
                                if (function.LocalVariables.ContainsKey(Value)) _refVar = function.LocalVariables[Value];
                                else if (G.GlobalVariables.ContainsKey(Value)) _refVar = G.GlobalVariables[Value];
                                else 
                                    throw new InterpretException(Context, "找不到这个变量");
                                break;
                        }
                        if (_refVar is Function)
                        {
                            Function func = _refVar as Function;
                            func.CallBacks.Add(
                                delegate(Function f)
                                {
                                    if (f.IsLimited) function.IsLimited = true;
                                });
                            func.Compile(function);
                            if (func.IsLimited) function.IsLimited = true;
                            _expressionAttributes |= ExpressionAttrs.Function;
                            //_expressionAttributes &= 
                        }
                        else
                        {
                            if (Parent != null && !(Parent is AssignmentExpression) && !_refVar.IsInitialized) Dict.AddException(new InterpretException(Context, "使用了未初始化的变量!"), this);
                        }
                        _isVariable = true;                        
                        //ValueType = _refVar.Type;
                        ValueType = _refVar is Function ? ValueType.FunctionReference : _refVar.Type;
                        _expressionAttributes |= M.ToExpressionAttribute(ValueType);                        
                        break;
                }
            }
            exit:
            IsCompiled = true;
        }


    }

}
