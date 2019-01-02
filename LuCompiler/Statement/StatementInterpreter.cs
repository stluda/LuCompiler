using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LuCompiler
{
    public class StatementInterpreter : SyntaxElement
    {
        private int _index_block = 0;
        private ITokenTaker _tokenTaker;
        private static Regex _regex_word = new Regex(@"^\w+$");
        private static Regex _regex_variable = new Regex(@"^[a-zA-Z]\w*$");
        private ExpressionInterpreter _ei;

        public string Token
        {
            get { return _tokenTaker.Token; }
        }

        public int BlockIndex
        {
            get { return _index_block; }
            set { _index_block = value; }
        }

        public StatementInterpreter(ITokenTaker tokenTaker)
        {
            _tokenTaker = tokenTaker;
            _ei = new ExpressionInterpreter(_tokenTaker);
        }

        public Statement Interpret()
        {
            getToken();
            return eval();
        }

        private static Regex r_case = new Regex("^('\\w+'|\\w+|[0-9]+(\\.[0-9]+)?|\"[^\"]*\")$");
        public Statement eval()
        {
            string op;          
            Expression e;
            Expression e1, e2, e3;
            Statement result=null;
            Block block = null;
            Context context = G.TokenTaker.CurrentContext;
            switch (op=Token.ToLower())
            {
                case "debug":
                    int debugLevel = 0;
                    if (getToken() == "(")
                    {
                        if (!int.TryParse(getToken(), out debugLevel)) throw new InterpretException(_tokenTaker.CurrentContext, "不是有效的数字");
                        if (getToken() != ")") throw new InterpretException(_tokenTaker.CurrentContext, "缺少')'");
                        getToken();
                    }
                    DebugStatement ds = new DebugStatement(debugLevel, eval());
                    if (Token == "else")
                    {
                        getToken();
                        ds.ElseStatement = eval();
                    }
                    result = ds;
                    break;
                case "trace":
                case "log":
                    e = _ei.evalCommaExp();
                    if (!(e is PostfixExpression)) throw new InterpretException(_tokenTaker.CurrentContext, "不是有效的trace语句");
                    if (Token != ";") 
                        throw new InterpretException(_tokenTaker.CurrentContext,"缺少';'");
                    getToken();
                    switch (op)
                    {
                        case "trace":
                            result = new TraceStatement(e as PostfixExpression);
                            break;
                        case "log":
                            result = new LogStatement(e as PostfixExpression);
                            break;
                    }                 
                    break;
                case "switch":
                    #region switch
                    SwitchStatement sws = null;
                    bool exFlag = false;
                    switch (getToken())
                    {
                        case "(":
                            getToken();
                            //Expression e1, e2, e3;
                            e = _ei.evalCommaExp();
                            if (Token != ")") throw new InterpretException(_tokenTaker.CurrentContext, "switch表达式必须包含于'('与')'中");
                            sws = new SwitchStatement(e);                            
                            break;
                        case "<":                           
                            string areaName, groupName;
                            areaName = getToken();
                            if (M.SetExflag(!G.AEObjectOfFullName.ContainsKey(areaName), ref exFlag)) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "不存在该域"),this);
                            if (!exFlag)
                            {
                                if (getToken() != ",") throw new InterpretException(_tokenTaker.CurrentContext, "必须用','隔开");
                                Area area = G.AEObjectOfFullName[areaName] as Area;
                                groupName = getToken();
                                if (M.SetExflag(!area.NameOfCategory.ContainsKey(groupName), ref exFlag)) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "不存在该组"),this);
                                if (getToken() != ">") throw new InterpretException(_tokenTaker.CurrentContext, "缺少'>'");
                                if (!exFlag) sws = new SwitchStatement(area, groupName);
                            }
                            //sws = new SwitchStatement(
                            break;
                    }                    
                    if (getToken() != "{") throw new InterpretException(_tokenTaker.CurrentContext, "必须以'{'开头");
                            getToken();
                            while (Token != "}")
                            {
                                switch (Token)
                                {
                                    case "case":
                                    case "default":
                                        break;
                                    default:
                                        throw new InterpretException(_tokenTaker.CurrentContext, "不支持");
                                }
                                List<Context> caseContexts = new List<Context>();

                                switch ((op = Token))
                                {
                                    case "case":
                                        while (Token == "case")
                                        {
                                            Context caseContext;
                                            getToken();
                                            string prefix = Token;
                                            if (prefix == "@" || prefix == "-")
                                            {
                                                if (!r_case.IsMatch(getToken()))
                                                    throw new InterpretException(_tokenTaker.CurrentContext, "不是有效的case标识");
                                                Context currentContext = _tokenTaker.CurrentContext;
                                                caseContext = new Context(prefix + Token, currentContext.RowIndex, currentContext.ColumnIndex, currentContext.LineValue);
                                            }
                                            else
                                            {

                                                if (!r_case.IsMatch(Token))
                                                    throw new InterpretException(_tokenTaker.CurrentContext, "不是有效的case标识");
                                                caseContext = _tokenTaker.CurrentContext;
                                            }

                                            caseContexts.Add(caseContext);
                                            if (getToken() != ":") throw new InterpretException(_tokenTaker.CurrentContext, "case和条件之间必须用':'隔开");
                                            getToken();
                                        }
                                        break;
                                    case "default":
                                        if (getToken() != ":") throw new InterpretException(_tokenTaker.CurrentContext, "default和条件之间必须用':'隔开");
                                        getToken();
                                        break;
                                    default:
                                        throw new InterpretException(_tokenTaker.CurrentContext, "暂不支持");
                                }
                                switch (op)
                                {
                                    case "case":
                                        if (!exFlag && caseContexts.Count > 0) sws.Add(caseContexts, eval());
                                        else eval();
                                        break;
                                    case "default":
                                        sws.DefaultStatement = eval();
                                        break;
                                }
                            }
                            if (exFlag) throw new InterpretException(_tokenTaker.CurrentContext, "组装switch语句的过程中发生了异常");
                            result = sws;
                            getToken();
                    break;
                    #endregion
                #region case break,continue,return
                case "break":                    
                    if (getToken() != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"应以';'结束"),this);
                    getToken();
                    result = new BreakStatement();
                    break;
                case "continue":
                    if (getToken() != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"应以';'结束"),this);
                    getToken();
                    result = new ContinueStatement();
                    break;
                case "return":
                    if (getToken() == ";")
                    {
                        getToken();
                        result = new ReturnStatement();
                    }
                    else
                    {
                        result = new ReturnStatement(_ei.evalAssignmentExp());
                        if (Token != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"应以';'结束"),this);
                        getToken();
                    }
                    break;
                #endregion
                #region case do,while,limit,for,...
                case "do":
                    getToken();
                    result = eval();
                    switch (Token)
                    {
                        case "while":
                            switch (getToken())
                            {
                                case "(":
                                    getToken();
                                    e = _ei.evalCommaExp();
                                    if (Token != ")") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"缺少')'"),this);
                                    if (getToken() != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"必须以';'结束"),this);
                                    getToken();
                                    result = new LoopStatement("dowhile", e, result);
                                    break;
                                case "<":
                                    getToken();
                                    e = _ei.evalSpecial2();
                                    if (Token != ">") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "缺少'>'"),this);
                                    if (getToken() != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"必须以';'结束"),this);
                                    getToken();
                                    result = new LoopStatement("dowhile", e, result);
                                    break;
                            }
                            break;
                        case "limit":
                            if (getToken() != "(") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"limit表达式必须包含于'('与')'中"),this);
                            getToken();
                            //Expression e1, e2, e3;
                            e1 = _ei.evalCommaExp();
                            if (Token != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"此处应为';'"),this);
                            getToken(); e2 = _ei.evalCommaExp();
                            if (Token == ")")
                            {
                                getToken();
                                if (Token != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "必须以';'结束"), this);
                                getToken();
                                result = new LimitedLoopStatement(e1, new Data("true"), e2, result,true);
                            }
                            else
                            {
                                if (Token != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "此处应为';'"), this);
                                getToken(); e3 = _ei.evalCommaExp();
                                if (Token != ")") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "limit表达式必须包含于'('与')'中"), this);
                                getToken();
                                if (Token != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "必须以';'结束"), this);
                                getToken();
                                result = new LimitedLoopStatement(e1, e2, e3, result, true);
                            }
                            break;
                        default:
                            Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"尚不支持"),this);
                            break;
                    }
                    break;                
                case "if":
                case "while":
                case "every":
                    #region condition
                    getToken();
                    switch (Token)
                    {
                        case "(":
                            getToken();
                            e = _ei.evalCommaExp();
                            if (Token != ")") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"缺少')'"),this);
                            switch (op)
                            {
                                case "if":
                                    getToken();
                                    result = eval();
                                    if (Token == "else")
                                    {
                                        getToken();
                                        result = new IfStatement(e, result, eval());
                                    }
                                    else
                                    {
                                        result = new IfStatement(e, result);
                                        //getToken();
                                    }
                                    break;
                                case "while":
                                    getToken();
                                    result = eval();
                                    bool flag;
                                    if (flag=result is Block) (block=result as Block).WrapType = op;
                                    result = new LoopStatement(op, e, result);
                                    if (flag) block.OutsideStatement = result as LoopStatement;
                                    break;
                                case "every":
                                    getToken();
                                    result = eval();
                                    if (Token == "else")
                                    {
                                        getToken();
                                        result = new EveryStatement(e, result, eval());
                                    }
                                    else
                                    {
                                        result = new EveryStatement(e,result);
                                        //getToken();
                                    }
                                    break;
                                default:
                                    Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"无法识别的条件语句"),this);
                                    break;
                            }
                            break;
                        case "<":
                            //getToken();
                            getToken();
                            e = _ei.evalSpecial2();
                            if (Token != ">") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"缺少'>'"),this);
                            switch (op)
                            {
                                case "if":
                                    getToken();
                                    result = eval();
                                    if (Token == "else")
                                    {
                                        getToken();
                                        result = new IfStatement(e, result, eval());
                                    }
                                    else
                                    {
                                        if (Token != null)
                                        {
                                            if (!moveBackword())
                                            {

                                            }
                                            getToken();
                                        }
                                        result = new IfStatement(e, result);
                                    }
                                    break;
                                case "while":
                                case "limit":
                                    getToken();
                                    result = eval();
                                    bool flag;
                                    if (flag=result is Block) (block=result as Block).WrapType = op;
                                    result = new LoopStatement(op, e, result);
                                    if (flag) block.OutsideStatement = result as LoopStatement;
                                    break;
                                default:
                                    Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"........"),this);
                                    break;
                            }
                            break;                         
                        default:
                            Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"...."),this);
                            break;
                    }
                    break;
                    #endregion
                case "limit":
                    if(getToken()!="(")Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"limit表达式必须包含于'('与')'中"),this);
                    getToken();
                    //Expression e1, e2, e3;
                    e1 = _ei.evalCommaExp();
                    if (Token != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"此处应为';'"),this);
                    getToken(); e2 = _ei.evalCommaExp();
                    if (Token == ")")
                    {
                        getToken();
                        result = new LimitedLoopStatement(e1, new Data("true"), e2, eval());
                    }
                    else
                    {
                        if (Token != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "此处应为';'"), this);
                        getToken(); e3 = _ei.evalCommaExp();
                        if (Token != ")") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "limit表达式必须包含于'('与')'中"), this);
                        getToken();
                        result = new LimitedLoopStatement(e1, e2, e3, eval());
                    }
                    break;
                #endregion
                #region case var,num,string,bool,state,array,map
                case "var":
                case "num":
                case "string":
                case "bool":
                case "super":
                case "map":
                case "array":
                case "funcref":
                    #region declaration
                    string type;
                    type = op;
                    bool isArray = false;
                    if (getToken() == "[]")
                    {
                        isArray = true;
                        getToken();
                    }
                    if (op == "array") isArray = true;

                    if (!_regex_variable.IsMatch(Token)) 
                        Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "......"), this);
                    result = new DeclarationStatement(type, _ei.evalCommaExp(), isArray);
                    if (Token != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "必须以';'结尾!"), this);
                    getToken();
                    break;
                    #endregion
                case "state":
                    string name;
                    if (!_regex_variable.IsMatch(name=getToken())) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"不是有效的变量名"),this);
                    if (getToken() != "=") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"state类型必须有初始值，需用'='连接"),this);
                    if (getToken() != "{") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"state类型里的内容请用大括号'{','}'括起"),this);
                    State state = new State(name,_tokenTaker.CurrentContext);
                    string key=getToken();
                    do
                    {
                        if (!_regex_variable.IsMatch(key=Token)) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"不是有效的变量名"),this);
                        if (getToken() != ":") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"类型键与表达式之间请用':'隔开"),this);
                        getToken();
                        state.Add(key, _ei.evalAssignmentExp());
                        if (Token != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"请用';'结束"),this);
                    } while (getToken()!="}");
                    result = new StateStatement(state);
                    getToken();
                    break;
                #endregion
                case "next":
                    if (getToken() != ":") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"此处应为':'"),this);
                    if (!_regex_word.IsMatch(getToken())) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"不是有效的单词"),this);
                    result = new NextStatement(Token);
                    if (getToken() != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"应以';'结束"),this);
                    getToken();
                    break;
                case "${":
                    ExternalStatement es = new ExternalStatement();
                    while (getToken() != null && Token != "}$") es.Add(Token);
                    if(Token!="}$") throw new InterpretException(_tokenTaker.CurrentContext, "缺少'}$'");
                    result = es;
                    getToken();
                    break;
                case "{":
                    #region block
                    block = new Block();
                    while (Token != "}")
                    {
                        getToken();
                        if (Token == "}")//允许空块
                        {
                            moveBackword();
                            break;
                        }
                        Statement statement = eval();
                        block.Add(statement);
                        if (statement is InLoopStatement) (statement as InLoopStatement).Block = block;
                        moveBackword();
                    }
                    if (!moveForword())
                    {
                        throw new NotImplementedException();
                    }
                    getToken();
                    result = block;
                    break;
                    #endregion
                default:
                    #region assignment
                    e = _ei.evalCommaExp();
                    //if (!(e is AssignmentExpression)) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"........"),this);
                    //getToken();
                    if (Token != ";") 
                        throw new InterpretException(_tokenTaker.CurrentContext,"缺少';'");
                    getToken();                    
                    result =  new SingleStatement(e);
                    break;
                    #endregion
            }          
            if(result!=null)result.Context = context;
            return result;
        }



        private string watchNextToken()
        {
            return _tokenTaker.WatchNextToken();
        }

        private string getToken()
        {
            return _tokenTaker.GetToken();
        }
        private bool moveForword()
        {
            return _tokenTaker.MoveForward();
        }
        /*
        private void moveForword(int count)
        {
            for (int i = count; i > 0; i--)
                _tokenTaker.MoveForward();
        }
        private void moveBackword(int count)
        {
            for (int i = count; i > 0; i--)
                _tokenTaker.MoveBackword();
        }*/
        private bool moveBackword()
        {
            return _tokenTaker.MoveBackword();
        }
    }
}
