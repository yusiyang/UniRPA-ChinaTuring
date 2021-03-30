using System.Windows.Forms;
using Uni.Core;

namespace Uni.GenerateWorkflow
{
    public partial class PropertyList : Form
    {
        private string _activityId;
        public PropertyList(string activityId)
        {
            _activityId = activityId;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            ActivityPropertyKeywordDataBind();
            ActivityPropertyKeywordAddLinkColumn();
        }

        private void ActivityPropertyKeywordDataBind()
        {
            using (var db = new DbContext())
            {
                PropertyGridView.DataSource = db.ActivityProperty.GetList(a => a.ActivityId == _activityId);
            }
        }
        private void ActivityPropertyKeywordAddLinkColumn()
        {
            PropertyGridView.AutoGenerateColumns = false;
            AddLinkColumn(PropertyGridView, "Add");
            AddLinkColumn(PropertyGridView, "Edit");
            AddLinkColumn(PropertyGridView, "Delete");
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

        private void PropertyGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (PropertyGridView.Columns[e.ColumnIndex].Name.ToLower())
            {
                case "add":
                    {
                        var addOrUpdateActivityFrom = new AddOrUpdateActivityProperty(_activityId);
                        addOrUpdateActivityFrom.ShowDialog();
                        if (addOrUpdateActivityFrom.DialogResult == DialogResult.OK)
                        {
                            ActivityPropertyKeywordDataBind();
                        }
                    }
                    break;
                case "edit":
                    {
                        var activityPropertyId = PropertyGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                        var addOrUpdateActivityPropertyFrom = new AddOrUpdateActivityProperty(_activityId, activityPropertyId);
                        addOrUpdateActivityPropertyFrom.ShowDialog();
                        if (addOrUpdateActivityPropertyFrom.DialogResult == DialogResult.OK)
                        {
                            ActivityPropertyKeywordDataBind();
                        }
                    }
                    break;
                case "delete":
                    {
                        MessageBoxButtons cancelButton = MessageBoxButtons.OKCancel;
                        DialogResult dialogResult = MessageBox.Show("你确定要删除此行嘛?", "确定", cancelButton);
                        if (dialogResult == DialogResult.OK)
                        {
                            var activityPropertyId = PropertyGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                            using (var db = new DbContext())
                            {
                                db.ActivityProperty.DeleteById(activityPropertyId);
                            }
                            ActivityPropertyKeywordDataBind();
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
