using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Replacer
    {
        private string _src;
        private HashSet<string> _set_srcs = new HashSet<string>();
        private HashSet<string> _set_replacements = new HashSet<string>();
        private List<string> _srcs = new List<string>();
        private List<string> _replacements = new List<string>();

        public Replacer(string src)
        {
            _src = src;
        }

        public void AddInfo(string src, string replacement)
        {
            if (_set_srcs.Contains(src)) return;
            _set_srcs.Add(src);
            _set_replacements.Add(replacement);
            _srcs.Add(src);
            _replacements.Add(replacement);
        }

        public string GetReplacedString()
        {
            StringBuilder sb = new StringBuilder(_src);
            for (int i = _replacements.Count - 1; i >= 0; i--)
            {
                sb.Replace(_srcs[i], _replacements[i]);
            }
            return sb.ToString();
        }
    }
}
