using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class FindAFunction : AEFindFunction
    {
        public string S_FunctionName
        {
            get { return _s_functionName; }
        }

        public FindAFunction(Context context, string arg)
            : base(context, arg)
        {
            G.AEFunctions["finda_" + arg] = this;
        }

        protected override void _register()
        {
            base._register();            
            _s_picPath = string.Format("a_{0}_{1}.bmp", _parentArea, _AEObject);
            if (!G.IsDebugMode)
            {
                _s_picPath = AEObject.MixCode + ".bmp";
            }
            else _s_functionName = string.Format("sfa_{0}", _arg);
        }

        public override string QTranslate()
        {
            FindableArea fa = AEObject as FindableArea;
            Area parent = fa.Parent;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            string debugString;
            if (!(parent is FindableArea))
            {
                if (parent != G.Root)
                {
                    debugString = M.GetDebugLog("\"找域[{0}]失败，因为所属域不可用且不可找。\"", DebugLevel.AllDetailLevel, fa.FullName);
                    sb2.AppendLine(string.Format("If NOT({0}) Then : {1} = False : {2}Exit Function : End If :", _s_parentArea_available, _s_functionName, debugString));
                }
            }
            else
            {                
                FindAFunction functionFa = G.AEFunctions["finda_" + parent.Name] as FindAFunction;
                //失败日志
                debugString = M.GetDebugLog("\"找域[{0}]失败，因为其所属域不可用且找不到所属域。\"", DebugLevel.AllDetailLevel, fa.FullName);
                //假如找域失败
                string sFindArea = string.Format("If NOT({0}({3})) Then : {1} = False: {2}Exit Function : End If", functionFa.S_FunctionName, _s_functionName, debugString,_s_delay);
                //假如域不可用，先找域
                sb2.AppendLine(string.Format("If NOT({0}) Then : {1} : End If :", _s_parentArea_available, sFindArea));
            }
            sb2.Append("Dim ret : ");
            //在父域中全屏扫描
            string fullScan = string.Format("ret = 0 = dm.FindPic({0},{1},{2},{3},\"{4}\",\"{5}\",{6},{7},{8},{9}) : Delay {10} : ",
fa.S_fBound_left, fa.S_fBound_top, fa.S_fBound_right, fa.S_fBound_bottom, _s_picPath, fa.OffsetColor, fa.Sim, 0, _s_retX, _s_retY,_s_delay);
            fullScan += M.GetDebugLog("\"找域[{0}] : [{1},{2}] -> \"&{3} : ", DebugLevel.AllDetailLevel, fa.FullName, fa.OffsetColor, fa.Sim, fa.S_findpos_info);

            string fixedScan = string.Format("ret = 0 = dm.FindPic({0},{1},{2},{3},\"{4}\",\"{5}\",{6},{7},{8},{9}) : Delay {10} : ",
fa.S_lFound_left, fa.S_lFound_top, fa.S_lFound_right, fa.S_lFound_bottom, _s_picPath, fa.OffsetColor, fa.Sim, 0, _s_retX, _s_retY,_s_delay);
            fixedScan += M.GetDebugLog("\"找域[{0}] : [{1},{2}] -> \"&{3} : ", DebugLevel.AllDetailLevel, fa.FullName, fa.OffsetColor, fa.Sim, fa.S_lFoundPosInfo);

            string refreshPos = string.Format("{0}={1} : {2}={3} : {4}{5}",fa.S_lFound_left,fa.S_retX,
                fa.S_lFound_top,fa.S_retY,fa.Q_UpdateRelatedInfoByRetPos(),fa.Q_GetFollowingString(null));

            switch (fa.SearchMode)
            {
                case SearchMode.FixedInArea:
                    break;
                case SearchMode.SearchInRange:
                    break;
                case SearchMode.SearchInArea:
                    break;
            }


            string scan = string.Format("If {0} Then : {1} : Else : {2} : If ret Then : {3} End If : End If : {4}=ret : {5}", _s_available, fa.DoesUseLastFoundPos ? fixedScan : fullScan, fullScan, refreshPos, _s_functionName,
                M.GetCondtionDebugLog("ret", DebugLevel.AllDetailLevel, "\"找到域[{0}],返回键坐标:\"&{1}", "\"找不到域[{0}]\"", fa.FullName, M.QCommaFormat1(_s_retX, _s_retY)));

            sb2.AppendLine(scan);
            sb.AppendLine(string.Format("Function {0}({1})", _s_functionName,_s_delay))
                .Append(M.Q_AddTab(sb2.ToString()))
                .AppendLine("End Function");
            return sb.ToString();
        }
    }
}
