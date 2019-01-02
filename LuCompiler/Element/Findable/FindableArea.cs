using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LuCompiler
{
    public class FindableArea : Area,IFindable
    {
        private int _keyWidth;
        private int _keyHeight;
        private string _offsetColor;        
        private string _s_lFoundPosInfo;

        public string S_lFoundPosInfo
        {
            get { return _s_lFoundPosInfo; }
        }


        public string OffsetColor
        {
            get { return _offsetColor; }
            set { _offsetColor = value; }
        }
        public int KeyWidth
        {
            get { return _keyWidth; }
            set { _keyWidth = value; }
        }
        public int KeyHeight
        {
            get { return _keyHeight; }
            set { _keyHeight = value; }
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

        public Rectangle FindRange
        {
            get { throw new Exception("..."); }
            set { throw new Exception("..."); }
        }

        public override void Register()
        {
            base.Register();
            _s_lFoundPosInfo = M.QCommaFormat1(_s_lFound_left, _s_lFound_top, _s_lFound_right, _s_lFound_bottom);
        }

        public override string Q_UpdateRelatedInfoByRetPos()
        {
            return string.Format("{0}=true : {1}={2}+{3} : {4}={5}+{6} : {7}={8}{9} : {10}={11}{12} : ", _s_available,
                _s_lFound_right,_s_lFound_left,KeyWidth+1,_s_lFound_bottom,_s_lFound_top,KeyHeight+1,
                _s_x,_s_lFound_left,_s_OffsetXWithSymbol,_s_y,_s_lFound_top,_s_OffsetYWithSymbol );
        }
    }
}
