using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace LuCompiler
{
    public class FileLinker
    {
        private string _src;
        public FileLinker(string src)
        {
            _src = src;
        }

        public string Link()
        {
            Regex r_import = new Regex(@"(//)?#import\[([^\]]+)\]");
            MatchCollection mc = r_import.Matches(_src);
            Replacer replacer = new Replacer(_src);
            if (mc.Count == 0) return _src;
            else
            {
                foreach (Match m in mc)
                {
                    if (m.Value.StartsWith("//")) continue;
                    string importFileName = m.Groups[2].Value;
                    if (!File.Exists(importFileName))
                    {
                        throw new InterpretException(Context.Empty, string.Format("#import错误,文件{0}不存在", importFileName));
                    }
                    G.ImportFiles.Add(importFileName);
                    string replacement = new FileLinker(Dict.GetFileContent(importFileName)).Link();
                    replacer.AddInfo(m.Value, replacement);
                }
            }
            return replacer.GetReplacedString();
        }
    }
}
