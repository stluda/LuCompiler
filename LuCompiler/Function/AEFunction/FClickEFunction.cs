using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class FClickEFunction : AEFindFunction
    {
        private FindEFunction _findEFunction;
        private Element _element;

        public FClickEFunction(Context context, string arg)
            : base(context, arg)
        {
            Type = ValueType.Boolean;
            G.AEFunctions["fclicke_"+arg] = this;
            _findEFunction = G.AEFunctions["finde_" + arg] as FindEFunction;
            _element = G.AEObjectOfFullName[arg] as Element;
        }

        public override string QTranslate()
        {
            return string.Format("Function {0}({4}) : Dim ret : ret = {1}({4}) : {0} = ret : If ret Then : click {2},{3} : End If : End Function"
                , _s_functionName, _findEFunction.S_FunctionName, _element.S_x, _element.S_y,_s_delay);
        }

        protected override void _register()
        {
            base._register();
            if (G.IsDebugMode)_s_functionName = string.Format("sfclk_{0}", _arg);
        }
    }
}
