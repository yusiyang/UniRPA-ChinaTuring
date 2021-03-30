using CSharp_easy_RSA_PEM;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UniWorkforce.Librarys
{
    public class LisenceHelper
    {
        private string PrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
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
-----END RSA PRIVATE KEY-----
";
        private string _licenseFile = null;

        public string LicenseFile
        {
            get
            {
                if(_licenseFile==null)
                {
                    var authorizationDir = Settings.Instance.LocalRPAStudioDir + @"\Authorization";
                    if (!System.IO.Directory.Exists(authorizationDir))
                    {
                        System.IO.Directory.CreateDirectory(authorizationDir);
                    }
                    _licenseFile = authorizationDir + @"\license.authorization";
                }
                return _licenseFile;
            }
        }

        private static LisenceHelper _instance = null;

        public static LisenceHelper Instance => _instance;

        static LisenceHelper()
        {
            _instance = new LisenceHelper();
        }

        /// <summary>
        /// 生成License
        /// </summary>
        public void GenerateLicense()
        {
            JObject authJson = new JObject();
            authJson.Add("IsRegister", "True");
            authJson.Add("CpuID", MyComputerInfo.Instance().CpuID);
            authJson.Add("DiskID", MyComputerInfo.Instance().DiskID);
            authJson.Add("MacAddress", MyComputerInfo.Instance().MacAddress);
            authJson.Add("ExpirationDate", "forever");

            string authJsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(authJson);
            byte[] authJsonByte = System.Text.Encoding.UTF8.GetBytes(authJsonStr);

            string data = Convert.ToBase64String(authJsonByte);


            RSACryptoServiceProvider privateRSAkey = Crypto.DecodeRsaPrivateKey(PrivateKey);
            string signature = Convert.ToBase64String(privateRSAkey.SignData(System.Text.Encoding.UTF8.GetBytes(data), typeof(SHA256)));

            //var rsa = new Plugins.Shared.Library.RSA.RSA(PrivateKey, true);
            //string signature=rsa.Sign("SHA256", data);

            JObject authFileJson = new JObject();
            authFileJson.Add("signature", signature);
            authFileJson.Add("data", data);

            string secretJsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(authFileJson);

            //保存文件
            System.IO.File.WriteAllText(LicenseFile, secretJsonStr);
        }
    }
}
