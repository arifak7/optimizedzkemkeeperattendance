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
    public partial class ErrorLog : Form
    {
        Dictionary<DateTime, String> errorLog;
        public ErrorLog(Dictionary<DateTime,String> errorLog)
        {
            InitializeComponent();
            this.errorLog = errorLog;
            init();
        }
        private void init()
        {
            foreach(KeyValuePair<DateTime, String> kvp in errorLog)
            {
                error_table.Rows.Add(kvp.Key.ToString("dd/MM/yyyy hh:mm:ss"), kvp.Value);
            }
        }

    }
}
