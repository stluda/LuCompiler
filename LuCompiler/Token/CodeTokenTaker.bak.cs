using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace LuCompiler
{
    public class CodeTokenTaker : ITokenTaker
    {
        private string _token;
        private int _rowIndex = -1;
        private string _wholeText;
        private string _line;
        private string[] _lines;
        private int _count_lines;
        private int _valid_start, _valid_end;
        private int _last_rowIndex = -1;
        private Match _m;
        private Context _currentContext;        
        private CaptureCollection _captures;
        private int _count_captures;
        private Regex r_checkComm = new Regex("(\"[^\"]*\"|/\\*|//|<%)");
        private bool _commMode = false;
        private bool _embeddedScriptMode = false;
        private StringBuilder _currentEmbeddedScriptBlockBuilder=new StringBuilder();
        private Hashtable<int, CaptureCollection> _table_rowCaptures = new Hashtable<int, CaptureCollection>();
        private Hashtable<int, int> _table_nextValidRowIndex = new Hashtable<int, int>();
        private Hashtable<int, int> _table_prevValidRowIndex = new Hashtable<int, int>();
        private CaptureCollection _bak_captures;
        private int _index = 0;
        private int _bak_index;
        private int _bak_rowIndex;
        private int _bak_count_captures;
        private string _bak_line;
        private string _bak_token;

        public Context CurrentContext
        {
            get { return _currentContext; }
        }
        public int RowIndex
        {
            get { return _rowIndex; }
        }
        public string Token
        {
            get { return _token; }
        }

        public CaptureCollection Captures
        {
            get { return _captures; }
            set { _captures = value; }
        }

        public CodeTokenTaker(string wholeText)
        {
            _wholeText = wholeText;
            _lines = wholeText.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            _count_lines = _lines.Length;
            //_init();
            //_work();
        }

        public bool Work()
        {
            if (_table_nextValidRowIndex.ContainsKey(_rowIndex))
            {
                _rowIndex = _table_nextValidRowIndex[_rowIndex];
                _line = _lines[_rowIndex];
                _analyseLine();
                _last_rowIndex = _rowIndex;
                return true;
            }
            if (++_rowIndex >= _count_lines) return false;
            _line = _lines[_rowIndex];
            if (checkComm()) return Work();
            _table_nextValidRowIndex[_last_rowIndex] = _rowIndex;
            _table_prevValidRowIndex[_rowIndex] = _last_rowIndex;
            _analyseLine();
            _last_rowIndex = _rowIndex;
            return true;
        }

        private string _buildEmbeddedScriptLine(string line)
        {
            int temp;
            _valid_start = 0;
            _valid_end = line.Length - 1;
            if (_commMode)
            {
                temp = line.IndexOf("*/");
                if (temp < 0) return line;                
                _valid_start = temp + 2;
                _commMode = false;
            }
            else if (_embeddedScriptMode)
            {
                temp = line.IndexOf("%>");
                _valid_start = temp + 2;
                if (temp < 0)
                {
                    _currentEmbeddedScriptBlockBuilder.AppendLine(line);
                    return null;
                }
                else
                {
                    _currentEmbeddedScriptBlockBuilder.AppendLine(line.Substring(0, temp));
                    G.LuaScriptCaller.AddBlock(_currentEmbeddedScriptBlockBuilder.ToString());
                    _currentEmbeddedScriptBlockBuilder.Clear();
                    _embeddedScriptMode = false;
                }
            }
            //r_checkQuote.Match(line, _valid_start)
            MatchCollection mc = r_checkComm.Matches(line, _valid_start);
            foreach (Match m in mc)
            {
                switch (m.Value)
                {
                    case "//":
                        if (_embeddedScriptMode) return null;
                        _valid_end = m.Index - 1;
                        return line;
                    case "/*":
                        if (_embeddedScriptMode) return null;
                        _valid_end = m.Index - 1;
                        _commMode = true;
                        return line;
                    case "<%":
                        _valid_end = m.Index - 1;
                        _embeddedScriptMode = true;
                        int tmp;//!!!有缺陷
                        if ((tmp = line.IndexOf("%>", m.Index)) != -1)
                        {
                            _currentEmbeddedScriptBlockBuilder.Append(line.Substring(m.Index + 2, tmp - (m.Index + 2)));
                            G.LuaScriptCaller.AddBlock(_currentEmbeddedScriptBlockBuilder.ToString());
                            _currentEmbeddedScriptBlockBuilder.Clear();
                            _embeddedScriptMode = false;
                        }
                        else if (m.Index + 2 <= line.Length - 1) _currentEmbeddedScriptBlockBuilder.Append(line.Substring(m.Index + 2, line.Length - (m.Index + 2)));
                        return null;
                    default:
                        continue;
                }
            }
            return line;
        }

        public void BuildEmbeddedScriptBlocks()
        {
            _commMode = false;
            List<string> list = new List<string>();
            List<int> rowlist = new List<int>();
            string lastResult = null;
            {
                int i = -1;
                foreach (string line in _lines)
                {
                    string ret = _buildEmbeddedScriptLine(line);
                    if (lastResult != null && ret == null) rowlist.Add(i + 1);
                    if (ret != null)
                    {
                        list.Add(ret);
                        i++;
                    }
                    lastResult = ret;
                }
            }
            G.LuaScriptCaller.Build();
            G.LuaScriptCaller.Run();
            for (int i = rowlist.Count - 1; i >= 0; i--)
            {
                int rowIndex = rowlist[i];
                list.Insert(rowIndex, Dict.GetEmbeddedScriptOutputString(i));
            }
            _wholeText = string.Join("\r\n", list);
            _lines = list.ToArray();
            _count_lines = _lines.Length;
            System.Diagnostics.Debug.WriteLine(_wholeText);
            _commMode = false;
        }

        private bool checkComm()
        {// if true then continue
            int temp;
            _valid_start = 0;
            _valid_end = _line.Length - 1;
            if (_commMode)
            {
                temp = _line.IndexOf("*/");
                if (temp < 0) return true;
                _valid_start = temp + 2;
                _commMode = false;
            }

            //r_checkQuote.Match(_line, _valid_start)
            MatchCollection mc = r_checkComm.Matches(_line, _valid_start);
            foreach (Match m in mc)
            {
                switch (m.Value)
                {
                    case "//":
                        _valid_end = m.Index - 1;
                        return _valid_start > _valid_end;
                    case "/*":
                        _valid_end = m.Index - 1;
                        _commMode = true;
                        return _valid_start > _valid_end;
                    default:
                        continue;
                }
            }
            return _valid_start > _valid_end;
        }

        public bool _analyseLine()
        {
            _index = 0;
            if (_table_rowCaptures.ContainsKey(_rowIndex))
            {
                _captures = _table_rowCaptures[_rowIndex];
                _count_captures = _captures.Count;
                return true;
            }
            _m = ElementRegex.Match(_line.Substring(_valid_start, _valid_end - _valid_start + 1));
            //_m = ElementRegex.Match(_line);
            if (_m.Success)
            {
                _captures = _m.Groups[2].Captures;
                _count_captures = _captures.Count;
            }
            else throw new Exception("...........");
            _table_rowCaptures[_rowIndex] = _captures;
            return _m.Success;
        }

        public string GetToken()
        {
            Capture currentCapture=null;
            if (_index >= _count_captures)
            {
                if (!Work())
                {
                    _currentContext = Context.Empty;
                    return _token = null;
                }
                else return _token = GetToken();                
            }
            else _token = (currentCapture=_captures[_index++]).Value;
            int tabCount = _line.Substring(0,currentCapture.Index+1).Count(new Func<char, bool>(delegate(char a)
            {
                return a == '\t';
            }));
            _currentContext = new Context(_token, _rowIndex, currentCapture.Index + 3 * tabCount);
            return _token;
        }

        /*public string MoveBackAndGetToken()
        {
            MoveBackword();
            return _token = _captures[_index].Value;
        }*/

        public string WatchNextToken()
        {
            if (_index >= _count_captures)
            {
                Save();
                string result = GetToken();
                Restore();
                return result;
            }
            return _captures[_index].Value;
        }

        public void Save()
        {
            _bak_index = _index;
            _bak_captures = _captures;
            _bak_count_captures = _count_captures;
            _bak_rowIndex = _rowIndex;
            _bak_line = _line;
            _bak_token = _token;
        }

        public void Restore()
        {
            _index = _bak_index;
            _captures = _bak_captures;
            _count_captures = _bak_count_captures;
            _rowIndex = _bak_rowIndex;
            _line = _bak_line;
            _token = _bak_token;
        }

        public bool MoveForward()
        {            
            if (_index >= _count_captures)
            {
                if (_rowIndex >= _count_lines-1) return false;
                Work();
            }
            else _index++;
            return true;
        }

        public bool MoveBackword()
        {
            if (_index == 0)
            {
                if (_rowIndex == 0) return false;
                _rowIndex = _table_prevValidRowIndex[_rowIndex];
                _analyseLine();
                _index = _count_captures - 1;
            }
            else _index--;
            return true;
        }


    }
}
