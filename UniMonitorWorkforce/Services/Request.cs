using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniWorkforce.Services
{
	public class Request
	{
		public long Timeout
		{
			get;
		}

		public string Id
		{
			get;
		}

		public string ApiName
		{
			get;
		}

		public string MethodName
		{
			get;
		}

		public string[] Parameters
		{
			get;
		}

		//public Request()
		//{ }

		public Request(string apiName, string methodName, object[] parameters,string id=null ,long timeout=30000)
		{
			Id = id??Guid.NewGuid().ToString();
			ApiName = apiName;
			MethodName = methodName;
			if(parameters==null||parameters.Length==0)
			{
				Parameters = null;
			}
			else
			{
				Parameters = new string[parameters.Length];
				for(var i=0;i<parameters.Length;i++)
				{
					Parameters[i] = JsonConvert.SerializeObject(parameters[i]);
				}
			}
			Timeout = timeout;
		}

		public override string ToString()
		{
			return ApiName + " " + MethodName + " " + Id + ".";
		}
	}
}
