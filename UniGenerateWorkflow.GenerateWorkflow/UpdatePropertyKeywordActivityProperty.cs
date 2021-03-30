using System;
using System.Windows.Forms;
using Uni.Core;
using Uni.Entity;

namespace Uni.GenerateWorkflow
{
    public partial class UpdatePropertyKeywordActivityProperty : Form
    {
        private string _mappingId;
        private ParameterKeywordActivityPropertyMapping _propertyKeywordActivityPropertyMapping;
        public UpdatePropertyKeywordActivityProperty(string mappingId)
        {
            _mappingId = mappingId;
            InitializeComponent();
            if (!string.IsNullOrEmpty(_mappingId))
            {
                using (var db = new DbContext())
                {
                    _propertyKeywordActivityPropertyMapping = db.ParameterKeywordActivityPropertyMapping.GetById(_mappingId);
                    textBox_PropertyName.Text = db.ParameterKeyword.GetById(_propertyKeywordActivityPropertyMapping.ParameterKeywordId).Name;
                }
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            using (var db = new DbContext())
            {
                db.ParameterKeywordActivityPropertyMapping.Update(_propertyKeywordActivityPropertyMapping);
            }
            this.DialogResult = DialogResult.OK;
            MessageBox.Show("保存成功");
            this.Close();
        }
    }
}
