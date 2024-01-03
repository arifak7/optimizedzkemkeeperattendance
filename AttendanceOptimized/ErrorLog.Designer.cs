namespace AttendanceOptimized
{
    partial class ErrorLog
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.error_table = new System.Windows.Forms.DataGridView();
            this.nik_karyawan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.detail_error = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.error_table)).BeginInit();
            this.SuspendLayout();
            // 
            // error_table
            // 
            this.error_table.AllowUserToAddRows = false;
            this.error_table.AllowUserToDeleteRows = false;
            this.error_table.AllowUserToResizeRows = false;
            this.error_table.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.error_table.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.error_table.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.error_table.ColumnHeadersHeight = 35;
            this.error_table.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.error_table.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nik_karyawan,
            this.detail_error});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Cascadia Mono", 10F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.RoyalBlue;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.error_table.DefaultCellStyle = dataGridViewCellStyle1;
            this.error_table.Dock = System.Windows.Forms.DockStyle.Fill;
            this.error_table.GridColor = System.Drawing.SystemColors.ControlLight;
            this.error_table.Location = new System.Drawing.Point(0, 0);
            this.error_table.MultiSelect = false;
            this.error_table.Name = "error_table";
            this.error_table.ReadOnly = true;
            this.error_table.RowHeadersVisible = false;
            this.error_table.RowHeadersWidth = 51;
            this.error_table.RowTemplate.Height = 30;
            this.error_table.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.error_table.Size = new System.Drawing.Size(830, 472);
            this.error_table.TabIndex = 3;
            // 
            // nik_karyawan
            // 
            this.nik_karyawan.HeaderText = "Date";
            this.nik_karyawan.MinimumWidth = 6;
            this.nik_karyawan.Name = "nik_karyawan";
            this.nik_karyawan.ReadOnly = true;
            // 
            // detail_error
            // 
            this.detail_error.HeaderText = "Detail";
            this.detail_error.MinimumWidth = 6;
            this.detail_error.Name = "detail_error";
            this.detail_error.ReadOnly = true;
            // 
            // ErrorLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(830, 472);
            this.Controls.Add(this.error_table);
            this.Name = "ErrorLog";
            this.Text = "ErrorLog";
            ((System.ComponentModel.ISupportInitialize)(this.error_table)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView error_table;
        private System.Windows.Forms.DataGridViewTextBoxColumn nik_karyawan;
        private System.Windows.Forms.DataGridViewTextBoxColumn detail_error;
    }
}