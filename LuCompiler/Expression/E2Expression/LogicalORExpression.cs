using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class LogicalORExpression : E2Expression
    {
        public LogicalORExpression(Expression e1, Expression e2)
            : base(e1, e2, Symbol.GetInstance("||", typeof(LogicalORExpression)))
        {
        }

        public override void Compile(Function function)
        {
            E1.Compile(function);
            E2.Compile(function);
            if ((ValueType.Boolean & E1.ValueType) != ValueType.Boolean || (ValueType.Boolean & E2.ValueType) != ValueType.Boolean) throw new InterpretException(Context,"逻辑连接符两端表达式必须都是布尔类型！");
            ValueType = ValueType.Boolean;
            IsCompiled = true;
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            string statement1, statement2;
            string value1, value2;
            bool flag1, flag2;
            value1 = E1.QTranslate(out statement1, out flag1, out postfixStatement);
            value2 = E2.QTranslate(out statement2, out flag2, out postfixStatement);            
            if (flag2 || E2 is PostfixExpression && (E2 as PostfixExpression).HasPostfixExpression)
            {
                if (!G.IndexOfTranslatedInterval.ContainsKey(RowIndex)) G.IndexOfTranslatedInterval[RowIndex] = 0;
                string returnValue = string.Format("lu_{0}_{1}", RowIndex+1, G.IndexOfTranslatedInterval[RowIndex]++);
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("{0}Dim {1} : ", statement1, returnValue))
                    .Append(string.Format("{0} = True : ", returnValue))
                    .Append(string.Format("If {0} Then : {1}{2} = {3} : End If : ", value1.StartsWith("NOT(")?value1.Substring(4,value1.Length-5):"NOT("+value1+")"
                    , statement2, returnValue, value2));
                interValFlag = true;
                prefixStatement = sb.ToString();
                return returnValue;
            }
            else
            {
                prefixStatement = statement1 + statement2;
                interValFlag = flag1 || flag2;
                return string.Format("{0} OR {1}", value1, value2);
            }
        }

    }
}
