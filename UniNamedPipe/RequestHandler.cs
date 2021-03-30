using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniNamedPipe.Exceptions;
using UniNamedPipe.Models;

namespace UniNamedPipe
{
    public class RequestHandler
    {
        public RequestHandler(string pipeName, Request request)
        {
            PipeName = pipeName;
            Request = request;
        }

        public Request Request { get; }

        public string PipeName { get; set; }

        public Response Handle()
        {
            try
            {
                if (!Configure.PipeNameTypesDic.TryGetValue(PipeName, out var apiInfos))
                {
                    throw new UniNamedPipeException("没有相应的管道");
                }
                var apiInfo = apiInfos.FirstOrDefault(a => a.ApiName == Request.ApiName);
                if (apiInfo == null)
                {
                    throw new UniNamedPipeException("没有找到相关的处理类");
                }
                var apiInstance = Activator.CreateInstance(apiInfo.Type);

                var methodInfo = apiInfo.Type.GetMethod(Request.MethodName);
                if(methodInfo==null)
                {
                    throw new UniNamedPipeException("没有找到相关的处理方法");
                }
                var paramInfoArray= methodInfo.GetParameters();
                object result;
                if(paramInfoArray.Length==0)
                {
                    result= methodInfo.Invoke(apiInstance, null);
                    return Response.Success(Request, result);
                }
                var paramArray = new object[paramInfoArray.Length];
                for(var i=0;i<paramInfoArray.Length;i++)
                {
                    paramArray[i] = JsonConvert.DeserializeObject(JsonConvert.DeserializeObject<string>(Request.Parameters[i]), paramInfoArray[i].ParameterType);
                }

                result = methodInfo.Invoke(apiInstance, paramArray);
                return Response.Success(Request, result);
            }
            catch(UniNamedPipeException ex)
            {
                return Response.Fail(Request, ex);
            }
            catch(Exception ex)
            {
                return Response.Fail(Request, ex.Message);
            }
        }
    }
}
