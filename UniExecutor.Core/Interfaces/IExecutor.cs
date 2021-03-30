using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Models;

namespace UniExecutor.Core.Interfaces
{
    public interface IExecutor
    {
        void Start();

        void InternalStart(JobModel jobModel, Dictionary<string, object> inputArguments);

        void Pause();

        void Resume();

        void Stop();

    }
}
