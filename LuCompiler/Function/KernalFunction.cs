using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class KernalFunction : Function
    {
        private readonly HashSet<string> _excludeList = new HashSet<string>() {"writelog" };

        private string _finalName;

        public override string FinalName
        {
            get { return _finalName; }
        }

        public override string GetDebugInfo_EnterFunction
        {
            get
            {
                return M.GetTraceString(string.Format("-----Enter Kernal : {0} ------ ", Name), DebugLevel.PartialDetailLevel, this);
            }
        }

        public override string GetDebugInfo_EndFunction
        {
            get
            {
                return M.GetTraceString(string.Format("-----End Kernal : {0} ------ ", Name), DebugLevel.PartialDetailLevel, this);
            }
        }


        public override string QTranslate()
        {
            //string name = Name.TrimStart('$');
            string qInArgs = "";
            if (InArgs != null) qInArgs = "(" + InArgs.QTranslate() + ")";
            string ret = string.Format("{0} {1}{2}\r\n", Q_functionType, _finalName, qInArgs) 
                + (_s_everyIndex==null?"" : string.Format("\t{0}={0}+1 : \r\n",_s_everyIndex))
                + M.Q_AddTab( GetDebugInfo_EnterFunction
                + Statement.QTranslate()
                + GetDebugInfo_EndFunction)
                + string.Format("End {0}", Q_functionType);
            return ret;
        }

        public KernalFunction(string name, Statement statement,Context context)
            : base(name,statement,context)
        {
            Name = name.TrimStart('$');
            _finalName = Name;
            if (!_excludeList.Contains(Name.ToLower()))
            {
                _finalName = M.EVar(Name);
            }
            
        }        
    }
}
