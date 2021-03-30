using System;
using System.Windows.Forms;
using Uni.Core;
using Uni.Entity;

namespace Uni.GenerateWorkflow
{
    public partial class AddOrUpdateActivityProperty : Form
    {
        private string _activityId;
        private ActivityProperty _activityProperty { get; set; }
        public AddOrUpdateActivityProperty()
        {
            InitializeComponent();
        }

        public AddOrUpdateActivityProperty(string activityId, string activityPropertyId = "")
        {
            _activityId = activityId;
            InitializeComponent();
            if (!string.IsNullOrEmpty(activityPropertyId))
            {
                using (var db = new DbContext())
                {
                    _activityProperty = db.ActivityProperty.GetById(activityPropertyId);
                    textBox_Name.Text = _activityProperty.Name;
                    checkBox_Required.Checked = _activityProperty.Required;
                }
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_Name.Text))
            {
                MessageBox.Show("请输入名称");
                return;
            }
            if (_activityProperty != null)
            {
                BindEntity(_activityProperty);
                using (var db = new DbContext())
                {
                    db.ActivityProperty.Update(_activityProperty);
                }
            }
            else
            {
                _activityProperty = new ActivityProperty();
                BindEntity(_activityProperty);
                using (var db = new DbContext())
                {
                    db.ActivityProperty.Insert(_activityProperty);
                }
            }
            this.DialogResult = DialogResult.OK;
            MessageBox.Show("保存成功");
            this.Close();
        }

        private void BindEntity(ActivityProperty entity)
        {
            entity.ActivityId = _activityId;
            entity.Name = textBox_Name.Text;
            entity.Required = checkBox_Required.Checked;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
