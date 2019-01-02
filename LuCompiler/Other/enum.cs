using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public enum ValueType
    {
        Unknown = 128,
        Numeric = 1,
        Boolean = 2,
        String = 4,
        Array = 8,
        Map = 16,
        FunctionReference = 32,
        Super = Numeric| Boolean | String | Array | Map | FunctionReference,
        State = 64,        
        None = 0
    }

    public enum ElementFunctionFilter
    {
        None = 0,
        FindE = 1,
        FClickE = 2,
        FDbClickE = 4,
        All = FindE|FClickE|FDbClickE
    }

    public enum ExpressionAttrs
    {
        None = 0,
        Numeric = 1,
        Integer = Numeric | 2,
        Decimal = Numeric | 4,
        Boolean = 8,
        String = 16,
        Array = 32,
        Map = 64,
        FunctionReference = 128,
        Super = Integer | Decimal | Boolean | String | Array | Map | FunctionReference,        
        ReadOnly = 256,
        Const = 512|ReadOnly,        
        Variable = 1024,     
        Function = Variable|2048,
        ObjectMember = 4096,   
        Leaf = 8192,
        External = 16384,
        NumericConst = Numeric | Const,
        IntegerConst = Integer | Const,
        DecimalConst = Decimal | Const,
        StringConst = String | Const,
        BooleanConst = Boolean | Const,
        ArrayConst = Array | Const,
        MapConst = Map | Const,
        IntegerVariable = Integer | Variable,
        DecimalVariable = Decimal | Variable,
        StringVariable = String | Variable,
        BooleanVariable = Boolean | Variable,
        ArrayVariable = Array | Variable,
        MapVariable = Map | Variable,
        Unknown = External * 2,
    }

    public enum SearchMode
    {
        FixedInArea,
        SearchInArea,
        SearchInElement,
        SearchInRange,
    }

}
