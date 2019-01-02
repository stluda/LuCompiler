using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace LuCompiler
{
    public class ExpressionInterpreter : SyntaxElement
    {
        public string Token
        {
            get { return _tokenTaker.Token; }
        }

        private static readonly List<string> assignmentSymbolList = new List<string>{"=","+=","-=","*=","/=","%=","&&=","||=" };
        private int _index_block = 0;
        private int _index_statement = 0;
        private int _count_expression = 0;
        private ITokenTaker _tokenTaker;
        private Expression _result;
        private Regex _regex_word = new Regex(@"^\$?\w+$");


        public int RowIndex
        {
            get { return _tokenTaker.RowIndex; }
        }
        public int BlockIndex
        {
            get { return _index_block; }
            set { _index_block = value; }
        }
        public int StatementIndex
        {
            get { return _index_statement; }
            set { _index_statement = value; }
        }
        public int ExpressionCount
        {
            get { return _count_expression; }
            set { _count_expression = value; }
        }

        public Expression Interpret()
        {
            getToken();
            _result = evalCommaExp();
            return _result;
        }

        public ExpressionInterpreter(ITokenTaker tokenTaker)
        {
            _tokenTaker = tokenTaker;
        }



        private void restore()
        {
            _tokenTaker.Restore();
        }



        public bool trySpecial1(Expression input,out Expression pE)
        {
            Data prefix;
            bool isUnary = false;
            if (input is Data)
            {
                prefix = input as Data;
            }
            else
            {
                isUnary = true;
                prefix = (input as UnaryExpression).E as Data;
            }

            pE = null;
            _tokenTaker.Save();
            Regex r = new Regex("^(\\$?\\w+|[0-9]+(\\.[0-9])*|\"[^\"]*\")$");
            Match m;
            if (Token == ">") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"........"),this);
            List<string> args = new List<string>();
            while ((m = r.Match(Token)).Success)
            {
                args.Add(Token);
                getToken();
                switch (Token)
                {
                    case ">":
                        Expression partialExpression;
                        Data first = new Data(args[0]);
                        if (args.Count == 1) partialExpression = first;
                        else
                        {
                            CommaExpression cE = new CommaExpression(first);
                            for (int i = 1, length = args.Count; i < length; i++)
                            {
                                cE.Add(new Data(args[i]));
                            }
                            partialExpression = cE;
                        }
                        if(isUnary){
                            (input as UnaryExpression).E = new PostfixExpression(prefix, Symbol.GetInstance("<", typeof(PostfixExpression)), partialExpression);
                            pE = input;
                        }
                        else pE = new PostfixExpression(prefix, Symbol.GetInstance("<", typeof(PostfixExpression)), partialExpression);
                        
                        getToken();
                        return true;
                    case ",":
                        getToken();
                        continue;
                    default:
                        restore();
                        return false;
                }
            }
            restore();
            return false;
        }

        public Expression evalCommaExp()
        {
            Expression result = evalAssignmentExp();
            string op;
            CommaExpression comma = null;
            if (Token == ",") comma = new CommaExpression(result);
            while ((op = Token) == ",")
            {                               
                getToken();
                comma.Add(evalAssignmentExp());
            }
            return comma == null ? result : comma;
        }

        public Expression evalAssignmentExp()
        {
            Expression result,lastPartialResult;
            lastPartialResult = result = evalConditionalExp();
            string op;
            List<string> symbols=new List<string>();
            List<Expression> expressions = new List<Expression>();
            expressions.Add(lastPartialResult);
            while (assignmentSymbolList.Contains(op = Token))
            {
                if (!lastPartialResult.IsAssignable) 
                    Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"{0}不是一个合法的赋值表达式",this),this);
                getToken();
                lastPartialResult = evalConditionalExp();
                symbols.Add(op);
                expressions.Add(lastPartialResult);
            }
            if (expressions.Count > 0)
            {
                result = expressions[expressions.Count - 1];
                for (int i = expressions.Count - 2; i >= 0; i--)
                {
                    result = new AssignmentExpression(expressions[i], result
                        , Symbol.GetInstance(symbols[i], typeof(AssignmentExpression)));
                }
            }
            return result;
        }

        public Expression evalConditionalExp()
        {
            Expression result;
            result = evalLogicalORExp();
            string op = Token;
            if (op == "?")
            {
                Expression e1, e2;
                getToken();
                e1 = evalCommaExp();
                if (Token != ":") Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,".........."),this);
                getToken();                
                e2 = evalConditionalExp();
                result = new ConditionalExpression(result, e1, e2);
            }
            return result;
        }

        public Expression evalLogicalORExp()
        {
            Expression result;
            result = evalLogicalANDExp();
            string op = Token;
            while ((op = Token) == "||")
            {
                getToken();
                result = new LogicalORExpression(result, evalLogicalANDExp());
            }
            return result;
        }

        public Expression evalLogicalANDExp()
        {
            Expression result;
            result = evalEqualityExp();
            string op;
            while ((op = Token) == "&&")
            {
                getToken();
                result = new LogicalANDExpression(result, evalEqualityExp());
            }
            return result;
        }

        public Expression evalEqualityExp()
        {
            Expression result;
            result = evalRelationalExp();
            string op;
            while ((op = Token) == "==" || op == "!=")
            {
                switch (op)
                {
                    case "==":
                    case "!=":
                        getToken();
                        result = new EqualityExpression(result, evalRelationalExp(), Symbol.GetInstance(op, typeof(EqualityExpression)));
                        break;
                }
            }

            return result;
        }

        public Expression evalRelationalExp()
        {
            Expression result;
            result = evalAddictiveExp();
            string op=Token;
            switch (op)
            {
                case "<":
                case ">":
                case "<=":
                    getToken();
                    if (op == ">" && Token == "=")
                    {
                        op = ">=";
                        getToken();
                    }
                    if (op == "<" && (result is Data||result is UnaryExpression))
                    {
                        Expression pE;
                        if (trySpecial1(result, out pE))
                        {
                            if (Token == ">" && watchNextToken() == "=")
                            {
                                if (!_tokenTaker.MoveForward())
                                {

                                }
                                getToken();
                                return new RelationalExpression(pE, evalAddictiveExp(), Symbol.GetInstance(">=", typeof(RelationalExpression)));
                            }
                            else return pE;
                        }
                    }
                    result = new RelationalExpression(result, evalAddictiveExp(), Symbol.GetInstance(op, typeof(RelationalExpression)));
                    break;
            }
            return result;
        }

        public Expression evalAddictiveExp()
        {
            Expression result;
            Expression partialResult;
            result = evalMultiplicativeExp();
            string op;
            while ((op = Token) == "+" || op == "-")
            {
                getToken();
                partialResult = evalMultiplicativeExp();
                switch (op)
                {
                    case "-":
                        result = new AdditiveExpression(result, partialResult, Symbol.GetInstance("-", typeof(AdditiveExpression)));
                        break;
                    case "+":
                        result = new AdditiveExpression(result, partialResult, Symbol.GetInstance("+", typeof(AdditiveExpression)));
                        break;
                }
            }
            return result;
        }

        public Expression evalMultiplicativeExp()
        {
            Expression result;
            Expression partialResult;
            result = evalUnaryExp();
            string op;
            while ((op=Token) == "*" || op == "/" || op=="%")
            {
                getToken();
                partialResult = evalUnaryExp();
                switch (op)
                {
                    case "*":
                        result = new MultiplicativeExpression(result, partialResult, Symbol.GetInstance("*", typeof(MultiplicativeExpression)));
                        break;
                    case "/":
                        result = new MultiplicativeExpression(result, partialResult, Symbol.GetInstance("/", typeof(MultiplicativeExpression)));
                        break;
                    case "%":
                        result = new MultiplicativeExpression(result, partialResult, Symbol.GetInstance("%", typeof(MultiplicativeExpression)));
                        break;
                }
            }
            return result;
        }

        public Expression evalUnaryExp()
        {
            Expression result;
            string op = Token;
            bool flag = false;
            switch (op)
            {
                case "++":
                case "--":
                case "!":
                case "+":
                case "-":                
                case "@":
                    getToken();
                    flag = true;                    
                    break;
            }
            result = evalDotExp();
            if (flag) result = new UnaryExpression(result, Symbol.GetInstance(op, typeof(UnaryExpression)));
            return result;
        }

        public Expression evalDotExp()
        {
            Expression result;
            Expression partialResult;
            result = evalPostfixExp();
            string op;
            while ((op = Token) == ".")
            {
                getToken();
                partialResult = evalPostfixExp();
                result = new DotExpression(result, partialResult);
            }
            return result;
        }

        private static HashSet<string> _postfixSets = new HashSet<string>() { "()","(","[","++","--"};
        public Expression evalPostfixExp()
        {
            Expression result;
            Regex r = new Regex(@" *\w+ *");

            result = evalBottom();
            string op;
            string lastOp=null;
            for (int round = 0; _postfixSets.Contains(op = Token); round++)
            {
                switch (op)
                {
                    case "()":
                        getToken();
                        result = new PostfixExpression(result, Symbol.GetInstance("(", typeof(PostfixExpression)), null);
                        break;
                    case "(":
                        getToken();
                        result = new PostfixExpression(result, Symbol.GetInstance(op, typeof(PostfixExpression)), evalCommaExp());
                        if (!Token.Equals(Symbol.GetReserve(op))) 
                            Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "缺少')'"), this);
                        getToken();
                        break;
                    case "[":
                        getToken();
                        result = new PostfixExpression(result, Symbol.GetInstance(op, typeof(PostfixExpression)), evalCommaExp());
                        if (!Token.Equals(Symbol.GetReserve(op))) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext, "缺少']'"), this);
                        getToken();
                        break;
                    case "++":
                    case "--":
                        if (lastOp==op) throw new InterpretException(_tokenTaker.CurrentContext, "不合法的格式");
                        result = new PostfixExpression(result, Symbol.GetInstance(op, typeof(PostfixExpression)));
                        getToken();
                        break;
                }
                lastOp = op;
            }
            return result;
        }

        public Expression evalBottom()
        {
            Expression result;
            string op;
            switch (op=Token)
            {
                case "(":
                    getToken();
                    result = evalCommaExp();
                    if (Token != Symbol.GetReserve(op)) 
                        Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"缺少')'"),this);        
                    getToken();                                
                    break;
                case "<":
                    getToken();
                    result = evalSpecial2();
                    if (Token != ">") 
                        Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"缺少'>'"),this);
                    getToken();
                    break;
                case "[":
                    getToken();
                    result = evalAssignmentExp();
                    if (Token != Symbol.GetReserve(op)) Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"缺少']'"),this);
                    getToken();
                    result = new PostfixExpression(new Data("array"), "[", result);
                    break;
                default:
                    result = atom();
                    break;
            }           
            return result;
        }

        private static Regex r = new Regex("(\\$?\\w+(\\.\\w+)*|\"[^\"]*\")");
        private static Regex r_state = new Regex(@"^[a-zA-Z]\w*\.\w+$");
        private Expression atom()
        {
            Expression result;
            if (Token == null) return null;
            else if (r_state.IsMatch(Token))
            {
                result = new StateExpression(Token);
                getToken();
                return result;
            }
            else if (!r.IsMatch(Token))
            {
                throw new InterpretException(_tokenTaker.CurrentContext, "不合法的格式");
            }
            else
            {
                result = new Data(Token);
                getToken();
                return result;
            }            
        }

        public Expression evalSpecial2()
        {
            Expression result;
            result = evalSpecial2_1();
            string op;
            while ((op = Token) == "|")
            {
                getToken();
                result = new LogicalORExpression(result, evalSpecial2_1());
            }
            return result;
        }

        private Expression evalSpecial2_1()
        {
            Expression result;
            result = evalSpecial2_2();
            string op;
            while ((op = Token) == "&")
            {
                getToken();
                result = new LogicalANDExpression(result, evalSpecial2_2());
            }
            return result;
        }

        private Expression evalSpecial2_2()
        {
            Expression result = null;
            string op;
            switch (op=Token)
            {
                case "!":
                    getToken();
                    return new UnaryExpression(evalSpecial2_2(), "!");
                case "(":
                    getToken();
                    result = evalSpecial2();
                    if (Token != ")") 
                        Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"缺少')'"),this);
                    getToken();
                    return result;
                default:
                    if (!_regex_word.IsMatch(Token)) 
                        Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"格式不正确"),this);
                    //string[] args = Token.Split('_');
                    result = new PostfixExpression(new Data(""), "<", new Data(Token));
                    /*
                    switch (args.Length)
                    {
                        case 1:
                            result = new PostfixExpression(new Data("findA"), "<", new Data(Token));
                            break;
                        case 2:
                            result = new PostfixExpression(new Data("findE"), "<", new Data(Token));
                            break;
                        default:
                            Dict.AddException(new InterpretException(_tokenTaker.CurrentContext,"......"),this);
                            break;
                    }*/
                    getToken();
                    return result;
            }
        }

        private string watchNextToken()
        {
            return _tokenTaker.WatchNextToken();
        }

        public void getToken()
        {
            _tokenTaker.GetToken();
        }

    }
}
