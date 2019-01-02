using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class InterpretException : Exception,IComparable
    {
        private Context _context;

        public Context Context
        {
            get { return _context; }
        }

        public InterpretException(Context context, string message) : base(message) 
        {
            _context = context;
        }

        public InterpretException(Context context, string message,params object[] obj)
            : this(context,string.Format(message,obj))
        {}

        public override string ToString()
        {
            if (_context == null||_context == Context.Empty) return Message;
            return string.Format("{0}:({1},{2}):{3}", G.CompileShowFileName, _context.RowIndex + 1, _context.ColumnIndex + 1, Message);
        }

        public int CompareTo(object obj)
        {
            InterpretException ie2 = obj as InterpretException;
            if (Context.RowIndex < ie2.Context.RowIndex) return -1;
            else if (Context.RowIndex == ie2.Context.RowIndex)
            {
                if (Context.ColumnIndex < ie2.Context.ColumnIndex) return -1;
                else if (Context.RowIndex == ie2.Context.RowIndex) return 0;
                else return 1;
            }
            else return 1;
        }
    }
}
