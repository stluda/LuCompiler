using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler{

    public class AEFindFunction : AEFunction
    {
        protected string _s_x;
        protected string _s_y;
        protected string _s_available;
        protected string _s_parentArea_x;
        protected string _s_parentArea_y;
        protected string _s_parentArea_right;
        protected string _s_parentArea_bottom;
        protected string _s_parentArea_available;        
        protected string _s_picPath;
        protected string _s_retX;
        protected string _s_retY;
        protected string _s_delay;

        public string S_parentArea_x
        {
            get { return _s_parentArea_x; }
        }
        public string S_parentArea_y
        {
            get { return _s_parentArea_y; }
        }
        public string S_parentArea_available
        {
            get { return _s_parentArea_available; }
        }

        public AEFindFunction(Context context, string arg)
            : base(context, arg)
        {
            Type = ValueType.Boolean;            
        }

        protected override void _register()
        {
            base._register();
            _s_x = _AEObject.S_x;
            _s_y = _AEObject.S_y;
            _s_available = _AEObject.S_available;
            _s_parentArea_x = _parentArea.S_x;
            _s_parentArea_y = _parentArea.S_y;
            _s_parentArea_right = _parentArea.S_right;
            _s_parentArea_bottom = _parentArea.S_bottom;
            _s_retX = _AEObject.S_retX;
            _s_retY = _AEObject.S_retY;
            _s_parentArea_available = _parentArea.S_available;
            _s_delay = M.EVar("t");
            /*_s_functionName = string.Format("sys_findE_{0}", _arg);
            if (!G.IsDebugMode)
            {
                _s_functionName = M.MixcodeOfExternalVar(_s_functionName);
            }*/
        }


    }
}
