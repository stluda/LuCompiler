using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public interface ITokenTaker
    {
        string Token { get; }
        Context CurrentContext { get; }
        int RowIndex { get; }
        string GetToken();
        void Save();
        void Restore();
        string WatchNextToken();
        bool MoveForward();
        bool MoveBackword();
    }
}
