using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AttendanceOptimized
{
    public partial class DatabaseSettings : Form
    {
        public DatabaseSettings()
        {
            InitializeComponent();
            initSavedData();
        }

        private void initSavedData()
        {
            connectionString1Box.Text = Properties.Settings.Default.db1connstring;
            connectionString2Box.Text = Properties.Settings.Default.db2connstring;
            autostartCheck.Checked = Properties.Settings.Default.autostart;
        }

        private void commitdb1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.db1connstring = connectionString1Box.Text;
            Properties.Settings.Default.Save();
            conn1stats.Text = "Saved";
        }

        private void commitDb2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.db2connstring = connectionString2Box.Text;
            Properties.Settings.Default.Save();
            conn2stats.Text = "Saved";
        }

        private void manualCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.autostart = autostartCheck.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
