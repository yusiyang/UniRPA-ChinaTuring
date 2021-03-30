using Newtonsoft.Json;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Windows.Forms;
using Uni.Core;

namespace Uni.GenerateWorkflow
{
    public partial class Main : Form
    {
        public Main()
        {
            //Tao Test
            var activityDescriptions = new List<ActivityDescription>()
            {
                new ActivityDescription
                {
                    ActivityName = "ExcelReadRangeActivity",
                    Properties = new Dictionary<string, object>
                    {
                        { "DataTable",  "测试表" }
                    },
                    UnrecognizedParameters = new List<string>
                    {
                        @"D:\data\开票凭证编号-填充前.xlsx"
                    }
                },
                new ActivityDescription
                {
                    ActivityName = "ForEachRowActivity",
                    Properties = new Dictionary<string, object>
                    {
                        { "DataTable",  "测试表" },
                        { "CurrentIndex",  "当前行" },
                        { "Body",new List<ActivityDescription>{
                                    //new ActivityDescription
                                    //{
                                    //    ActivityName = "TypeIntoActivity",
                                    //    Properties = new Dictionary<string, object>
                                    //    {
                                    //        { "Text",  "开票凭证" },
                                    //        { "Selector","<SAPNode Name='VBRK-VBELN' UserDefineId='/app/con[0]/ses[0]/wnd[0]/usr/ctxtVBRK-VBELN' Role='GuiCTextField' />"}
                                    //    }
                                    //},
                                    //new ActivityDescription
                                    //{
                                    //    ActivityName = "TypeIntoActivity",
                                    //    Properties = new Dictionary<string, object>
                                    //    {
                                    //        { "Text",  "100" },
                                    //        { "Selector","<SAPNode Name='VBRK-VBELN' UserDefineId='/app/con[0]/ses[0]/wnd[0]/usr/ctxtVBRK-VBELN' Role='GuiCTextField' />"}
                                    //    }
                                    //},
                                    //new ActivityDescription
                                    //{
                                    //    ActivityName = "HotKeyActivity",
                                    //    Properties = new Dictionary<string, object>
                                    //    {
                                    //        { "SelectedKey",  "ENTER" },
                                    //    }
                                    //},

                                    new ActivityDescription
                                    {
                                        ActivityName = "GetTextActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Selector",  "<SAPNode Name='VBRP-POSNR' UserDefineId='/app/con[0]/ses[0]/wnd[0]/usr/tblSAPMV60ATCTRL_UEB_FAKT/ctxtVBRP-POSNR[0,0]' Role='GuiCTextField' />" },
                                             { "Value",  "项目" },
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "ExcelWriteCellActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Cell",  "B" },
                                             { "CellContent",  "项目" },
                                        },
                                        UnrecognizedParameters = new List<string>
                                        {
                                            @"D:\data\开票凭证编号-填充后.xlsx"
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "GetTextActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Selector",  "<SAPNode Name='VBRP-MATNR' UserDefineId='/app/con[0]/ses[0]/wnd[0]/usr/tblSAPMV60ATCTRL_UEB_FAKT/ctxtVBRP-MATNR[1,0]' Role='GuiCTextField' />" },
                                             { "Value",  "物料" },
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "ExcelWriteCellActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Cell",  "C" },
                                             { "CellContent",  "物料" },
                                        },
                                        UnrecognizedParameters = new List<string>
                                        {
                                            @"D:\data\开票凭证编号-填充后.xlsx"
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "GetTextActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Selector",  "<SAPNode Name='VBRP-ARKTX' UserDefineId='/app/con[0]/ses[0]/wnd[0]/usr/tblSAPMV60ATCTRL_UEB_FAKT/txtVBRP-ARKTX[2,0]' Role='GuiTextField' />" },
                                             { "Value",  "项目描述" },
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "ExcelWriteCellActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Cell",  "D" },
                                             { "CellContent",  "项目描述" },
                                        },
                                        UnrecognizedParameters = new List<string>
                                        {
                                            @"D:\data\开票凭证编号-填充后.xlsx"
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "GetTextActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Selector",  "<SAPNode Name='VBRP-FKIMG' UserDefineId='/app/con[0]/ses[0]/wnd[0]/usr/tblSAPMV60ATCTRL_UEB_FAKT/txtVBRP-FKIMG[3,0]' Role='GuiTextField' />" },
                                             { "Value",  "开票数量" },
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "ExcelWriteCellActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Cell",  "E" },
                                             { "CellContent",  "开票数量" },
                                        },
                                        UnrecognizedParameters = new List<string>
                                        {
                                            @"D:\data\开票凭证编号-填充后.xlsx"
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "GetTextActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Selector",  "<SAPNode Name='VBRP-VRKME' UserDefineId='/app/con[0]/ses[0]/wnd[0]/usr/tblSAPMV60ATCTRL_UEB_FAKT/ctxtVBRP-VRKME[4,0]' Role='GuiCTextField' />" },
                                             { "Value",  "SU" },
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "ExcelWriteCellActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Cell",  "F" },
                                             { "CellContent",  "SU" },
                                        },
                                        UnrecognizedParameters = new List<string>
                                        {
                                            @"D:\data\开票凭证编号-填充后.xlsx"
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "GetTextActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Selector",  "<SAPNode Name='VBRP-NETWR' UserDefineId='/app/con[0]/ses[0]/wnd[0]/usr/tblSAPMV60ATCTRL_UEB_FAKT/txtVBRP-NETWR[5,0]' Role='GuiTextField' />" },
                                             { "Value",  "净值" },
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "ExcelWriteCellActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Cell",  "G" },
                                             { "CellContent",  "净值" },
                                        },
                                        UnrecognizedParameters = new List<string>
                                        {
                                            @"D:\data\开票凭证编号-填充后.xlsx"
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "GetTextActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Selector",  "<SAPNode Name='VBRP-WAERK' UserDefineId='/app/con[0]/ses[0]/wnd[0]/usr/tblSAPMV60ATCTRL_UEB_FAKT/txtVBRP-WAERK[6,0]' Role='GuiTextField' />" },
                                             { "Value",  "货币" },
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "ExcelWriteCellActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Cell",  "H" },
                                             { "CellContent",  "货币" },
                                        },
                                        UnrecognizedParameters = new List<string>
                                        {
                                            @"D:\data\开票凭证编号-填充后.xlsx"
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "GetTextActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Selector",  "<SAPNode Name='VBRP-MWSBP' UserDefineId='/app/con[0]/ses[0]/wnd[0]/usr/tblSAPMV60ATCTRL_UEB_FAKT/txtVBRP-MWSBP[7,0]' Role='GuiTextField' />" },
                                             { "Value",  "税额" },
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "ExcelWriteCellActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                             { "Cell",  "I" },
                                             { "CellContent",  "税额" },
                                        },
                                        UnrecognizedParameters = new List<string>
                                        {
                                            @"D:\data\开票凭证编号-填充后.xlsx"
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "HotKeyActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                            { "SelectedKey",  "F3" },
                                        }
                                    },
                                    new ActivityDescription
                                    {
                                        ActivityName = "KillProcessActivity",
                                        Properties = new Dictionary<string, object>
                                        {
                                            { "ProcessName",  "Sap" },
                                        }
                                    }

                            }
                        }
                    }
                }
            };

            //new ActivityXamlManager().GenerateXaml(activityDescriptions);

            InitializeComponent();
            Init();
        }

        private void Init()
        {
            ActionKeywordInit();
            ObjectKeywordInit();
            ActivityInit();
            PropertyKeywordInit();
            BehaviorInit();
        }

        private void ActionKeywordInit()
        {
            ActionKeywordDataBind();
            ActionKeywordAddLinkColumn();
        }

        private void ActionKeywordDataBind()
        {
            using (var db = new DbContext())
            {
                ActionKeywordGridView.DataSource = db.ActionKeyword.GetList();
            }
        }
        private void ActionKeywordAddLinkColumn()
        {
            ActionKeywordGridView.AutoGenerateColumns = false;
            AddLinkColumn(ActionKeywordGridView, "Add");
            AddLinkColumn(ActionKeywordGridView, "Edit");
            AddLinkColumn(ActionKeywordGridView, "Delete");
        }

        private void ObjectKeywordInit()
        {
            ObjectKeywordDataBind();
            ObjectKeywordAddLinkColumn();
        }

        private void ObjectKeywordDataBind()
        {
            using (var db = new DbContext())
            {
                ObjectKeywordGridView.DataSource = db.ObjectKeyword.GetList();
            }
        }
        private void ObjectKeywordAddLinkColumn()
        {
            ObjectKeywordGridView.AutoGenerateColumns = false;
            AddLinkColumn(ObjectKeywordGridView, "Add");
            AddLinkColumn(ObjectKeywordGridView, "Edit");
            AddLinkColumn(ObjectKeywordGridView, "Delete");
        }


        private void ActivityInit()
        {
            ActivityDataBind();
            ActivityAddLinkColumn();
        }
        private void ActivityDataBind()
        {
            using (var db = new DbContext())
            {
                ActivityGridView.DataSource = db.Activity.GetList();
            }
        }
        private void ActivityAddLinkColumn()
        {
            ActivityGridView.AutoGenerateColumns = false;
            AddLinkColumn(ActivityGridView, "Add");
            AddLinkColumn(ActivityGridView, "Edit");
            AddLinkColumn(ActivityGridView, "Delete");
            AddLinkColumn(ActivityGridView, "Property");
            AddLinkColumn(ActivityGridView, "ActionKeyword");
            AddLinkColumn(ActivityGridView, "ObjectKeyword");
        }



        private void AddLinkColumn(DataGridView dataGridView, string text)
        {
            DataGridViewLinkColumn dataGridViewLink = new DataGridViewLinkColumn();
            dataGridViewLink.Text = text;
            dataGridViewLink.Name = text;
            dataGridViewLink.HeaderText = text;

            dataGridViewLink.UseColumnTextForLinkValue = true;
            dataGridView.Columns.Add(dataGridViewLink);
        }

        private void PropertyKeywordInit()
        {
            PropertyKeywordDataBind();
            PropertyKeywordAddLinkColumn();
        }

        private void PropertyKeywordDataBind()
        {
            using (var db = new DbContext())
            {
                PropertyKeywordGridView.DataSource = db.ParameterKeyword.GetList();
            }
        }
        private void PropertyKeywordAddLinkColumn()
        {
            PropertyKeywordGridView.AutoGenerateColumns = false;
            AddLinkColumn(PropertyKeywordGridView, "Add");
            AddLinkColumn(PropertyKeywordGridView, "Edit");
            AddLinkColumn(PropertyKeywordGridView, "Delete");
            AddLinkColumn(PropertyKeywordGridView, "Property");
        }

        private void BehaviorInit()
        {
            BehaviorDataBind();
            BehaviorAddLinkColumn();
        }

        private void BehaviorDataBind()
        {
            using (var db = new DbContext())
            {
                BehaviorGridView.DataSource = db.Behavior.GetList();
            }
        }
        private void BehaviorAddLinkColumn()
        {
            BehaviorGridView.AutoGenerateColumns = false;
            AddLinkColumn(BehaviorGridView, "Add");
            AddLinkColumn(BehaviorGridView, "Edit");
            AddLinkColumn(BehaviorGridView, "Delete");
        }

        private void AddCheckboxTo(DataGridView dataGridView)
        {
            DataGridViewCheckBoxColumn columncb = new DataGridViewCheckBoxColumn
            {
                HeaderText = "选择",
                Name = "cb_check",
                TrueValue = true,
                FalseValue = false,
                DataPropertyName = "IsChecked"
            };
            dataGridView.Columns.Insert(0, columncb);
        }

        private void ActionKeywrodGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (ActionKeywordGridView.Columns[e.ColumnIndex].Name.ToLower())
            {
                case "add":
                    {
                        var addOrUpdateKeywordFrom = new AddOrUpdateActionKeyword();
                        addOrUpdateKeywordFrom.ShowDialog();
                        if (addOrUpdateKeywordFrom.DialogResult == DialogResult.OK)
                        {
                            ActionKeywordDataBind();
                        }
                    }
                    break;
                case "edit":
                    {
                        var actionKeywordId = ActionKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                        var addOrUpdateKeywordFrom = new AddOrUpdateActionKeyword(actionKeywordId);
                        addOrUpdateKeywordFrom.ShowDialog();
                        if (addOrUpdateKeywordFrom.DialogResult == DialogResult.OK)
                        {
                            ActionKeywordDataBind();
                        }
                    }
                    break;
                case "delete":
                    {
                        MessageBoxButtons cancelButton = MessageBoxButtons.OKCancel;
                        DialogResult dialogResult = MessageBox.Show("你确定要删除此行嘛?", "确定", cancelButton);
                        if (dialogResult == DialogResult.OK)
                        {
                            var actionKeywordId = ActionKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                            using (var db = new DbContext())
                            {
                                db.ActionKeyword.DeleteById(actionKeywordId);
                            }
                            ActionKeywordDataBind();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void ObjectKeywrodGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (ObjectKeywordGridView.Columns[e.ColumnIndex].Name.ToLower())
            {
                case "add":
                    {
                        var addOrUpdateKeywordFrom = new AddOrUpdateObjectKeyword();
                        addOrUpdateKeywordFrom.ShowDialog();
                        if (addOrUpdateKeywordFrom.DialogResult == DialogResult.OK)
                        {
                            ObjectKeywordDataBind();
                        }
                    }
                    break;
                case "edit":
                    {
                        var objectKeywordId = ObjectKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                        var addOrUpdateKeywordFrom = new AddOrUpdateObjectKeyword(objectKeywordId);
                        addOrUpdateKeywordFrom.ShowDialog();
                        if (addOrUpdateKeywordFrom.DialogResult == DialogResult.OK)
                        {
                            ObjectKeywordDataBind();
                        }
                    }
                    break;
                case "delete":
                    {
                        MessageBoxButtons cancelButton = MessageBoxButtons.OKCancel;
                        DialogResult dialogResult = MessageBox.Show("你确定要删除此行嘛?", "确定", cancelButton);
                        if (dialogResult == DialogResult.OK)
                        {
                            var objectKeywordId = ObjectKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                            using (var db = new DbContext())
                            {
                                db.ObjectKeyword.DeleteById(objectKeywordId);
                            }
                            ObjectKeywordDataBind();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void ActivityGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            //GridViewCellMouseClickHandle(sender, e);
            switch (ActivityGridView.Columns[e.ColumnIndex].Name.ToLower())
            {
                case "add":
                    {
                        var addOrUpdateActivityFrom = new AddOrUpdateActivity();
                        addOrUpdateActivityFrom.ShowDialog();
                        if (addOrUpdateActivityFrom.DialogResult == DialogResult.OK)
                        {
                            ActivityDataBind();
                        }
                    }
                    break;
                case "edit":
                    {
                        var activityId = ActivityGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                        var addOrUpdateKeywordFrom = new AddOrUpdateActivity(activityId);
                        addOrUpdateKeywordFrom.ShowDialog();
                        if (addOrUpdateKeywordFrom.DialogResult == DialogResult.OK)
                        {
                            ActivityDataBind();
                        }
                    }
                    break;
                case "delete":
                    {
                        MessageBoxButtons cancelButton = MessageBoxButtons.OKCancel;
                        DialogResult dialogResult = MessageBox.Show("你确定要删除此行嘛?", "确定", cancelButton);
                        if (dialogResult == DialogResult.OK)
                        {
                            var activityId = ActivityGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                            using (var db = new DbContext())
                            {
                                db.Activity.DeleteById(activityId);
                            }
                            ActivityDataBind();
                        }
                    }
                    break;
                case "property":
                    {
                        var activityId = ActivityGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                        var activityPropertyFrom = new PropertyList(activityId);
                        activityPropertyFrom.ShowDialog();
                    }
                    break;
                case "actionkeyword":
                    {
                        var activityId = ActivityGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                        var actionKeywordListFrom = new ActionKeywordList(activityId);
                        actionKeywordListFrom.ShowDialog();
                    }
                    break;
                case "objectkeyword":
                    {
                        var activityId = ActivityGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                        var objectKeywordListFrom = new ObjectKeywordList(activityId);
                        objectKeywordListFrom.ShowDialog();
                    }
                    break;
                default:
                    break;
            }
        }

        private void PropertyKeywrodGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (PropertyKeywordGridView.Columns[e.ColumnIndex].Name.ToLower())
            {
                case "add":
                    {
                        var addOrUpdatePropertyKeywordFrom = new AddOrUpdatePropertyKeyword();
                        addOrUpdatePropertyKeywordFrom.ShowDialog();
                        if (addOrUpdatePropertyKeywordFrom.DialogResult == DialogResult.OK)
                        {
                            PropertyKeywordDataBind();
                        }
                    }
                    break;
                case "edit":
                    {
                        var actionKeywordId = PropertyKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                        var addOrUpdatePropertyKeywordFrom = new AddOrUpdatePropertyKeyword(actionKeywordId);
                        addOrUpdatePropertyKeywordFrom.ShowDialog();
                        if (addOrUpdatePropertyKeywordFrom.DialogResult == DialogResult.OK)
                        {
                            PropertyKeywordDataBind();
                        }
                    }
                    break;
                case "delete":
                    {
                        MessageBoxButtons cancelButton = MessageBoxButtons.OKCancel;
                        DialogResult dialogResult = MessageBox.Show("你确定要删除此行嘛?", "确定", cancelButton);
                        if (dialogResult == DialogResult.OK)
                        {
                            var propertyKeywordId = PropertyKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                            using (var db = new DbContext())
                            {
                                db.ParameterKeyword.DeleteById(propertyKeywordId);
                            }
                            PropertyKeywordDataBind();
                        }
                    }
                    break;
                case "property":
                    {
                        var propertyKeywordId = PropertyKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                        var activityPropertyFrom = new ActivityPropertyList(propertyKeywordId);
                        activityPropertyFrom.ShowDialog();
                    }
                    break;
                default:
                    break;
            }
        }

        private void BehaviorGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (BehaviorGridView.Columns[e.ColumnIndex].Name.ToLower())
            {
                case "add":
                    {
                        var addOrUpdateBehaviorFrom = new AddOrUpdateBehavior();
                        addOrUpdateBehaviorFrom.ShowDialog();
                        if (addOrUpdateBehaviorFrom.DialogResult == DialogResult.OK)
                        {
                            BehaviorDataBind();
                        }
                    }
                    break;
                case "edit":
                    {
                        var behaviorId = BehaviorGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                        var addOrUpdateBehaviorFrom = new AddOrUpdateBehavior(behaviorId);
                        addOrUpdateBehaviorFrom.ShowDialog();
                        if (addOrUpdateBehaviorFrom.DialogResult == DialogResult.OK)
                        {
                            BehaviorDataBind();
                        }
                    }
                    break;
                case "delete":
                    {
                        MessageBoxButtons cancelButton = MessageBoxButtons.OKCancel;
                        DialogResult dialogResult = MessageBox.Show("你确定要删除此行嘛?", "确定", cancelButton);
                        if (dialogResult == DialogResult.OK)
                        {
                            var behaviorId = BehaviorGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                            using (var db = new DbContext())
                            {
                                db.Behavior.DeleteById(behaviorId);
                            }
                            BehaviorDataBind();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private GenerateWorkflowManager _generateWorkflowManager = GenerateWorkflowManager.Instance;

        private void button1_Click(object sender, EventArgs e)
        {

            List<ActivityDescription> activityDescriptions = _generateWorkflowManager.InputText(richTextBox1.Text, out string error);

            _generateWorkflowManager.GenerateXaml(activityDescriptions, null, AppDomain.CurrentDomain.BaseDirectory + "Workflows\\");

            System.IO.StringWriter textWriter = new System.IO.StringWriter();
            JsonTextWriter writer = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented,//格式化缩进
                Indentation = 4,  //缩进四个字符
                IndentChar = ' '  //缩进的字符是空格
            };

            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, activityDescriptions);
            richTextBox2.Text = textWriter.ToString();
            ;
        }
    }
}
