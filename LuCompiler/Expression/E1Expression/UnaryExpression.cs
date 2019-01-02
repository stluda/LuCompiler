using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class UnaryExpression : E1Expression
    {
        private string _finalValue;

        public UnaryExpression(Expression e,string symbol)
            : this(e, Symbol.GetInstance(symbol,typeof(UnaryExpression)))
        {
        }

        public UnaryExpression(Expression e, Symbol symbol) : base(e,symbol)
        {
            Value = string.Format("{0}{1}", Symbol, E); 
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            switch (Symbol.Value)
            {
                case "@":
                    prefixStatement = "";
                    interValFlag = false;
                    postfixStatement = "";
                    return _finalValue;
            }

            string value = E.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
            switch (Symbol.Value)
            {
                case "!":
                    value = value.StartsWith("NOT(") ? value.Substring(3, value.Length - 4) : "NOT(" + value + ")";
                    break;
                case "++":
                case "--":
                    prefixStatement += string.Format("{0}={0}{1}1 : ", value, Symbol.Value[0]);
                    break;
                case "+":
                case "-":
                    value = Symbol + value;
                    break;
            }            
            return value;
        }

        public override void Compile(Function function)
        {
            switch (Symbol.Value)
            {
                case "@":
                    string nameOfAEObject = E.Value.ToLower();
                    string errmsg;
                    AEObject obj = Dict.GetAEObject(nameOfAEObject,out errmsg);
                    if (obj == null) 
                        throw new InterpretException(Context, "不是有效的域/元素名");
                    _finalValue = Dict.IndexOfAEObject[obj].ToString();
                    ValueType = ValueType.Numeric;
                    _isCompiled = true;
                    return;
            }

            E.Compile(function);
            switch (Symbol.Value)
            {
                case "!":
                    if (!E.ValueType.HasFlag(ValueType.Boolean)) 
                        throw new InterpretException(Context,"非连接符'!'后面必须是布尔类型的表达式！");
                    ValueType = ValueType.Boolean;
                    IsCompiled = true;
                    break;
                case "++":
                case "--":
                    if (E is Data)
                    {
                        Variable var = (E as Data).RefVar;
                        if (var == null) throw new InterpretException(Context,"自加(减)连接符后面必须跟随变量！");
                        if (var is Function) throw new InterpretException(Context,"函数不能被自加(减)！");
                    }
                    else throw new InterpretException(Context,"自加(减)连接符后面必须跟随变量！");
                    if ((E.ValueType & ValueType.Numeric) != ValueType.Numeric) throw new InterpretException(Context,"自加(减)连接符后面必须跟随数字类型的变量！");
                    ValueType = ValueType.Numeric;
                    IsCompiled = true;
                    break;
                case "-":
                case "+":
                    if ((E.ValueType & ValueType.Numeric) != ValueType.Numeric) throw new InterpretException(Context,"...");
                    ValueType = ValueType.Numeric;
                    IsCompiled = true;
                    break;
            }
        }
    }
}
