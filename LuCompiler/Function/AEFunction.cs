using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class AEFunction : Function
    {
        protected string _AEType;
        protected string _arg;
        protected string _s_functionName;
        protected AEObject _AEObject;
        protected Area _parentArea;
        public override string FinalName
        {
            get
            {
                return _s_functionName;
            }
        }

        public string AEType
        {
            get { return _AEType; }
            set { _AEType = value; }
        }
        public string Arg
        {
            get { return _arg; }
            set { _arg = value; }
        }
        public string S_FunctionName
        {
            get { return _s_functionName; }
        }
        public AEObject AEObject
        {
            get { return _AEObject; }
            set { _AEObject = value; }
        }

        private static AEFunction _getFunctionByType(Context context,string type, AEObject obj)
        {
            if (G.AEFunctions.ContainsKey(type + "_" + obj.FullName)) return G.AEFunctions[type + "_" + obj.FullName];
            switch (type)
            {
                case "fclicke":
                    return new FClickEFunction(context, obj.FullName);
                case "fdbclicke":
                    return new FDBClickEFunction(context, obj.FullName);
                case "finde":
                    return new FindEFunction(context, obj.FullName);
                case "finda":
                    return new FindAFunction(context, obj.FullName);
                case "find":
                    if (obj is Area) return new FindAFunction(context, obj.FullName);
                    else if (obj is Element) return new FindEFunction(context, obj.FullName);
                    else throw new NotSupportedException();
                default:
                    return null;
            }
        }

        public static AEFunction GetInstance(Context context, string AEType,AEObject obj)
        {                 
            AEType = AEType.ToLower();     
            #region Init_Check
            switch (AEType)
            {
                case "finde":
                case "clicke":
                case "fclicke":
                    if (!(obj is Element)) 
                        throw new InterpretException(context, string.Format("{0}里的内容必须是一个元素", AEType));
                    break;
                case "finda":
                    if (!(obj is Area)) throw new InterpretException(context, string.Format("{0}里的内容必须是一个域", AEType));
                    break;
            }
            switch (AEType)
            {
                case "finde":
                    Element e = obj as Element;
                    if (e.OcrStringInfo == null && !(e is FindableElement)) 
                        throw new InterpretException(context, "该元素不可找！");
                    break;
                case "fclicke":
                    e = obj as Element;
                    if (e.OcrStringInfo==null && !(e is IFindable)) 
                        throw new InterpretException(context, string.Format("这是一个无法被找到的域/元素", AEType));
                    break;
                case "finda":
                    if (!(obj is IFindable)) 
                        throw new InterpretException(context, string.Format("这是一个无法被找到的域/元素", AEType));
                    //Type = ValueType.Boolean;
                    break;
                case "clicke":
                    //Type = ValueType.None;
                    break;
            }
            #endregion
            switch (AEType)
            {
                case "fclicke":
                case "fdbclicke":
                    if (!G.AEFunctions.ContainsKey("finde_" + obj.FullName))
                        GetInstance(context, "finde", obj);
                    return _getFunctionByType(context, AEType, obj);
                case "finde":
                case "finda":                
                case "find":
                    if (obj.Parent is FindableArea && !G.AEFunctions.ContainsKey("finda_" + obj.Parent.Name))
                        GetInstance(context, "finda", obj.Parent);
                    return _getFunctionByType(context, AEType, obj);                
                default:
                    throw new InterpretException(context,"不支持这种类型");
            }
        }

        public override void Compile()
        {
            return;
        }

        public override void Compile(Function sender)
        {
            return;
        }

        protected AEFunction(Context context, string arg)
        {
            //_AEType = AEType.ToLower();
            _arg = arg;
            _AEObject = G.AEObjectOfFullName[_arg.ToLower()];
            _parentArea = _AEObject.Parent;
            Context = context;
            _register();        
        }

        protected AEFunction() { }

        public override string QTranslate()
        {
            return "";
        }

        protected virtual void _register()
        {
            if (!G.IsDebugMode) _s_functionName = Mixcode;
        }

        public static void CreateFunctions()
        {
            int i = 0;

            Hashtable<string, List<int>> table = new Hashtable<string,List<int>>();
            foreach (string type in Dict.TypeOfAEFunctions) table[type] = new List<int>();
            foreach (AEObject obj in G.AEObjectOfFullName.Values)
            {
                Dict.AEObjectOfIndex[i] = obj;
                Dict.IndexOfAEObject[obj] = i;
                
                foreach (string type in Dict.TypeOfAEFunctions)
                {
                    if (tryCreateFunction(obj, type.ToLower()))table[type].Add(i);
                }
                i++;
            }
            foreach (string type in Dict.TypeOfAEFunctions) new AEFunctionSwitcher(type , table[type]);
            
        }


        private static bool tryCreateFunction(AEObject obj, string type)
        {            
            Func<string,bool> condition = (string t) =>
            {
                if (obj is Area) return obj is IFindable;
                else if (obj is Element)
                {
                    Element e = obj as Element;
                    switch (t)
                    {
                        case "find":
                            if (!e.FunctionFilter.HasFlag(ElementFunctionFilter.FindE)) return false;
                            break;
                        case "fclicke":
                            if (!e.FunctionFilter.HasFlag(ElementFunctionFilter.FClickE)) return false;
                            break;
                        case "fdbclicke":
                            if (!e.FunctionFilter.HasFlag(ElementFunctionFilter.FDbClickE)) return false;
                            break;
                    }
                    return  e.OcrStringInfo != null || e is IFindable;
                }
                else throw new NotSupportedException();
            };

            switch (type)
            {
                case "find":
                    if (condition(type))
                    {                        
                        GetInstance(Context.Empty, type, obj);
                        return true;
                    }
                    break;
                case "fclicke":
                case "fdbclicke":
                    if (obj is Element && condition(type))
                    {
                        GetInstance(Context.Empty, type, obj);
                        return true;
                    }
                    break;
                case "clicke":
                case "dbclicke":
                    return true;
            }
            return false;
        }

    }
}
