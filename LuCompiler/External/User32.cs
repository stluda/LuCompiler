using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LuCompiler
{
    public delegate bool CallBack(IntPtr hwnd, int lParam);
    public delegate bool EnumChildWindow(IntPtr WindowHandle, string num);
    
    public class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string IpClassName,string IpWindowName);
        /*
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(
            string lpClassName,
            string lpWindowName
        );*/

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "GetWindow")]//获取窗体句柄，hwnd为源窗口句柄
        /*wCmd指定结果窗口与源窗口的关系，它们建立在下述常数基础上：
              GW_CHILD
              寻找源窗口的第一个子窗口
              GW_HWNDFIRST
              为一个源子窗口寻找第一个兄弟（同级）窗口，或寻找第一个顶级窗口
              GW_HWNDLAST
              为一个源子窗口寻找最后一个兄弟（同级）窗口，或寻找最后一个顶级窗口
              GW_HWNDNEXT
              为源窗口寻找下一个兄弟窗口
              GW_HWNDPREV
              为源窗口寻找前一个兄弟窗口
              GW_OWNER
              寻找窗口的所有者
         */
        public static extern IntPtr GetWindow(
            IntPtr hwnd,
            int wCmd
        );

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hwnd);

        [DllImport("user32")]
        public static extern int EnumWindows(CallBack x, int y);

        [DllImport("user32")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder sb, int nMaxCount);

        [DllImport("user32")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder sb, int nMaxCount);

        [DllImport("User32")]
        public static extern int EnumChildWindows(IntPtr WinHandle, EnumChildWindow ecw, string name);


        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "SetParent")]//设置父窗体
        public static extern int SetParent(
            int hWndChild,
            int hWndNewParent
        );

        [DllImport("user32.dll", EntryPoint = "GetCursorPos")]//获取鼠标坐标
        public static extern int GetCursorPos(
            ref POINTAPI lpPoint
        );

        [StructLayout(LayoutKind.Sequential)]//定义与API相兼容结构体，实际上是一种内存转换
        public struct POINTAPI
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll", EntryPoint = "WindowFromPoint")]//指定坐标处窗体句柄
        public static extern IntPtr WindowFromPoint(
            int xPoint,
            int yPoint
        );

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(
        IntPtr hWnd,
        int Msg,
        int wParam,
        string lParam
        );

        [DllImport("User32.dll", EntryPoint = "SendMessageCallback")]
        public static extern int SendMessageCallback(
        IntPtr hWnd,
        int Msg,
        int wParam,
        string lParam,
        int callback,
        int dwdata
        );

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(
        IntPtr hWnd,
        int Msg,
        int wParam,
        int lParam
        );

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
        IntPtr hWnd,
        int Msg,
        int wParam,
        string lParam
        );

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
        IntPtr hWnd,
        int Msg,
        int wParam,
        int lParam
        );

        public static int MakeDWORD(int low, int high)
        {
            return (high << 16) + low;
        }

        public static int LOWORD(int value)
        {
            return (int)(value & 0xFFFF);
        }
        public static int HIWORD(int value)
        {
            return (int)(value >> 16);
        }
        public static int LOWBYTE(int value)
        {
            return (int)(value & 0xFF);
        }
        public static int HIGHBYTE(int value)
        {
            return (int)(value >> 8);
        }  
    }
}
