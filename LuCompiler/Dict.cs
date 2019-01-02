using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Dict
    {
        private static readonly HashSet<AEObject> UsedAEObj = new HashSet<AEObject>();
        private static Hashtable<string, string> _fileContentOfName = new Hashtable<string, string>();
        private static Hashtable<string, Area> _areaOfChName = new Hashtable<string, Area>();
        private static HashSet<string> _duplicatedAreaNames = new HashSet<string>();
        private static HashSet<string> _errorInfos = new HashSet<string>();
        private static Hashtable<string, Macro> _macroOfKey = new Hashtable<string, Macro>();
        private static Hashtable<int, HashSet<char>> _usedOcrChars = new Hashtable<int, HashSet<char>>();
        private static List<int> _usedDictIndexs = new List<int>();
        private static HashSet<int> _ocrUseAll = new HashSet<int>();
        private static Hashtable<AEObject, int> _indexOfAEObject = new Hashtable<AEObject,int>();
        private static Hashtable<int, AEObject> _AEObjectOfIndex = new Hashtable<int, AEObject>();
        private static Hashtable<Function, int> _indexOfFunction = new Hashtable<Function, int>();
        private static Hashtable<int, Function> _FunctionOFIndex = new Hashtable<int, Function>();
        private readonly static HashSet<string> _typeOfAEFunctions = new HashSet<string>() 
        { "find", "clickE", "dbclickE", "fclickE", "fdbclickE" };
        private static Hashtable<int, StringBuilder> _embeddedScriptOutputs = new Hashtable<int, StringBuilder>();

        public static Hashtable<int, Function> FunctionOFIndex
        {
            get { return Dict._FunctionOFIndex; }
            set { Dict._FunctionOFIndex = value; }
        }
        public static Hashtable<Function,int> IndexOfFunction
        {
            get { return Dict._indexOfFunction; }
            set { Dict._indexOfFunction = value; }
        }
        public static Hashtable<AEObject, int> IndexOfAEObject
        {
            get { return _indexOfAEObject; }
        }
        public static Hashtable<int, AEObject> AEObjectOfIndex
        {
            get { return _AEObjectOfIndex; }
        }


        public static HashSet<string> TypeOfAEFunctions
        {
            get { return _typeOfAEFunctions; }
        } 

        //private static Hashtable<Type, int> _countOfAEFunctionType = ;

        public static HashSet<int> OcrUseAll
        {
            get { return Dict._ocrUseAll; }
        }

        public static List<int> UsedDictIndexs
        {
            get { return _usedDictIndexs; }
        }
        public static Hashtable<int, HashSet<char>> UsedOcrChars
        {
            get { return _usedOcrChars; }
        }
        public static Hashtable<string, Macro> MacroOfKey
        {
            get { return Dict._macroOfKey; }
        }
        public static void SetAreaOfCHNameDict(Area area)
        {
            if (_areaOfChName.ContainsKey(area.CHName)) _duplicatedAreaNames.Add(area.CHName);
            else _areaOfChName[area.CHName] = area;
        }
        public static string GetEmbeddedScriptOutputString(int index)
        {
            StringBuilder sb = _embeddedScriptOutputs[index];
            return sb==null?"":sb.ToString();            
        }
        public static void PutEmbeddedScriptOutputString(int index,string value)
        {
            if (_embeddedScriptOutputs[index] == null) _embeddedScriptOutputs[index] = new StringBuilder();
            StringBuilder sb = _embeddedScriptOutputs[index];
            sb.Append(value);
        }

        public static void RegisterOcrChar(int dictIndex, string str)
        {
            HashSet<char> list = _usedOcrChars[dictIndex];
            if (list == null) list = _usedOcrChars[dictIndex] = new HashSet<char>();
            foreach(char c in str.ToCharArray())list.Add(c);
        }

        public static Area GetArea(string key, out string errmsg)
        {
            errmsg = null;
            if (G.AEObjectOfFullName.ContainsKey(key)) return G.AEObjectOfFullName[key] as Area;

            if (_duplicatedAreaNames.Contains(key))
            {
                errmsg = string.Format("因为不止一个域的别名为'{0}'，所以不能用该别名做索引", key);
                return null;
            }
            else if (!_areaOfChName.ContainsKey(key))
            {
                errmsg = string.Format("不存在键名或别名为'{0}'的域", key);
            }
            return _areaOfChName[key];
        }

        public static AEObject GetAEObject(string key,out string errmsg)
        {
            key = key.ToLower();
            errmsg = null;
            string[] args = key.Split('_');
            switch (args.Length)
            {
                //域
                case 1:                    
                    //先看看是不是真名，否则再看看是不是别名
                    return GetArea(key, out errmsg);
                //元素
                case 2:
                    //先检查域是否存在
                    Area area = GetArea(args[0], out errmsg);
                    if (area == null) return null;
                    else return area.GetElement(args[1], out errmsg);
                default:
                    errmsg = "不合法的格式";
                    return null;
            }
        }

        public static void AddException(InterpretException ie)
        {
            AddException(ie, null);
        }
        public static void AddException(InterpretException ie,SyntaxElement syntax)
        {
            string errmsg;
            if (_errorInfos.Contains(errmsg = ie.ToString())) return;
            _errorInfos.Add(errmsg);
            G.InterpretExceptions.Add(ie);
            if (syntax != null) syntax.HasException = true;
        }

        public static string GetFileContent(string file)
        {
            if (_fileContentOfName.ContainsKey(file)) return _fileContentOfName[file];
            else
            {
                string ret = M.ReadFile(file);
                _fileContentOfName[file] = ret;
                return ret;
            }
        }
    }
}
