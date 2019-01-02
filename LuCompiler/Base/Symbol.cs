using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Symbol : IComparable
    {
        private static Hashtable<string, Symbol> _table_symbol = new Hashtable<string,Symbol>();
        private Type _context;    
        private string _value;
        private string _finalValue;

        public string FinalValue
        {
            get { return _finalValue; }
            set { _finalValue = value; }
        }
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        private Symbol(string value, Type context)
        {
            Value = value;
            _context = context;
            _finalValue = value;
        }
        public static Symbol GetInstance(string value, Type context)
        {
            Symbol symbol = _table_symbol[context.Name + value];
            if (symbol != null) return symbol;
            symbol = new Symbol(value, context);
            _table_symbol[context.Name + value] = symbol;
            return symbol;
        }

        //public 

        public override string ToString()
        {
            return _value;
        }

        public static string GetReserve(string symbol)
        {
            switch (symbol)
            {
                case "(":
                    return ")";
                case "[":
                    return "]";
                case "<":
                    return ">";
                default:
                    throw new Exception("......");
            }
        }
        public string ToReserve()
        {
            return GetReserve(_value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Symbol) return object.ReferenceEquals(this, obj);
            else if (obj is string) return (obj as string) == _value;
            else return false;
        }

        public int CompareTo(object obj)
        {
            if (obj is Symbol)
            {
                return SymbolPriority.Compare(this, obj as Symbol);
            }
            else if (obj is string)
            {
                return SymbolPriority.Compare(this.FinalValue, obj as string);
            }
            else throw new Exception(".....");
        }
    }
}
