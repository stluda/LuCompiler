using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class CaptureEFunction : AEFindFunction
    {
        private string _s_path;

        public CaptureEFunction(Context context, string arg)
            : base(context, arg)
        {
            G.AEFunctions["capturee_" + arg] = this;
        }

        protected override void _register()
        {
            base._register();
            _s_path = M.EVar("path");
            /*
            
            _s_picPath = string.Format("e_{0}_{1}.bmp",_parentArea,_AEObject);
            if (!G.IsDebugMode)
            {
                _s_picPath = AEObject.MixCode + ".bmp";
            }
            else _s_functionName = string.Format("sfe_{0}", _arg);*/
        }



        public override string QTranslate()
        {
            Element e = AEObject as Element;
            Area area = e.Parent;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();            
            string debugString;
            #region 确保所属域可用，否则直接退出
            if (!(area is FindableArea))
            {
                if (area != G.Root)
                {
                    debugString =  M.ValueIfDebug(string.Format("{0} \"{1}\" : ",DebugLogFunction.Name_FoundElementFailed1,e.FullName),DebugLevel.AllDetailLevel);                    
                    sb2.AppendLine(string.Format("If NOT({0}) Then : {1} = False : {2} Exit Function : End If :", _s_parentArea_available, _s_functionName, debugString));
                }
            }
            else
            {                
                FindAFunction fa = G.AEFunctions["finda_"+area.Name] as FindAFunction;
                //失败日志
                debugString = M.ValueIfDebug(string.Format("{0} \"{1}\" : ", DebugLogFunction.Name_FoundElementFailed2, e.FullName), DebugLevel.AllDetailLevel);                
                //假如找域失败
                string sFindArea = string.Format("If NOT({0}) Then : {1} = False: {2}Exit Function : End If", fa.S_FunctionName, S_FunctionName, debugString);
                //假如域不可用，先找域
                sb2.AppendLine(string.Format("If NOT({0}) Then : {1} : End If :", _s_parentArea_available, sFindArea, _s_functionName));
            }
            #endregion
            sb2.Append("Dim ret : ");

            //string resultLog = M.GetCondtionDebugLog("ret", DebugLevel.AllDetailLevel, "\"找到元素[{0}],返回键坐标:\"&{1}", "\"找不到元素[{0}]\"", e.FullName, M.QCommaFormat1(_s_retX, _s_retY));
            string resultLog = "";



            FindableElement fe = e as FindableElement;
            sb2.AppendLine(string.Format("ret = 0 = dm.Capture({0},{1},{2},{3},{4}) : Delay 50 : {10}=ret : ",
fe.S_fBound_left, fe.S_fBound_top, fe.S_fBound_right, fe.S_fBound_bottom, _s_path, _s_functionName));



            sb2.AppendLine(M.ValueIfDebug(string.Format("{0} ret,\"{1}\",{2},{3},{4},{5} : "
                , DebugLogFunction.Name_CaptureElementDetailInfo, e.FullName,
                e.S_fBound_left, e.S_fBound_top, e.S_fBound_right, e.S_fBound_bottom), DebugLevel.AllDetailLevel));

            sb.AppendLine(string.Format("Function {0}({1})", _s_functionName, _s_path))
                .Append(M.Q_AddTab(sb2.ToString()))
                .AppendLine("End Function");
            return sb.ToString();
    }

    }
}
