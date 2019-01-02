using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Area : AEObject
    {
        private Hashtable<OcrStringInfo, Hashtable<string, OcrStringInfo>> _dictCategory = new Hashtable<OcrStringInfo, Hashtable<string, OcrStringInfo>>();
        private Hashtable<string, OcrStringInfo> _nameOfCategory = new Hashtable<string, OcrStringInfo>();

        public Hashtable<string, OcrStringInfo> NameOfCategory
        {
            get { return _nameOfCategory; }
            set { _nameOfCategory = value; }
        }
        public Hashtable<OcrStringInfo, Hashtable<string, OcrStringInfo>> DictCategory
        {
            get { return _dictCategory; }
            set { _dictCategory = value; }
        }
        private Hashtable<string, Element> _elementOfName = new Hashtable<string, Element>();
        private Hashtable<string, Element> _elementOfChName = new Hashtable<string, Element>();
        private HashSet<string> _duplicatedElementNames = new HashSet<string>();

        private static Area _all;



        public static Area All
        {
            get 
            {
                if (_all == null)
                {
                    _all = new Area() { Name = "all" };
                }
                return Area._all; 
            }
        }

        private int _width;
        private int _height;
        private List<AEObject> _children = new List<AEObject>();

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }
        public AEObject this[int index]
        {
            get
            {
                return _children[index];
            }
        }

        public int ChildrenCount
        {
            get { return _children.Count; }         
        }

        public void AddChild(AEObject aeObject)
        {
            _children.Add(aeObject);
            aeObject.Parent = this;
            if (aeObject is Element)
            {
                Element e = aeObject as Element;
                _elementOfName[e.Name] = e;
                if (_elementOfChName.ContainsKey(e.CHName)) _duplicatedElementNames.Add(e.CHName);
                else _elementOfChName[e.CHName] = e;
            }
        }

        public Element GetElement(string key, out string errmsg)
        {
            errmsg = null;
            if (_elementOfName.ContainsKey(key)) return _elementOfName[key];
            
            if (_duplicatedElementNames.Contains(key))
            {
                errmsg = string.Format("因为域{0}({1})下存在不止一个别名为'{2}'的元素，所以不能用别名做索引", this.Name, this.CHName, key);
                return null;
            }
            else if (!_elementOfChName.ContainsKey(key))
            {
                errmsg = string.Format("域{0}({1})中不存在键名或别名为'{2}'的元素", this.Name, this.CHName, key);
            }
            return _elementOfChName[key];
        }

        public override void Register()
        {
            base.Register();
            /*
            if (Name == "all")
            {

            }
            else
            {

            }*/



            //System.Diagnostics.Debug.WriteLine(_s_bound_info);
            foreach (AEObject o in _children) o.Register();
        }
    }
}
