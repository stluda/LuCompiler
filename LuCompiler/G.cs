using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LuCompiler
{
    public static class G
    {
        public static readonly HashSet<string> ImportFiles = new HashSet<string>();

        private static int _debugLevel = 0;
        private static bool _doesAutoPutIntoQMacro = true;
        private static bool _isDebugMode = true;
        private static bool _doesWriteTraceprintToLog = false;
        private static bool _doesCopyToClipBoard = true;
        private static bool _doesShowOutputDir = true;
        private static bool _doesFtpUploadResult = false;
        private static List<Variable> _staticVars = new List<Variable>();
        private static Hashtable<Variable, string> _mixCodeOfVariable = new Hashtable<Variable, string>();
        private static Hashtable<string, string> _mixcodeOfExternalVariable = new Hashtable<string, string>();
        private static Hashtable<int, int> _indexOfTranslatedInterval = new Hashtable<int, int>();
        private static Hashtable<string, Variable> _global_variables = new Hashtable<string, Variable>();
        private static Hashtable<string, AEFunction> _AEFunctions = new Hashtable<string, AEFunction>();
        private static List<Function> _functions = new List<Function>();
        private static List<Module> _modules = new List<Module>();
        private static List<KernalFunction> _kernals = new List<KernalFunction>();
        private static HashSet<string> _kernalNames = new HashSet<string>();
        private static Hashtable<string, int> _map_moduleIndexOfName = new Hashtable<string, int>();
        private static string _argumentsPath;
        private static string _templatePath;
        private static string _outputPath;
        private static string _workhomePath;
        private static List<DeclarationStatement> _globalDeclarations = new List<DeclarationStatement>();
        private static ITokenTaker _tokenTaker;
        private static string _fileName;
        private static string _compileShowFileName;
        private static List<InterpretException> _interpretExceptions = new List<InterpretException>();
        private static HashSet<string> _mixcodes = new HashSet<string>();
        private static Area _root;
        private static Hashtable<string, AEObject> _map_AEObject = new Hashtable<string,AEObject>();
        private static Hashtable<string, List<string>> _references = new Hashtable<string, List<string>>();
        private static Hashtable<string, Context> _referencesContext = new Hashtable<string, Context>();
        private static LuaScriptCaller _luaScriptCaller = new LuaScriptCaller();
        private static FunctionSwitcher _functionSwitcher;
        private static FtpWeb _ftpWeb;


        public static bool DoesFtpUploadResult
        {
            get { return G._doesFtpUploadResult; }
            set { G._doesFtpUploadResult = value; }
        }
        public static FtpWeb FtpWeb
        {
            get { return G._ftpWeb; }
            set { G._ftpWeb = value; }
        }
        public static bool DmEncode { get; set; }
        public static string DmEncodePassword { get; set; }
        public static bool DoesAutoPutIntoQMacro
        {
            get { return G._doesAutoPutIntoQMacro; }
            set { G._doesAutoPutIntoQMacro = value; }
        }
        public static bool DoesWriteTraceprintToLog
        {
            get { return G._doesWriteTraceprintToLog; }
            set { G._doesWriteTraceprintToLog = value; }
        }
        public static FunctionSwitcher FunctionSwitcher
        {
            get { return G._functionSwitcher; }
            set { G._functionSwitcher = value; }
        }
        public static string CompileShowFileName
        {
            get { return G._compileShowFileName; }
            set { G._compileShowFileName = value; }
        }
        public static LuaScriptCaller LuaScriptCaller
        {
            get { return G._luaScriptCaller; }
        }
        public static bool DoesCopyToClipBoard
        {
            get { return G._doesCopyToClipBoard; }
            set { G._doesCopyToClipBoard = value; }
        }
        public static bool DoesShowOutputDir
        {
            get { return G._doesShowOutputDir; }
            set { G._doesShowOutputDir = value; }
        }
        public static List<Variable> StaticVars
        {
            get { return G._staticVars; }
            set { G._staticVars = value; }
        }
        public static Hashtable<string, Context> ReferencesContext
        {
            get { return G._referencesContext; }
            set { G._referencesContext = value; }
        }
        public static int DebugLevel
        {
            get { return G._debugLevel; }
            set { G._debugLevel = value; }
        }
        public static Hashtable<string, List<string>> References
        {
            get { return G._references; }
        }
        public static HashSet<string> KernalNames
        {
            get { return G._kernalNames; }
        }
        public static List<Function> Functions
        {
            get { return G._functions; }
            set { G._functions = value; }
        }
        public static Hashtable<string, AEFunction> AEFunctions
        {
            get { return G._AEFunctions; }
            set { G._AEFunctions = value; }
        }
        public static Hashtable<string, AEObject> AEObjectOfFullName
        {
            get { return G._map_AEObject; }
            set { G._map_AEObject = value; }
        }
        public static Area Root
        {
            get { return _root; }
            set { _root = value; }
        }
        public static HashSet<string> Mixcodes
        {
            get { return G._mixcodes; }
            set { G._mixcodes = value; }
        }
        public static Hashtable<string, string> MixcodeOfExternalVariable
        {
            get { return G._mixcodeOfExternalVariable; }
        }
        public static Hashtable<Variable, string> MixCodeOfVariable
        {
            get { return G._mixCodeOfVariable; }         
        }
        public static bool IsDebugMode
        {
            get { return G._isDebugMode; }
            set { G._isDebugMode = value; }
        }
        public static List<InterpretException> InterpretExceptions
        {
            get { return G._interpretExceptions; }
        }
        public static string FileName
        {
            get { return G._fileName; }
            set { G._fileName = value; }
        }
        public static ITokenTaker TokenTaker
        {
            get { return G._tokenTaker; }
            set { G._tokenTaker = value; }
        }
        public static string WorkhomePath
        {
            get { return G._workhomePath; }
            set { G._workhomePath = value; }
        }
        public static string OutputPath
        {
            get { return G._outputPath; }
            set { G._outputPath = value; }
        }
        public static string ArgumentsPath
        {
            get { return _argumentsPath; }
            set { _argumentsPath = value; }
        }
        public static string TemplatePath
        {
            get { return _templatePath; }
            set { _templatePath = value; }
        }

        public static List<DeclarationStatement> GlobalDeclarations
        {
            get { return G._globalDeclarations; }
        }
        public static List<KernalFunction> Kernals
        {
            get { return G._kernals; }
        }
        public static Hashtable<string, int> ModuleIndexOfName
        {
            get { return G._map_moduleIndexOfName; }
            set { G._map_moduleIndexOfName = value; }
        }
        public static List<Module> Modules
        {
            get { return G._modules; }
            set { G._modules = value; }
        }
        
        public static Hashtable<string, Variable> GlobalVariables
        {
            get { return G._global_variables; }
            set { G._global_variables = value; }
        }
        public static Hashtable<int, int> IndexOfTranslatedInterval
        {
            get { return G._indexOfTranslatedInterval; }
            set { G._indexOfTranslatedInterval = value; }
        }

        public static void init()
        {
            _kernalNames.Add("executeinstruction");
            _kernalNames.Add("g_cpu_iptr");
            _kernalNames.Add("common_check");
            _kernalNames.Add("my_init");
            _kernalNames.Add("my_init2");
            _kernalNames.Add("cpu");
            _kernalNames.Add("initializer");
            if (!Directory.Exists("DictHash")) Directory.CreateDirectory("DictHash");
            if (!Directory.Exists("Result")) Directory.CreateDirectory("Result");
        }

    }
}
