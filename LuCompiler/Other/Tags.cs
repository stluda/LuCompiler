using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace LuCompiler
{
    public class Tags
    {
        public static void CreateCTags()
        {
            using (StreamWriter sw = new StreamWriter("openctags.tags"))
            {
              

                Regex reg = new Regex("^[0-9a-zA-Z_]+$");
                Hashtable<string, string> table = new Hashtable<string,string>();
                List<string> names = new List<string>();
                Action<string> myAdd = (string value) =>
                {
                    if (!names.Contains(value)) names.Add(value);
                };
                foreach (Macro macro in Dict.MacroOfKey.Values)
                {
                    if (!reg.IsMatch(macro.Key)) continue;
                    table[macro.Key] = string.Format("{0}\t{1}\t/^{2}$/;\"\tm", macro.Key, G.FileName, macro.SrcCtx.LineValue);
                    myAdd(macro.Key);
                }

                foreach (Variable var in G.GlobalVariables.Values)
                {
                    if (var is Function) continue;
                    if (var.Name == null) continue;
                    if (!reg.IsMatch(var.Name)) continue;
                    table[var.Name] = string.Format("{0}\t{1}\t/^{2}$/;\"\tv", var.Name, G.FileName, var.Context.LineValue);
                    myAdd(var.Name);
                    //sw.WriteLine(string.Format("{0}\t{1}\t/^{2}$/;\"\tv", var.Name, G.FileName, var.Context.LineValue));
                }

                foreach (Function func in G.Functions)
                {
                    if (func.Name == null) continue;
                    if (!reg.IsMatch(func.Name)) continue;
                    table[func.Name] = string.Format("{0}\t{1}\t/^{2}$/;\"\tf", func.Name, G.FileName, func.Context.LineValue);
                    myAdd(func.Name);
                    //sw.WriteLine(string.Format("{0}\t{1}\t/^{2}$/;\"\tf", func.Name, G.FileName, func.Context.LineValue));
                }
                foreach (Function func in G.Kernals)
                {
                    if (func.Name == null) continue;
                    if (!reg.IsMatch(func.Name)) continue;
                    table[func.Name] = string.Format("{0}\t{1}\t/^{2}$/;\"\tf", func.Name, G.FileName, func.Context.LineValue);
                    myAdd(func.Name);
                    //sw.WriteLine(string.Format("{0}\t{1}\t/^{2}$/;\"\tf", func.Name, G.FileName, func.Context.LineValue));
                }
                Comparison<string> mySort = null;
                Func<string, string, int,int> sortFunc = null;
                sortFunc= (string s1, string s2,int i) =>
                {
                    if(s1[i]<s2[i])return -1;
                    else if(s1[i]>s2[i])return 1;
                    else
                    {
                        if (i < Math.Min(s1.Length, s2.Length)-1) return sortFunc(s1, s2, i + 1);
                        else return s1.Length.CompareTo(s2.Length);;
                    }
                };

                mySort = (string s1, string s2) => sortFunc(s1, s2, 0);

                names.Sort(mySort);
                foreach (string name in names) sw.WriteLine(table[name]);
            }
        }
    }
}
