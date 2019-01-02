using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class InLoopStatement : Statement
    {
        private Block _block;
        public Block Block
        {
            get { return _block; }
            set { _block = value; }
        }
        private LoopStatement _loopStatement;
        private string _loopType;

        public LoopStatement LoopStatement
        {
            get { return _loopStatement; }
        }
        public string LoopType
        {
            get { return _loopType; }
        }

        public override void Compile(Function function)
        {
            if(!FindLoopStatement(out _loopStatement))
                throw new InterpretException(Context,"break语句必须内嵌于循环语句中");
            _loopType = _loopStatement.Keyword;
            switch (LoopType)
            {
                case "do":
                case "while":
                case "limit":
                case "dowhile":
                case "dolimit":
                    break;                
                default:
                    throw new InterpretException(Context,"尚不支持的类型！");
            }
        }
    }
}
