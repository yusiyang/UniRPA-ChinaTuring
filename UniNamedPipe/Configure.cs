using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UniNamedPipe.Attributes;
using UniNamedPipe.Models;

namespace UniNamedPipe
{
    public class Configure
    {
        public static Dictionary<string, List<ApiInfo>> PipeNameTypesDic { get; }

        static Configure()
        {
            PipeNameTypesDic = new Dictionary<string, List<ApiInfo>>();
        }

        public static void ConfigureServer()
        {
            var assembly = Assembly.GetEntryAssembly();
            var types = assembly.GetTypes();

            foreach(var type in types)
            {
                var attribute = type.GetCustomAttribute<PipeServerAttribute>();
                if(attribute!=null)
                {
                    if(!PipeNameTypesDic.ContainsKey(attribute.PipeName))
                    {
                        PipeNameTypesDic[attribute.PipeName] = new List<ApiInfo>();
                    }
                    var serverType = PipeNameTypesDic[attribute.PipeName].FirstOrDefault(s => s.ApiName == attribute.Name);
                    if(serverType==null)
                    {
                        PipeNameTypesDic[attribute.PipeName].Add(new ApiInfo
                        {
                            PipeServerName = attribute.PipeName,
                            ApiName = attribute.Name,
                            Type = type
                        });
                    }
                }
            }

            foreach(var key in PipeNameTypesDic.Keys)
            {
                NamedPipeServerManager.Create(key);
            }
        }

        public static void ConfigureServer(string serverName)
        {
            var assembly = Assembly.GetEntryAssembly();
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<PipeServerAttribute>();
                if (attribute != null)
                {
                    if (!PipeNameTypesDic.ContainsKey(attribute.PipeName))
                    {
                        PipeNameTypesDic[attribute.PipeName] = new List<ApiInfo>();
                    }
                    var serverType = PipeNameTypesDic[attribute.PipeName].FirstOrDefault(s => s.ApiName == attribute.Name);
                    if (serverType == null)
                    {
                        PipeNameTypesDic[attribute.PipeName].Add(new ApiInfo
                        {
                            PipeServerName = attribute.PipeName,
                            ApiName = attribute.Name,
                            Type = type
                        });
                    }
                }
            }

            foreach (var key in PipeNameTypesDic.Keys)
            {
                if (serverName == key)
                {
                    NamedPipeServerManager.Create(key);
                }
            }
        }

        public static void ConfigureClient()
        {

        }
    }
}
