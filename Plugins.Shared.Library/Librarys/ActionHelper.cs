using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Librarys
{
    public class ActionHelper
    {
        /// <summary>
        /// 在有效时间完成action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeout">以毫秒为单位</param>
        public static void DoActionInTime(Action action,bool throwExceptionIfTimeout=false,int timeout=30000)
        {
            Exception exception = null;
            var thread = new Thread(state =>
              {
                  try
                  {
                      action();
                  }
                  catch(Exception ex)
                  {
                      exception = ex;
                  }
              });
            thread.Start();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                if(thread.ThreadState != System.Threading.ThreadState.Stopped)
                {
                    Thread.Sleep(50);
                }
                else if(exception!=null)
                {
                    throw exception;
                }
                else
                {
                    return;
                }
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    break;
                }
            }
            if(throwExceptionIfTimeout)
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// 在有效时间完成func
        /// </summary>
        /// <param name="func"></param>
        /// <param name="timeout">以毫秒为单位</param>
        public static T DoFuncInTime<T>(Func<T> func, bool throwExceptionIfTimeout = false, int timeout = 30000)
        {
            T result = default;
            Exception exception = null;
            var thread = new Thread(state =>
            {
                try
                {
                    result = func();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.Start();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                if (thread.ThreadState != System.Threading.ThreadState.Stopped)
                {
                    Thread.Sleep(50);
                }
                else if (exception != null)
                {
                    throw exception;
                }
                else
                {
                    return result;
                }
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    break;
                }
            }

            if (throwExceptionIfTimeout)
            {
                throw new TimeoutException();
            }

            return result;
        }


        /// <summary>
        /// 在有效时间完成func
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="func"></param>
        /// <param name="timeout">以毫秒为单位</param>
        public static T DoFuncInTime<T>(Func<T> func,Predicate<T> predicate, bool throwExceptionIfTimeout = false, int timeout = 30000)
        {
            T result = default;
            Exception exception = null;
            var thread = new Thread(state =>
            {
                try
                {
                    result = func();
                    while(!predicate(result))
                    {
                        Thread.Sleep(5);
                        result = func();
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.Start();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                if (thread.ThreadState != System.Threading.ThreadState.Stopped)
                {
                    Thread.Sleep(50);
                }
                else if (exception != null)
                {
                    throw exception;
                }
                else
                {
                    return result;
                }
                if(stopwatch.ElapsedMilliseconds>timeout)
                {
                    break;
                }
            }

            if (throwExceptionIfTimeout)
            {
                throw new TimeoutException();
            }

            return result;
        }
    }
}
