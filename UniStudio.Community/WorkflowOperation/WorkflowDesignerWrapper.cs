using System;
using System.Activities.Presentation;
using System.IO;
using Plugins.Shared.Library;
using UniStudio.Community.Librarys;

namespace UniStudio.Community.WorkflowOperation
{
    public class WorkflowDesignerWrapper
    {
        private DateTime _lastUpdateTime;

        public WorkflowDesigner WorkflowDesigner { get; private set; }

        public EditingContext Context => WorkflowDesigner?.Context;

        public string XmalPath { get; private set; }


        private string _relativeXmalPath;

        public string RelativeXmalPath
        {
            get
            {
                if (XmalPath == null)
                {
                    _relativeXmalPath = null;
                    return null;
                }
                if (_relativeXmalPath == null)
                {
                    _relativeXmalPath = Common.MakeRelativePath(SharedObject.Instance.ProjectPath, XmalPath);
                }
                return _relativeXmalPath;
            }
        }

        public WorkflowDesignerWrapper()
        {
            WorkflowDesigner = new WorkflowDesigner();
        }

        public void Load(string fileName)
        {
            XmalPath = fileName;
            _lastUpdateTime = File.GetLastWriteTime(XmalPath);
            WorkflowDesigner.Load(fileName);
        }

        public void Update()
        {
            var lastWriteTime= File.GetLastWriteTime(XmalPath);
            if(lastWriteTime>_lastUpdateTime)
            {
                WorkflowDesigner = new WorkflowDesigner();
                _lastUpdateTime = lastWriteTime;
                WorkflowDesigner.Load(XmalPath);
            }
        }
    }
}
