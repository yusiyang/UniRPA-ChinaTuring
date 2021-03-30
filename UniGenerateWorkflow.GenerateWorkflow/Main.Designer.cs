namespace Uni.GenerateWorkflow
{
    partial class Main
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.ActionKeywordGridView = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ObjectKeywordGridView = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.ActivityGridView = new System.Windows.Forms.DataGridView();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.PropertyKeywordGridView = new System.Windows.Forms.DataGridView();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.BehaviorGridView = new System.Windows.Forms.DataGridView();
            this.tabControl1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ActionKeywordGridView)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ObjectKeywordGridView)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ActivityGridView)).BeginInit();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PropertyKeywordGridView)).BeginInit();
            this.tabPage6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BehaviorGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Location = new System.Drawing.Point(6, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(687, 642);
            this.tabControl1.TabIndex = 2;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.richTextBox2);
            this.tabPage4.Controls.Add(this.richTextBox1);
            this.tabPage4.Controls.Add(this.button1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(679, 616);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "测试";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // richTextBox2
            // 
            this.richTextBox2.Location = new System.Drawing.Point(24, 249);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.Size = new System.Drawing.Size(527, 347);
            this.richTextBox2.TabIndex = 5;
            this.richTextBox2.Text = "";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(24, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(527, 231);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(570, 52);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(83, 75);
            this.button1.TabIndex = 3;
            this.button1.Text = "识别测试";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.ActionKeywordGridView);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(783, 419);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "行为关键字管理";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // ActionKeywordGridView
            // 
            this.ActionKeywordGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ActionKeywordGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ActionKeywordGridView.Location = new System.Drawing.Point(3, 3);
            this.ActionKeywordGridView.Name = "ActionKeywordGridView";
            this.ActionKeywordGridView.RowTemplate.Height = 23;
            this.ActionKeywordGridView.Size = new System.Drawing.Size(777, 413);
            this.ActionKeywordGridView.TabIndex = 0;
            this.ActionKeywordGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ActionKeywrodGridView_CellMouseClick);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.ObjectKeywordGridView);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(783, 419);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "对象关键字管理";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // ObjectKeywordGridView
            // 
            this.ObjectKeywordGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ObjectKeywordGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ObjectKeywordGridView.Location = new System.Drawing.Point(3, 3);
            this.ObjectKeywordGridView.Name = "ObjectKeywordGridView";
            this.ObjectKeywordGridView.RowTemplate.Height = 23;
            this.ObjectKeywordGridView.Size = new System.Drawing.Size(777, 413);
            this.ObjectKeywordGridView.TabIndex = 0;
            this.ObjectKeywordGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ObjectKeywrodGridView_CellMouseClick);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.ActivityGridView);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(783, 419);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "活动管理";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // ActivityGridView
            // 
            this.ActivityGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ActivityGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ActivityGridView.Location = new System.Drawing.Point(3, 3);
            this.ActivityGridView.Name = "ActivityGridView";
            this.ActivityGridView.RowTemplate.Height = 23;
            this.ActivityGridView.Size = new System.Drawing.Size(777, 413);
            this.ActivityGridView.TabIndex = 2;
            this.ActivityGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ActivityGridView_CellMouseClick);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.PropertyKeywordGridView);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(783, 419);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "属性关键字";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // PropertyKeywordGridView
            // 
            this.PropertyKeywordGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PropertyKeywordGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyKeywordGridView.Location = new System.Drawing.Point(3, 3);
            this.PropertyKeywordGridView.Name = "PropertyKeywordGridView";
            this.PropertyKeywordGridView.RowTemplate.Height = 23;
            this.PropertyKeywordGridView.Size = new System.Drawing.Size(777, 413);
            this.PropertyKeywordGridView.TabIndex = 1;
            this.PropertyKeywordGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.PropertyKeywrodGridView_CellMouseClick);
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.BehaviorGridView);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(783, 419);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "知识库";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // BehaviorGridView
            // 
            this.BehaviorGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BehaviorGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BehaviorGridView.Location = new System.Drawing.Point(3, 3);
            this.BehaviorGridView.Name = "BehaviorGridView";
            this.BehaviorGridView.RowTemplate.Height = 23;
            this.BehaviorGridView.Size = new System.Drawing.Size(777, 413);
            this.BehaviorGridView.TabIndex = 1;
            this.BehaviorGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.BehaviorGridView_CellMouseClick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 647);
            this.Controls.Add(this.tabControl1);
            this.Name = "Main";
            this.Text = "Main";
            this.tabControl1.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ActionKeywordGridView)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ObjectKeywordGridView)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ActivityGridView)).EndInit();
            this.tabPage5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PropertyKeywordGridView)).EndInit();
            this.tabPage6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BehaviorGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView ActivityGridView;
        private System.Windows.Forms.DataGridView ActionKeywordGridView;
        private System.Windows.Forms.DataGridView ObjectKeywordGridView;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.DataGridView PropertyKeywordGridView;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.DataGridView BehaviorGridView;
    }
}

