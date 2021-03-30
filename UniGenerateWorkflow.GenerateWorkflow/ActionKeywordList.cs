using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Uni.Core;
using Uni.Entity;

namespace Uni.GenerateWorkflow
{
    public partial class ActionKeywordList : Form
    {
        private string _activityId;
        private List<string> _selectedToDeleteIdsList = new List<string>();
        private List<string> _selectedToAddIdsList = new List<string>();
        public ActionKeywordList(string activityId)
        {
            _activityId = activityId;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            ActivityPropertyKeywordDataBind();
            ActivityPropertyKeywordAddLinkColumn();
            AddCheckboxTo(ActionKeywordGridView);
            this.AddActionKeywordGridView.Visible = false;
        }

        private void ActivityPropertyKeywordDataBind()
        {
            using (var db = new DbContext())
            {
                ActionKeywordGridView.DataSource = db.Client.Ado.SqlQuery<ActionKeyword>("select * from ActionKeyword where id in (select ActionKeywordId from ActionKeywordActivityMapping where ActivityId=@ActivityId)", new { ActivityId = _activityId });
            }
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

        private void ActivityPropertyKeywordAddLinkColumn()
        {
            ActionKeywordGridView.AutoGenerateColumns = false;
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
            if ((bool)ActionKeywordGridView.Rows[e.RowIndex].Cells[0].EditedFormattedValue == true)
            {
                var actionKeywordId = ActionKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                _selectedToDeleteIdsList.Remove(actionKeywordId);
            }
            else
            {
                var actionKeywordId = ActionKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                if (!_selectedToDeleteIdsList.Contains(actionKeywordId))
                {
                    _selectedToDeleteIdsList.Add(actionKeywordId);
                }
            }
        }

        private void AddActionKeywordGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if ((bool)AddActionKeywordGridView.Rows[e.RowIndex].Cells[0].EditedFormattedValue == true)
            {
                var actionKeywordId = AddActionKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                _selectedToAddIdsList.Remove(actionKeywordId);
            }
            else
            {
                var actionKeywordId = AddActionKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                _selectedToAddIdsList.Add(actionKeywordId);
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            AddActionKeywordDataBind();
            AddCheckboxTo(AddActionKeywordGridView);
            this.AddActionKeywordGridView.Visible = true;
            this.Add.Visible = false;
        }

        private void AddActionKeywordDataBind()
        {
            using (var db = new DbContext())
            {
                AddActionKeywordGridView.DataSource = db.Client.Ado.SqlQuery<ActionKeyword>("select * from ActionKeyword where not id in (select ActionKeywordId from ActionKeywordActivityMapping where ActivityId=@ActivityId)", new { ActivityId = _activityId });
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            var list = new List<ActionKeywordActivityMapping>();
            if (_selectedToAddIdsList.Count > 0)
            {
                foreach (var item in _selectedToAddIdsList)
                {
                    list.Add(new ActionKeywordActivityMapping { ActivityId = _activityId, ActionKeywordId = item });
                }
                using (var db = new DbContext())
                {
                    db.Client.Insertable(list.ToArray()).ExecuteCommand();
                }
                var result = MessageBox.Show("添加成功");
                if (result == DialogResult.OK)
                {
                    ActivityPropertyKeywordDataBind();
                    AddActionKeywordDataBind();
                }
            }
            else
            {
                MessageBox.Show("请选择要添加的行");
            }
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (_selectedToDeleteIdsList.Count > 0)
            {
                using (var db = new DbContext())
                {
                    db.Client.Deleteable<ActionKeywordActivityMapping>().Where(t => t.ActivityId == _activityId && _selectedToDeleteIdsList.Contains(t.ActionKeywordId)).ExecuteCommand();
                }
                var result = MessageBox.Show("删除成功");
                if (result == DialogResult.OK)
                {
                    ActivityPropertyKeywordDataBind();
                    AddActionKeywordDataBind();
                }
            }
            else
            {
                MessageBox.Show("请选择要删除的行");
            }
        }
    }
}
