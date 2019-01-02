using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LuCompiler
{
    public class FindableElement : Element,IFindable
    {        
        private string _offsetColor;
        private Rectangle _findRange;
        private Rectangle _findInElementRange;//在元素作用域中找


        public override string Q_UpdateRelatedInfoByRetPos()
        {
            return string.Format("{0}=true : {1}={2}{3} : {4}={5}{6} : ", _s_available, _s_x, _s_lFound_left, _s_OffsetXWithSymbol,
                _s_y, _s_lFound_top, _s_OffsetYWithSymbol);
        }
        public string OffsetColor
        {
            get { return _offsetColor; }
            set { _offsetColor = value; }
        }

        public Area SearchContext
        {
            get
            {
                return Parent;
            }
            set
            {
                Parent = value;
            }
        }

        public override void Register()
        {
            base.Register();
            int x1, x2, y1, y2;
            if (SearchMode != SearchMode.SearchInArea)
            {
            }


        }
    }
}
