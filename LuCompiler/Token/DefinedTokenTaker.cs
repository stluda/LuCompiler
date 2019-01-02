using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class DefinedTokenTaker : ITokenTaker
    {
        private string _currentToken = null;
        private Context _currentContext = null;
        private string[] _tokens;
        private int _index = -1;
        private int _contextRowIndex;
        private int _contextColumnIndex;

        private int _bak_index;

        public string Token
        {
            get { return _currentToken; }
        }

        public Context CurrentContext
        {
            get { return _currentContext; }
        }

        public int RowIndex
        {
            get { return _contextRowIndex; }
        }

        public string GetToken()
        {
            if (_index == _tokens.Length - 1)
            {
                _currentToken = null;
                _currentContext = Context.Empty;
                return null;
            }
            else
            {
                _currentToken = _tokens[++_index];
                _currentContext = new Context(_currentToken, _contextRowIndex, _contextColumnIndex,"");
                return _currentToken;
            }
        }

        public void Save()
        {
            _bak_index = _index;
        }

        public void Restore()
        {
            _index = _bak_index - 1;
            GetToken();
        }

        public string WatchNextToken()
        {
            if (_index == _tokens.Length - 1)
            {
                return null;
            }
            else
            {
                return _currentToken = _tokens[_index+1];
            }
        }

        public bool MoveForward()
        {
            if (_index == _tokens.Length - 1) return false;
            _index++;
            return true;
        }

        public bool MoveBackword()
        {
            if (_index == -1) return false;
            _index--;
            return true;
        }

        public DefinedTokenTaker(string[] tokens,Context context)
        {
            _tokens = tokens;
            _contextRowIndex = context.RowIndex;
            _contextColumnIndex = context.ColumnIndex;
        }
    }
}
