using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LuCompiler
{
    public class ExternalStatement : Statement
    {
        private static Regex r_variable = new Regex(@"~?[a-zA-Z\u4e00-\u9fa5]\w*");
        private Function _function;

        private List<string> _content = new List<string>();
        public ExternalStatement()
        {
        }

        public void Add(string value)
        {
            _content.Add(value);
        }

        public override void Compile(Function function)
        {
            base.Compile(function);
            _function = function;
        }

        public override string QTranslate()
        {
            StringBuilder sb = new StringBuilder();
            bool isLastWord = false;
            foreach (string value in _content)
            {
                bool isWord;
                string finalValue = value;
                string lValue;
                if (isWord = r_variable.IsMatch(value))
                {
                    if (finalValue.StartsWith("~"))
                    {
                        finalValue = finalValue.TrimStart('~');
                        Variable var;
                        if (_function.LocalVariables.ContainsKey(finalValue)) var = _function.LocalVariables[finalValue];
                        else var = G.GlobalVariables[finalValue];
                        if (var != null) finalValue = var.FinalName;
                    }
                    else switch (lValue = value.ToLower())
                    {
                        case "function":
                        case "sub":
                        case "if":
                        case "else":
                        case "then":
                        case "end":
                        case "exit":
                        case "select":
                        case "case":
                            break;
                        default:
                            if (G.MixcodeOfExternalVariable.ContainsKey(lValue))
                                finalValue = G.MixcodeOfExternalVariable[lValue];
                            break;
                    }
                }
                if (isLastWord && isWord) sb.Append(' ');
                sb.Append(finalValue);
                isLastWord = isWord;
            }
            return sb.ToString();
        }

    }
}
