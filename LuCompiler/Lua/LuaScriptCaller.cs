using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace LuCompiler
{
    public class LuaScriptCaller
    {
        private int _blockIndex = -1;
        private StringBuilder _sb = new StringBuilder("sys_blockIndex = -1;\r\n");
        private string _src;
        private Lua _lua = new Lua();

        public LuaScriptCaller(string src) : this()
        {
            _src = src;
        }

        public LuaScriptCaller()
        {
            _lua.RegisterFunction("put", this, new Action<string,int>(Put).Method);
            _lua.RegisterFunction("test", this, new Action<string>(Test).Method);
            _lua.RegisterFunction("console", this, new Action<string>(Console).Method);  
        }

        public void Put(string value,int index)
        {
            Dict.PutEmbeddedScriptOutputString(index, value);
        }

        public void Exit()
        {
            _lua.Close();
            _lua.Dispose();
        }

        public void Build()
        {
            string value = _sb.ToString();
            _src = string.Format("function main()\r\n{0}\r\nend", M.Q_AddTab(value));
            Debug.WriteLine(_src);

            string fileName = M.GetRelativeFileName(G.FileName);
            fileName = fileName + ".lua";
            fileName = M.GetNewPath(G.FileName,@"Result\" + fileName);
            FileInfo fi = new FileInfo(fileName);

            //FileInfo fi = new FileInfo(fileName);
            if (fi.Exists) fi.Attributes = FileAttributes.Normal;
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.Write(_src);
                sw.Close();
                fi.Attributes = FileAttributes.ReadOnly;
            }
        }

        public void Run()
        {
            _lua.DoString(_src);
            FuncTimeout funcTimeOut = new FuncTimeout(() =>
            {
                try
                {
                    _lua.GetFunction("main").Call();
                }
                catch (Exception e)
                {
                    Exit();
                    throw e;
                }
            }, 50000);
            if (!funcTimeOut.doAction()) throw new InterpretException(Context.Empty, "Lua函数执行超时");
        }



        private static Regex _r_putFunc = new Regex(@"put<(.+)>");
        public void AddBlock(string block)
        {
            _blockIndex++;
            _sb.AppendLine(string.Format("sys_blockIndex = {0};",_blockIndex));
            block = _r_putFunc.Replace(block,"put($1,sys_blockIndex)");
            _sb.AppendLine(block.TrimEnd("\r\n".ToCharArray()));            
        }

        public void AddAssignmentBlock(string block)
        {
            _blockIndex++;
            _sb.AppendLine(string.Format("sys_blockIndex = {0};", _blockIndex));
            block = string.Format("put({0},sys_blockIndex);", block.TrimEnd("\r\n".ToCharArray()));
            _sb.AppendLine(block);   
        }

        public void Console(dynamic value)
        {
            if (value == null) System.Windows.Forms.MessageBox.Show("{nil}");
            else System.Console.Write(value.ToString());  
        }

        public void Test(dynamic value)
        {


            if (value == null) System.Windows.Forms.MessageBox.Show("{nil}");  
            else System.Windows.Forms.MessageBox.Show(value.ToString());  
            //_lua.RegisterFunction("zzz", this, this.GetType().GetMethod("ttt"));
                     
        }

        public static void WriteResultLuScript()
        {
            string wholeText = (G.TokenTaker as CodeTokenTaker).WholeText;
            string fileName = M.GetRelativeFileName(G.FileName);

            string resultFileName = fileName + ".result.lu";
            resultFileName = M.GetNewPath(G.FileName, @"Result\" + resultFileName);
            FileInfo fi = new FileInfo(resultFileName);
            if (fi.Exists) fi.Attributes = FileAttributes.Normal;
            using (StreamWriter sw = new StreamWriter(resultFileName))
            {
                sw.Write(wholeText);
                sw.Close();
                fi.Attributes = FileAttributes.ReadOnly;                
            }
            G.CompileShowFileName = resultFileName;
        }
    }
}
