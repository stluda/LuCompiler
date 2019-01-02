using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public static class SysFunction
    {
        public static void Append_Initializer(StringBuilder sb)
        {
            string name = M.EVar("initializer");
            sb.AppendLine(string.Format("Sub {0}", name))
                .AppendLine(string.Format("{0}", M.Q_AddTab(G.Root.Q_GetFollowingString(null))))
                .AppendLine("End Sub\r\n");
        }

        public static void Append_Cpu(StringBuilder sb)
        {
            string name = M.EVar("cpu");
            string s_g_cpu_iPtr = M.EVar("g_cpu_iPtr");
            string s_common_check = M.EVar("common_check");
            string s_ExecuteInstruction = M.EVar("ExecuteInstruction");
            sb.AppendLine(string.Format("Sub {0}", name))
                .AppendLine(string.Format("\t{0} = 0", s_g_cpu_iPtr))
                .AppendLine("\tWhile True")
                .AppendLine(string.Format("\t\t{0} : {1}", s_common_check, s_ExecuteInstruction))
                .AppendLine("\tWend")
                .AppendLine("End Sub\r\n");

        }

        public static void Append_ExecuteInstruction(StringBuilder sb)
        {
            string name = M.EVar("ExecuteInstruction");
            string s_g_cpu_iPtr = M.EVar("g_cpu_iPtr");
            sb.AppendLine(string.Format("Sub {0}",name))
    .AppendLine(string.Format("\tSelect Case {0}",s_g_cpu_iPtr));
            for (int i = 0, length = G.Modules.Count; i < length; i++)
            {
                sb.AppendLine(string.Format("\tcase {0} : {1}", i,M.EVar(string.Format("sys_module{0}",i))));
            }
            sb.AppendLine(string.Format("\tcase {0} : ExitScript", G.Modules.Count))
                .AppendLine("\tEnd Select")
                .AppendLine(string.Format("\t{0} = {0} + 1",s_g_cpu_iPtr))
                .AppendLine("End Sub\r\n");
        }

        public static void Append_SetDictionary(StringBuilder sb)
        {
            sb.AppendLine("Sub SetOcrDictionary");
            IEnumerable<int> indexs = Dict.UsedDictIndexs.Distinct<int>();
            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("Select Case dm.GetOsType");
            sb2.Append("Case 3 : ");
            foreach (int index in indexs) sb2.Append(string.Format("dm.SetDict {0},\"b{0}.txt\" : ",index));
            sb2.AppendLine().Append("Case Else : ");
            foreach (int index in indexs) sb2.Append(string.Format("dm.SetDict {0},\"a{0}.txt\" : ", index));
            sb2.AppendLine().AppendLine("End Select");
            sb.Append(M.Q_AddTab(sb2.ToString())).AppendLine("End Sub\r\n");                
        }
    }
}
