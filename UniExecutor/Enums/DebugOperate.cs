using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor.Enums
{
    public enum DebugOperate
    {
        Null,//无操作
        StepInto,//步入
        StepOver,//步过
        Continue,//继续
        Break,//中断
        Stop,//停止
    }
}
