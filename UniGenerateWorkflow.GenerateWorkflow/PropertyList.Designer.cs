namespace Uni.GenerateWorkflow
{
    partial class PropertyList
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
            this.PropertyGridView = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.PropertyGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // PropertyGridView
            // 
            this.PropertyGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PropertyGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyGridView.Location = new System.Drawing.Point(0, 0);
            this.PropertyGridView.Name = "PropertyGridView";
            this.PropertyGridView.RowTemplate.Height = 23;
            this.PropertyGridView.Size = new System.Drawing.Size(800, 450);
            this.PropertyGridView.TabIndex = 0;
            this.PropertyGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.PropertyGridView_CellMouseClick);
            // 
            // PropertyList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.PropertyGridView);
            this.Name = "PropertyList";
            this.Text = "PropertyList";
            ((System.ComponentModel.ISupportInitialize)(this.PropertyGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView PropertyGridView;
    }
}