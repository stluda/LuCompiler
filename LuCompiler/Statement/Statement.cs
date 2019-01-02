using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Statement : SyntaxElement
    {
        protected Statement _parent;
        private int _index_block = 0;
        private int _index_statement = 0;
        protected bool _isCompiled;
        private int _rowIndex;
        private Context _context;

        public Context Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public virtual Statement Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public bool IsCompiled
        {
            get { return _isCompiled; }
        }
        public int RowIndex
        {
            get { return _rowIndex; }
        }
        public int BlockIndex
        {
            get
            {
                return _index_block;
            }
        }
        public int StatementIndex
        {
            get
            {
                return _index_statement;
            }
        }

        public virtual string QTranslate()
        {
            return "";
        }

        public virtual void Compile(Function function)
        {
            _isCompiled = true;
        }

        public bool FindLoopStatement(out LoopStatement result)
        {
            if (this is LoopStatement) { result = this as LoopStatement; return true; }
            else if (Parent == null)
            {
                result = null;
                return false;
            }
            else return Parent.FindLoopStatement(out result);
        }
    }
}
