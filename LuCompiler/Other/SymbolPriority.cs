using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class SymbolPriority
    {
        private static List<string[]> _list_symbolPriority = new List<string[]>();
        private static Hashtable<string, int> _table_symbolPriority = new Hashtable<string, int>();

        public static Hashtable<string, int> SymbolPriorityTable
        {
            get { return SymbolPriority._table_symbolPriority; }
        }

        public static void init()
        {
            _list_symbolPriority.Add(new string[]{
                "(" , ")" , "[" , "]"
            });
            _list_symbolPriority.Add(new string[]{
                "++" , "--" , "!"  , @"\-"
            });
            _list_symbolPriority.Add(new string[]{
                "*" , "/" , "%"
            });
            _list_symbolPriority.Add(new string[]{
                "+" , "-"
            });
            _list_symbolPriority.Add(new string[]{
                ">" , ">=" , "<" , "<="
            });
            _list_symbolPriority.Add(new string[]{
                "==" , "!="
            });
            _list_symbolPriority.Add(new string[]{
                "&&"
            });
            _list_symbolPriority.Add(new string[]{
                "||"
            });
            _list_symbolPriority.Add(new string[]{
                "=" , "+=" ,"-=" , "*=" , "/=" , "%=" , "&=" , "|="
            });
            _list_symbolPriority.Add(new string[]{
                ","
            });
            for (int i = 0, length = _list_symbolPriority.Count; i < length; i++)
            {
                foreach (string sym in _list_symbolPriority[i])
                {
                    _table_symbolPriority[sym] = i;
                }
            }
        }

        public static int Compare(Symbol sym1, Symbol sym2)
        {
            int value1 = _table_symbolPriority[sym1.FinalValue];
            int value2 = _table_symbolPriority[sym2.FinalValue];
            if (value1 < value2) return 1;
            else if (value1 == value2) return 0;
            else return -1;
        }
        public static int Compare(string s1, string s2)
        {
            int value1 = _table_symbolPriority[s1];
            int value2 = _table_symbolPriority[s2];
            if (value1 < value2) return 1;
            else if (value1 == value2) return 0;
            else return -1;
        }
    }
}
