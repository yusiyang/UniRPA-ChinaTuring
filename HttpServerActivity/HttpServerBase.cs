using System;
using System.Diagnostics;
using System.Net;


namespace HttpServerActivity
{
    public class HttpServerBase
    {
        public HttpServerBase()
        {
            httpListener = new HttpListener();
        }

        public void StartServer(int port)
        {
            var url = string.Format("http://*:{0}/", port);

            AddAddress(url, Environment.UserDomainName, Environment.UserName);

            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            httpListener.Prefixes.Add(url);
            httpListener.Start();
            httpListener.BeginGetContext(new AsyncCallback(Receive), null);
        }


        private static void AddAddress(string address, string domain, string user)
        {
            string args = string.Format(@"http add urlacl url={0}", address) + " user=\"" + domain + "\\" + user + "\"";

            ProcessStartInfo psi = new ProcessStartInfo("netsh", args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = true;

            Process.Start(psi).WaitForExit();
        }


        private void Receive(IAsyncResult ar)
        {
            var context = httpListener.EndGetContext(ar);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            RevMessageEvent?.Invoke(this, request, response);
            httpListener.BeginGetContext(new AsyncCallback(Receive), null);
        }

        private HttpListener httpListener;
        public delegate void ReceiveMessage(object sender, HttpListenerRequest request, HttpListenerResponse response);
        public event ReceiveMessage RevMessageEvent;
    }
}
