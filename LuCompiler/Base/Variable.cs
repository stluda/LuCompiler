using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Variable : SyntaxElement
    {
        private bool _isTypeChangable = false;
        private bool _isInitialized = false;
        protected ValueType _type = ValueType.Unknown;
        private string _name;
        private SyntaxElement _content;
        private bool _isExternal = false;
        private Context _context;
        protected string _mixcode;

        public string Mixcode
        {
            get { return _mixcode; }
        }

        public Context Context
        {
            get { return _context; }
            set { _context = value; }
        }


        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        [Obsolete("IsExternal已过时，用ExpressionType.HasFlag(ExpressionType.External)来代替", true)]
        public bool IsExternal
        {
            get { return _isExternal; }
            set { _isExternal = value; }
        }

        public ValueType Type
        {
            get { return _type; }
            set 
            {
                if (_type == ValueType.Super || value == _type || _isInitialized && value == ValueType.Super) return;
                if (_isInitialized&&!_isTypeChangable) 
                    throw new InterpretException(Context,"已经指定类型的变量，不可以再更改其类型");
                _isInitialized = true;
                _type = value; 
            }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public virtual string FinalName
        {
            get { return G.IsDebugMode ? "lu_" + Name : Mixcode; }
        }
        public SyntaxElement Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public bool IsTypeChangable
        {
            get { return _isTypeChangable; }
            set { _isTypeChangable = value; }
        }

        protected Variable()
        {
            if (!G.IsDebugMode)
            {
                _mixcode = M.MixcodeOfVar(this);
            }
        }

        public Variable(string type, string name, Context context)
        {
            switch (type)
            {
                case "var":
                    _isTypeChangable = true;
                    break;
                case "num":
                    Type = ValueType.Numeric;
                    break;
                case "bool":
                    Type = ValueType.Boolean;
                    break;
                case "string":
                    Type = ValueType.String;
                    break;
                case "state":
                    Type = ValueType.State;
                    break;
                case "super":
                    Type = ValueType.Super;
                    break;
                case "funcref":
                    Type = ValueType.FunctionReference;
                    break;
                case "array":
                    Type = ValueType.Array;
                    break;
                case "unknown":
                case "function":
                    break;
                default: 
                    throw new InterpretException(Context,"找不到这个类型");
            }            
            _name = name;
            _context = context;
            if (!G.IsDebugMode)
            {
                _mixcode = M.MixcodeOfVar(this);
            }
        }

        public Variable(ValueType type, string name,Context context)
        {
            Type = type;
            _name = name;
            _context = context;
            if (!G.IsDebugMode)
            {
                _mixcode = M.MixcodeOfVar(this);
            }
            //M.MixcodeOfVar
        }
    }
}
