using System;
using System.Windows.Forms;
using Uni.Core;
using Uni.Entity;

namespace Uni.GenerateWorkflow
{
    public partial class AddOrUpdateActionKeyword : Form
    {
        private string _actionKeywordId;
        private ActionKeyword _actionKeyword { get; set; }
        public AddOrUpdateActionKeyword()
        {
            InitializeComponent();
            ComboxInit();
        }

        public AddOrUpdateActionKeyword(string id)
        {
            _actionKeywordId = id;
            InitializeComponent();
            ComboxInit();
            if (!string.IsNullOrEmpty(id))
            {
                using (var db = new DbContext())
                {
                    var actionKeyword = db.ActionKeyword.GetById(id);
                    _actionKeyword = actionKeyword;
                    textBox_Name.Text = actionKeyword.Name;
                    comboBox_KeywordType.SelectedIndex = (int)actionKeyword.KeywordType;
                }
            }
        }

        private void ComboxInit()
        {
            comboBox_KeywordType.DataSource = System.Enum.GetNames(typeof(KeywordTypeEnum));
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_Name.Text))
            {
                MessageBox.Show("请输入名称");
                return;
            }
            if (_actionKeyword != null)
            {
                BindEntity(_actionKeyword);
                using (var db = new DbContext())
                {
                    db.ActionKeyword.Update(_actionKeyword);
                }
            }
            else
            {
                _actionKeyword = new ActionKeyword();
                BindEntity(_actionKeyword);
                using (var db = new DbContext())
                {
                    db.ActionKeyword.Insert(_actionKeyword);
                }
            }
            this.DialogResult = DialogResult.OK;
            MessageBox.Show("保存成功");
            this.Close();
        }

        private void BindEntity(ActionKeyword entity)
        {
            entity.Name = textBox_Name.Text;
            entity.KeywordType = (KeywordTypeEnum)comboBox_KeywordType.SelectedIndex;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
