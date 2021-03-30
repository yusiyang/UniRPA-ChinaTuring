using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.XamlIntegration;
using System.IO;
using System.Xaml;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Librarys;

namespace UniStudio.Community.WorkflowOperation
{
    public class SimpleWorkflow
    {
        private DateTime _lastUpdateTime;

        public ActivityBuilder Root { get; private set; }

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

        public EditingContext Context { get; private set; }

        public SimpleWorkflow(EditingContext context=null)
        {
            Context = context;
        }

        public void Load(string filePath)
        {
            XmalPath = filePath;
            _lastUpdateTime = File.GetLastWriteTime(XmalPath);

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                using (XamlXmlReader innerReader = new XamlXmlReader(fileStream))
                {
                    Root = XamlServices.Load(ActivityXamlServices.CreateBuilderReader(innerReader)) as ActivityBuilder;
                }
            }

            if(Context==null)
            {
                Context = new EditingContext();
            }

            var modelTreeManager = new ModelTreeManager(Context);
            modelTreeManager.Load(Root);
            Context.Services.Publish(modelTreeManager);
        }

        public void Update()
        {
            var lastWriteTime = File.GetLastWriteTime(XmalPath);
            if (lastWriteTime > _lastUpdateTime)
            {
                Context.Dispose();
                Context = null;

                Load(XmalPath);
            }
        }
    }
}
