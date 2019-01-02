using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class FindEFunction : AEFindFunction
    {                

        public FindEFunction(Context context, string arg)
            : base(context, arg)
        {
            G.AEFunctions["finde_" + arg] = this;
        }

        protected override void _register()
        {
            base._register();
            _s_picPath = string.Format("e_{0}_{1}.bmp",_parentArea,_AEObject);
            if (!G.IsDebugMode)
            {
                _s_picPath = AEObject.MixCode + ".bmp";
            }
            else _s_functionName = string.Format("sfe_{0}", _arg);
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
                string sFindArea = string.Format("If NOT({0}({3})) Then : {1} = False: {2}Exit Function : End If", fa.S_FunctionName, S_FunctionName, debugString,_s_delay);
                //假如域不可用，先找域
                sb2.AppendLine(string.Format("If NOT({0}) Then : {1} : End If :", _s_parentArea_available, sFindArea));
            }
            #endregion
            sb2.Append("Dim ret : ");

            //string resultLog = M.GetCondtionDebugLog("ret", DebugLevel.AllDetailLevel, "\"找到元素[{0}],返回键坐标:\"&{1}", "\"找不到元素[{0}]\"", e.FullName, M.QCommaFormat1(_s_retX, _s_retY));
            string resultLog = "";

            if (e.OcrStringInfo == null)
            {
                #region 通过找图          
                FindableElement fe = e as FindableElement;
                sb2.AppendLine(string.Format("ret = 0 = dm.FindPic({0},{1},{2},{3},\"{4}\",\"{5}\",{6},{7},{8},{9}) : Delay {10} : {11}=ret : ",
    fe.S_fBound_left, fe.S_fBound_top, fe.S_fBound_right, fe.S_fBound_bottom, _s_picPath, fe.OffsetColor, fe.Sim, 0, _s_retX, _s_retY,_s_delay ,_s_functionName));

                sb2.AppendLine(M.ValueIfDebug(string.Format("{0} ret,\"{1}\",{2},{3},{4},{5},\"{6}\",{7} : "
                    , DebugLogFunction.Name_FoundElementStrDetailedInfo, e.FullName,
                    e.S_fBound_left, e.S_fBound_top, e.S_fBound_right, e.S_fBound_bottom,
                    fe.OffsetColor, fe.Sim), DebugLevel.AllDetailLevel));

                //sb2.Append(M.GetDebugLog("\"找元素[{0}] : [{1},{2}] -> \"&{3} : ", DebugLevel.AllDetailLevel, fe.FullName, fe.OffsetColor, fe.Sim, fe.S_findpos_info));

                //刷新坐标
                string refreshPos = string.Format("{0}={1} : {2}={3} : {4}{5}", fe.S_lFound_left, fe.S_retX,
                    fe.S_lFound_top, fe.S_retY, fe.Q_UpdateRelatedInfoByRetPos(), fe.Q_GetFollowingString(null));
                //如果元素本身可用，只返回结果，否则刷新坐标
                string afterScan;                
                if (fe.SearchMode==SearchMode.FixedInArea)
                {
                    afterScan = string.Format("If {0} Then : {2}Exit Function : Else : If ret then :{1}End If : {2} End If", _s_available, refreshPos,
                       resultLog);
                }
                else
                {
                    afterScan = string.Format("{0}{1}", refreshPos, resultLog);
                }
                sb2.AppendLine(afterScan);
                #endregion
            }
            else
            {
                OcrStringInfo ocrInfo = e.OcrStringInfo;
                string refreshPos = string.Format("{0}={1}+3 : {2}={3}+3 : ", e.S_x,e.S_retX, e.S_y,e.S_retY);
                //暂时不支持找字的同步模式
                sb2.AppendLine(string.Format("dm.UseDict {10} : ret = 0 = dm.FindStrFast({0},{1},{2},{3},{4},{5},{6},{7},{8}) : Delay {12} : {9}=ret : {11}",
                    e.S_fBound_left, e.S_fBound_top, e.S_fBound_right, e.S_fBound_bottom, ocrInfo.FindStr, ocrInfo.Color_info, ocrInfo.Sim, _s_retX, _s_retY, _s_functionName, ocrInfo.DictIndex, refreshPos,_s_delay));
                sb2.AppendLine(M.ValueIfDebug(string.Format("{0} ret,\"{1}\",{2},{3},{4},{5},{6},{7} : "
                    ,DebugLogFunction.Name_FoundElementStrDetailedInfo,e.FullName,
                    e.S_fBound_left, e.S_fBound_top, e.S_fBound_right, e.S_fBound_bottom,
                    ocrInfo.Color_info, ocrInfo.Sim), DebugLevel.AllDetailLevel));
                
                //sb2.Append(M.GetDebugLog("\"找元素{0}[{1},{2}] -> \"&{3} : ", DebugLevel.AllDetailLevel, e.FullName, ocrInfo.Color_info.Trim('"'), ocrInfo.Sim, e.S_findpos_info));
                //sb2.AppendLine(resultLog);
                
                //FindStr(x1,y1,x2,y2,string,color_format,sim,intX,intY)

            }
            sb.AppendLine(string.Format("Function {0}({1})", _s_functionName,_s_delay))
                .Append(M.Q_AddTab(sb2.ToString()))
                .AppendLine("End Function");
            return sb.ToString();
        }
    }
}

/* 


 //更新所有引用该元素的元素/域
 string followingStr = fe.Q_GetFollowingString(null);
 //找元素成功后执行语句：
 debugString = M.GetDebugLog("\"找到元素[{0}]\"",2, fe.FullName);
 string case_success = string.Format("{0} = {1} + {2} : {3} = {4} + {5} : {6} = True : {7}{8}", _s_x, _s_retX, fe.OffsetX, _s_y, _s_retY, fe.OffsetY,_s_available ,followingStr, debugString);
 //找元素失败后执行语句：
 debugString2 = M.GetDebugLog("\"找不到元素[{0}]\"",2, fe.FullName);
 string case_fail = string.Format("{0} = False{1}", _s_x, debugString2);
 //找完元素整体执行语句：
 string wholeStatement = string.Format("{0} = ret : If ret Then : {1} : Else : {2} : End If : ", _s_functionName, case_success, case_fail);*/