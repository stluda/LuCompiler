using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class OcrInAreaFunction : AEFindFunction
    {
        private string _groupName;
        private Area _area;
        private OcrStringInfo _ocrInfo;

        public static OcrInAreaFunction GetInstance(Context context, Area area, string groupName)
        {
            if (!area.NameOfCategory.ContainsKey(groupName)) throw new InterpretException(context
                    , string.Format("域{0}不存在组{1}", area, groupName));
            if (area is FindableArea && !G.AEFunctions.ContainsKey("finda_" + area.Name))
                AEFunction.GetInstance(context, "finda", area);

            AEFunction func = G.AEFunctions[string.Format("ocra_{0}_{1}", area.Name, groupName)];
            if (func != null) return func as OcrInAreaFunction;
            else return new OcrInAreaFunction(context, area, groupName);
        }

        public OcrInAreaFunction(Context context, Area area, string groupName)
            : base(context, area.Name)
        {
            G.AEFunctions[string.Format("ocra_{0}_{1}", area.Name, groupName)] = this;
            _area = area;
            _arg = _area.Name;
            _groupName = groupName;  
            _ocrInfo = _area.NameOfCategory[_groupName];
            _register2();
        }
        
        private void _register2()
        {
            if (G.IsDebugMode)_s_functionName = string.Format("sys_ocrA_{0}_{1}",_arg ,_groupName);
            _s_available = _area.S_available;
        }

        public override string QTranslate()
        {
            Area area = _area;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            string debugString;
            #region 确保所属域可用，否则直接退出
            if (!(area is FindableArea))
            {
                if (area != G.Root)
                {
                    debugString = M.GetDebugLog("\"在域[{0}]中OCR失败，因为域不可用且不可找。\"", DebugLevel.AllDetailLevel, area);
                    sb2.AppendLine(string.Format("If NOT({0}) Then : {1} = False : {2} Exit Function : End If :", _s_available, _s_functionName, debugString));
                }
            }
            else
            {
                FindAFunction fa = G.AEFunctions["finda_" + area.Name] as FindAFunction;
                //失败日志
                debugString = M.GetDebugLog("\"在域[{0}]中OCR失败，因为其所属域不可用且找不到所属域。\"", DebugLevel.AllDetailLevel, area);
                //假如找域失败
                string sFindArea = string.Format("If NOT({0}) Then : {1} = False: {2}Exit Function : End If", fa.S_FunctionName, _s_functionName, debugString);
                //假如域不可用，先找域
                sb2.AppendLine(string.Format("If NOT({0}) Then : {1} : End If :", _s_available, sFindArea, _s_functionName));
            }
            #endregion
            sb2.AppendLine(string.Format("dm.UseDict {7} : {6} = dm.Ocr({0},{1},{2},{3},{4},{5}) : Delay 50 : ",
                    area.S_x, area.S_y, area.S_right, area.S_bottom, _ocrInfo.Color_info, _ocrInfo.Sim, _s_functionName ,_ocrInfo.DictIndex));
            sb2.Append(M.GetDebugLog("\"OCR域[{0}],组[{1}] : [{2},{3}] -> \"&{4}", DebugLevel.AllDetailLevel, area.FullName, _groupName, _ocrInfo.Color_info.Trim('"'), _ocrInfo.Sim, area.S_bound_info));
            sb.AppendLine(string.Format("Function {0}", _s_functionName))
                .Append(M.Q_AddTab(sb2.ToString()))
                .Append(M.GetDebugLog("\"OCR域[{0}],组[{1}],结果:\"&{2}", DebugLevel.AllDetailLevel, area.FullName, _groupName, _s_functionName))
                .AppendLine("End Function");
            return sb.ToString();
                
            //string Ocr(x1,y1,x2,y2,color_format,sim)
        }

    }
}
