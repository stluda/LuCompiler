using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LuCompiler
{
    public class Dictmapper
    {        
        public static void CreateDict()
        {
            string mapData;
            string dictPath = G.WorkhomePath + @"\dict";
            IEnumerable<int> indexs = Dict.UsedDictIndexs.Distinct<int>();


            foreach(int index in indexs)
            {
                HashSet<char> chars = Dict.UsedOcrChars[index];
                for(int i=0;i<2;i++)
                {                    
                    string filename = string.Format(@"{0}\{1}.{2}.txt", dictPath, index, i == 0 ? "xp" : "win7");
                    if (!File.Exists(filename)) throw new InterpretException(Context.Empty,"字典文件不存在");
                    string hash = GetMD5HashFromFile(filename);
                    string hashFileName = string.Format(@"DictHash\{0}", hash);
                    StreamReader sr;
                    sr = new StreamReader(filename, Encoding.GetEncoding("gbk"));
                    string text = sr.ReadToEnd();
                    if (Dict.OcrUseAll.Contains(index))
                    {
                        string fileName = string.Format(@"{0}\{1}{2}.txt", G.OutputPath, i == 0 ? "a" : "b", index);
                        string releaseFileName = string.Format(@"{0}\release\{1}{2}.txt", G.OutputPath, i == 0 ? "a" : "b", index);
                        using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding("gbk")))
                        {
                            writer.Write(text);
                            writer.Close();
                        }
                        if (!G.IsDebugMode) File.Copy(fileName, releaseFileName, true);
                    }
                    else
                    {
                        string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        sr.Close();
                        if (File.Exists(hashFileName))
                        {
                            sr = new StreamReader(hashFileName, Encoding.GetEncoding("gbk"));
                            mapData = sr.ReadToEnd();
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (string line in lines) sb.Append(line.Split('$')[1]);
                            mapData = sb.ToString();
                            StreamWriter sw = new StreamWriter(hashFileName, false, Encoding.GetEncoding("gbk"));
                            sw.Write(mapData);
                            sw.Close();
                        }
                        sr.Close();
                        StringBuilder builder = new StringBuilder();
                        bool exflag = false;
                        foreach (char c in chars)
                        {
                            int idx = mapData.IndexOf(c);
                            if (idx < 0)
                            {
                                exflag = true;
                                Dict.AddException(new InterpretException(Context.Empty, string.Format("字典源不存在字符'{0}'", c)));
                            }
                            else builder.AppendLine(lines[idx]);
                        }
                        if (exflag) return;
                        string fileName = string.Format(@"{0}\{1}{2}.txt", G.OutputPath, i == 0 ? "a" : "b", index);
                        string releaseFileName = string.Format(@"{0}\release\{1}{2}.txt", G.OutputPath, i == 0 ? "a" : "b", index);
                        using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding("gbk")))
                        {
                            writer.Write(builder);
                            writer.Close();
                        }
                        if(!G.IsDebugMode)File.Copy(fileName, releaseFileName,true);
                    }
                }

                
            }
        }

        private static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();                
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
