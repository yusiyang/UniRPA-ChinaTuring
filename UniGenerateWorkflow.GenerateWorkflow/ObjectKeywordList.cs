using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Uni.Core;
using Uni.Entity;

namespace Uni.GenerateWorkflow
{
    public partial class ObjectKeywordList : Form
    {
        private string _activityId;
        private List<string> _selectedToDeleteIdsList = new List<string>();
        private List<string> _selectedToAddIdsList = new List<string>();
        public ObjectKeywordList(string activityId)
        {
            _activityId = activityId;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            ActivityPropertyKeywordDataBind();
            ActivityPropertyKeywordAddLinkColumn();
            AddCheckboxTo(ObjectKeywordGridView);
            this.AddObjectKeywordGridView.Visible = false;
        }

        private void ActivityPropertyKeywordDataBind()
        {
            using (var db = new DbContext())
            {
                ObjectKeywordGridView.DataSource = db.Client.Ado.SqlQuery<ObjectKeyword>("select * from ObjectKeyword where id in (select ObjectKeywordId from ObjectKeywordActivityMapping where ActivityId=@ActivityId)", new { ActivityId = _activityId });
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
            ObjectKeywordGridView.AutoGenerateColumns = false;
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
            if ((bool)ObjectKeywordGridView.Rows[e.RowIndex].Cells[0].EditedFormattedValue == true)
            {
                var ObjectKeywordId = ObjectKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                _selectedToDeleteIdsList.Remove(ObjectKeywordId);
            }
            else
            {
                var ObjectKeywordId = ObjectKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                if (!_selectedToDeleteIdsList.Contains(ObjectKeywordId))
                {
                    _selectedToDeleteIdsList.Add(ObjectKeywordId);
                }
            }
        }

        private void AddObjectKeywordGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if ((bool)AddObjectKeywordGridView.Rows[e.RowIndex].Cells[0].EditedFormattedValue == true)
            {
                var ObjectKeywordId = AddObjectKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                _selectedToAddIdsList.Remove(ObjectKeywordId);
            }
            else
            {
                var ObjectKeywordId = AddObjectKeywordGridView.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                _selectedToAddIdsList.Add(ObjectKeywordId);
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            AddObjectKeywordDataBind();
            AddCheckboxTo(AddObjectKeywordGridView);
            this.AddObjectKeywordGridView.Visible = true;
            this.Add.Visible = false;
        }

        private void AddObjectKeywordDataBind()
        {
            using (var db = new DbContext())
            {
                AddObjectKeywordGridView.DataSource = db.Client.Ado.SqlQuery<ObjectKeyword>("select * from ObjectKeyword where not id in (select ObjectKeywordId from ObjectKeywordActivityMapping where ActivityId=@ActivityId)", new { ActivityId = _activityId });
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            var list = new List<ObjectKeywordActivityMapping>();
            if (_selectedToAddIdsList.Count > 0)
            {
                foreach (var item in _selectedToAddIdsList)
                {
                    list.Add(new ObjectKeywordActivityMapping { ActivityId = _activityId, ObjectKeywordId = item });
                }
                using (var db = new DbContext())
                {
                    db.Client.Insertable(list.ToArray()).ExecuteCommand();
                }
                var result = MessageBox.Show("添加成功");
                if (result == DialogResult.OK)
                {
                    ActivityPropertyKeywordDataBind();
                    AddObjectKeywordDataBind();
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
                    db.Client.Deleteable<ObjectKeywordActivityMapping>().Where(t => t.ActivityId == _activityId && _selectedToDeleteIdsList.Contains(t.ObjectKeywordId)).ExecuteCommand();
                }
                var result = MessageBox.Show("删除成功");
                if (result == DialogResult.OK)
                {
                    ActivityPropertyKeywordDataBind();
                    AddObjectKeywordDataBind();
                }
            }
            else
            {
                MessageBox.Show("请选择要删除的行");
            }
        }
    }
}
