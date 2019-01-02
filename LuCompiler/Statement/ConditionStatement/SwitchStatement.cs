using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace LuCompiler
{
    public class SwitchStatement : Statement
    {
        private ValueType _switchType;
        private List<Statement> _statements = new List<Statement>();
        private List<List<Context>> _cases = new List<List<Context>>();
        private List<List<Expression>> _casesE = new List<List<Expression>>();

        private List<List<string>> _caseValues = new List<List<string>>();
        
        public Statement DefaultStatement { get; set; }
        private List<List<string>> _translatedCases;
        private bool _q_needClassify = false;
        private Expression _condition;
        private IfStatement _ifStatement;
        private Area _area;
        private OcrInAreaFunction _ocrInAreaFunction;
        private Function _function;
        private string _group;

        public SwitchStatement(Expression condition)
        {
            _condition = condition;
        }

        public SwitchStatement(Area area, string group)
        {
            _area = area;
            _group = group;           
        }

        private static Regex r_stateCase = new Regex(@"^'(\w+)'$");
        private static Regex r_areaOcrCase = new Regex(@"^(\w+)$");
        private static Regex r_stringCase = new Regex("^\"[^\"]*\"$");
        private static Regex r_numCase = new Regex("^\\-?[0-9]+(\\.[0-9]+)?$");
        private static Regex r_elementCase = new Regex(@"^@(\w+)$");

        private bool _checkIsValidCase(string caseValue, ValueType valueType,out string translatedValue)
        {
            translatedValue = caseValue;
            switch (valueType)
            {
                case ValueType.String:
                    return r_stringCase.IsMatch(caseValue);
                case ValueType.Numeric:
                    if (r_elementCase.IsMatch(caseValue))
                    {
                        string nameOfAEObject = caseValue.TrimStart('@');
                        string errmsg;
                        AEObject obj = Dict.GetAEObject(nameOfAEObject, out errmsg);
                        if (obj == null)
                            throw new InterpretException(Context, "不是有效的域/元素名");
                        translatedValue = Dict.IndexOfAEObject[obj].ToString();
                        return true;
                    }

                    ConstantVariable constVar;
                    if ((constVar = _function.LocalVariables[caseValue] as ConstantVariable) != null)
                    {
                        translatedValue = constVar.FinalName;
                        return true;
                    }
                    else if ((constVar = G.GlobalVariables[caseValue] as ConstantVariable) != null)
                    {
                        translatedValue = constVar.FinalName;
                        return true;
                    }
                    else return r_numCase.IsMatch(caseValue);
            }
            return false;
        }

        public override void Compile(Function function)
        {
            _function = function;
            bool exflag = false;
            Match m;
            if (_area != null)
            {
                _ocrInAreaFunction = OcrInAreaFunction.GetInstance(Context, _area, _group);
                OcrStringInfo info = _area.NameOfCategory[_group];
                Hashtable<string, OcrStringInfo> ocrInfoOfGroup = _area.DictCategory[info];               
                _translatedCases = new List<List<string>>();
                IfStatement partialIf = null;
                for (int i = 0, length = _statements.Count; i < length; i++)
                {
                    List<Context> casegroup = _cases[i];
                    
                    
                    
                    if (casegroup.Count > 1) _q_needClassify = true;
                    Statement statement = _statements[i];
                    statement.Compile(function);
                    List<string> aTranslatedCaseGroup = new List<string>();
                    Expression resultCondition = null;
                    foreach (Context context in casegroup)
                    {
                        string key = context.Value;
                        if (M.SetExflag(!(m = r_areaOcrCase.Match(key)).Success, ref exflag)) Dict.AddException(new InterpretException(context, "不是合法的元素类型case格式"),this);
                        if (!exflag)
                        {                            
                            string eCase = key;//.TrimStart('@');
                            if (M.SetExflag(!ocrInfoOfGroup.ContainsKey(eCase), ref exflag)) Dict.AddException(new InterpretException(context
                                ,string.Format("域{0}的组[{1}]不存在元素{2}",_area,_group,eCase)));
                            if (!exflag)
                            {
                                OcrStringInfo ocrInfo = ocrInfoOfGroup[eCase];
                                aTranslatedCaseGroup.Add(ocrInfo.FindStr);
                                Expression exp = new ExternalExpression(string.Format("Instr({0},{1})<>0", M.EVar("g_temp"), ocrInfo.FindStr));
                                //Expression exp = new PostfixExpression(new ExternalExpression("Instr"), Symbol.GetInstance("(", typeof(PostfixExpression)), CommaExpression.Parse(context,ocrInfo.FindStr, "$g_temp")) { Context = context };
                                exp.Compile(function);
                                if (resultCondition == null) resultCondition = exp;
                                else resultCondition = new LogicalORExpression(resultCondition, exp) {Context = context };
                            }
                        }
                    }
                    
                    if (!exflag)
                    {
                        if (_ifStatement == null)
                        {
                            partialIf=_ifStatement = new IfStatement(resultCondition, statement);
                            _ifStatement.Context = casegroup.Last();
                        }
                        else
                        {
                            IfStatement temp = partialIf;
                            partialIf = new IfStatement(resultCondition, statement);
                            temp.ElseStatement = partialIf;
                            partialIf.Context = casegroup.Last();
                        }
                        _translatedCases.Add(aTranslatedCaseGroup);
                    }
                }
                if (!exflag)
                {
                    _ifStatement.Compile(function);
                    if (DefaultStatement != null) DefaultStatement.Compile(function);
                }
                return;
            }
            #region normal
            _condition.Compile(function);
            ValueType type = _condition.ValueType;
            switch (type)
            {
                case ValueType.String:
                case ValueType.Numeric:
                    for (int i = 0, length = _statements.Count; i < length; i++)
                    {
                        List<Context> casegroup = _cases[i];

                        List<string> caseValues = new List<string>();
                        string translatedValue;

                        if (casegroup.Count > 1) _q_needClassify = true;
                        foreach (Context context in casegroup)
                        {
                            string key = context.Value;
                            if (!_checkIsValidCase(context.Value, type, out translatedValue)) Dict.AddException(new InterpretException(context, "case类型值不匹配"), this);
                            caseValues.Add(translatedValue);
                        }
                        _statements[i].Compile(function);
                        if (DefaultStatement != null) DefaultStatement.Compile(function);
                        _caseValues.Add(caseValues);
                    }
                    break;
                case ValueType.State:
                    #region state
                    Data d = _condition as Data;
                    State state = d.RefVar as State;
                    IfStatement ifs = null;
                    IfStatement partialIf = null;
                    for (int i=0,length=_statements.Count;i<length;i++)
                    {
                        List<Context> casegroup = _cases[i];
                        Statement statement = _statements[i];
                        Expression resultCondition=null;
                        foreach (Context context in casegroup)
                        {
                            string key = context.Value;
                            if (!(m = r_stateCase.Match(key)).Success) Dict.AddException(new InterpretException(context,"state类型的case请用''括起"),this);
                            if (!state.ExpressionOfStates.ContainsKey(key=m.Groups[1].Value)) Dict.AddException(new InterpretException(context,"该state类型不包含该键值"),this);
                            if (resultCondition == null) resultCondition = state.ExpressionOfStates[key];
                            else resultCondition = new LogicalORExpression(resultCondition,state.ExpressionOfStates[key]){
                                Context = context
                            };
                        }
                        
                        if (ifs == null)
                        {
                            partialIf = ifs = new IfStatement(resultCondition, statement);
                            ifs.Context = casegroup.Last();
                        }
                        else
                        {
                            IfStatement temp = partialIf;
                            partialIf = new IfStatement(resultCondition, statement);
                            temp.ElseStatement = partialIf;
                            partialIf.Context = casegroup.Last();
                        }
                        //ifs.Compile(function);
                    }
                    _ifStatement = ifs;
                    _ifStatement.Compile(function);
                    break;
                    #endregion
                default:
                    Dict.AddException(new InterpretException(Context,"该类型不支持switch语句"),this);
                    break;
            }
            _switchType = _condition.ValueType;
            #endregion
        }

        public void Add(List<Context> list, Statement statement)
        {
            _statements.Add(statement);
            _cases.Add(list);
        }
        public void Add(Context key, Statement statement)
        {
            _statements.Add(statement);
            _cases.Add(new List<Context>() { key });
        }



        public void Add(List<Expression> list, Statement statement)
        {
            _statements.Add(statement);
            _casesE.Add(list);
        }
        public void Add(Expression key, Statement statement)
        {
            _statements.Add(statement);
            _casesE.Add(new List<Expression>() { key });
        }

        
        public override string QTranslate()
        {
            if (_area != null) 
                return string.Format("{0} = {1} : {2}",M.EVar("g_temp"),_ocrInAreaFunction.S_FunctionName,_ifStatement.QTranslate());
            switch (_switchType)
            {
                case ValueType.State:
                    return _ifStatement.QTranslate();
                case ValueType.Numeric:
                case ValueType.String:
                    string prefix, condition, postfix;
                    condition = _condition.QTranslate(out prefix, out postfix);
                    return _classicalSwitch_QTranslate(prefix, condition, postfix, null);
                default:
                    throw new InterpretException(Context,"尚不支持");
            }
            
        }

        public string _ocrArea_QTranslate()
        {
            return "";
            //return _classicalSwitch_QTranslate("", _ocrInAreaFunction.S_FunctionName, "", _translatedCases);

        }

        public string _classicalSwitch_QTranslate(string prefix, string condition, string postfix, List<List<string>> cases)
        {
            StringBuilder sb = new StringBuilder();
            string rCond, rPrefix;
            M.Q_ConditionMix(out rCond, out rPrefix, prefix, condition, postfix);
            
            if (_q_needClassify)
            {
                string tempSave = M.GetATempVar();
                sb.AppendLine(string.Format("{0} = \"$default$\" : ",tempSave));
                sb.Append(rPrefix).Append(string.Format("Select Case {0} : ", rCond));
                int caseCount = _statements.Count;              
                Action<StringBuilder, IEnumerable, int> setCases =
                    new Action<StringBuilder, IEnumerable, int>(delegate(StringBuilder builder, IEnumerable ie, int index)
                    {
                        foreach (object o in ie)
                        {
                            sb.Append(string.Format("Case {0} : {1}={2} : ", o,tempSave,index));
                        }
                    });
                for (int i = 0; i < caseCount; i++)
                {
                    if (cases != null) setCases(sb, cases[i], i);
                    else setCases(sb, _caseValues[i], i);
                }                
                sb.AppendLine("End Select");
                sb.AppendLine(string.Format("Select Case {0}", tempSave));
                for (int i = 0; i < caseCount; i++)
                {
                    sb.AppendLine(string.Format("Case {0} : ", i));
                    sb.AppendLine(M.Q_AddTab(_statements[i].QTranslate()));
                }
                if (DefaultStatement != null)
                {
                    sb.AppendLine("Case Else : ");
                    sb.AppendLine(M.Q_AddTab(DefaultStatement.QTranslate()));
                }

            }
            else
            {
                sb.Append(rPrefix).AppendLine(string.Format("Select Case {0}", rCond));
                Action<StringBuilder, IEnumerable,int> setCases = new Action<StringBuilder, IEnumerable,int>(
                    delegate(StringBuilder builder, IEnumerable ie, int index)
                    {
                        IEnumerator ienu = ie.GetEnumerator();
                        ienu.MoveNext();
                        sb.AppendLine(string.Format("Case {0} : ", ienu.Current));
                        sb.AppendLine(M.Q_AddTab(_statements[index].QTranslate()));
                    });
                if (cases != null) for (int i = 0, length = cases.Count; i < length; i++) setCases(sb, cases[i], i);
                else for (int i = 0, length = _cases.Count; i < length; i++) setCases(sb, _caseValues[i], i);
                if (DefaultStatement != null)
                {
                    sb.AppendLine("Case Else : ");
                    sb.AppendLine(M.Q_AddTab(DefaultStatement.QTranslate()));
                }
            }
            sb.AppendLine("End Select");
            return sb.ToString();
        }
            
    }
}
