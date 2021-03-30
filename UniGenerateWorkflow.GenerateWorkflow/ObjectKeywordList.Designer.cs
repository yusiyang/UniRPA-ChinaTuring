namespace Uni.GenerateWorkflow
{
    partial class ObjectKeywordList
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
            this.ObjectKeywordGridView = new System.Windows.Forms.DataGridView();
            this.Add = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.AddObjectKeywordGridView = new System.Windows.Forms.DataGridView();
            this.Save = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ObjectKeywordGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AddObjectKeywordGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // ObjectKeywordGridView
            // 
            this.ObjectKeywordGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ObjectKeywordGridView.Location = new System.Drawing.Point(0, 0);
            this.ObjectKeywordGridView.Name = "ObjectKeywordGridView";
            this.ObjectKeywordGridView.RowTemplate.Height = 23;
            this.ObjectKeywordGridView.Size = new System.Drawing.Size(800, 199);
            this.ObjectKeywordGridView.TabIndex = 0;
            this.ObjectKeywordGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.PropertyGridView_CellMouseClick);
            // 
            // Add
            // 
            this.Add.Location = new System.Drawing.Point(209, 225);
            this.Add.Name = "Add";
            this.Add.Size = new System.Drawing.Size(75, 23);
            this.Add.TabIndex = 1;
            this.Add.Text = "添加";
            this.Add.UseVisualStyleBackColor = true;
            this.Add.Click += new System.EventHandler(this.Add_Click);
            // 
            // Delete
            // 
            this.Delete.Location = new System.Drawing.Point(411, 225);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(75, 23);
            this.Delete.TabIndex = 2;
            this.Delete.Text = "删除";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // AddObjectKeywordGridView
            // 
            this.AddObjectKeywordGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AddObjectKeywordGridView.Location = new System.Drawing.Point(0, 271);
            this.AddObjectKeywordGridView.Name = "AddObjectKeywordGridView";
            this.AddObjectKeywordGridView.RowTemplate.Height = 23;
            this.AddObjectKeywordGridView.Size = new System.Drawing.Size(800, 178);
            this.AddObjectKeywordGridView.TabIndex = 3;
            this.AddObjectKeywordGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.AddObjectKeywordGridView_CellMouseClick);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(310, 225);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 4;
            this.Save.Text = "保存";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // ObjectKeywordList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.AddObjectKeywordGridView);
            this.Controls.Add(this.Delete);
            this.Controls.Add(this.Add);
            this.Controls.Add(this.ObjectKeywordGridView);
            this.Name = "ObjectKeywordList";
            this.Text = "PropertyList";
            ((System.ComponentModel.ISupportInitialize)(this.ObjectKeywordGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AddObjectKeywordGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView ObjectKeywordGridView;
        private System.Windows.Forms.Button Add;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.DataGridView AddObjectKeywordGridView;
        private System.Windows.Forms.Button Save;
    }
}