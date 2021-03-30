using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniWorkforce.Executor.Enums
{
    public enum StoppedType
    {
        Normal,//正常结束
        Force,//主动停止
        Exception,//因为异常而停止
    }
}
