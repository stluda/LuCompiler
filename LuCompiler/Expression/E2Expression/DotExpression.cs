using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class DotExpression : E2Expression
    {
        private string _translateType = "common";

        public DotExpression(Expression e1, Expression e2)
            : base(e1, e2, Symbol.GetInstance(".", typeof(DotExpression)))
        {        
        }

        public override void Compile(Function function)
        {            
            E1.Compile(function);            
            InterpretException exc = new InterpretException(Context, "不合法的表达式{0}！", this);
            if (E1.HasAttr(ExpressionAttrs.Leaf)) throw exc;
            else if (E1.ExpressionAttributes.HasFlag(ExpressionAttrs.Super))
            {
                if (E1.ExpressionAttributes.HasFlag(ExpressionAttrs.External))
                {
                    //Expression lastMember = GetLastMember();
                    ExpressionAttributes = E2.ExpressionAttributes = ExpressionAttrs.Super | ExpressionAttrs.External;
                    ValueType = ValueType.Super;
                    E2.RecursiveDo((Expression e) =>
                    {                        
                        e.ExpressionAttributes = ExpressionAttrs.Super | ExpressionAttrs.External | ExpressionAttrs.ObjectMember;
                        if (e is PostfixExpression)
                        {
                            PostfixExpression pe = e as PostfixExpression;
                            pe.E.ExpressionAttributes = ExpressionAttrs.Super | ExpressionAttrs.External | ExpressionAttrs.ObjectMember;
                            return false;
                        }
                        return true;
                    });
                    E2.Compile(function);
                }
                else throw exc;                
            }
            else
            {
                if (E1.ExpressionAttributes.HasFlag(ExpressionAttrs.Array)){
                    switch (E2.Value.ToLower())
                    {
                        case "length":
                            E2.IsCompiled = true;
                            _translateType = "array_length";
                            ExpressionAttributes = ExpressionAttrs.Integer | ExpressionAttrs.ReadOnly | ExpressionAttrs.Leaf;
                            ValueType = ValueType.Numeric;
                            break;
                        default:
                            throw exc;
                    }                    
                }
            }
        }

        public string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement, bool isSingleStatement)
        {
            switch (_translateType)
            {
                case "common":
                    if (E2 is PostfixExpression)
                    {
                        string statement1, statement2;
                        string pStatement1, pStatement2;
                        bool flag1, flag2;
                        string value = getCombinedValue(E1.QTranslate(out statement1, out flag1, out pStatement1), (E2 as PostfixExpression).QTranslate(out statement2, out flag2, out pStatement2, isSingleStatement));
                        interValFlag = flag1 || flag2;
                        prefixStatement = statement1 + statement2;
                        postfixStatement = pStatement1 + pStatement2;
                        return value;
                    }
                    else return base.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
                case "array_length":
                    E1.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
                    return string.Format("(ubound({0})+1)", E1.QTranslate());
                default:
                    throw new NotImplementedException();
            }
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            return QTranslate(out prefixStatement, out interValFlag, out postfixStatement, false);            
        }

        public Expression GetLastMember()
        {
            if (E2 is DotExpression)
            {
                return (E2 as DotExpression).GetLastMember();
            }
            else return E2;
        }

        public void ForEachObjectMember(Action<Expression> act)
        {
            act(E1);
            if (E2 is DotExpression) (E2 as DotExpression).ForEachObjectMember(act);
            else act(E2);
        }
    }
}
