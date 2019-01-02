using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LuCompiler
{
    public interface IFindable
    {
        float Sim
        {
            get;
            set;
        }
        string OffsetColor
        {
            get;
            set;
        }
        Area SearchContext
        {
            set;
            get;
        }
        string S_fBound_left
        {
            get;
        }
        string S_fBound_top
        {
            get;
        }
        string S_fBound_right
        {
            get;
        }
        string S_fBound_bottom
        {
            get;
        }
        string S_lFound_left
        {
            get;
        }
        string S_lFound_top
        {
            get;
        }
    }
}
