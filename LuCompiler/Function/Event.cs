using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Event : Function
    {

        public Event(string name, Statement statement, Context context)
            : base(name, statement, context)
        {

        }

        public override string QTranslate()
        {
            return
                string.Format("Event {0}\r\n", Name)
                + M.Q_AddTab((_s_everyIndex == null ? "" : string.Format("{0}={0}+1 : \r\n", _s_everyIndex)) + Statement.QTranslate())
                + " : End Event";
        }
    }
}
