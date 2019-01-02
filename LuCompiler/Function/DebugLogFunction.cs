using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public static class DebugLogFunction
    {
        public static readonly string Name_FoundElementFailed1 = "dlg1";
        public static readonly string Name_FoundElementFailed2 = "dlg2";
        public static readonly string Name_FoundElementStrDetailedInfo = "dlg3";
        public static readonly string Name_CaptureElementDetailInfo = "dlg4";

        public static string GetDebugLogFunctionStrings()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(FoundElementFailed1());
            sb.AppendLine(FoundElementFailed2());//FoundElementStrDetailedInfo
            sb.AppendLine(FoundElementStrDetailedInfo());//
            sb.AppendLine(CaptureElementDetailInfo());
            return sb.ToString();
        }

        public static string FoundElementFailed1()
        {
            QFunctionWriter qfw = new QFunctionWriter(QFunctionType.Sub, Name_FoundElementFailed1);
            string inArg = "e";
            qfw.AddArg(inArg);
            qfw.AddLine(M.GetDebugLog("\"找元素[\"&{0}&\"]失败，因为所属域不可用且不可找。\"", DebugLevel.AllDetailLevel, inArg));
            return qfw.ToString();
        }

        public static string FoundElementFailed2()
        {
            QFunctionWriter qfw = new QFunctionWriter(QFunctionType.Sub, Name_FoundElementFailed2);
            string inArg = "e";
            qfw.AddArg(inArg);
            qfw.AddLine(M.GetDebugLog("\"找元素[\"&{0}&\"]失败，因为其所属域不可用且找不到所属域。\"", DebugLevel.AllDetailLevel, inArg));
            return qfw.ToString();
        }

        public static string FoundElementStrDetailedInfo()
        {
            QFunctionWriter qfw = new QFunctionWriter(QFunctionType.Sub, Name_FoundElementStrDetailedInfo);
            string ret,element,left, top, right, bottom;
            string colorInfo, sim;

            ret = "ret"; element = "e"; left = "l"; top = "t"; right = "r"; bottom = "b";
            colorInfo = "color"; sim = "sim";

            qfw.AddArg(ret).AddArg(element).AddArg(left).AddArg(top).AddArg(right).AddArg(bottom)
                .AddArg(colorInfo).AddArg(sim);


            //sb2.Append(M.GetDebugLog("\"找元素{0}[{1},{2}] -> \"&{3} : ", DebugLevel.AllDetailLevel, e.FullName, ocrInfo.Color_info.Trim('"'), ocrInfo.Sim, e.S_findpos_info));
            qfw.AddLine(M.GetDebugLog("\"找元素\"&{0}&\"[\"&{1}&\",\"&{2}&\"] -> \"&{3} : ", DebugLevel.AllDetailLevel, element,colorInfo,sim,
                M.QCommaFormat1(left, top, right, bottom)));
            qfw.AddLine( M.GetCondtionDebugLog("ret", DebugLevel.AllDetailLevel, "\"找到元素[\"&{0}&\"],返回键坐标:\"&{1}", "\"找不到元素[\"&{0}&\"]\""
                , element, M.QCommaFormat1(M.EVar("g_retX"), M.EVar("g_retY"))) );
            //qfw.AddLine(
            return qfw.ToString();
        }


        public static string CaptureElementDetailInfo()
        {
            QFunctionWriter qfw = new QFunctionWriter(QFunctionType.Sub, Name_CaptureElementDetailInfo);
            string ret, element, left, top, right, bottom;

            ret = "ret"; element = "e"; left = "l"; top = "t"; right = "r"; bottom = "b";

            qfw.AddArg(ret).AddArg(element).AddArg(left).AddArg(top).AddArg(right).AddArg(bottom);


            //sb2.Append(M.GetDebugLog("\"找元素{0}[{1},{2}] -> \"&{3} : ", DebugLevel.AllDetailLevel, e.FullName, ocrInfo.Color_info.Trim('"'), ocrInfo.Sim, e.S_findpos_info));
            qfw.Add(M.GetDebugLog("\"截屏元素\"&{0}&\" -> \"&{1} : ", DebugLevel.AllDetailLevel, element,
                M.QCommaFormat1(left, top, right, bottom)));
            qfw.AddLine(M.GetCondtionDebugLog("ret", DebugLevel.AllDetailLevel, "\"成功\"", "\"失败\""));
            //qfw.AddLine(
            return qfw.ToString();
        }
    }
}
