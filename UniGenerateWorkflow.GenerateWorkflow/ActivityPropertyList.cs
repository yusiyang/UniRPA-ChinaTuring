using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Uni.Core;
using Uni.Entity;

namespace Uni.GenerateWorkflow
{
    public partial class ActivityPropertyList : Form
    {
        private string _parameterKeywordId;
        private List<string> _selectedToDeleteIdsList = new List<string>();
        private List<string> _selectedToAddIdsList = new List<string>();

        public ActivityPropertyList(string propertyKeywordId)
        {
            _parameterKeywordId = propertyKeywordId;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            ActionKeywordActivityPropertyDataBind();
            ActivityPropertyKeywordAddLinkColumn();
            AddCheckboxTo(ActivityPropertyGridView);
            this.AddActivityPropertyGridView.Visible = false;
        }

        private void ActionKeywordActivityPropertyDataBind()
        {
            using (var db = new DbContext())
            {
                var sql = $"SELECT * FROM {nameof(ParameterKeywordActivityPropertyMapping)} WHERE {nameof(ParameterKeywordActivityPropertyMapping.ParameterKeywordId)} = @{nameof(ParameterKeywordActivityPropertyMapping.ParameterKeywordId)}";
                var list = db.Client.Ado.SqlQuery<ParameterKeywordActivityPropertyMapping>(sql, new { ParameterKeywordId = _parameterKeywordId });
                ActivityPropertyGridView.DataSource = list;
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
            ActivityPropertyGridView.AutoGenerateColumns = false;
            AddLinkColumn(ActivityPropertyGridView, "Edit");
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

        private void ActivityPropertyGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (ActivityPropertyGridView.Columns[e.ColumnIndex].Name.ToLower() == "edit")
            {
                var id = ActivityPropertyGridView.Rows[e.RowIndex].Cells[nameof(ParameterKeywordActivityPropertyMapping.Id)].Value.ToString();
                var updateActivityKeywordActivityPropertyForm = new UpdatePropertyKeywordActivityProperty(id);
                updateActivityKeywordActivityPropertyForm.ShowDialog();
                if (updateActivityKeywordActivityPropertyForm.DialogResult == DialogResult.OK)
                {
                    ActionKeywordActivityPropertyDataBind();
                }
            }
            else
            {
                if ((bool)ActivityPropertyGridView.Rows[e.RowIndex].Cells[0].EditedFormattedValue == true)
                {
                    var activityPropertyId = ActivityPropertyGridView.Rows[e.RowIndex].Cells[nameof(ParameterKeywordActivityPropertyMapping.ActivityPropertyId)].Value.ToString();
                    _selectedToDeleteIdsList.Remove(activityPropertyId);
                }
                else
                {
                    var activityPropertyId = ActivityPropertyGridView.Rows[e.RowIndex].Cells[nameof(ParameterKeywordActivityPropertyMapping.ActivityPropertyId)].Value.ToString();
                    if (!_selectedToDeleteIdsList.Contains(activityPropertyId))
                    {
                        _selectedToDeleteIdsList.Add(activityPropertyId);
                    }
                }
            }
        }

        private void AddActivityPropertyGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if ((bool)AddActivityPropertyGridView.Rows[e.RowIndex].Cells[0].EditedFormattedValue == true)
            {
                var actionKeywordId = AddActivityPropertyGridView.Rows[e.RowIndex].Cells[nameof(ActivityProperty.Id)].Value.ToString();
                _selectedToAddIdsList.Remove(actionKeywordId);
            }
            else
            {
                var actionKeywordId = AddActivityPropertyGridView.Rows[e.RowIndex].Cells[nameof(ActivityProperty.Id)].Value.ToString();
                _selectedToAddIdsList.Add(actionKeywordId);
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            AddActionKeywordDataBind();
            AddCheckboxTo(AddActivityPropertyGridView);
            this.AddActivityPropertyGridView.Visible = true;
            this.Add.Visible = false;
        }

        private void AddActionKeywordDataBind()
        {
            using (var db = new DbContext())
            {
                var sql = $"SELECT * FROM {nameof(ActivityProperty)} " +
                    $" WHERE {nameof(ActivityProperty.Id)} NOT IN " +
                    $" (SElECT {nameof(ActivityProperty.Id)} FROM {nameof(ParameterKeywordActivityPropertyMapping)} WHERE {nameof(ParameterKeywordActivityPropertyMapping.ParameterKeywordId)}=@{nameof(ParameterKeywordActivityPropertyMapping.ParameterKeywordId)})";
                var list = db.Client.Ado.SqlQuery<ActivityProperty>(sql, new { ParameterKeywordId = _parameterKeywordId });
                if (!string.IsNullOrEmpty(textBox_Search.Text))
                {
                    list = list.Where(a => a.Name.ToLower().Contains(textBox_Search.Text.ToLower())).ToList();
                }
                AddActivityPropertyGridView.DataSource = list;
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            var list = new List<ParameterKeywordActivityPropertyMapping>();
            if (_selectedToAddIdsList.Count > 0)
            {
                foreach (var item in _selectedToAddIdsList)
                {
                    using (var db = new DbContext())
                    {
                        list.Add(new ParameterKeywordActivityPropertyMapping { ActivityPropertyId = item, ParameterKeywordId = _parameterKeywordId });
                    }
                }
                using (var db = new DbContext())
                {
                    db.Client.Insertable(list.ToArray()).ExecuteCommand();
                }
                var result = MessageBox.Show("添加成功");
                if (result == DialogResult.OK)
                {
                    ActionKeywordActivityPropertyDataBind();
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
                    db.Client.Deleteable<ParameterKeywordActivityPropertyMapping>().Where(t => t.ParameterKeywordId == _parameterKeywordId && _selectedToDeleteIdsList.Contains(t.ActivityPropertyId)).ExecuteCommand();
                }
                var result = MessageBox.Show("删除成功");
                if (result == DialogResult.OK)
                {
                    ActionKeywordActivityPropertyDataBind();
                    AddActionKeywordDataBind();
                }
            }
            else
            {
                MessageBox.Show("请选择要删除的行");
            }
        }

        private void Search_Click(object sender, EventArgs e)
        {
            AddActionKeywordDataBind();
        }
    }
}
