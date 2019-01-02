using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LuCompiler
{
    public class PathOpener
    {
        [DllImport("shell32.dll", ExactSpelling = true)]
        public static extern int SHOpenFolderAndSelectItems(
            IntPtr pidlFolder,
            uint cidl,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
            uint dwFlags);
        [DllImport("ole32.dll", ExactSpelling = true)]
        public static extern int CoCreateInstance(
            [In] ref Guid rclsid,
            IntPtr pUnkOuter,
            CLSCTX dwClsContext,
            [In] ref Guid riid,
            [Out] out IntPtr ppv);

        public enum CLSCTX : uint
        {
            INPROC_SERVER = 0x1
        }
        private static Guid CLSID_ShellLink = new Guid("00021401-0000-0000-C000-000000000046");
        private static Guid IID_IShellLink = new Guid("000214F9-0000-0000-C000-000000000046");



        public static void Open(string path)
        {
            IntPtr ppsl = IntPtr.Zero;
            int result = CoCreateInstance(
            ref CLSID_ShellLink,
            IntPtr.Zero,
            CLSCTX.INPROC_SERVER,
            ref IID_IShellLink,
            out ppsl);
            IShellLinkW psl = Marshal.GetObjectForIUnknown(ppsl) as IShellLinkW;
            psl.SetPath(path);

            IntPtr pidl = IntPtr.Zero;
            psl.GetIDList(out pidl);

            SHOpenFolderAndSelectItems(pidl, 0, null, 0);
            Marshal.FreeCoTaskMem(pidl);
            Marshal.Release(ppsl);
        }
    }
}
