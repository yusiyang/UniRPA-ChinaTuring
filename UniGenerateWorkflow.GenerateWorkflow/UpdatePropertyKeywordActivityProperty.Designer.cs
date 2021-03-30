namespace Uni.GenerateWorkflow
{
    partial class UpdatePropertyKeywordActivityProperty
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Cancel = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_PropertyKeyword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_PropertyName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_PropertyValue = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(404, 271);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 11;
            this.Cancel.Text = "取消";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(287, 271);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 10;
            this.Save.Text = "保存";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(237, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "关键字名称：";
            // 
            // textBox_PropertyKeyword
            // 
            this.textBox_PropertyKeyword.Location = new System.Drawing.Point(331, 58);
            this.textBox_PropertyKeyword.Name = "textBox_PropertyKeyword";
            this.textBox_PropertyKeyword.Size = new System.Drawing.Size(148, 21);
            this.textBox_PropertyKeyword.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(249, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 13;
            this.label3.Text = "属性名称：";
            // 
            // textBox_PropertyName
            // 
            this.textBox_PropertyName.Location = new System.Drawing.Point(331, 96);
            this.textBox_PropertyName.Name = "textBox_PropertyName";
            this.textBox_PropertyName.Size = new System.Drawing.Size(148, 21);
            this.textBox_PropertyName.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(249, 140);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 15;
            this.label2.Text = "属性值：";
            // 
            // textBox_PropertyValue
            // 
            this.textBox_PropertyValue.Location = new System.Drawing.Point(331, 140);
            this.textBox_PropertyValue.Name = "textBox_PropertyValue";
            this.textBox_PropertyValue.Size = new System.Drawing.Size(148, 21);
            this.textBox_PropertyValue.TabIndex = 16;
            // 
            // UpdatePropertyKeywordActivityProperty
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBox_PropertyValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_PropertyName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_PropertyKeyword);
            this.Name = "UpdatePropertyKeywordActivityProperty";
            this.Text = "UpdateActivityKeywordActivityProperty";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_PropertyKeyword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_PropertyName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_PropertyValue;
    }
}