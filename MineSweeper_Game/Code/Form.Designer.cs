namespace Minesweeper.UI
{
    partial class MainForm
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
            this.gridField = new System.Windows.Forms.DataGridView();
            this.btValidate = new System.Windows.Forms.Button();
            this.btReset = new System.Windows.Forms.Button();
            this.btNewMap = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridField)).BeginInit();
            this.SuspendLayout();
            // 
            // gridField
            // 
            this.gridField.AllowUserToAddRows = false;
            this.gridField.AllowUserToDeleteRows = false;
            this.gridField.AllowUserToResizeColumns = false;
            this.gridField.AllowUserToResizeRows = false;
            this.gridField.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.gridField.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.gridField.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
            this.gridField.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridField.ColumnHeadersVisible = false;
            this.gridField.GridColor = System.Drawing.SystemColors.ControlDarkDark;
            this.gridField.Location = new System.Drawing.Point(0, 0);
            this.gridField.MultiSelect = false;
            this.gridField.Name = "gridField";
            this.gridField.ReadOnly = true;
            this.gridField.RowHeadersVisible = false;
            this.gridField.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.gridField.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.gridField.Size = new System.Drawing.Size(276, 215);
            this.gridField.TabIndex = 0;
            this.gridField.TabStop = false;
            this.gridField.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridField_CellDoubleClick);
            this.gridField.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.gridField_CellMouseClick);
            // 
            // btValidate
            // 
            this.btValidate.Location = new System.Drawing.Point(300, 11);
            this.btValidate.Name = "btValidate";
            this.btValidate.Size = new System.Drawing.Size(75, 23);
            this.btValidate.TabIndex = 1;
            this.btValidate.Text = "Validate";
            this.btValidate.UseVisualStyleBackColor = true;
            this.btValidate.Click += new System.EventHandler(this.btValidate_Click);
            // 
            // btReset
            // 
            this.btReset.Location = new System.Drawing.Point(299, 45);
            this.btReset.Name = "btReset";
            this.btReset.Size = new System.Drawing.Size(75, 23);
            this.btReset.TabIndex = 2;
            this.btReset.Text = "Restart";
            this.btReset.UseVisualStyleBackColor = true;
            this.btReset.Click += new System.EventHandler(this.btReset_Click);
            // 
            // btNewMap
            // 
            this.btNewMap.Location = new System.Drawing.Point(298, 78);
            this.btNewMap.Name = "btNewMap";
            this.btNewMap.Size = new System.Drawing.Size(75, 23);
            this.btNewMap.TabIndex = 3;
            this.btNewMap.Text = "New map";
            this.btNewMap.UseVisualStyleBackColor = true;
            this.btNewMap.Click += new System.EventHandler(this.btNewMap_Click);
            // 
            // UI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 216);
            this.Controls.Add(this.btNewMap);
            this.Controls.Add(this.btReset);
            this.Controls.Add(this.btValidate);
            this.Controls.Add(this.gridField);
            this.Name = "UI";
            this.Text = this.Name;
            ((System.ComponentModel.ISupportInitialize)(this.gridField)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gridField;
        private System.Windows.Forms.Button btValidate;
        private System.Windows.Forms.Button btReset;
        private System.Windows.Forms.Button btNewMap;
    }
}

