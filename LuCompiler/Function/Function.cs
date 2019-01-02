using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuCompiler{

    public class Function : Variable
    {        
        private bool _isLimited;
        private Expression _inArgs;
        private string _compileState = "uncompiled";
        private int loopCount = 0;
        private string _q_functionType;
        private Hashtable<string, Variable> _local_variables = new Hashtable<string, Variable>();
        private int _argCount = 0;
        private bool _hasEveryStatement = false;
        public string _s_everyIndex = null;
        private List<Action<Function>> _callBacks = new List<Action<Function>>();

        public List<Action<Function>> CallBacks
        {
            get { return _callBacks; }
        }
        public int ArgCount
        {
            get { return _argCount; }
        }

        public string Q_functionType
        {
            get { return _q_functionType; }
        }

        public bool IsLimited
        {
            get { return _isLimited; }
            set { _isLimited = value; }
        }
        public int LoopCount
        {
            get { return loopCount; }
            set { loopCount = value; }
        }

        public string CompileState
        {
            get { return _compileState; }
            set { _compileState = value; }
        }
        

        public virtual Hashtable<string, Variable> LocalVariables
        {
            get { return _local_variables; }
        }

        public Expression InArgs
        {
            get { return _inArgs; }
            set 
            {
                _inArgs = value; 
            }
        }

        public Statement Statement
        {
            get { return base.Content as Statement; }
            set { base.Content = value; }
        }

        protected Function()
        {            
        }

        public Function(string name,Statement statement,Context context)
            : base("function", name,context)
        {
            Statement = statement;
            _type = ValueType.None;
        }

        public virtual string GetDebugInfo_EnterFunction
        {
            get
            {
                return M.GetTraceString(string.Format("-----Enter Function : {0} ------ ", Name), DebugLevel.PartialDetailLevel, this);
            }
        }

        public virtual string GetDebugInfo_EndFunction
        {
            get
            {
                return M.GetTraceString(string.Format("-----End Function : {0} ------ ", Name), DebugLevel.PartialDetailLevel, this);
            }
        }

        public virtual string QTranslate()
        {
            string qInArgs="";
            if (_inArgs != null) 
                qInArgs = "(" + _inArgs.QTranslate() + ")";
            //new TraceStatement(new PostfixExpression(new Data("trace"), "("));
            return
                string.Format("{0} {1}\r\n", _q_functionType, string.Format("{0} {1}", this.FinalName, qInArgs)) +           
                M.Q_AddTab((_s_everyIndex==null?"" : string.Format("{0}={0}+1 : \r\n",_s_everyIndex)) +
                M.GetTraceString(string.Format("-----Enter Function : {0} ------ ", Name),DebugLevel.PartialDetailLevel,this)+
                //((G.IsDebugMode && G.DebugLevel >= DebugLevel.PartialDetailLevel) ? string.Format("Traceprint \" -----Enter Function : {0} ------ \" : ", Name) : "") +
                Statement.QTranslate() + 
                "\r\n"+M.GetTraceString(string.Format("-----End Function : {0} ------ ", Name),DebugLevel.PartialDetailLevel,this))
                +string.Format("\r\nEnd {0}",_q_functionType);
        }

        public virtual void Compile()
        {
            Compile(null);
        }
        public virtual void Compile(Function sender)
        {
            if (_compileState == "compiled" || _compileState == "compiling") return;
            _compileState = "compiling";
            if (InArgs != null)
            {
                if (InArgs is CommaExpression)
                {
                    foreach (Expression e in (InArgs as CommaExpression).Expressions)
                    {
                        Data data = e as Data;
                        if (data == null)
                        {
                            throw new InterpretException(Context, "传入参数格式非法");
                        }
                        Variable var = new Variable("var", data.Value,Context);
                        var.Type = ValueType.Super;
                        LocalVariables[data.Value] = var;
                    }
                    _argCount = (InArgs as CommaExpression).Expressions.Count;
                }
                else if (InArgs is Data)
                {
                    Data data = InArgs as Data;
                    Variable var = new Variable("var", data.Value, Context);
                    var.Type = ValueType.Super;
                    LocalVariables[data.Value] = var;
                    _argCount = 1;
                }
                else throw new InterpretException(Context,"传入参数格式非法");
                InArgs.Compile(this);
                /*InArgs.RecursiveDo(delegate(Expression e)
                {
                    e.ValueType = ValueType.Super;
                },1);*/
            }            
            Statement.Compile(this);
            _compileState = "compiled";
            _q_functionType = ValueType.None == Type ? "Sub" : "Function";
            foreach (Action<Function> act in _callBacks)
            {
                act(this);
            }
        }
    }
}
