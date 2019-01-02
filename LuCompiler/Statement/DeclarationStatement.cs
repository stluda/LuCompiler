using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LuCompiler
{
    public class DeclarationStatement : Statement
    {
        private bool _isStatic;
        private bool _isConst = false;
        private bool _isArray;
        private string _type;
        private string _name;
        private ValueType _valueType = ValueType.Unknown;
        private Expression _value;
        private static Regex _regex_variable = new Regex(@"^[a-zA-Z]\w*$");
        private string declarationType="";
        private Variable _refVar;

        public bool IsConst
        {
            get { return _isConst; }
            set { _isConst = value; }
        }
        public bool IsStatic
        {
            get { return _isStatic; }
            set { _isStatic = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected DeclarationStatement(){}

        public DeclarationStatement(string type, Expression value, bool isArray)
        {
            _isArray = isArray;
            _value = value;
            _type = type;
            if (isArray)
            {
                _valueType = ValueType.Array;
            }
            else
            {
                _valueType = M.ToValueType(_type);
            }
        }

        public DeclarationStatement(string type, Expression value) : this(type, value, false){}

        public override string ToString()
        {
            return string.Format("{0} {1}{2}", _type, _name, _value == null ? ";" : " = " + _value + ";");
        }

        public override string QTranslate()
        {
            switch (declarationType)
            {
                case "multi":
                    StringBuilder sb = new StringBuilder();
                    foreach (Expression e in (_value as CommaExpression).Expressions)
                    {
                        sb.Append(_QTranslate_itemOfMulti(e));
                    }
                    return sb.ToString();
                case "single":
                    return _QTranslate_single(_value as Data);
                case "singleWithInit":
                    return _QTranslate_singleWithInit(_value as AssignmentExpression);
                default:
                    throw new InterpretException(Context,"无法识别的赋值种类");
            }
        }

        private string _QTranslate_itemOfMulti(Expression e)
        {
            if (_isConst) return "";

            if (e is Data) return _QTranslate_single(e as Data);
            else if (e is AssignmentExpression) return _QTranslate_singleWithInit(e as AssignmentExpression);
            else throw new InterpretException(Context,"..........");
        }

        private string _QTranslate_single(Data d)
        {
            if (_isStatic) return "";
            if (!G.GlobalVariables.ContainsValue(d.RefVar))
            {
                if (!G.IsDebugMode) return string.Format("Dim {0} : ", d.RefVar.Mixcode);
                else return string.Format("Dim lu_{0} : ", d.Value);
            }
            return "";
        }
        private string _QTranslate_singleWithInit(AssignmentExpression a)
        {
            if (_isConst) return "";

            string prefix,postfix;
            string value = a.QTranslate(out prefix, out postfix);
            string dimmer = "";
            if (!_isStatic && !G.GlobalVariables.ContainsValue((a.E1 as Data).RefVar))
            {
                dimmer = string.Format("Dim {0} : ", (a.E1 as Data).RefVar.FinalName);
            }

            return string.Format("{0}{1}", dimmer, prefix + postfix);
        }

        private void compileAssignmentExpression(AssignmentExpression a, Function function)
        {
            Variable var;
            if (a.E1 is Data)
            {
                if (!a.Symbol.Equals("=")) throw new InterpretException(Context, "初始化的赋值表达式只能用'='");
                if (!_regex_variable.IsMatch(a.E1.Value)) throw new InterpretException(Context,"不是有效的变量名称");
                if (function.LocalVariables.ContainsKey(a.E1.Value)) 
                    throw new InterpretException(Context,"重复定义!");
                if (_isConst)
                {
                    a.E2.Compile(function);
                    switch (_type)
                    {
                        case "num":
                            if(!a.E2.ExpressionAttributes.HasFlag(ExpressionAttrs.NumericConst))
                                throw new InterpretException(Context, "{0}不是数字常数",a.E2);
                            break;
                        case "bool":
                            if(!a.E2.ExpressionAttributes.HasFlag(ExpressionAttrs.BooleanConst))
                                throw new InterpretException(Context, "{0}不是布尔常数",a.E2);
                            break;
                        case "string":
                            if(!a.E2.ExpressionAttributes.HasFlag(ExpressionAttrs.StringConst))
                                throw new InterpretException(Context, "{0}不是字符串常数",a.E2);
                            break;
                        default: 
                            throw new InterpretException(Context, "({0}):常数不支持这种类型");
                    }
                    var = new ConstantVariable(_type, a.E1.Value, Context, a.E2.Value);
                }
                else if (_isArray)
                {
                    var = new Array(a.E1.Value, Context, _type);
                }
                else var = new Variable(_type, a.E1.Value, Context);

                function.LocalVariables[a.E1.Value] = var;
                if (_isStatic) G.StaticVars.Add(var);

            }
            else throw new InterpretException(Context,"赋值表达式左项必须是一个有效的变量声明！");
            a.Compile(function);
        }

        private void compileData(Data e, Function function)
        {
            if (_isConst) throw new InterpretException(Context,"({0}):常数必须赋初值！",this);

            if (!_regex_variable.IsMatch(e.Value)) throw new InterpretException(Context,"不是有效的变量名称");
            if(function.LocalVariables.ContainsKey(e.Value))
                throw new InterpretException(Context,"重复定义!");
            Variable var = e.RefVar;
            var = _isArray ? new Array(e.Value, Context, _type) 
                : 
                new Variable(_type, e.Value, Context);
            function.LocalVariables[e.Value] = e.RefVar = var;
            if (_isStatic) G.StaticVars.Add(e.RefVar);
            //e.IsVariable = true;
            e.IsCompiled = true;
            e.ValueType = _valueType;
        }

        public override void Compile(Function function)
        {
            AssignmentExpression a;
            if (_value is CommaExpression)
            {
                foreach (Expression e in (_value as CommaExpression).Expressions)
                {
                    if (e is AssignmentExpression)
                    {
                        a = e as AssignmentExpression;
                        compileAssignmentExpression(a, function);
                    }
                    else if (e is Data)
                    {
                        compileData(e as Data, function);
                    }
                    else throw new InterpretException(Context,"不是有效的声明表达式");
                }
                declarationType = "multi";
            }
            else if (_value is AssignmentExpression)
            {
                compileAssignmentExpression(_value as AssignmentExpression, function);
                declarationType = "singleWithInit";
            }
            else if (_value is Data)
            {
                compileData(_value as Data, function);
                declarationType = "single";
            }
            else throw new InterpretException(Context,"不支持的类型！");
        }
    }
}
