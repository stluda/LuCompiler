using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LuCompiler
{
    public class Element : AEObject
    {
        public ElementFunctionFilter FunctionFilter
        {
            get;
            set;
        }

        private OcrStringInfo _ocrStringInfo;

        public OcrStringInfo OcrStringInfo
        {
            get { return _ocrStringInfo; }
            set 
            {
                
                _ocrStringInfo = value; 
            }
        }

        public override string FullName
        {
            get
            {
                return string.Format("{0}_{1}", Parent.Name, Name);
            }
        }

        public override void Register()
        {
            base.Register();
            
            //_s_debug_find_log = M.GetDebugLog("\"找元素[{0},{1},{2}]:{3}\"", 3,FullName,"{0}","{1}",Parent.S_findpos_info);
        }

        public Element()
        {
            FunctionFilter = ElementFunctionFilter.All;
            SearchMode = SearchMode.SearchInArea;
        }
    }
}
