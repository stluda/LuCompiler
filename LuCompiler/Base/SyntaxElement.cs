using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class SyntaxElement
    {
        private bool _hasException;

        public bool HasException
        {
            get { return _hasException; }
            set { _hasException = value; }
        }

    }
}
