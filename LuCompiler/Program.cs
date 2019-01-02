using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;
using LuaInterface;
using System.Threading;
using dm;

namespace LuCompiler
{
    class Program
    {

        static bool writeExceptionLog()
        {
            if (G.InterpretExceptions.Count > 0)
            {
                foreach (InterpretException ie in G.InterpretExceptions)
                {
                    Debug.WriteLine(ie.ToString());
                    Console.WriteLine(ie.ToString());
                }
                return true;
            }
            return false;
        }

        private static Regex r_checkComm = new Regex("(\"[^\"]*\"|/\\*|//)");

        [STAThreadAttribute]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }
            string fileName;
            G.CompileShowFileName = G.FileName = fileName = args[0];// = "test.lu";

            G.init();            

            SymbolPriority.init();
            if (!File.Exists(fileName))
            {
                Console.WriteLine("文件不存在！");
                return;
            }
            //string fileName = args[0];
            string str = M.ReadFile(fileName);
            try
            {
                str = new FileLinker(str).Link();
            }
            catch (InterpretException ie)
            {
                Debug.WriteLine(ie.ToString());
                Console.WriteLine(ie.ToString());
                return;
            }

            CodeTokenTaker tokenTaker = new CodeTokenTaker(str);
            G.TokenTaker = tokenTaker;
            try
            {
                tokenTaker.BuildEmbeddedScriptBlocks();
            }
            catch (LuaScriptException le)
            {
                //string msg = 

                string errmsg = string.Format("在编译时发生lua脚本错误：\r\n{0}", le.Message.Replace("[string \"chunk\"]", @"result\"+G.FileName + ".lua"));
                Debug.WriteLine(errmsg);
                Console.WriteLine(errmsg);

                return;
            }
            catch (InterpretException ie)
            {
                writeExceptionLog();
                Debug.WriteLine(ie.ToString());
                Console.WriteLine(ie.ToString());
                return;
            }
            tokenTaker.Work();

            ProgramInterpreter pi = new ProgramInterpreter(tokenTaker);
            try
            {
                pi.Interpret();                
            }
            catch(InterpretException ie)
            {
                writeExceptionLog();
                Debug.WriteLine(ie.ToString());
                Console.WriteLine(ie.ToString());
                return;
            }
            writeExceptionLog();

            string modules, functions, globalVariables,initializers;
            try
            {
                pi.Compile();
            }
            catch (InterpretException ie)
            {
                writeExceptionLog();
                Debug.WriteLine(ie.ToString());
                Console.WriteLine(ie.ToString());
                return;
            }

            G.InterpretExceptions.Sort();
            if (writeExceptionLog()) return;
            Tags.CreateCTags();

            pi.QTranslate(out modules, out functions, out globalVariables, out initializers);
            
            string arguments,template,result;
            arguments = M.ReadFile(G.ArgumentsPath);
            template = M.ReadFile(G.TemplatePath);
            Debug.Write(template);
            result = template.Replace("$globalVariables$", M.Q_AddTab(globalVariables));
            result = result.Replace("$modules$", modules);
            result = result.Replace("$arguments$", initializers);
            result = result.Replace("$functions$", functions);
            result = result.Replace("$workhome$", "\"" + G.OutputPath + (G.IsDebugMode?"":"\\release") + "\"");
            result = result.Replace("$cpu$", M.EVar("cpu"));
            result = result.Replace("$my_init$", M.EVar("my_init"));
            result = result.Replace("$initializer$", M.EVar("initializer"));
            result = result.Replace("$staticVariables$", M.ParseStaticVars());
            if (G.IsDebugMode)
            {
                result = new Regex(@"\$releaseStart\$(.(?!\$releaseStart\$))+\$releaseEnd\$", RegexOptions.Singleline)
                    .Replace(result, "");
                result = result.Replace("$debugStart$", "").Replace("$debugEnd$", "");
            }
            else
            {
                result = new Regex(@"\$debugStart\$(.(?!\$debugStart\$))+\$debugEnd\$", RegexOptions.Singleline)
                    .Replace(result, "");
                result = result.Replace("$releaseStart$", "").Replace("$releaseEnd$", "");
            }
            if (!G.IsDebugMode)
            {
                Regex r = new Regex(@"\t+");
                result = r.Replace(result, " ");
                //result = result.Replace("\r\n", ":");
            }            
            StreamWriter sw = new StreamWriter(G.OutputPath + "\\output.vbs", false);
            sw.Write(result, Encoding.GetEncoding("gbk"));
            sw.Close();
            if(G.DoesShowOutputDir)PathOpener.Open(G.OutputPath + "\\output.vbs");
            if(G.DoesCopyToClipBoard)Clipboard.SetText(result);
            
            Console.WriteLine("编译成功");
            if (G.DoesFtpUploadResult) G.FtpWeb.Upload(G.OutputPath + "\\output.vbs");

            if (!G.IsDebugMode && G.DmEncode && G.DmEncodePassword!=null)
            {
                string password = G.DmEncodePassword;
                dmsoft dm = new dm.dmsoftClass();
                if (dm.Reg("---", "0001") == 1)
                {
                    Console.WriteLine("大漠插件注册成功，准备进行文件加密");
                    M.ForEachFileDo(string.Format(@"{0}\release", G.OutputPath)
                        , "txt", (string file) =>
                        {
                            dm.EncodeFile(file, password);
                        });
                    M.ForEachFileDo(string.Format(@"{0}\release", G.OutputPath)
           , "bmp", (string file) =>
           {
               dm.EncodeFile(file, password);
           });       
                }
                else
                {
                    Console.WriteLine("大漠插件注册失败，无法进行文件加密");
                }

            }

            string pluginHome = string.Format(@"{0}\plugins", G.WorkhomePath);
            if (Directory.Exists(pluginHome))
            {
                string[] files = Directory.GetFiles(pluginHome);
                foreach (string file in files)
                {
                    int index = file.LastIndexOf('\\') + 1;
                    string relativeFileName = file.Substring(index, file.Length - index);
                    string copyPath = string.Format(@"{0}\release\{1}", G.OutputPath, relativeFileName);
                    if (G.IsDebugMode)
                    {
                        try
                        {
                            copyPath = string.Format(@"{0}\{1}", G.OutputPath, relativeFileName);
                            File.Copy(file, copyPath, true);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    else
                    {                        
                        File.Copy(file, copyPath, true);
                    }
                    
                }
            }
            if (G.DoesAutoPutIntoQMacro)
            {
                IntPtr ptr_root = Windows.FindTopWindow("Afx:400000", "按键精灵");
                if (ptr_root == default(IntPtr)) return;
                IntPtr ptr=default(IntPtr),ptr_toolbar=default(IntPtr);
                /*
                bool flag = true;
                if (File.Exists("ptr"))
                {
                    using (sr = new StreamReader("ptr"))
                    {
                        args = sr.ReadLine().Split('|');
                        if (ptr_root.ToString() == args[0])
                        {
                            ptr = new IntPtr(int.Parse(args[1]));
                            ptr_toolbar = new IntPtr(int.Parse(args[2]));
                            flag = false;
                        }
                        sr.Close();
                    }
                }*/
                ptr_toolbar = Windows.RecursiveFind(ptr_root, "BCGPToolBar:400000:8:10003:10", "", 1);
                ptr = Windows.RecursiveFind(ptr_root, "MDIClient", "", 1);
                ptr = Windows.RecursiveFind(ptr, "", "按键精灵");
                //ptr = Windows.RecursiveFind(ptr, "AfxFrameOrView42", "",1);
                ptr = Windows.RecursiveFind(ptr, "BCGPEditCtrl", "", 2);
                User32.SendMessage(ptr, 0x00C, 0, result);
                if (G.IsDebugMode)
                {
                    User32.PostMessage(ptr_toolbar, 0x201, 0, User32.MakeDWORD(284, 27));
                    User32.PostMessage(ptr_toolbar, 0x202, 0, User32.MakeDWORD(284, 27));
                    User32.PostMessage(ptr_toolbar, 0x100, (int)Keys.F5, 0);
                    User32.PostMessage(ptr_toolbar, 0x101, (int)Keys.F5, 0);               
                }
                User32.SetForegroundWindow(ptr_root);
                /*
                using (sw = new StreamWriter("ptr",false))
                {
                    sw.Write(string.Format("{0}|{1}|{2}", ptr_root, ptr, ptr_toolbar));
                    sw.Close();
                }*/
            }
            
        }

        static bool checkComm(string line,ref bool commMode, ref int valid_start, ref int valid_end)
        {// if true then continue
            int temp;
            valid_start = 0;
            valid_end = line.Length - 1;
            if (commMode)
            {
                temp = line.IndexOf("*/");
                if (temp < 0) return true;
                valid_start = temp + 2;
                commMode = false;
            }

            //r_checkQuote.Match(line, valid_start)
            MatchCollection mc = r_checkComm.Matches(line, valid_start);
            foreach (Match m in mc)
            {
                switch (m.Value)
                {
                    case "//":
                        valid_end = m.Index - 1;
                        return valid_start > valid_end;
                    case "/*":
                        valid_end = m.Index - 1;
                        commMode = true;
                        return valid_start > valid_end;
                    default:
                        continue;
                }
            }
            return valid_start > valid_end;
        }

    }
}

//Regex r = new Regex(@"[ \t]*({|}|;|[a-zA-Z0-9])[ \t]*");
//Regex r_sentence = new Regex(@"[ \t]*()[ \t]*");
//string[] result = ElementRegex.GetValues("if(c||f(c*=x)&&b)g(\"sx\",cc){ int a=1; c=2; }");
//new ExpressionInterpreter();
//new ExpressionInterpreter(ElementRegex.GetValues("a||(x+y)&&f(a+b)"));


//return;

//Regex r_pattern1 = new Regex(@"^[ ]*\w+[ ]*(([+\-\*/]|&&|\|\|)[ ]*(\w+)[ ]*)*$");
//Regex r_pattern2 = new Regex(@"^[ ]*(F?)[ ]*\w+[ ]*\(([^\)]+)\)({?)[ ]*$");
//Regex r_pattern3 = new Regex(@"^[ ]*([MF]?)[ ]*(\w+)?[ ]*{$");
//Regex rrrr = new Regex(@"\-");
//Match m = rrrr.Match("-");

//MatchCollection mc = r_pattern1.Matches("a - a + b1b && c_c - xx / d * e || f");
//mc = r_pattern2.Matches("sdf(casd,qweqe)");
//mc = r_pattern3.Matches("M{");
//Match mm = ElementRegex.Match("findE (a,b) +findC(sa,sdfsdfc* d)&&x*- 3");