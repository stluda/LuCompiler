using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace LuCompiler
{
    public class ProgramInterpreter : SyntaxElement
    {
        private StatementInterpreter _si;
        private ExpressionInterpreter _ei;
        //private int _index_block = 0;
        //private int _count_statement = 0;
        private CodeTokenTaker _tokenTaker;
        private Regex _regex_word = new Regex(@"^\w+$");
        private Regex _regex_variable = new Regex(@"^[a-zA-Z\u4e00-\u9fa5]\w*$");
        private Regex _regex_kernalFunc = new Regex(@"^\$[a-zA-Z]\w*$");
        private Hashtable<string, Variable> _global_variables = new Hashtable<string,Variable>();
        private List<Function> _functions = new List<Function>();
        private List<Module> _modules = G.Modules;
        //private static Regex _r_OcrStr = new Regex("^\"[a-zA-Z0-9\\u4e00-\\u9fa5]+\"$");
        private static Regex _r_OcrStr = new Regex("^\"[^\"]+\"$");
        private static Regex _r_OffsetColor = new Regex("^\"[0-9a-fA-F]{6}\\-[0-9a-fA-F]{6}(\\|[0-9a-fA-F]{6}\\-[0-9a-fA-F]{6})*\"$");
        private static Regex _r_num = new Regex("^[0-9]+$");
        private static Regex _r_dec = new Regex(@"^[0-1]\.[0-9]*$");

        public string Token
        {
            get { return _tokenTaker.Token; }
        }

        public ProgramInterpreter(CodeTokenTaker tokenTaker)
        {
            _tokenTaker = tokenTaker;
            _ei = new ExpressionInterpreter(tokenTaker);
            _si = new StatementInterpreter(tokenTaker);
            G.GlobalVariables = _global_variables;
            G.Functions = _functions;
        }


        public void Interpret()
        {
            string mode = "#none";
            getToken();
            Statement statement;
            string funcName;           
            while (Token != null)
            {
                Expression inArgs = null;
                switch (mode)
                {
                    case "#config":
                        #region config
                        string op;
                        switch (op=Token.ToLower())
                        {
                            case "templatepath":
                            case "workhomepath":
                            case "isdebug":
                            case "debuglevel":
                            case "showoutputdir":
                            case "copytoclipboard":
                            case "writeresultluscript":
                            case "writetraceprinttolog":
                            case "autoputintoqmacro":
                            case "ftpserver":
                            case "ftpuploadresult":
                            case "dmencode":
                            case "dmencodepassword":
                                break;
                            default:
                                throw new InterpretException(_tokenTaker.CurrentContext,"无法识别的设置参数");
                        }
                        if (getToken() != "=") throw new InterpretException(_tokenTaker.CurrentContext, "必须以'='隔开");
                        getToken();
                        switch (op)
                        {
                            case "ftpserver":
                                string[] args = Token.Trim('"').Split('|');
                                try
                                {
                                    string root = args[1];
                                    if (root == "/") root = "";
                                    G.FtpWeb = new FtpWeb(args[0], root, args[2], args[3]);
                                }
                                catch(Exception ex)
                                {
                                    throw new InterpretException(Context.Empty, ex.Message);
                                }
                                break;
                            case "templatepath":
                            case "workhomepath":
                                Func<string,bool> actionToDo = null;
                                List<string> paths = new List<string>();
                                do
                                {
                                    paths.Add(Token.Trim('"'));
                                    
                                    if(watchNextToken() != "|")break;
                                    else{
                                        getToken();
                                        getToken();
                                    }
                                } while (true);
                                bool flag = false;
                                InterpretException excToThrow = null;
                                switch (op)
                                {
                                    case "templatepath":
                                        actionToDo = (string path) =>
                                        {
                                            if (File.Exists(path))
                                            {
                                                G.TemplatePath = path;
                                                flag = true;
                                                return true;
                                            }
                                            else return false;
                                        };
                                        excToThrow = new InterpretException(Context.Empty, string.Format("模板文件路径有误，文件{0}不存在", G.TemplatePath));
                                        break;
                                    case "workhomepath":
                                        actionToDo = (string path) =>
                                        {
                                            if (Directory.Exists(path))
                                            {
                                                G.WorkhomePath = path;
                                                G.OutputPath = G.WorkhomePath + @"\output";
                                                if (!Directory.Exists(G.OutputPath))
                                                {
                                                    excToThrow = new InterpretException(Context.Empty, string.Format("工作目录路径有误，输出目录{0}不存在", G.OutputPath));
                                                    flag = false;
                                                    return false;
                                                }
                                                G.ArgumentsPath = G.OutputPath + @"\result.xml";
                                                if (!File.Exists(G.ArgumentsPath))
                                                {
                                                    excToThrow = new InterpretException(Context.Empty, string.Format("参数文件路径有误，文件{0}不存在", G.ArgumentsPath));
                                                    flag = false;
                                                    return false;
                                                }
                                                flag = true;
                                                return true;
                                            }
                                            else
                                            {
                                                excToThrow = new InterpretException(Context.Empty, string.Format("工作目录路径有误，输出目录{0}不存在", G.OutputPath));
                                                return false;
                                            }
                                        };
                                        excToThrow = new InterpretException(Context.Empty, string.Format("模板文件路径有误，文件{0}不存在", G.TemplatePath));
                                        break;
                                }

                                foreach (string path in paths)
                                {
                                    if (actionToDo(path)) break;
                                }
                                if(!flag)
                                    throw excToThrow;
                                break;
                            case "showoutputdir":
                            case "copytoclipboard":
                            case "isdebug":
                            case "writeresultluscript":
                            case "writetraceprinttolog":
                            case "autoputintoqmacro":
                            case "ftpuploadresult":
                            case "dmencode":
                                if (Token != "true" && Token != "false") throw new InterpretException(_tokenTaker.CurrentContext, "必须是true或false");
                                switch (op)
                                {
                                    case "ftpuploadresult":
                                        G.DoesFtpUploadResult = bool.Parse(Token);
                                        break;
                                    case "showoutputdir":
                                        G.DoesShowOutputDir = bool.Parse(Token);
                                        break;
                                    case "copytoclipboard":
                                        G.DoesCopyToClipBoard = bool.Parse(Token);
                                        break;
                                    case "isdebug":
                                        G.IsDebugMode = bool.Parse(Token);
                                        break;
                                    case "writetraceprinttolog":
                                        G.DoesWriteTraceprintToLog = bool.Parse(Token);
                                        break;
                                    case "writeresultluscript":
                                        if (bool.Parse(Token))LuaScriptCaller.WriteResultLuScript();
                                        break;
                                    case "autoputintoqmacro":
                                        G.DoesAutoPutIntoQMacro = bool.Parse(Token);
                                        break;
                                    case "dmencode":
                                        G.DmEncode = bool.Parse(Token);
                                        break;
                                }                                
                                break;
                            case "debuglevel":
                                int debugLevel;
                                if (!int.TryParse(Token, out debugLevel)) throw new InterpretException(_tokenTaker.CurrentContext, "不是有效的数字");
                                G.DebugLevel = debugLevel;
                                break;
                            case "dmencodepassword":
                                G.DmEncodePassword = Token.Trim('"');
                                break;
                        }
                        getToken();
                        _tryChangeMode(ref mode);
                        if (mode != "#config")
                        {
                            if (G.ArgumentsPath == null || G.OutputPath == null || G.TemplatePath == null)throw new InterpretException(Context.Empty, "存在路径参数未设置");
                            G.Root = AEObjectReader.Read();
                        }
                        break;
                        #endregion
                    case "#none":
                        #region init
                        switch (Token)
                        {
                            case "#config":
                                mode = "#config";
                                break;
                            default:
                                Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"必须先设置路径参数"),this);
                                break;
                        }
                        getToken();
                        break;
                        #endregion
                    case "#variables":
                        #region variable
                        string type;
                        Statement s;
                        bool isStatic = false;
                        switch (type=Token)
                        {
                            case "var":
                            case "num":
                            case "bool":
                            case "string":
                            case "state":
                            case "array":
                            case "funcref":
                            case "super":
                            case "map":
                                s = _si.eval();
                                if (!(s is DeclarationStatement)) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"不是赋值语句!"),this);
                                DeclarationStatement ds = s as DeclarationStatement;
                                G.GlobalDeclarations.Add(ds);
                                //_global_variables[ds.Name] = new Variable(type, ds.Name);
                                break;
                            case "const":
                                bool isConst;
                                switch (getToken())
                                {
                                    case "num":
                                    case "bool":
                                    case "string":
                                        isConst = true;
                                        break;
                                    default:
                                        throw new InterpretException(_tokenTaker.CurrentContext, "无法识别的类型{0}",Token);
                                }
                                s = _si.eval();
                                if (!(s is DeclarationStatement)) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"不是赋值语句!"),this);
                                ds = s as DeclarationStatement;
                                ds.IsConst = isConst;
                                G.GlobalDeclarations.Add(ds);
                                break;
                            case "static":
                                switch (getToken())
                                {
                                    case "var":
                                    case "num":
                                    case "bool":
                                    case "string":
                                    case "state":
                                    case "super":
                                        isStatic = true;
                                        break;
                                    default:
                                        throw new InterpretException(_tokenTaker.CurrentContext, "无法识别的类型{0}",Token);
                                }
                                s = _si.eval();
                                if (!(s is DeclarationStatement)) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"不是赋值语句!"),this);
                                ds = s as DeclarationStatement;
                                ds.IsStatic = isStatic;
                                G.GlobalDeclarations.Add(ds);
                                break;
                            default:
                                throw new InterpretException(_tokenTaker.CurrentContext,"无法识别的类型{0}",Token);
                        }
                        _tryChangeMode(ref mode);
                        break;
                        #endregion
                    case "#macros":
                        #region macros
                        Expression e = _ei.evalCommaExp();
                        if (!(e is PostfixExpression)) throw new InterpretException(_tokenTaker.CurrentContext, "不是正确的宏格式，格式应为A(B)或A<B>");
                        PostfixExpression pe = e as PostfixExpression;
                        if (Token != "=>") throw new InterpretException(_tokenTaker.CurrentContext, "缺少'=>'");
                        getToken();
                        Statement stmt = _si.eval();
                        Macro macro = new Macro(pe.E.Value, e.ToString(), stmt.ToString(),e.Context,stmt.Context);
                        _tryChangeMode(ref mode);
                        break;
                        #endregion
                    case "#functions":
                    case "#events":
                        #region functions
                        Context saveContext = _tokenTaker.CurrentContext;
                        if (mode == "#functions")
                        {
                            funcName = Token;
                            getToken();
                        }
                        else
                        {
                            funcName = _ei.evalDotExp().ToString();                            
                        }
                        
                        if (mode == "#functions" && !_regex_variable.IsMatch(funcName)) 
                            Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "不是有效的函数名'{0}'",funcName),this);
                        if (Token == "(")
                        {
                            getToken();
                            inArgs = _ei.evalCommaExp();
                            if(Token!=")")Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"应以')'结束"),this);
                            getToken();
                        }
                        if (Token != "=") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"必须以等号连接"),this);
                        getToken();
                        if (Token == "<")
                        {
                            getToken();
                            Expression condition = _ei.evalSpecial2();
                            if (Token != ">") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"应以'>'结束"),this);
                            getToken();
                            statement = new IfStatement(condition, _si.eval());
                            
                        }
                        else statement = _si.eval();

                        Function f = mode=="#events" ?
                            new Event(funcName, statement, saveContext) :
                            new Function(funcName, statement, saveContext);
                        f.InArgs = inArgs;
                        _global_variables[funcName] = f;
                        _functions.Add(f);
                        _tryChangeMode(ref mode);
                        break;
                        #endregion
                    case "#modules":
                        #region modules
                        Module module;
                        if (Token == "M")
                        {
                            getToken();
                            statement = _si.eval();
                            G.Modules.Add(module = new Module(null, statement,_tokenTaker.CurrentContext));
                            module.Index = _modules.Count - 1;
                            //getToken();
                        }
                        else
                        {
                            string name = Token;
                            if (getToken() != "=") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"此处应为'='"),this);
                            if (getToken() != "M") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"此处应为'M'"),this);
                            getToken();
                            statement = _si.eval();
                            G.Modules.Add(module=new Module(name, statement,_tokenTaker.CurrentContext));
                            G.ModuleIndexOfName[name] = module.Index = G.Modules.Count - 1;
                            //getToken();
                        }
                        _tryChangeMode(ref mode);
                        break;
                        #endregion
                    case "#kernal":
                        #region kernal
                        saveContext = _tokenTaker.CurrentContext;
                        if (!_regex_kernalFunc.IsMatch(funcName=Token)) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"不是有效的核心函数名(必须以'$'开头)"),this);
                        if (getToken() == "(")
                        {
                            getToken();
                            inArgs = _ei.evalCommaExp();
                            if(Token!=")")Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"应以')'结束"),this);
                            getToken();
                        }
                        if (Token != "=") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"必须以等号连接"),this);
                        getToken();
                        if (Token == "<")
                        {
                            getToken();
                            Expression condition = _ei.evalSpecial2();
                            if (Token != ">") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"应以'>'结束"),this);
                            getToken();
                            statement = new IfStatement(condition, _si.eval());
                            
                        }
                        else statement = _si.eval();
                        KernalFunction k = new KernalFunction(funcName, statement, saveContext);
                        k.InArgs = inArgs;
                        G.Kernals.Add(k);
                        G.KernalNames.Add(k.Name.ToLower());
                        _tryChangeMode(ref mode);
                        break;
                        #endregion
                    case "#elements":
                        #region elements
                        if (!Token.StartsWith("#"))
                        {
                            if (!_regex_variable.IsMatch(Token))
                                throw new InterpretException(_tokenTaker.CurrentContext, "不是合法的元素名");
                            switch (Token)
                            {
                                default:
                                    bool createNew = false;
                                    string eName = Token;
                                    Context eContext = _tokenTaker.CurrentContext;
                                    switch (getToken())
                                    {
                                        case "=":
                                            createNew = true;
                                            break;
                                        case "+=":
                                            break;
                                        case "refers":
                                            do
                                            {
                                                string name2 = getToken();
                                                if (!_regex_variable.IsMatch(name2)) throw new InterpretException(_tokenTaker.CurrentContext, "不是合法的域/元素名");
                                                Context context2 = _tokenTaker.CurrentContext;
                                                M.AddRefers(eContext, context2);
                                            } while (getToken() == ",");
                                            if (Token != ";") throw new InterpretException(_tokenTaker.CurrentContext, "缺少';'");
                                            getToken();

                                            goto exit;
                                        default:
                                            Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "无法识别的运算符"), this);
                                            //goto exit;                                        
                                            break;
                                    }
                                    Area area = null;
                                    Element element = null;
                                    bool isArea = false;
                                    string[] args = eName.Split('_');
                                    isArea = args.Length == 1;
                                    if (createNew)
                                    {
                                        if (G.AEObjectOfFullName.ContainsKey(eName)) throw new InterpretException(_tokenTaker.CurrentContext, "该元素(域)已存在，不能重复定义");
                                        if (isArea) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "暂时不支持新增域"), this);
                                        else if (args.Length > 2) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "格式错误"), this);
                                        else
                                        {
                                            Area a = G.AEObjectOfFullName[args[0]] as Area;
                                            element = new Element()
                                            {
                                                Name = args[1],
                                                CHName = "未命名"
                                            };
                                            a.AddChild(element);
                                            element.Register();
                                            G.AEObjectOfFullName[eName] = element;
                                        }
                                    }
                                    else
                                    {
                                        if (!G.AEObjectOfFullName.ContainsKey(eName)) throw new InterpretException(_tokenTaker.CurrentContext,
                                            "不存在该元素/域");
                                        if (isArea)
                                            area = G.AEObjectOfFullName[eName] as Area;
                                        else
                                            element = G.AEObjectOfFullName[eName] as Element;
                                    }
                                    getToken();
                                    if (isArea)
                                    {
                                        string groupName;
                                        if (!_regex_variable.IsMatch(groupName = Token)) throw new InterpretException(_tokenTaker.CurrentContext,
                                                "不是合法的域选项组名");
                                        Hashtable<string, OcrStringInfo> ocrStringInfoTable = new Hashtable<string, OcrStringInfo>();
                                        if (getToken() == "[")
                                        {
                                            OcrStringInfo ocrInfo = _parseOcrStrInfo();
                                            area.NameOfCategory[groupName] = ocrInfo;
                                            int index = ocrInfo.DictIndex;
                                            string colorInfo = ocrInfo.Color_info;
                                            float sim = ocrInfo.Sim;
                                            if (index == -1 || colorInfo == null) throw new InterpretException(_tokenTaker.CurrentContext, "必须指定字典编号以及色彩字符串");
                                            area.DictCategory[ocrInfo] = ocrStringInfoTable;
                                            bool brk = false;
                                            switch (Token)
                                            {
                                                /*case "[":
                                                    switch (getToken())
                                                    {
                                                        case "useall":
                                                            if (getToken() != "]") throw new InterpretException(_tokenTaker.CurrentContext, "缺少']'");
                                                            if (getToken() != ";") throw new InterpretException(_tokenTaker.CurrentContext, "缺少';'");
                                                            Dict.OcrUseAll.Add(index);
                                                            getToken();
                                                            brk = true;
                                                            break;
                                                        default:
                                                            throw new InterpretException(_tokenTaker.CurrentContext, "未知的字符信息属性");
                                                    }
                                                    break;*/
                                                case ";":
                                                    getToken();
                                                    brk = true;
                                                    break;
                                                case "{":
                                                    break;
                                                default:
                                                    throw new InterpretException(_tokenTaker.CurrentContext, "元素字符串信息集必须用'{'及'}'括起");
                                            }
                                            if (brk) break;
                                            getToken();
                                            while (Token != "}")
                                            {
                                                #region 检验元素合法性
                                                bool exceptionFlag = false;
                                                if (!_regex_variable.IsMatch(Token))
                                                {
                                                    Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "不是合法的元素名"), this);
                                                    exceptionFlag = true;
                                                }
                                                /*else if (G.AEObjectOfFullName.ContainsKey(Token))
                                                {
                                                    Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "暂不支持覆盖元素"),this);
                                                    exceptionFlag = true; 
                                                }*/
                                                if (exceptionFlag) { moveForword(3); getToken(); continue; }
                                                #endregion
                                                string name = Token;
                                                bool isOverride = false;
                                                switch (getToken())
                                                {
                                                    case "=":
                                                        element = new Element() { Name = name, CHName = "未定义" };
                                                        element.FunctionFilter = ocrInfo.Filter;
                                                        break;
                                                    case "+=":
                                                        AEObject obj = G.AEObjectOfFullName[area.Name + "_" + name];
                                                        if (obj == null) new InterpretException(_tokenTaker.CurrentContext, "不存在该元素");
                                                        element = obj as Element;
                                                        element.FunctionFilter = ocrInfo.Filter;
                                                        isOverride = true;
                                                        break;
                                                    default:
                                                        throw new InterpretException(_tokenTaker.CurrentContext, "请用'='或'+='隔开");
                                                }
                                                if (!_r_OcrStr.IsMatch(getToken())) throw new InterpretException(_tokenTaker.CurrentContext, "不是有效的检索字符串");
                                                ocrStringInfoTable[element.Name] = element.OcrStringInfo = new OcrStringInfo() { DictIndex = index, FindStr = Token, Color_info = colorInfo, Sim = sim };

                                                if (!isOverride)
                                                {
                                                    area.AddChild(element);
                                                    element.Register();
                                                    G.AEObjectOfFullName[element.FullName] = element;
                                                }
                                                if (getToken() != ";") throw new InterpretException(_tokenTaker.CurrentContext, "应以';'结束");
                                                getToken();
                                            }
                                        }
                                        else throw new Exception("暂不支持");
                                    }
                                    else
                                    {
                                        OcrStringInfo ocrInfo = _parseOcrStrInfo();
                                        if (Token != ";") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "应以';'结尾"), this);
                                        element.OcrStringInfo = ocrInfo;
                                    }
                                    getToken();
                                    break;
                            }
                        }

                    exit:
                        if (_tryChangeMode(ref mode)) AEFunction.CreateFunctions();
                        break;
                        #endregion
                }
            }
        }


        private OcrStringInfo _parseOcrStrInfo()
        {
            if (Token != "[") throw new InterpretException(_tokenTaker.CurrentContext, "元素表达式必须以'<','>'括起");
            OcrStringInfo ocrSInfo = new OcrStringInfo();
            bool numSetted = false, offsetColorSetted = false;
            Action<string> checkToken = (string tok) =>
            {
                if (Token != tok) throw new InterpretException(_tokenTaker.CurrentContext, "缺少{0}",tok);
            };
            Action<string> setAndCheck = (string type) =>
            {
                #region setAndCheck
                switch (type)
                {
                    case "i":
                    case "index":
                        if(!_r_num.IsMatch(Token))throw new InterpretException(_tokenTaker.CurrentContext, "字典序号格式非法");
                        int index = int.Parse(Token);
                        if (index > 20 || index < 1) throw new InterpretException(_tokenTaker.CurrentContext, "字典序号应在1～20之间");
                        ocrSInfo.DictIndex = index;
                        numSetted = true;
                        break;
                    case "s":
                    case "sim":
                        if (!_r_dec.IsMatch(Token)) 
                            throw new InterpretException(_tokenTaker.CurrentContext, "相似度格式非法");
                        float sim = float.Parse(Token);
                        if (sim < 0 || sim > 1) throw new InterpretException(_tokenTaker.CurrentContext, "相似度应在0～ 1之间");
                        ocrSInfo.Sim = sim;
                        break;
                    case "c":
                    case "colorinfo":
                        if (!_r_OffsetColor.IsMatch(Token)) throw new InterpretException(_tokenTaker.CurrentContext, "色彩字符串格式非法");
                        ocrSInfo.Color_info = Token;
                        offsetColorSetted = true;
                        break;
                    case "f":
                    case "findstr":
                        if (!_r_OcrStr.IsMatch(Token)) throw new InterpretException(_tokenTaker.CurrentContext, "查询文字串格式非法");
                        ocrSInfo.Color_info = Token;
                        break;
                    case "finde":
                    case "fclicke":
                    case "fdbclicke":
                        ElementFunctionFilter filter;
                        switch (type)
                        {
                            case "finde": filter = ElementFunctionFilter.FindE; break;
                            case "fclicke": filter = ElementFunctionFilter.FClickE; break;
                            case "fdbclicke": filter = ElementFunctionFilter.FDbClickE; break;
                            default: throw new NotSupportedException();
                        }
                        switch (Token)
                        {
                            case "on":
                                ocrSInfo.Filter |= filter;
                                break;
                            case "off":
                                ocrSInfo.Filter ^= filter;
                                break;
                            default:
                                throw new InterpretException(_tokenTaker.CurrentContext, "函数开关值非法");
                        }
                        break;
                }
                #endregion
            };
            while (true)
            {
                string op = getToken().ToLower();
                switch (op)
                {
                    case "i":
                    case "index":
                    case "s":
                    case "sim":
                    case "c":
                    case "colorinfo":
                    case "f":
                    case "findstr":
                    case "finde":
                    case "fclicke":
                    case "fdbclicke":
                        getToken();
                        checkToken("=");
                        getToken();
                        setAndCheck(op);
                        break;
                    case "useall":
                        if (ocrSInfo.DictIndex == -1) throw new InterpretException(_tokenTaker.CurrentContext, "必须先设置字典序号");
                        Dict.OcrUseAll.Add(ocrSInfo.DictIndex);
                        break;
                    default:
                        if (!numSetted && _r_num.IsMatch(Token))
                        {
                            setAndCheck("index");
                        }
                        else if (!offsetColorSetted && _r_OffsetColor.IsMatch(Token))
                        {
                            setAndCheck("colorinfo");
                        }
                        else if (_r_OcrStr.IsMatch(Token))
                        {
                            setAndCheck("findstr");
                        }
                        else
                            throw new InterpretException(_tokenTaker.CurrentContext, "发现无法识别的字符串信息");
                        break;
                }
                if (getToken() == "]") break;
                else if (Token != ",") throw new InterpretException(_tokenTaker.CurrentContext, "应以','隔开");
            }
            getToken();
            return ocrSInfo;
        }

        private bool _tryChangeMode(ref string mode)
        {
            if (Token == null) return false;
            string oldMode = mode;
            switch (Token)
            {
                case "#variables":
                case "#functions":
                case "#modules":
                case "#kernal":
                case "#elements":
                case "#events":
                case "#macros":
                    mode = Token;
                    getToken();
                    return oldMode != mode;          
            }
            return false;
        }

        public void EvalVariable()
        {

        }

        public void EvalFunction()
        {

        }

        public void EvalModule()
        {

        }

        private string watchNextToken()
        {
            return _tokenTaker.WatchNextToken();
        }

        private string getToken()
        {
            return _tokenTaker.GetToken();
        }
        private void moveForword()
        {
            _tokenTaker.MoveForward();
        }
        private void moveForword(int count)
        {
            for (int i = count; i > 0; i--)
                _tokenTaker.MoveForward();
        }
        private void moveBackword(int count)
        {
            for (int i = count; i > 0; i--)
                _tokenTaker.MoveBackword();
        }
        private void moveBackword()
        {
            _tokenTaker.MoveBackword();
        }

        /*
        bool breakFlag = false;
        if (!File.Exists(G.ArgumentsPath))
        {
            Dict.AddException(new InterpretException(Context.Empty, string.Format("参数文件路径有误，文件{0}不存在",G.ArgumentsPath)),this);
            breakFlag = true;
        }
        if (!Directory.Exists(G.OutputPath))
        {
            Dict.AddException(new InterpretException(Context.Empty, string.Format("工作目录路径有误，输出目录{0}不存在", G.OutputPath)),this);
            breakFlag = true;
        }
        if (!File.Exists(G.TemplatePath))
        {
            Dict.AddException(new InterpretException(Context.Empty, string.Format("模板文件路径有误，文件{0}不存在", G.TemplatePath)),this);
            breakFlag = true;
        }
        if (breakFlag) return;*/


        private void _compileRefers()
        {
            bool exflag = false;
            foreach (string key in G.References.Keys)
            {
                AEObject obj=null;
                List<string> list = G.References[key];
                if (M.SetExflag(!G.AEObjectOfFullName.ContainsKey(key),ref exflag)) Dict.AddException
                      (new InterpretException(G.ReferencesContext[key],"域/元素不存在"));
                if (!exflag) obj = G.AEObjectOfFullName[key];
                foreach (string value in list)
                {
                    if (M.SetExflag(!G.AEObjectOfFullName.ContainsKey(value), ref exflag)) Dict.AddException
                      (new InterpretException(G.ReferencesContext[value], "域/元素不存在"));
                    if(!exflag)
                    {
                        AEObject obj2 = G.AEObjectOfFullName[value];
                        if(M.SetExflag(obj.Parent != obj2.Parent && obj.Parent != obj2,ref exflag))
                            Dict.AddException(new InterpretException(G.ReferencesContext[value], "被引用的元素(域)必须是该元素(域)的同级或者父级"),this);
                        if(!exflag)obj2.Followings.Add(obj);
                    }
                }
            }
        }

        public void Compile()
        {
            if(G.ArgumentsPath!=null)_compileRefers();
            new FunctionSwitcher(G.Kernals, _modules, _functions);
            foreach (DeclarationStatement ds in G.GlobalDeclarations) ds.Compile(new Main(Context.Empty));
            foreach (KernalFunction kernal in G.Kernals) kernal.Compile();            
            foreach (Function function in _functions) function.Compile();
            foreach (Module module in _modules) module.Compile();            
            Dictmapper.CreateDict();
        }

        public void QTranslate(out string modules,out string functions,out string globalVariables,out string initializers)
        {
            StringBuilder sb = new StringBuilder();
     
     
            foreach (DeclarationStatement ds in G.GlobalDeclarations)
            {
                if (ds is StateStatement) continue;
                sb.AppendLine(ds.QTranslate());
            }
            globalVariables = sb.ToString();

            sb.Clear();
            foreach (KernalFunction kernal in G.Kernals)
            {
                sb.AppendLine(kernal.QTranslate());
                sb.AppendLine();
            }
            SysFunction.Append_Initializer(sb);
            SysFunction.Append_Cpu(sb);
            SysFunction.Append_ExecuteInstruction(sb);
            SysFunction.Append_SetDictionary(sb);

            if (G.IsDebugMode) 
                sb.AppendLine(DebugLogFunction.GetDebugLogFunctionStrings());


            foreach (Module module in _modules)
            {
                sb.AppendLine(module.QTranslate());
                sb.AppendLine();
            }
            modules = sb.ToString();
            sb.Clear();
            foreach (Function function in _functions)
            {
                sb.AppendLine(function.QTranslate());
                sb.AppendLine();
            }
            foreach (AEFunction aeFunction in G.AEFunctions.Values)
            {
                sb.AppendLine(aeFunction.QTranslate());
                sb.AppendLine();
            }
            functions = sb.ToString();
            //
            sb.Clear();
            //sb.AppendLine(string.Format("{0}=0 : {1} = 0",G.Root.S_x,G.Root.S_y))
            sb.AppendLine(string.Format("dm.GetClientSize hwnd,{0},{1}",G.Root.S_width,G.Root.S_height));
            initializers = M.Q_AddTab(sb.ToString(), 3);
        }
    }
}
