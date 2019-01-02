using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Array : Variable
    {
        private ValueType _itemValueType;

        public ValueType ItemValueType
        {
            get { return _itemValueType; }
            set { _itemValueType = value; }
        }

        public Array(string name, Context context, string type) : this(name, context, M.ToValueType(type)) { }

        public Array(string name, Context context,ValueType itemValueType)
            : base(ValueType.Array, name, context)
        {
            _itemValueType = itemValueType;
        }

    }
}
