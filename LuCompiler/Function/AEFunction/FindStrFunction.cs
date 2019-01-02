using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class FindStrFunction : AEFindFunction
    {
        private string _s_str;
        private string _s_colorInfo;
        private string _s_sim;
        private string _s_fontname;
        private string _s_fontsize;
        private string _s_flag;

        public static FindStrFunction GetInstance(Context context, string arg)
        {
            string errmsg;
            arg = arg.ToLower();
            AEObject obj = Dict.GetAEObject(arg, out errmsg);
            if (obj == null) throw new InterpretException(context, errmsg);
            AEFunction func = G.AEFunctions["findstr_" + arg];
            if (func != null) return func as FindStrFunction;
            else return new FindStrFunction(context, arg);
        }

        public FindStrFunction(Context context, string arg)
            : base(context, arg)
        {
            G.AEFunctions["findstr_" + arg] = this;
        }

        protected override void _register()
        {
            base._register();
            if (G.IsDebugMode) _s_functionName = string.Format("sys_findStr_{0}", _arg);
            _s_str = "str";
            _s_colorInfo = "colorInfo";
            _s_sim = "sim";
            _s_fontname = "fontname";
            _s_fontsize = "fontsize";
            _s_flag = "flag";
            if (!G.IsDebugMode)
            {
                _s_str = M.EVar(_s_str);
                _s_colorInfo = M.EVar(_s_colorInfo);
                _s_sim = M.EVar(_s_sim);
                _s_fontname = M.EVar(_s_fontname);
                _s_fontsize = M.EVar(_s_fontsize);
                _s_flag = M.EVar(_s_flag);
            }
        }

        public override string QTranslate()
        {
            AEObject e = AEObject;
            Area area = AEObject.Parent;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            string debugString;
            #region 确保所属域可用，否则直接退出
            if (!(area is FindableArea))
            {
                if (area != G.Root)
                {
                    debugString = M.GetDebugLog("\"找元素[{0}]失败，因为所属域不可用且不可找。\"", DebugLevel.AllDetailLevel, e.FullName);
                    sb2.AppendLine(string.Format("If NOT({0}) Then : {1} = False : {2} Exit Function : End If :", _s_parentArea_available, _s_functionName, debugString));
                }
            }
            else
            {
                FindAFunction fa = G.AEFunctions["finda_" + area.Name] as FindAFunction;
                //失败日志
                debugString = M.GetDebugLog("\"找元素[{0}]失败，因为其所属域不可用且找不到所属域。\"", DebugLevel.AllDetailLevel, e.FullName);
                //假如找域失败
                string sFindArea = string.Format("If NOT({0}) Then : {1} = False: {2}Exit Function : End If", fa.S_FunctionName, S_FunctionName, debugString);
                //假如域不可用，先找域
                sb2.AppendLine(string.Format("If NOT({0}) Then : {1} : End If :", _s_parentArea_available, sFindArea, _s_functionName));
            }
            #endregion
            sb2.Append("Dim ret : ");
            string resultLog = M.GetCondtionDebugLog("ret", DebugLevel.AllDetailLevel, "\"在[{0}]中找到字符串'\"&{1}&\"',返回坐标:\"&{2}", "\"在[{0}]中找不到字符串'\"&{1}&\"'\"", AEObject.FullName, _s_str, M.QCommaFormat1(_s_retX, _s_retY));
          
           
    //        sb2.AppendLine(string.Format("ret = 0 = dm.FindStrWithFont({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}) : Delay 50 : {12}=ret : ",
    //e.S_fBound_left, e.S_fBound_top, e.S_fBound_right, e.S_fBound_bottom, _s_str, _s_colorInfo, _s_sim, _s_fontname, _s_fontsize, _s_flag, _s_retX, _s_retY, _s_functionName));


            sb2.AppendLine(string.Format("ret = 0 = dm.FindStrWithFont({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}) : Delay 50 : {12}=ret : ",
    e.S_x, e.S_y, e.S_right, e.S_bottom, _s_str, _s_colorInfo, _s_sim, _s_fontname, _s_fontsize, _s_flag, _s_retX, _s_retY, _s_functionName));
            


            /*sb2.AppendLine(string.Format("ret = 0 = dm.FindStrWithFont({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}) : Delay 50 : {12}=ret : ",
                e.S_fBound_left, e.S_fBound_top, e.S_fBound_right, e.S_fBound_bottom, _s_str,_s_colorInfo, _s_sim,_s_fontname,_s_fontsize,_s_flag, _s_retX, _s_retY, _s_functionName));*/
            //sb2.Append(M.GetDebugLog("\"在{0}中找字符串[{1},{2}] -> \"&{3} : ", DebugLevel.AllDetailLevel, e.FullName,_s_colorInfo, _s_sim, e.S_findpos_info));

            sb2.Append(M.GetDebugLog("\"在{0}中找字符串[\"&{1}&\",\"&{2}&\"] -> \"&{3} : ", DebugLevel.AllDetailLevel, e.FullName, _s_colorInfo, _s_sim, e.S_bound_info));
                        
            
            sb2.AppendLine(resultLog);

            sb.AppendLine(string.Format("Function {0}({1},{2},{3},{4},{5},{6})", _s_functionName,_s_str,_s_colorInfo,_s_sim,_s_fontname,_s_fontsize,_s_flag))
                .Append(M.Q_AddTab(sb2.ToString()))
                .AppendLine("End Function");
            return sb.ToString();
        }


    }
}
