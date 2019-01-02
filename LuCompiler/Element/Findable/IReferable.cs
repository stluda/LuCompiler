using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public interface IReferable
    {
        void Refers(AEObject aeObject);
    }
}
