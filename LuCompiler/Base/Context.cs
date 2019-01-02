using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Context
    {
        private static Context _empty = new Context(null, -1, -1,"");
        public static Context Empty
        {
            get
            {
                return _empty;
            }
        }

        private string _value;
        private int _rowIndex;
        private int _columnIndex;
        private string _lineValue;

        public string LineValue
        {
            get { return _lineValue; }
            set { _lineValue = value; }
        }
        public string Value
        {
            get { return _value; }
        }
        public int RowIndex
        {
            get { return _rowIndex; }
        }
        public int ColumnIndex
        {
            get { return _columnIndex; }
        }


        public Context(string value, int rowIndex, int columnIndex,string lineValue)
        {
            _value = value;
            _rowIndex = rowIndex;
            _columnIndex = columnIndex;
            _lineValue = lineValue;
        }

        public override string ToString()
        {
            return _value;
        } 
    }
}
