using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniNamedPipe.Exceptions;

namespace UniNamedPipe.Models
{
	public class Response
	{
		public string RequestId
		{
			get;
		}

		public string Data
		{
			get;
		}

		public int Code
		{
			get;
		}

		public bool IsSuccess => Code == 0;

		public string Message
		{
			get;
		}

		public Response(string requestId, string data, int code=0,string message=null)
		{
			RequestId = requestId;
			Data = data;
			Code = code;
			Message = message;
		}

		public T GetDataResult<T>()
		{
			if(Data==null)
			{
				return default;
			}

			if(typeof(T)==typeof(string))
			{
				return (T)Convert.ChangeType(Data, typeof(T));
			}
			return JsonConvert.DeserializeObject<T>(Data);
		}

		public static Response Fail(Request request, string message,int code=1)
		{
			return new Response(request.Id, null, code, message);
		}

		public static Response Fail(Request request, UniNamedPipeException ex)
		{
			return new Response(request.Id, null, ex.Code, ex.Message);
		}

		public static Response Success<T>(Request request, T data)
		{
			string dataStr = null;

			if(typeof(T)==typeof(string))
			{
				dataStr = (string)Convert.ChangeType(data, typeof(string));
			}
			else
			{
				dataStr = JsonConvert.SerializeObject(data);
			}
			 
			return new Response(request.Id, dataStr);
		}
	}
}
