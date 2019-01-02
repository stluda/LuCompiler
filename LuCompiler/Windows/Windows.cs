using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler
{
    public class Windows
    {
        private static StringBuilder sb1 = new StringBuilder(128, 128);
        private static StringBuilder sb2 = new StringBuilder(128, 128);

        public static IntPtr RecursiveFind(IntPtr ptr, string findClassName, string findTitleName)
        {
            return RecursiveFind(ptr, findClassName, findTitleName, -1);
        }
        public static IntPtr RecursiveFind(IntPtr ptr, string findClassName, string findTitleName,int depth)
        {
            if (ptr != default(IntPtr))
            {
                User32.GetClassName(ptr, sb1, 128);
                User32.GetWindowText(ptr, sb2, 128);
                string className = sb1.ToString();
                string titleName = sb2.ToString();
                if (titleName.Contains(findTitleName) && className.Contains(findClassName)) return ptr;
            }

            IntPtr result = default(IntPtr);
            if (depth != 0)
            {
                IntPtr first_child_ptr = User32.GetWindow(ptr, 0x5);
                for (IntPtr p = first_child_ptr; p != default(IntPtr); p = User32.GetWindow(p, 0x2))
                {
                    result = RecursiveFind(p, findClassName, findTitleName, depth - 1);
                    if (result != default(IntPtr))
                        break;
                }
            }
            return result;           
        }

        public static IntPtr FindTopWindow(string findClassName, string findTitleName)
        {
            IntPtr result = default(IntPtr);
            User32.EnumWindows((IntPtr p, int lparam) =>
            {
                User32.GetClassName(p, sb1, 128);
                User32.GetWindowText(p, sb2, 128);
                string className = sb1.ToString();
                string titleName = sb2.ToString();
                if (titleName.Contains(findTitleName) && className.Contains(findClassName))
                {
                    result = p;
                    return false;
                }
                return true;
            },0);
            return result;
        }

    }
}
