namespace Uni.GenerateWorkflow
{
    partial class ActivityPropertyList
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
            this.ActivityPropertyGridView = new System.Windows.Forms.DataGridView();
            this.Save = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.Add = new System.Windows.Forms.Button();
            this.AddActivityPropertyGridView = new System.Windows.Forms.DataGridView();
            this.textBox_Search = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ActivityPropertyGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AddActivityPropertyGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // ActivityPropertyGridView
            // 
            this.ActivityPropertyGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ActivityPropertyGridView.Location = new System.Drawing.Point(1, 2);
            this.ActivityPropertyGridView.Name = "ActivityPropertyGridView";
            this.ActivityPropertyGridView.RowTemplate.Height = 23;
            this.ActivityPropertyGridView.Size = new System.Drawing.Size(800, 199);
            this.ActivityPropertyGridView.TabIndex = 1;
            this.ActivityPropertyGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ActivityPropertyGridView_CellMouseClick);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(355, 224);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 7;
            this.Save.Text = "保存";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // Delete
            // 
            this.Delete.Location = new System.Drawing.Point(456, 224);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(75, 23);
            this.Delete.TabIndex = 6;
            this.Delete.Text = "删除";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // Add
            // 
            this.Add.Location = new System.Drawing.Point(254, 224);
            this.Add.Name = "Add";
            this.Add.Size = new System.Drawing.Size(75, 23);
            this.Add.TabIndex = 5;
            this.Add.Text = "添加";
            this.Add.UseVisualStyleBackColor = true;
            this.Add.Click += new System.EventHandler(this.Add_Click);
            // 
            // AddActivityPropertyGridView
            // 
            this.AddActivityPropertyGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AddActivityPropertyGridView.Location = new System.Drawing.Point(1, 271);
            this.AddActivityPropertyGridView.Name = "AddActivityPropertyGridView";
            this.AddActivityPropertyGridView.RowTemplate.Height = 23;
            this.AddActivityPropertyGridView.Size = new System.Drawing.Size(800, 178);
            this.AddActivityPropertyGridView.TabIndex = 8;
            this.AddActivityPropertyGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.AddActivityPropertyGridView_CellMouseClick);
            // 
            // textBox_Search
            // 
            this.textBox_Search.Location = new System.Drawing.Point(45, 224);
            this.textBox_Search.Name = "textBox_Search";
            this.textBox_Search.Size = new System.Drawing.Size(100, 21);
            this.textBox_Search.TabIndex = 9;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(163, 224);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "搜索";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Search_Click);
            // 
            // ActivityPropertyList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox_Search);
            this.Controls.Add(this.AddActivityPropertyGridView);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Delete);
            this.Controls.Add(this.Add);
            this.Controls.Add(this.ActivityPropertyGridView);
            this.Name = "ActivityPropertyList";
            this.Text = "ActivityPropertyList";
            ((System.ComponentModel.ISupportInitialize)(this.ActivityPropertyGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AddActivityPropertyGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView ActivityPropertyGridView;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.Button Add;
        private System.Windows.Forms.DataGridView AddActivityPropertyGridView;
        private System.Windows.Forms.TextBox textBox_Search;
        private System.Windows.Forms.Button button1;
    }
}