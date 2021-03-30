using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlActivity.Designer;

namespace ControlActivity
{
    [Designer(typeof(ForEachRowDesigner))]
    public class ForEachRow:ForEach<DataRow>
    {
        [RequiredArgument]
        [Category("输入")]
        [DisplayName("DataTable")]
        public InArgument<DataTable> DataTable { get; set; }

        [Browsable(false)]
        public new string IcoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/for-each-row.png";
            }
        }

        public ForEachRow()
        {
            valueEnumerator = new Variable<IEnumerator>();
            base.Body = new ActivityAction<DataRow>
            {
                Argument = new DelegateInArgument<DataRow>
                {
                    Name = "row"
                },
                //Handler = new Sequence
                //{
                //    DisplayName = "Body"
                //}
            };
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("DataTable", typeof(DataTable), ArgumentDirection.In, isRequired: true);
            metadata.Bind(DataTable, argument);
            metadata.AddArgument(argument);
            base.CacheMetadata(metadata);
        }

        protected override void SetValues(NativeActivityContext context)
        {
            DataTable dataTable = DataTable.Get(context);
            if (dataTable != null)
            {
                base.Values.Set(context, dataTable.Rows);
            }
        }
    }
}
