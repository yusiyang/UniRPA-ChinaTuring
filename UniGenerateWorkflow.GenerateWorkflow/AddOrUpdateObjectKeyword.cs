using System;
using System.Windows.Forms;
using Uni.Core;
using Uni.Entity;

namespace Uni.GenerateWorkflow
{
    public partial class AddOrUpdateObjectKeyword : Form
    {
        private string _objectKeywordId;
        private ObjectKeyword _objectKeyword { get; set; }
        public AddOrUpdateObjectKeyword()
        {
            InitializeComponent();
        }

        public AddOrUpdateObjectKeyword(string id)
        {
            _objectKeywordId = id;
            InitializeComponent();
            if (!string.IsNullOrEmpty(id))
            {
                using (var db = new DbContext())
                {
                    _objectKeyword = db.ObjectKeyword.GetById(id);
                    textBox_Name.Text = _objectKeyword.Name;
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
            if (_objectKeyword != null)
            {
                BindEntity(_objectKeyword);
                using (var db = new DbContext())
                {
                    db.ObjectKeyword.Update(_objectKeyword);
                }
            }
            else
            {
                _objectKeyword = new ObjectKeyword();
                BindEntity(_objectKeyword);
                using (var db = new DbContext())
                {
                    db.ObjectKeyword.Insert(_objectKeyword);
                }
            }
            this.DialogResult = DialogResult.OK;
            MessageBox.Show("保存成功");
            this.Close();
        }

        private void BindEntity(ObjectKeyword entity)
        {
            entity.Name = textBox_Name.Text;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
