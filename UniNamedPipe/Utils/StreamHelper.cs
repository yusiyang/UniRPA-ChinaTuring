using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniNamedPipe.Models;

namespace UniNamedPipe.Utils
{
    public static class StreamHelper
    {
        public static void WriteObject<T>(this Stream stream,T obj)
        {
            if (stream==null)
            {
                return;
            }
            string objJson;
            if (typeof(T) == typeof(string))
            {
                objJson = (string)Convert.ChangeType(obj, typeof(string));
            }
            else
            {
                objJson = JsonConvert.SerializeObject(obj);
            }
            var bytes = Encoding.UTF8.GetBytes(objJson);
            var lengthBytes = BitConverter.GetBytes(bytes.Length);
            stream.Write(lengthBytes, 0, lengthBytes.Length);

            var totalLength = bytes.Length;
            var position = 0;
            while(totalLength>0)
            {
                var perLength = totalLength > 1000 ? 1000 : totalLength;
                stream.Write(bytes, position, perLength);
                totalLength -= perLength;
                position += perLength;
            }
            stream.Flush();
        }

        public static T ReadObject<T>(this Stream stream)
        {
            var lengthBytes = stream.ReadBuffer(4);
            if(lengthBytes.Length==0)
            {
                return default;
            }
            var dataLength = BitConverter.ToInt32(lengthBytes, 0);
            var bytes = stream.ReadBuffer(dataLength);

            var objJson = Encoding.UTF8.GetString(bytes);

            T obj;
            if (typeof(T) == typeof(string))
            {
                obj = (T)Convert.ChangeType(objJson, typeof(T));
            }
            else
            {
                obj = JsonConvert.DeserializeObject<T>(objJson);
            }

            return obj;
        }

        private static byte[] ReadBuffer(this Stream stream, int length)
        {
            byte[] bytes = new byte[length];
            if (stream == null)
            {
                return bytes;
            }
            int offset = 0;
            int remaining = length;
            while (remaining > 0)
            {
                int num = stream.Read(bytes, offset, remaining);
                if (num == 0)
                {
                    return new byte[0];
                }
                remaining -= num;
                offset += num;
            }
            return bytes;
        }
    }
}
