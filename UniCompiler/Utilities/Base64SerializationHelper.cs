using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;

namespace UniCompiler.Utilities
{
	internal static class Base64SerializationHelper
    {
        private const string Prefix = "UPTF";

        private static readonly int HeaderLength = "UPTF".Length + 8;

        public static string Serialize<T>(T data) where T : class
        {
            if (data == null)
            {
                return null;
            }
            DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    dataContractJsonSerializer.WriteObject(memoryStream, data);
                    string text = Convert.ToBase64String(memoryStream.ToArray());
                    return "UPTF" + text.Length.ToString("X8") + text;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return null;
            }
        }

        public static bool IsValidFormat(string input)
        {
            if (!string.IsNullOrWhiteSpace(input) && input.StartsWith("UPTF") && input.Length >= HeaderLength)
            {
                int num = input.Length - HeaderLength;
                if (int.TryParse(input.Substring("UPTF".Length, 8), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
                {
                    return num == result;
                }
            }
            return false;
        }

        public static T Deserialize<T>(string input) where T : class, new()
        {
            if (!IsValidFormat(input))
            {
                return null;
            }
            DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));
            try
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(input.Substring(HeaderLength, input.Length - HeaderLength))))
                {
                    return dataContractJsonSerializer.ReadObject(stream) as T;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return null;
            }
        }
    }
}
