using System;
using System.Windows.Forms;
using Uni.Core;
using Uni.Entity;

namespace Uni.GenerateWorkflow
{
    public partial class AddOrUpdateActivity : Form
    {
        private string _activityId;
        private Activity _activity { get; set; }
        public AddOrUpdateActivity()
        {
            InitializeComponent();
        }

        public AddOrUpdateActivity(string id)
        {
            _activityId = id;
            InitializeComponent();
            if (!string.IsNullOrEmpty(id))
            {
                using (var db = new DbContext())
                {
                    _activity = db.Activity.GetById(id);
                    textBox_Name.Text = _activity.Name;
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
            if (_activity != null)
            {
                BindEntity(_activity);
                using (var db = new DbContext())
                {
                    db.Activity.Update(_activity);
                }
            }
            else
            {
                _activity = new Activity();
                BindEntity(_activity);
                using (var db = new DbContext())
                {
                    db.Activity.Insert(_activity);
                }
            }
            this.DialogResult = DialogResult.OK;
            MessageBox.Show("保存成功");
            this.Close();
        }

        private void BindEntity(Activity entity)
        {
            entity.Name = textBox_Name.Text;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
