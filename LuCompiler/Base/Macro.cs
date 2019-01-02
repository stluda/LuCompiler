using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LuCompiler
{
    public class Macro : SyntaxElement
    {
        private string _key;
        private string _src;
        private string _dst;
        private Context _srcCtx;

        public Context SrcCtx
        {
            get { return _srcCtx; }
        }
        private Context _dstCtx;

        public string Key
        {
            get { return _key; }
        }

        private static Regex _r_capture=new Regex(@"\$([0-9]+)");
        private static Regex _r_unwantedSyntax = new Regex(@"[\t\r\n]");

        private Hashtable<int, int> _srcCaptures = new Hashtable<int, int>();
        private Hashtable<int, int> _dstCaptures = new Hashtable<int, int>();
        private Regex _r_macro;
        private string[] _rawSrcTokens;
        private string[] _rawDstTokens;
        

        public Macro(string key,string src, string dst,Context srcCtx,Context dstCtx)
        {
            _key = key;
            _src = _r_unwantedSyntax.Replace(src,"");
            _dst = _r_unwantedSyntax.Replace(dst,"");
            _srcCtx = srcCtx;
            _dstCtx = dstCtx;
            _init();
            if (Dict.MacroOfKey.ContainsKey(key)) throw new InterpretException(srcCtx,string.Format( "宏重复定义，键名[{0}]重复",key));
            Dict.MacroOfKey[key] = this;
        }

        private void _init()
        {
            _rawSrcTokens = ElementRegex.GetValues(_src);
            StringBuilder sb = new StringBuilder("^");
            for (int i = 0, length = _rawSrcTokens.Length; i < length; i++)
            {
                string token = _rawSrcTokens[i];
                Match m = null;
                
                if ((m = _r_capture.Match(token)).Success)
                {
                    int captureIndex = int.Parse(m.Groups[1].Value);
                    if (_srcCaptures.ContainsValue(captureIndex))
                        throw new InterpretException(_srcCtx, string.Format("宏传入参数重复(组{0})",captureIndex));
                    _srcCaptures[i] = captureIndex;
                    sb.Append("(\\$?\\w+|\"[^\"]*\")");
                }
                else sb.Append(_toRegexStr(token));
            }
            sb.Append("$");
            _r_macro = new Regex(sb.ToString());


            _rawDstTokens = ElementRegex.GetValues(_dst);
            for (int i = 0, length = _rawDstTokens.Length; i < length; i++)
            {
                string token = _rawDstTokens[i];
                Match m = null;
                if ((m = _r_capture.Match(token)).Success)
                {
                    int captureIndex = int.Parse(m.Groups[1].Value);
                    if (!_srcCaptures.ContainsValue(captureIndex))
                        throw new InterpretException(_srcCtx, string.Format("传入参数中不包含(组{0})",captureIndex));
                    _dstCaptures[i] = int.Parse(m.Groups[1].Value);
                }               
            }
        }

        public Statement GetStatement(PostfixExpression pe)
        {
            string input = _r_unwantedSyntax.Replace(pe.ToString(),"");
            if (_r_macro.IsMatch(input))
            {
                string[] tokens = ElementRegex.GetValues(input);
                Hashtable<int, string> map = new Hashtable<int, string>();
                foreach (int i in _srcCaptures.Keys)
                {
                    int captureIndex = _srcCaptures[i];
                    map[captureIndex] = tokens[i];
                }
                string[] dstTokenSource = new string[_rawDstTokens.Length];
                _rawDstTokens.CopyTo(dstTokenSource, 0);
                foreach (int i in _dstCaptures.Keys)
                {
                    int captureIndex = _dstCaptures[i];
                    dstTokenSource[i] = map[captureIndex];
                }
                ITokenTaker bak = G.TokenTaker;
                ITokenTaker tokenTaker = new DefinedTokenTaker(dstTokenSource, _dstCtx);
                G.TokenTaker = tokenTaker;
                StatementInterpreter si = new StatementInterpreter(tokenTaker);
                Statement stmt = si.Interpret();
                G.TokenTaker = bak;
                return stmt;
            }
            else return null;
        }

        private string _toRegexStr(string input)
        {
            switch (input)
            {
                case "(":
                case ")":
                case "{":
                case "}":
                case "[":
                case "]":
                case "|":
                case "$":
                    return @"\" + input;
                default:
                    return input;
            }
        }

    }
}
