using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;

namespace UniExecutor.Service.Interface
{
    public interface IExecutorService
    {
        bool IsExecutorRunning { get;}

        void Start();

        void InternalStart(Dictionary<string, object> inputArguments, JobModel jobModel=null);

        void Pause();

        void Resume();

        void Stop();
    }
}
