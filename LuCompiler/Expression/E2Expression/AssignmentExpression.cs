using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LuCompiler
{
    public class AssignmentExpression : E2Expression
    {
        private static Regex _regex_variable = new Regex(@"^[a-zA-Z]\w*$");
        private bool _ifE2IsDataAndIsFunctionCall = false;

        public AssignmentExpression(Expression e1, Expression e2, Symbol symbol)
            : base(e1,e2,symbol)
        {
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            string statement1, statement2;
            string value1, value2;
            bool flag1, flag2;
            value1 = E1.QTranslate(out statement1, out flag1, out postfixStatement);
            value2 = E2.QTranslate(out statement2, out flag2, out postfixStatement);
            //if (_ifE2IsDataAndIsFunctionCall) value2 += "()";
            string partialPrefixStatement;
            switch (Symbol.Value)
            {
                case "=":
                    partialPrefixStatement = string.Format("{0}={1} : ", value1, value2);
                    break;
                case "+=":
                case "-=":
                case "*=":
                case "/=":
                    partialPrefixStatement = string.Format("{0}={0}{1}{2} : ", value1, Symbol.Value[0],value2);
                    break;
                case "%=":
                    partialPrefixStatement = string.Format("{0}={0} mod {1} : ", value1, value2);
                    break;
                case "&&=":
                    partialPrefixStatement = string.Format("If {0} Then : {0}={1} : End If :", value1, value2);
                    break;
                case "||=":
                    partialPrefixStatement = string.Format("If NOT({0}) Then : {0}={1} : End If :", value1, value2);
                    break;
                default:
                    throw new InterpretException(Context,".....");
            }
            prefixStatement = statement1 + statement2 + partialPrefixStatement;
            interValFlag = false;
            return value1;
        }

        public override void Compile(Function function)
        {
            Data d;           
            E2.Compile(function);
            Action readOnlyCheck = ()=>
            {
                if (E1.ExpressionAttributes.HasFlag(ExpressionAttrs.ReadOnly)) throw new InterpretException(Context, "{0}不可以被赋值，因为它是只读的",E1);
            };

            if (E1 is Data)
            {
                Variable var;
                if (E1.Value.StartsWith("$")) E1.Compile(function);
                else if (!_regex_variable.IsMatch(E1.Value)) throw new InterpretException(Context, "不是有效的变量名称");
                if (function.LocalVariables.ContainsKey(E1.Value)) var = function.LocalVariables[E1.Value];
                else if (G.GlobalVariables.ContainsKey(E1.Value)) var = G.GlobalVariables[E1.Value];
                else
                {
                    G.GlobalVariables[E1.Value] = var = new Variable("var", E1.Value, E1.Context);
                }
                if (var is Function) throw new InterpretException(Context, "函数不可被赋值！");                
                E1.Compile(function);
                readOnlyCheck();
                try
                {
                    var.Type = this.ValueType = E1.ValueType = E2.ValueType;
                }
                catch (InterpretException ie)
                {
                    //Dict.AddException(ie, this);
                    throw new InterpretException(Context, ie.Message);
                }
                d = E1 as Data;
                if (d.IsResult) function.Type = this.ValueType;

            }
            else if(E1 is PostfixExpression)
            {
                E1.Compile(function);
                if (!E1.ExpressionAttributes.HasFlag(ExpressionAttrs.Variable))throw new InterpretException(Context, "只有变量，数组下标引用可以被赋值");
                if ((E1.ValueType&E2.ValueType)==ValueType.None)
                    throw new InterpretException(Context, "赋值类型不匹配");
                
                readOnlyCheck();
                this.ValueType = E2.ValueType;
            }
            else if (E1 is DotExpression)
            {
                readOnlyCheck();
                //if(E1.ExpressionAttributes.HasFlag(ExpressionAttributes.ReadOnly))
            }
            else throw new InterpretException(Context, "不支持的类型");
            _isCompiled = true;

        }

    }
}
