using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class PostfixExpression : E1Expression
    {
        private AEFunction _externalRefFunc;
        private bool _isFunctionCall = false;
        private Expression _postfixExpression;
        private AEObject _aeObject;
        private bool _isMacro = false;
        private Statement _macroStatement = null;
        private CommaExpression _truePostE;
        public bool IsMacro
        {
            get { return _isMacro; }
        }

        public bool HasPostfixExpression
        {
            get
            {
                return _postfixExpression != null;
            }
        }
        public bool IsFunctionCall
        {
            get { return _isFunctionCall; }
        }
        public Expression PostE
        {
            get { return _postfixExpression; }
            set { _postfixExpression = value; }
        }

        public PostfixExpression(Expression e, Symbol symbol) : base(e,symbol)
        {
            Value = string.Format("{0}{1}", e, symbol);
        }

        public PostfixExpression(Expression e, string symbol, Expression postfixExpression)
            : this(e, Symbol.GetInstance(symbol, typeof(PostfixExpression)),postfixExpression) 
        { 

        }

        public PostfixExpression(Expression e, Symbol symbol, Expression postfixExpression)
            : base(e,symbol)
        {
            PostE = postfixExpression;
            string eValue = e.Value;
            if (!(e is Data)) eValue = string.Format("({0})", eValue);
            Value = string.Format("{3}{0}{1}{2}", symbol, postfixExpression, symbol.ToReserve(), eValue);
        }

        public override void Compile(Function function)
        {
            bool flag=true;
            Variable var=null;
            if(!(E is Data))flag = false;
            if (E.Value.StartsWith("$")) flag = true;
            else if (function.LocalVariables.ContainsKey(E.Value)) var = function.LocalVariables[E.Value];
            else if (G.GlobalVariables.ContainsKey(E.Value)) var = G.GlobalVariables[E.Value];
            else flag = false;
            string type;
            switch (Symbol.Value)
            {
                #region macro
                case "<":
                case "(":
                    Macro macro = Dict.MacroOfKey[E.Value];
                    if (macro != null)
                    {
                        _macroStatement = macro.GetStatement(this);
                        if (_macroStatement == null) break;
                        _macroStatement.Compile(function);
                        if (_macroStatement.HasException)
                            throw new InterpretException(
                                Context, string.Format("在翻译宏[{0}]的过程中发生了异常", macro.Key));
                        _isMacro = true;
                        _isCompiled = true;
                        return;
                    }
                    break;
                #endregion
            }

            switch (Symbol.Value)
            {
                case "<":          
                    type = E.Value.ToLower();
                    if (type == "")
                    {
                        string[] args = PostE.ToString().Split('_');
                        switch (args.Length)
                        {
                            case 1:
                                E.Value = type = "finda";
                                break;
                            case 2:
                                E.Value = type = "finde";
                                break;
                            default:
                                Dict.AddException(new InterpretException(Context, "无法识别的元素类型"), this);
                                break;
                        }
                    }
                    string errmsg;
                    switch (type)
                    {
                        case "ocra":
                            string[] args = PostE.ToString().Split(',');
                            if (args.Length != 2) throw new InterpretException(Context, "OCRA参数个数不正确！");
                            Area area = Dict.GetArea(args[0], out errmsg);
                            if (area == null) throw new InterpretException(Context, errmsg);
                            _externalRefFunc = OcrInAreaFunction.GetInstance(Context, area, args[1].ToLower());
                            E.IsCompiled = true;
                            PostE.IsCompiled = true;
                            ValueType = ValueType.String;
                            break;
                        case "finde":
                        case "finda":
                        case "fclicke":
                        case "fdbclicke":
                            E.IsCompiled = true;
                            PostE.IsCompiled = true;


                            args = PostE.ToString().Split(',');
                            CommaExpression cma;

                            Action<Expression> checkElement = (Expression e) =>
                            {
                                _aeObject = Dict.GetAEObject(e.ToString(), out errmsg);
                                if (_aeObject == null) throw new InterpretException(Context, errmsg);
                                _externalRefFunc = AEFunction.GetInstance(Context, type, _aeObject);
                                ValueType = ValueType.Boolean;
                            };

                            switch (args.Length)
                            {
                                case 1:
                                    checkElement(PostE);
                                    _truePostE = new CommaExpression(M.GetFindPicDelayExpression());
                                    break;
                                case 2:
                                    cma = PostE as CommaExpression;
                                    checkElement(cma.Expressions[0]);
                                    _truePostE = new CommaExpression(cma.Expressions[1]);
                                    break;
                                default:
                                    throw new InterpretException(Context, "find参数个数不正确哦");
                            }
                            _truePostE.Compile(function);
                            break;
                        case "clicke":
                        case "dbclicke":
                        case "movee":                            
                            E.IsCompiled = true;
                            PostE.IsCompiled = true;
                            _aeObject = Dict.GetAEObject(PostE.ToString(), out errmsg);
                            if (!(_aeObject is Element)) 
                                throw new InterpretException(Context, "{0}不是元素",PostE);
                            if (_aeObject == null) throw new InterpretException(Context, errmsg);
                            ValueType = ValueType.None;
                            break;
                        case "getstr":
                            E.IsCompiled = true;
                            PostE.IsCompiled = true;
                            _aeObject = Dict.GetAEObject(PostE.ToString(), out errmsg);
                            if (!(_aeObject is Element)) throw new InterpretException(Context, "{0}不是元素", E);
                            if (_aeObject == null) throw new InterpretException(Context, errmsg);
                            ValueType = ValueType.String;
                            break;
                        case "width":
                        case "height":
                            E.IsCompiled = true;
                            PostE.IsCompiled = true;
                            _aeObject = Dict.GetArea(PostE.ToString(), out errmsg);
                            if (_aeObject == null) throw new InterpretException(Context, errmsg);
                            ValueType = ValueType.Numeric;
                            break;
                        case "findstr":
                            args = PostE.ToString().Split(',');

                            cma = PostE as CommaExpression;
                            if (cma == null || cma.Expressions.Count != 7) throw new InterpretException(Context, "FindStr参数个数不正确！");

                            for (int i = 1; i <= 6; i++) cma.Expressions[i].Compile(function);
                            if (!cma.Expressions[1].ValueType.HasFlag(ValueType.String))
                                new InterpretException(Context, "FindStr第一个参数(查询字符串)必须为字符串类型！");
                            if (!cma.Expressions[2].ValueType.HasFlag(ValueType.String))
                                new InterpretException(Context, "FindStr第二个参数(颜色信息)必须为字符串类型！");
                            if (!cma.Expressions[3].ValueType.HasFlag(ValueType.Numeric))
                                new InterpretException(Context, "FindStr第三个参数(相似度)必须为数字类型！");
                            if (!cma.Expressions[4].ValueType.HasFlag(ValueType.String))
                                new InterpretException(Context, "FindStr第四个参数(字体名称)必须为字符串类型！");
                            if (!cma.Expressions[5].ValueType.HasFlag(ValueType.Numeric))
                                new InterpretException(Context, "FindStr第五个参数(字体大小)必须为数字类型！");
                            if (!cma.Expressions[6].ValueType.HasFlag(ValueType.Numeric))
                                new InterpretException(Context, "FindStr第六个参数(字体参数)必须为数字类型！");

                            AEObject obj = Dict.GetAEObject(args[0], out errmsg);
                            if (obj == null) throw new InterpretException(Context, errmsg);
                            _externalRefFunc = FindStrFunction.GetInstance(Context, args[0].ToLower());
                            E.IsCompiled = true;
                            PostE.IsCompiled = true;
                            ValueType = ValueType.Boolean;
                            _truePostE = new CommaExpression(cma.Expressions[1]);
                            for (int i = 2; i <= 6; i++) _truePostE.Add(cma.Expressions[i]);
                            _truePostE.Compile(function);
                            break;
                        default:
                            throw new InterpretException(Context,"无法识别的A<B>函数");
                    }                    
                    _isFunctionCall = true;
                    break;           
                case "(":
                    type = E.Value.ToLower();

                    switch (type)
                    {
                        case "delay":
                            E.Value = "$delay";
                            break;
                        case "array":
                            E.Value = "$array";
                            break;
                    }
                    switch (type)
                    {
                        case "find":
                        case "finda":                            
                        case "finde":
                        case "fclicke":
                        case "fdbclicke":
                        case "clicke":
                        case "dbclicke":
                            {
                                switch (type)
                                {
                                    case "find":
                                    case "finda":
                                    case "finde":
                                        _externalRefFunc = G.AEFunctions["find"];
                                        break;
                                    case "fclicke":
                                    case "fdbclicke":
                                    case "clicke":
                                    case "dbclicke":
                                        _externalRefFunc = G.AEFunctions[type];
                                        break;
                                }
                                E.Compile(function);
                                PostE.Compile(function);
                                string[] args = PostE.Value.Split(',');
                                Action<Expression> checkExp = (Expression e) =>
                                    { if (!e.ValueType.HasFlag(ValueType.Numeric)) throw new InterpretException(Context, "AE函数传入值必须是数字(可转义)类型"); };

                                switch (args.Length)
                                {
                                    case 1:
                                        checkExp(PostE);
                                        _truePostE = new CommaExpression(PostE);
                                        _truePostE.Add(M.GetFindPicDelayExpression());
                                        _truePostE.Compile(function);
                                        break;
                                    case 2:
                                        CommaExpression cma = PostE as CommaExpression;
                                        checkExp(cma.Expressions[0]);
                                        checkExp(cma.Expressions[1]);
                                        _truePostE = cma;
                                       
                                        break;
                                    default:
                                        throw new InterpretException(Context, "传入的参数个数不匹配！");
                                }
                                switch (type)
                                {
                                    case "find":
                                    case "finda":
                                    case "finde":
                                    case "fclicke":
                                    case "fdbclicke":
                                        ValueType = ValueType.Boolean;
                                        break;
                                    case "clicke":
                                    case "dbclicke":
                                        ValueType = ValueType.None;
                                        break;
                                }
                            }
                            break;
                        case "log":
                            break;
                        case "trace":
                            break;
                        default:
                            E.Compile(function);
                            bool isExternal = E.ExpressionAttributes.HasFlag(ExpressionAttrs.External);
                            /*if (!isExternal && !flag) 
                                throw new InterpretException(Context, "找不到该函数的声明");*/
                            if (!E.ExpressionAttributes.HasFlag(ExpressionAttrs.FunctionReference))
                                throw new InterpretException(Context, string.Format("'{0}'不是函数/函数引用！", var.Name));
                            _isFunctionCall = true;
                            int argCount = 0;
                            if (PostE != null)
                            {
                                PostE.Compile(function);
                                if (PostE is CommaExpression) argCount = (PostE as CommaExpression).Expressions.Count;
                                else argCount = 1;
                            }
                            if (isExternal) ValueType = ValueType.Super;
                            else
                            {
                                if (E.ExpressionAttributes.HasFlag(ExpressionAttrs.Function))
                                {
                                    Function func = E.RefVar as Function;
                                    _refVar = func;
                                    if (func.ArgCount != argCount) throw new InterpretException(Context, "传入参数个数不匹配");
                                    ValueType = func.Type;
                                }
                                else
                                {
                                    _expressionAttributes |= ExpressionAttrs.Super;
                                    ValueType = ValueType.Super;
                                }
                            }
                            break;
                    }            
                    break;
                case "[":
                    switch (E.Value.ToLower())
                    {
                        case "array":
                            #region intializer of array
                            PostE.Compile(function);
                            if (PostE is CommaExpression) throw new InterpretException(Context, "暂不支持！");
                            if (!PostE.ValueType.HasFlag(ValueType.Numeric)) throw new InterpretException(Context, "数组的初始化大小必须是一个数字！");
                            ValueType = ValueType.Array;
                            break;
                            #endregion
                        case "map":
                            break;
                        default:
                            #region normal array of map
                            bool isValid = true;
                            E.Compile(function);
                            switch (E.ValueType)
                            {
                                case ValueType.Super:
                                    //E.Compile(function);
                                    PostE.Compile(function);
                                    ValueType = ValueType.Super;
                                    break;
                                case ValueType.Array:
                                    //E.Compile(function);
                                    if (!E.ExpressionAttributes.HasFlag(ExpressionAttrs.Array)) throw new InterpretException(Context, string.Format("{0}不是一个数组", E));
                                    Array array = E.RefVar as Array;
                                    _refVar = array;
                                    if (E is Data)
                                    {
                                        ValueType = array.ItemValueType;
                                    }
                                    else
                                    {
                                        if (array.ItemValueType == ValueType.Array) ValueType = ValueType.Super;
                                        else ValueType = array.ItemValueType;
                                    }
                                    ExpressionAttributes = ExpressionAttrs.Variable | M.ToExpressionAttribute(ValueType);
                                    
                                    PostE.Compile(function);
                                    PostE.RecursiveDo((Expression e) =>
                                    {
                                        if (!(e is CommaExpression))
                                        {
                                            isValid = isValid && (e.ValueType.HasFlag(ValueType.Numeric));
                                        }
                                        return true;
                                    }, 1);
                                    if (!isValid) throw new InterpretException(Context, string.Format("数组{0}里面的每个成员必须都是数字类型！", E));
                                    break;
                                default:
                                    throw new InterpretException(Context, string.Format("{0}不是一个集合！", E));
                            }                            
                            break;
                            #endregion
                    }
                    _isCompiled = true;
                    break;
                case "++":
                case "--":
                    if (!E.Value.StartsWith("$"))
                    {
                        if (E is Data)
                        {
                            if (var == null) throw new InterpretException(Context,"自加(减)连接符后面必须跟随变量！");
                            if (var is Function) throw new InterpretException(Context,"函数不能被自加(减)！");
                        }
                        else throw new InterpretException(Context,"自增(减)表达式目前只适用于变量！");
                        if (!flag) new InterpretException(Context, string.Format("找不到变量'{0}'", E));
                        if (var.Type != ValueType.Numeric) throw new InterpretException(Context,"只有数字类型的变量才能自增(减)");
                    }
                    ValueType = ValueType.Numeric;
                    E.Compile(function);
                    break;
            }            
            IsCompiled = true;            
        }

        public string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement, bool isSingleStatement)
        {
            prefixStatement = "";
            postfixStatement = "";
            interValFlag = false;
            if (_isMacro)
                return _macroStatement.QTranslate();

            string ret = "";
            string partialPrefixStatement;
            string postfixStatement2;
            switch (Symbol.Value)
            {
                case "<":
                    #region A<B>
                    switch (E.Value.ToLower())
                    {
                        case "width":
                            ret = (_aeObject as Area).S_width;
                            break;
                        case "height":
                            ret = (_aeObject as Area).S_height;
                            break;
                        case "finde":
                        case "finda":
                        case "fclicke":
                        case "fdbclicke":
                            partialPrefixStatement = "";
                            postfixStatement2 = "";
                            if (isSingleStatement)
                                ret = string.Format("{0} {1}", _externalRefFunc.S_FunctionName,_truePostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2));
                            else
                                ret = string.Format("{0}({1})", _externalRefFunc.S_FunctionName, _truePostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2));
                            prefixStatement += partialPrefixStatement;
                            interValFlag = true;
                            postfixStatement = postfixStatement2 + postfixStatement;
                            break;
                        case "ocra":
                            ret = _externalRefFunc.S_FunctionName;
                            break;
                        case "clicke":
                            Element element = _aeObject as Element;
                            ret = string.Format("click {0},{1} : {2}", element.S_x, element.S_y, M.GetDebugLog("\"Click {2}:\"&{0}&\",\"&{1}&\" :\"", DebugLevel.AllDetailLevel, element.S_x, element.S_y, _aeObject.FullName));
                            break;
                        case "dbclicke":
                            element = _aeObject as Element;
                            ret = string.Format("dbclick {0},{1} : {2}", element.S_x, element.S_y, M.GetDebugLog("\"DBClick {2}:\"&{0}&\",\"&{1}&\" :\"", DebugLevel.AllDetailLevel, element.S_x, element.S_y, _aeObject.FullName));
                            break;
                        case "movee":
                            element = _aeObject as Element;
                            ret = string.Format("dm.MoveTo {0},{1} : ", element.S_x, element.S_y);
                            break;
                        case "getstr":
                            element = _aeObject as Element;
                            ret = string.Format("{0}",element.OcrStringInfo.FindStr);
                            break;
                        case "findstr":
                            //truePostE.Compile(fun
                            partialPrefixStatement = "";
                            postfixStatement2 = "";
                            if (isSingleStatement)
                                ret = string.Format("{0} {1}", _externalRefFunc.S_FunctionName,_truePostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2));
                            else
                                ret = string.Format("{0}({1})", _externalRefFunc.S_FunctionName, _truePostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2));
                            prefixStatement += partialPrefixStatement;
                            interValFlag = true;
                            postfixStatement = postfixStatement2 + postfixStatement;
                            break;
                        default:
                            throw new InterpretException(Context, "无法识别的A<B>函数");
                    }
                    interValFlag = true;
                    break;
                    #endregion
                case "[":
                    #region A[B]
                    string val;
                    switch (E.Value.ToLower())
                    {
                        case "array":
                            val = PostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2);
                            if (M.GetExpressionAttribute(val) == ExpressionAttrs.IntegerConst)
                            {
                                val = (int.Parse(val) - 1).ToString();
                            }
                            else
                            {
                                val = string.Format("{0}-1", val);
                            }
                            prefixStatement = partialPrefixStatement + string.Format("Redim {0}({1}) : ", M.EVar("arrayCreater"), val);
                            interValFlag = true;
                            postfixStatement = "";
                            return M.EVar("arrayCreater");
                        case "map":
                            ret = E.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
                            throw new NotImplementedException();
                        default:
                            ret = E.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
                            val = PostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2);
                            StringBuilder sb = new StringBuilder();
                            if (PostE is CommaExpression)
                            {
                                PostE.RecursiveDo((Expression e) =>
                                {
                                    if (!(e is CommaExpression))
                                        sb.Append(string.Format("({0})", e.QTranslate()));
                                    return true;
                                }, 1);
                                ret = string.Format("{0}{1}", ret, sb);
                            }
                            else
                            {
                                ret = string.Format("{0}({1})", ret, val);
                            }

                            prefixStatement += partialPrefixStatement;
                            //interValFlag = true;
                            postfixStatement = postfixStatement2 + postfixStatement;
                            break;
                    }

                    break;
                    #endregion
                case "(":
                    #region A(B)
                    ret = E.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
                    partialPrefixStatement = "";
                    postfixStatement2 = "";
                    switch (E.Value.ToLower())
                    {
                        case "find":
                        case "finde":
                        case "finda":
                        case "fclicke":
                        case "fdbclicke":
                                                        if (E.ExpressionAttributes.HasFlag(ExpressionAttrs.Function) || E.ExpressionAttributes.HasFlag(ExpressionAttrs.External))
                            {
                                string prefixValue = E.RefVar.FinalName;
                                if (isSingleStatement)
                                    ret = prefixValue + " " + (_truePostE == null ? "" : _truePostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2));
                                else
                                    ret = prefixValue + Symbol + (_truePostE == null ? "" : _truePostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2)) + Symbol.ToReserve();
                            }
                            break;
                        default:
                            if (E.ExpressionAttributes.HasFlag(ExpressionAttrs.Function) || E.ExpressionAttributes.HasFlag(ExpressionAttrs.External))
                            {
                                string prefixValue = E.RefVar.FinalName;
                                //string postfixValue = "";
                                //if (PostE != null) postfixValue=PostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2);
                                //if(postfixValue is Data)

                                //string prefixValue = E.ExpressionAttributes.HasFlag(ExpressionAttrs.ObjectMember)
                                  //  ? E.Value : E.RefVar.FinalName;

                                if (isSingleStatement)
                                    ret = prefixValue + " " + (PostE == null ? "" : PostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2));
                                else
                                    ret = prefixValue + Symbol + (PostE == null ? "" : PostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2)) + Symbol.ToReserve();
                            }
                            else
                            {
                                //StringBuilder sb = new StringBuilder();
                                if (PostE != null)
                                {
                                    PostE.QTranslate(out partialPrefixStatement, out interValFlag, out postfixStatement2);
                                    partialPrefixStatement += FunctionSwitcher.ToSetSysInArg(PostE);
                                }
                                if (isSingleStatement)
                                    ret = string.Format("{0} {1}", G.FunctionSwitcher.FinalName, E.QTranslate());
                                else
                                    ret = string.Format("{0}({1})", G.FunctionSwitcher.FinalName, E.QTranslate());
                            }
                            break;
                    }

                    prefixStatement += partialPrefixStatement;
                    interValFlag = true;
                    postfixStatement = postfixStatement2 + postfixStatement;
                    break;
                    #endregion
                case "++":
                case "--":
                    ret = E.QTranslate(out prefixStatement, out interValFlag, out postfixStatement);
                    postfixStatement += string.Format("{0}={0}{1}1 : ", ret, Symbol.Value[0]);
                    break;
            }
            return ret;
        }

        public override string QTranslate(out string prefixStatement, out bool interValFlag, out string postfixStatement)
        {
            return QTranslate(out prefixStatement, out interValFlag, out postfixStatement, false);
        }

        public override void RecursiveDo(Func<Expression, bool> act, int depth)
        {
            if (!act(this) || depth == 0) return;
            E.RecursiveDo(act, depth - 1);
            if (PostE != null)
            {
                act(PostE);
                PostE.RecursiveDo(act, depth - 1);
            }
        }
    }
}
