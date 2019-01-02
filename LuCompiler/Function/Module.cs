using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Module : Function
    {
        private int _index;

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public override string FinalName
        {
            get
            {
                return M.EVar(string.Format("sys_module{0}", _index));
            }
        }

        public Module(string name,Statement statement,Context context)
            : base(name,statement,context)
        {            
        }

        public override string GetDebugInfo_EnterFunction
        {
            get
            {
                string moduleInfo = string.Format("{0}{1}", Index
                 , (Name == null ? "" : string.Format("({0})", Name)));
                return M.GetTraceString(string.Format("-----Enter Module{0} ------", moduleInfo), DebugLevel.PartialDetailLevel, this);
            }
        }

        public override string GetDebugInfo_EndFunction
        {
            get
            {
                string moduleInfo = string.Format("{0}{1}", Index
                 , (Name == null ? "" : string.Format("({0})", Name)));
                return M.GetTraceString(string.Format("-----End Module{0} ------", moduleInfo), DebugLevel.PartialDetailLevel, this);
            }
        }

        public override string QTranslate()
        {
            string moduleInfo = string.Format("{0}{1}",Index
                 ,(Name==null ? "":string.Format("({0})",Name)));

            return string.Format("Sub {0}\r\n{1}{2}End Sub", FinalName,
                _s_everyIndex==null?"" : string.Format("\t{0}={0}+1 : \r\n",_s_everyIndex),
                 M.Q_AddTab(
                 GetDebugInfo_EnterFunction 
                 + Statement.QTranslate()
                 + GetDebugInfo_EndFunction)
                 );
              //  + "\r\nEnd Sub";
            //return base.QTranslate();
        }
    }
}
