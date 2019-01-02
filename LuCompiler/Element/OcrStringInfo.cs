using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class OcrStringInfo
    {
        private ElementFunctionFilter _filter = ElementFunctionFilter.All;
        private int _dictIndex=-1;
        private string _findStr;
        private string _color_info=null;
        private float _sim = 0.95f;
        private AEObject _owner;
        public int DictIndex
        {
            get { return _dictIndex; }
            set 
            {
                Dict.UsedDictIndexs.Add(value);
                _dictIndex = value; 
            }
        }
        public float Sim
        {
            get { return _sim; }
            set { _sim = value; }
        }
        public string FindStr
        {
            get { return _findStr; }
            set 
            {
                if (_dictIndex == -1) throw new Exception(".....");
                Dict.RegisterOcrChar(_dictIndex, value.Trim('"'));
                _findStr = value; 
            }
        }
        public string Color_info
        {
            get { return _color_info; }
            set { _color_info = value; }
        }
        public AEObject Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }
        public ElementFunctionFilter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
    }
}
