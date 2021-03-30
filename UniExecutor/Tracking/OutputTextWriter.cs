using Plugins.Shared.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Models;
using UniExecutor.Enums;
using UniExecutor.Output;
using UniExecutor.Service.Interface;
using UniExecutor.Services;

namespace UniExecutor.Tracking
{
    public class OutputTextWriter : TextWriter
    {
        private IViewOperateService _viewOperateService;

        public OutputTextWriter()
        {
            _viewOperateService = ExecutorContext.Current.ViewOperateService;
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        public override void WriteLine(string value)
        {
            try
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Trace, value);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, "有一个错误产生", e.Message);
            }
        }
    }
}
