using System;
using System.Windows.Forms;
using Uni.Core;
using Uni.Entity;

namespace Uni.GenerateWorkflow
{
    public partial class AddOrUpdatePropertyKeyword : Form
    {
        private string _propertyKeywordId;
        private ParameterKeyword _propertyKeyword { get; set; }
        public AddOrUpdatePropertyKeyword()
        {
            InitializeComponent();
        }

        public AddOrUpdatePropertyKeyword(string id)
        {
            _propertyKeywordId = id;
            InitializeComponent();
            if (!string.IsNullOrEmpty(id))
            {
                using (var db = new DbContext())
                {
                    var actionKeyword = db.ParameterKeyword.GetById(id);
                    _propertyKeyword = actionKeyword;
                    textBox_Name.Text = actionKeyword.Name;
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
            if (_propertyKeyword != null)
            {
                BindEntity(_propertyKeyword);
                using (var db = new DbContext())
                {
                    db.ParameterKeyword.Update(_propertyKeyword);
                }
            }
            else
            {
                _propertyKeyword = new ParameterKeyword();
                BindEntity(_propertyKeyword);
                using (var db = new DbContext())
                {
                    db.ParameterKeyword.Insert(_propertyKeyword);
                }
            }
            this.DialogResult = DialogResult.OK;
            MessageBox.Show("保存成功");
            this.Close();
        }

        private void BindEntity(ParameterKeyword entity)
        {
            entity.Name = textBox_Name.Text;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
