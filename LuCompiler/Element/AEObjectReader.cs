using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;

namespace LuCompiler
{
    public class AEObjectReader
    {        
        public static XmlReader _xr;

        public static Area Read()
        {            
            _xr = XmlReader.Create(G.ArgumentsPath, new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            });
            Area area = _read(null) as Area;
            G.Root = area;
            area.Register();            
            return area;
        }

        public static AEObject _read(Area parent)
        {
            bool flag;
            while ((flag=_xr.Read()) &&_xr.NodeType!=XmlNodeType.EndElement&&_xr.NodeType != XmlNodeType.Element) ;
            if (!flag||_xr.NodeType != XmlNodeType.Element) return null;
            AEObject aeObject = null;
            bool addChildren = false;
            Area area = null;
            bool isFindable = bool.Parse(_xr.GetAttribute("IsFindable"));
            string name = _xr.GetAttribute("Name");
            switch (_xr.Name)
            {
                case "Area":
                    if (name == "all")
                    {
                        area = Area.All;
                        isFindable = false;
                    }
                    else
                    {
                        if (isFindable)
                        {
                            FindableArea fa = new FindableArea();
                            fa.KeyWidth = int.Parse(_xr.GetAttribute("KeyWidth"));
                            fa.KeyHeight = int.Parse(_xr.GetAttribute("KeyHeight"));
                            area = fa;
                        }
                        else area = new Area();
                        area.Width = int.Parse(_xr.GetAttribute("Width"));
                        area.Height = int.Parse(_xr.GetAttribute("Height"));
                    }
                    addChildren = !_xr.IsEmptyElement;
                    aeObject = area;                    
                    break;
                case "Element":
                    Element element;
                    if (isFindable)
                    {
                        FindableElement fe = new FindableElement();
                        //fe.FindRange = M.ToRectangle(_xr.GetAttribute("FindInRange"));
                        //fe.FindInElementRange = M.ToRectangle(_xr.GetAttribute("FindInElementRange"));
                        element = fe;
                    }
                    else element = new Element();                    
                    aeObject = element;
                    break;
            }        
    
            aeObject.Name = name.ToLower();
            aeObject.FPoint = M.ToPoint(_xr.GetAttribute("FPoint"));
            aeObject.OffsetPos = M.ToPoint(_xr.GetAttribute("OffsetPos"));
            aeObject.SearchMode = (SearchMode)Enum.Parse(typeof(SearchMode),_xr.GetAttribute("SearchMode"));

            aeObject.Range = M.ToRectangle(_xr.GetAttribute("Range"));
            aeObject.KeyRange = M.ToRectangle(_xr.GetAttribute("KeyRange"));

            string temp = _xr.GetAttribute("SearchRange");
            if (temp != null) aeObject.SearchRange = M.ToRectangle(temp);
            aeObject.CHName = _xr.GetAttribute("CHName");
            temp = _xr.GetAttribute("DoesUseLastFoundPos");
            if (temp != null) aeObject.DoesUseLastFoundPos = bool.Parse(temp);
            
            G.Mixcodes.Add(aeObject.MixCode = _xr.GetAttribute("Mixcode"));
            bool isRefersParent = bool.Parse(_xr.GetAttribute("IsRefersParent"));
            if (isFindable)
            {
                IFindable fd = aeObject as IFindable;
                fd.Sim = float.Parse(_xr.GetAttribute("Sim"));
                fd.OffsetColor = _xr.GetAttribute("OffsetColor").TrimStart('#');                
            }
            if (addChildren)
            {
                AEObject child;
                while ((child = _read(area)) != null) ;
            }
            if (parent != null)
            {
                parent.AddChild(aeObject);
                if (isRefersParent)
                {
                    aeObject.Refers(parent);
                }
            }
            G.AEObjectOfFullName[aeObject.FullName]= aeObject;
            if (aeObject is Area)
            {
                Dict.SetAreaOfCHNameDict(area);
            }
            return aeObject;
        }
    }
}
