using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class KeyWords
    {
        static List<string> list_keywords1 = new List<string>();

        public static void init()
        {
            string[] words={"if","while"};
        }

        public static string RegexStr
        {
            get
            {
                return "";
            }
        }

    }
}
