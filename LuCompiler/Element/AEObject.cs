using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;

namespace LuCompiler
{
    public class AEObject : IReferable
    {
        private Rectangle _searchRange;
        private List<AEObject> _followings = new List<AEObject>();
        private string _name;
        private string _chName;
        private Area _parent;
        private Point _fPoint;
        private string _mixCode;
        protected string _s_fBound_left;
        protected string _s_fBound_top;
        protected string _s_fBound_right;
        protected string _s_fBound_bottom;
        protected string _s_x;
        protected string _s_y;
        protected string _s_available;
        protected string _s_picPath;
        protected string _s_retX;
        protected string _s_retY;
        protected string _s_OffsetXWithSymbol;
        protected string _s_OffsetYWithSymbol;
        protected string _s_debug_find_log;
        protected string _s_findpos_info;
        protected string _s_bound_info;
        protected string _s_lfound_info;
        protected string _s_lFound_left;
        protected string _s_lFound_top;
        protected string _s_lFound_right;
        protected string _s_lFound_bottom;
        protected bool _DoesUseLastFoundPos = false;


        protected float _sim = 0.8f;
        private Point _offsetPos;
        private SearchMode _searchMode = SearchMode.SearchInArea;


        protected string _s_width;
        protected string _s_height;
        protected string _s_right;
        protected string _s_bottom;

        public string S_width
        {
            get { return _s_width; }
        }
        public string S_height
        {
            get { return _s_height; }
        }
        public string S_right
        {
            get { return _s_right; }
        }
        public string S_bottom
        {
            get { return _s_bottom; }
        }

        public bool DoesUseLastFoundPos
        {
            get { return _DoesUseLastFoundPos; }
            set { _DoesUseLastFoundPos = value; }
        }
        public Rectangle Range
        {
            get;
            set;
        }
        public Rectangle KeyRange
        {
            get;
            set;
        }
        public Rectangle SearchRange
        {
            get { return _searchRange; }
            set { _searchRange = value; }
        }
        public SearchMode SearchMode
        {
            get { return _searchMode; }
            set { _searchMode = value; }
        }
        public string CHName
        {
            get { return _chName; }
            set { _chName = value; }
        }
        public string S_fBound_left
        {
            get { return _s_fBound_left; }
        }
        public string S_fBound_top
        {
            get { return _s_fBound_top; }
        }
        public string S_fBound_right
        {
            get { return _s_fBound_right; }
        }
        public string S_fBound_bottom
        {
            get { return _s_fBound_bottom; }
        }
        public string S_findpos_info
        {
            get { return _s_findpos_info; }
        }
        public string S_bound_info
        {
            get { return _s_bound_info; }
        }
        public string S_lFound_left
        {
            get { return _s_lFound_left; }
        }
        public string S_lFound_top
        {
            get { return _s_lFound_top; }
        }
        public string S_lFound_right
        {
            get { return _s_lFound_right; }
        }
        public string S_lFound_bottom
        {
            get { return _s_lFound_bottom; }
        }
        public float Sim
        {
            get { return _sim; }
            set { _sim = value; }
        }
        public Point OffsetPos
        {
            get { return _offsetPos; }
            set { _offsetPos = value; }
        }
        public int OffsetX
        {
            get { return _offsetPos.X; }
        }
        public int OffsetY
        {
            get { return _offsetPos.Y; }
        }

        public string S_x
        {
            get { return _s_x; }
        }
        public string S_y
        {
            get { return _s_y; }
        }
        public string S_available
        {
            get { return _s_available; }
        }
        public string S_picPath
        {
            get { return _s_picPath; }
        }
        public string S_retX
        {
            get { return _s_retX; }
        }
        public string S_retY
        {
            get { return _s_retY; }
        }
        public string S_OffsetXWithSymbol
        {
            get { return _s_OffsetXWithSymbol; }
        }
        public string S_OffsetYWithSymbol
        {
            get { return _s_OffsetYWithSymbol; }
        }
        public string MixCode
        {
            get { return _mixCode; }
            set { _mixCode = value; }
        }

        public Point FPoint
        {
            get { return _fPoint; }
            set { _fPoint = value; }
        }
        public Area Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value.ToLower(); }
        }

        public List<AEObject> Followings
        {
            get { return _followings; }
        }

        public void Refers(AEObject aeObject)
        {
            aeObject._followings.Add(this);
        }
        public virtual string FullName
        {
            get
            {
                return Name;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual void Register()
        {
            _s_x = string.Format("g_{0}_x", FullName);
            _s_y = string.Format("g_{0}_y", FullName);
            _s_retX = string.Format("g_retX", FullName);
            _s_retY = string.Format("g_retY", FullName);            
            _s_available = string.Format("g_{0}_available", FullName);

            _s_lFound_left = string.Format("g_{0}_lFoundX1", FullName);
            _s_lFound_top = string.Format("g_{0}_lFoundY1", FullName);
            _s_lFound_right = string.Format("g_{0}_lFoundX2", FullName);
            _s_lFound_bottom = string.Format("g_{0}_lFoundY2", FullName);
            if (!G.IsDebugMode)
            {
                _s_lFound_left = M.MixcodeOfExternalVar(_s_lFound_left);
                _s_lFound_top = M.MixcodeOfExternalVar(_s_lFound_top);
                _s_lFound_right = M.MixcodeOfExternalVar(_s_lFound_right);
                _s_lFound_bottom = M.MixcodeOfExternalVar(_s_lFound_bottom);
            }

            if (OffsetX == 0) _s_OffsetXWithSymbol = "";
            else
            {
                _s_OffsetXWithSymbol = OffsetX.ToString();
                if (_s_OffsetXWithSymbol[0] != '-') _s_OffsetXWithSymbol = "+" + _s_OffsetXWithSymbol;
            }
            if (OffsetY == 0) _s_OffsetYWithSymbol = "";
            else
            {
                _s_OffsetYWithSymbol = OffsetY.ToString();
                if (_s_OffsetYWithSymbol[0] != '-') _s_OffsetYWithSymbol = "+" + _s_OffsetYWithSymbol;
            }
            if (!G.IsDebugMode)
            {
                _s_x = M.MixcodeOfExternalVar(_s_x);
                _s_y = M.MixcodeOfExternalVar(_s_y);
                _s_retX = M.MixcodeOfExternalVar(_s_retX);
                _s_retY = M.MixcodeOfExternalVar(_s_retY);
                _s_available = M.MixcodeOfExternalVar(_s_available);
            }


            if (this == G.Root)//全域
            {
                _s_width = "g_all_width";
                _s_height = "g_all_height";
                if (!G.IsDebugMode)
                {
                    _s_width = M.MixcodeOfExternalVar(_s_width);
                    _s_height = M.MixcodeOfExternalVar(_s_height);
                }
                _s_right = _s_width;
                _s_bottom = _s_height;
                _s_x = "0";
                _s_y = "0";
            }
            else//子域及元素
            {
                _s_width = Range.Width.ToString();
                _s_height = Range.Height.ToString();
                _s_right = M.CombineIntOrStr(_s_x, _s_width);
                _s_bottom = M.CombineIntOrStr(_s_y, _s_height);

                int x1, x2, y1, y2;
                switch (SearchMode)
                {
                    case SearchMode.SearchInArea:
                        _s_fBound_left = Parent.S_x;
                        _s_fBound_top = Parent.S_y;
                        _s_fBound_right = Parent.S_right;
                        _s_fBound_bottom = Parent.S_bottom;
                        _s_findpos_info = Parent.S_bound_info;
                        _s_lfound_info = M.QFormat(M.QCommaFormat1(_s_lFound_left, _s_lFound_top, _s_lFound_right, _s_lFound_bottom));
                        break;
                    case SearchMode.SearchInElement:
                    case SearchMode.FixedInArea:
                    case SearchMode.SearchInRange:
                        switch (SearchMode)
                        {
                            case SearchMode.SearchInElement:
                                x1 = Range.Left;
                                y1 = Range.Top;
                                x2 = Range.Right + 1;
                                y2 = Range.Bottom + 1;
                                break;
                            case SearchMode.FixedInArea:
                                x1 = KeyRange.Left;
                                y1 = KeyRange.Top;
                                x2 = KeyRange.Right + 1;
                                y2 = KeyRange.Bottom + 1;
                                break;
                            case SearchMode.SearchInRange:
                                x1 = SearchRange.Left;
                                y1 = SearchRange.Top;
                                x2 = SearchRange.Right;
                                y2 = SearchRange.Bottom;
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        _s_fBound_left = M.CombineIntOrStr(Parent.S_x, x1);
                        _s_fBound_top = M.CombineIntOrStr(Parent.S_y, y1);
                        _s_fBound_right = M.CombineIntOrStr(Parent.S_x, x2);
                        _s_fBound_bottom = M.CombineIntOrStr(Parent.S_y, y2);
                        _s_findpos_info = M.QFormat(
        M.QCommaFormat1(_s_fBound_left, _s_fBound_top, _s_fBound_right, _s_fBound_bottom),
        M.Quote("="),
        M.Group(Parent.S_x), M.Quote("+"), M.Group(x1), M.Quote(","),
        M.Group(Parent.S_y), M.Quote("+"), M.Group(y1), M.Quote(","),
        M.Group(Parent.S_x), M.Quote("+"), M.Group(x2), M.Quote(","),
        M.Group(Parent.S_y), M.Quote("+"), M.Group(y2));

                        _s_lfound_info = M.QFormat(
                        M.QCommaFormat1(_s_lFound_left, _s_lFound_top, _s_lFound_right, _s_lFound_bottom),
                        M.Quote("="),
                        M.Group(_s_lFound_left), M.Quote("+"), M.Group(x1), M.Quote(","),
                        M.Group(_s_lFound_top), M.Quote("+"), M.Group(y1), M.Quote(","),
                        M.Group(_s_lFound_left), M.Quote("+"), M.Group(x2), M.Quote(","),
                        M.Group(_s_lFound_top), M.Quote("+"), M.Group(y2));
                        break;
                }


                _s_bound_info = M.QFormat(
M.QCommaFormat1(_s_x, _s_y, _s_right, _s_bottom),
M.Quote("="),
M.QCommaFormat1(_s_x, _s_y),
M.Quote(","), M.Group(_s_x), M.Quote("+"), M.Group(_s_width),
M.Quote(","), M.Group(_s_y), M.Quote("+"), M.Group(_s_height));

            }

        }

        public virtual string Q_UpdateRelatedInfoByRetPos()
        {
            return string.Format("{0}=true : ", _s_available);
        }

        public virtual string Q_GetFollowingString(AEObject leader)
        {
            if (_followings.Count == 0) return "";
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb = new StringBuilder();

            Action<string> mySbAppend = (string value) =>
            {
                if (sb2.Length < 2048)
                {
                    sb2.Append(value);
                }
                else
                {
                    sb.AppendLine(sb2.ToString());
                    sb2.Clear();
                    sb2.Append(value);
                }
            };
            Action mySbAppendEnd = () => { if (sb2.Length > 0)sb.AppendLine(sb2.ToString()); };

            int dif;
            string inArg1_left, inArg2_left, inArg1_top, inArg2_top;
            foreach (AEObject o in _followings)
            {
                if (leader == o) continue;
                IFindable fd_this = this as IFindable;
                IFindable fd_follower = o as IFindable;
                inArg2_left = fd_this == null||this==o.Parent ? _s_x:fd_this.S_lFound_left;
                inArg1_left = fd_follower != null ? fd_follower.S_lFound_left : o._s_x;
                inArg2_top = fd_this == null || this == o.Parent ? _s_y : fd_this.S_lFound_top;
                inArg1_top = fd_follower != null ? fd_follower.S_lFound_top : o._s_y;
                
                dif = this==o.Parent ? o.FPoint.X : o.FPoint.X - FPoint.X;
                mySbAppend(_followStr(inArg1_left, inArg2_left, dif));
                dif = this == o.Parent ? o.FPoint.Y : o.FPoint.Y - FPoint.Y;
                mySbAppend(_followStr(inArg1_top, inArg2_top, dif));
                string test = o.Q_UpdateRelatedInfoByRetPos();
                mySbAppend(o.Q_UpdateRelatedInfoByRetPos());
                mySbAppend(o.Q_GetFollowingString(this));
            }
            mySbAppendEnd();
            return sb.ToString();
        }
        private string _followStr(string type, int dif)
        {
            return _followStr(type, type, dif);
        }
        private string _followStr(string type1,string type2,int dif)
        {
            string difStrWithSymbol;
            if (dif == 0) difStrWithSymbol="";
            else
            {
                difStrWithSymbol = dif.ToString();
                if (difStrWithSymbol[0] != '-') difStrWithSymbol = "+" + difStrWithSymbol;
            }
            return string.Format("{0}={1}{2} : ", type1,type2, difStrWithSymbol);

        }
    }
}
