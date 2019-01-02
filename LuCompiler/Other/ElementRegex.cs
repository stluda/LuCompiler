using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LuCompiler
{
    public static class ElementRegex
    {
        private static List<string> _list_elements = new List<string>();
        private static bool _hasInit = false;
        private static string _pattern;
        private static Regex _regex;
        private static string _regexString;

        private static void _init()
        {

            #region oneSymbols
                List<string> _oneSymbols = new List<string>();
                _oneSymbols.Add(@"\+");
                _oneSymbols.Add(@"\-");
                _oneSymbols.Add(@"\*");
                _oneSymbols.Add(@"/");
                _oneSymbols.Add(@"%");
                _oneSymbols.Add(@">");
                _oneSymbols.Add(@"<");
                _oneSymbols.Add(@"=");
                _oneSymbols.Add(@"\(");
                _oneSymbols.Add(@"\)");
                _oneSymbols.Add(@"&");
                _oneSymbols.Add(@"\|");
                //_oneSymbols.Add(@"\w");
                StringBuilder sb = new StringBuilder();
                sb.Append("[^");
                foreach (string sym in _oneSymbols)
                {
                    sb.Append(sym);
                }
                string exclude_osl = sb.Append("]").ToString();
            #endregion
            //_list_elements.Add(@"/\*");
            //_list_elements.Add(@"\*/");
            //_list_elements.Add(@"//");
            _list_elements.Add("\"(\\\\\"|[^\"])*\"");
            //_list_elements.Add("\"[^\"]*\"");
            _list_elements.Add(@"'\w+'");
            _list_elements.Add(@"\$[0-9]+");
            _list_elements.Add(@"[0-9]+\.[0-9]+");
            //_list_elements.Add(@"\$?\w+(\.\w+)*");
            _list_elements.Add(@"\$?\w+");
            _list_elements.Add(@"[~#]\w+");          
            _list_elements.Add(@"&&=");
            _list_elements.Add(@"\|\|=");
            _list_elements.Add(@"&&");
            _list_elements.Add(@"\|\|");
            _list_elements.Add(@"\+\+");
            _list_elements.Add(@"\-\-");
            //_list_elements.Add(@">=");
            _list_elements.Add(@"=>");
            _list_elements.Add(@"<=");
            _list_elements.Add(@"==");
            _list_elements.Add(@"!=");
            _list_elements.Add(@"\+=");
            _list_elements.Add(@"\-=");
            _list_elements.Add(@"\*=");
            _list_elements.Add(@"/=");
            _list_elements.Add(@"%=");            
            _list_elements.Add(@"\w+");
            _list_elements.Add(@"\$\{");
            _list_elements.Add(@"\}\$");
            _list_elements.Add(@"\{");
            _list_elements.Add(@"\}");
            _list_elements.Add(@"\[\]");            ;
            _list_elements.Add(@"\[");
            _list_elements.Add(@"\]");
            _list_elements.Add(@",");
            _list_elements.Add(@"\(\)");
            _list_elements.Add(@"\(");
            _list_elements.Add(@"\)");
            _list_elements.Add(@"\+");
            _list_elements.Add(@"\-");
            _list_elements.Add(@"\*");
            _list_elements.Add(@"\.");
            _list_elements.Add(@"/");
            _list_elements.Add(@"%");
            _list_elements.Add(@">");
            _list_elements.Add(@"<");
            _list_elements.Add(@"=");
            _list_elements.Add(@"\?");
            _list_elements.Add(@":");
            _list_elements.Add(@"!");
            _list_elements.Add(@"@");
            _list_elements.Add(@";");
            _list_elements.Add(@"&");
            _list_elements.Add(@"\|");
            sb = new StringBuilder();
            sb.Append(@"^[ \t]*([ \t]*(");
            foreach (string sym in _list_elements)
            {
                sb.Append(sym).Append("|");
            }
            sb.Remove(sb.Length - 1, 1).Append(")[ \t]*)*[ \t]*$");
            _regexString = sb.ToString();
            _regex = new Regex(_regexString);
            _hasInit = true;
        }

        public static Match Match(string input)
        {
            if (!_hasInit) _init();
            return _regex.Match(input);
        }

        public static string[] GetValues(string input)
        {
            Match m = Match(input);
            if (!m.Success) return null;
            string[] ret = new string[m.Groups[2].Captures.Count];
            int i=0;
            foreach (Capture c in m.Groups[2].Captures)
            {
                ret[i++] = c.Value;
            }
            return ret;
        }
    }

    public static class ElementReader
    {
        //public static
    }
}
