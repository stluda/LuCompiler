using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LuCompiler
{
    public class FuncTimeout
    {
        private Action _action;

        /// <summary>   
        /// 信号量   
        /// </summary>   
        private ManualResetEvent manu = new ManualResetEvent(false);
        /// <summary>  
        /// 是否接受到信号   
        /// </summary>   
        private bool isGetSignal;
        /// <summary>   
        /// 设置超时时间   
        /// </summary>   
        private int timeout;


        public delegate bool EventNeedRun(ITimeOutPara paras);
        /// <summary>   
        /// 要调用的方法的一个委托   
        /// </summary>   
        private EventNeedRun FunctionNeedRun;
        /// <summary>   
        /// 构造函数，传入超时的时间以及运行的方法   
        /// </summary>   
        /// <param name="_action"></param>   
        /// <param name="_timeout"></param>   
        public FuncTimeout(EventNeedRun _action, int _timeout)
        {
            FunctionNeedRun = _action;
            timeout = _timeout;
        }

        public FuncTimeout(Action action, int _timeout)
        {
            _action = action;
            timeout = _timeout;
        }

        /// <summary>   
        /// 回调函数   
        /// </summary>   
        /// <param name="ar"></param>   
        public void MyAsyncCallback(IAsyncResult ar)
        {
            //isGetSignal为false,表示异步方法其实已经超出设置的时间，此时不再需要执行回调方法。   
            if (isGetSignal == false)
            {
                //Console.WriteLine("放弃执行回调函数");
                Thread.CurrentThread.Abort();
            }
            else
            {
                //Console.WriteLine("调用回调函数");
            }
        }

        public bool doAction()
        {
            Action WhatToDo = () =>
            {
                //bool pass = FunctionNeedRun(paras);
                _action();
                manu.Set();
                //return pass;
            };


            //EventNeedRun WhatTodo = CombineActionAndManuset;
            //通过BeginInvoke方法，在线程池上异步的执行方法。   
            var r = WhatToDo.BeginInvoke(MyAsyncCallback,null);
            //设置阻塞,如果上述的BeginInvoke方法在timeout之前运行完毕，则manu会收到信号。此时isGetSignal为true。   
            //如果timeout时间内，还未收到信号，即异步方法还未运行完毕，则isGetSignal为false。   
            isGetSignal = manu.WaitOne(timeout);


            if (isGetSignal == true)
            {
                //Console.WriteLine("函数运行完毕，收到设置信号,异步执行未超时");
                return true;
            }
            else
            {
                //Console.WriteLine("没有收到设置信号,异步执行超时");
                return false;
            }
        }

        /// <summary>   
        /// 调用函数   
        /// </summary>   
        /// <param name="param1"></param>   
        public bool doAction(ITimeOutPara paras)
        {
            EventNeedRun WhatTodo = CombineActionAndManuset;
            //通过BeginInvoke方法，在线程池上异步的执行方法。   
            var r = WhatTodo.BeginInvoke(paras, MyAsyncCallback, null);
            //设置阻塞,如果上述的BeginInvoke方法在timeout之前运行完毕，则manu会收到信号。此时isGetSignal为true。   
            //如果timeout时间内，还未收到信号，即异步方法还未运行完毕，则isGetSignal为false。   
            isGetSignal = manu.WaitOne(timeout);


            if (isGetSignal == true)
            {
                //Console.WriteLine("函数运行完毕，收到设置信号,异步执行未超时");
                return true;
            }
            else
            {
                //Console.WriteLine("没有收到设置信号,异步执行超时");
                return false;
            }
        }
        /// <summary>   
        /// 把要传进来的方法，和 manu.Set()的方法合并到一个方法体。   
        /// action方法运行完毕后，设置信号量，以取消阻塞。   
        /// </summary>   
        /// <param name="num"></param>   
        private bool CombineActionAndManuset(ITimeOutPara paras)
        {
            bool pass = FunctionNeedRun(paras);
            manu.Set();
            return pass;
        }
    }

    /// <summary>  
    /// 可以借助这个接口进行异步方法的参数传入和传出。  
    /// </summary>  
    public interface ITimeOutPara
    {
        /// <summary>  
        /// 参数集合，顺序不能乱  
        /// </summary>  
        List<object> Parameters { set; get; }
    }

    public class CheckFtpPatermeters : ITimeOutPara
    {
        public List<object> Parameters { set; get; }

        public CheckFtpPatermeters(string DomainName, string FtpUserName, string FtpUserPwd)
        {
            Parameters = new List<object>();
            Parameters.Add(DomainName);
            Parameters.Add(FtpUserName);
            Parameters.Add(FtpUserPwd);
        }
    }  
}
