using System;
using System.Windows.Forms;
using Uni.Core;
using Uni.Entity;

namespace Uni.GenerateWorkflow
{
    public partial class AddOrUpdateBehavior : Form
    {
        private string _behaviorId;
        private Behavior _behavior { get; set; }
        public AddOrUpdateBehavior()
        {
            InitializeComponent();
            ComboxInit();
        }

        private void ComboxInit()
        {
            comboBox_BehaviorType.DataSource = System.Enum.GetNames(typeof(BehaviorTypeEnum));
        }

        public AddOrUpdateBehavior(string id)
        {
            _behaviorId = id;
            InitializeComponent();
            ComboxInit();
            if (!string.IsNullOrEmpty(id))
            {
                using (var db = new DbContext())
                {
                    _behavior = db.Behavior.GetById(id);
                    textBox_Name.Text = _behavior.Key;
                    textBox_Value.Text = _behavior.Value;
                    comboBox_BehaviorType.SelectedIndex = (int)_behavior.BehaviorType;
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
            if (_behavior != null)
            {
                BindEntity(_behavior);
                using (var db = new DbContext())
                {
                    db.Behavior.Update(_behavior);
                }
            }
            else
            {
                _behavior = new Behavior();
                BindEntity(_behavior);
                using (var db = new DbContext())
                {
                    db.Behavior.Insert(_behavior);
                }
            }
            this.DialogResult = DialogResult.OK;
            MessageBox.Show("保存成功");
            this.Close();
        }

        private void BindEntity(Behavior entity)
        {
            entity.Key = textBox_Name.Text;
            entity.Value = textBox_Value.Text;
            entity.BehaviorType = (BehaviorTypeEnum)comboBox_BehaviorType.SelectedIndex;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
