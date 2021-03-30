using System;
using System.Diagnostics;

namespace UniBrowserHost
{
    class Program
    {
        private static MessageHandler Handler;
        static void Main(string[] args)
        {
            //Debugger.Launch();
            //Debugger.Break();
            //KillOther();
            Handler = new MessageHandler();
            Handler?.Init();
            Handler?.WaitForExit();
        }
    }
}
