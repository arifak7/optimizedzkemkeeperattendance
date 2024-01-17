using AttendanceOptimized.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AttendanceOptimized
{
    
    public partial class Main : Form
    {
        public Dictionary<DateTime, String> errorLogs;
        ErrorLog errorlog;
        public List<Machine> Machines;
        bool dateChangeAutomated;
        Dictionary<String, List<String>> dataInTable;
        List<String> displayIndex = new List<String>() {"Null", "Not in DB", "New Data", "Inputting", "Valid" };
        Dictionary<String, Color> colorMapping = new Dictionary<string, Color> { { "Null", Color.Black }, { "Not in DB", Color.Red }, { "New Data", Color.Red }, { "Inputting", Color.Blue }, { "Valid", Color.Green } };
        Thread dataThread;
        Thread resetCheckerThread;
        DatabaseSettings databaseSettings;
        Database database;
        Random random;
        public DateTime dateUsed;
        Dictionary<String, Color> colorRef = new Dictionary<string, Color> { { "Connected", Color.Green }, {"Disconnected",Color.Red },
            {"Trying...", Color.Blue }, {"Idle", Color.Black } };
        bool automated;

        public Main()
        {
            InitializeComponent();
            init();
        }


        //<===========================  INITS   ===========================>
        //Initialization
        public void init()
        {
            initVariables();
            getSavedMachines();
        }
        private void initVariables()
        {
            random = new Random();
            errorLogs = new Dictionary<DateTime, string>();
            dateChangeAutomated = false;
            automated = true;
            dataInTable = new Dictionary<String, List<String>>();
            dateUsed = date_used.Value.Date;
        }
        //machine init first object creation
        public void initMachines()
        {
            Machines = new List<Machine>();
            foreach (DataGridViewRow row in device_table.Rows)
            {
                String Name = row.Cells[0].Value as String;
                String ipAddr = row.Cells[1].Value as String;
                if (Name != null && ipAddr != null)
                {
                    Machine machine = new Machine(ipAddr, Name, this);
                    Machines.Add(machine);
                }
            }
        }

        //<===========================  TRIGGERS   ===========================>

        //get from properties
        private void getSavedMachines()
        {
            for(int i=0; i<Properties.Settings.Default.ipaddress.Count; i++)
            {
                device_table.Rows.Add(Properties.Settings.Default.machinename[i], Properties.Settings.Default.ipaddress[i]);
            }
        }
        //trigger when device connects
        private void startConnectionChecker()
        {
            initMachines();
            foreach (Machine machine in Machines)
            {
                machine.startMachine();
                Thread.Sleep(100);
            }
            dataThread = new Thread(() => startDeviceThread()) { IsBackground = true };
            dataThread.Start();
        }

        //when changing date
        private void changeDate(object sender, EventArgs e)
        {
        }

        //when new Entry of machine entered, check if its valid (both ip and name must present)
        private void dataUpdated(object sender, DataGridViewCellEventArgs e)
        {
            saveDeviceChange();
        }

        //trigger on Enter?
        private void device_modified(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                saveDeviceChange();
            }
        }

        //add Row
        private void addRow(object sender, EventArgs e)
        {
            device_table.Rows.Add();
        }

        //delete Row
        private void deleteRow(object sender, EventArgs e)
        {
            device_table.Rows.RemoveAt(device_table.SelectedCells[0].RowIndex);
            saveDeviceChange();
        }

        //save changes
        private void saveDeviceChange()
        {
            Properties.Settings.Default.ipaddress.Clear();
            Properties.Settings.Default.machinename.Clear();
            Properties.Settings.Default.enabled.Clear();
            foreach (DataGridViewRow row in device_table.Rows)
            {
                String Nama = row.Cells[0].Value as String;
                String ipDevice = row.Cells[1].Value as String;
                if (Nama != null && ipDevice != null)
                {
                    Properties.Settings.Default.ipaddress.Add(ipDevice);
                    Properties.Settings.Default.machinename.Add(Nama);
                }
            }
            Properties.Settings.Default.Save();
        }



        //Button enable disable config
        private void enableConnectButtonStatus(bool enable)
        {
            date_used.Enabled = !enable;
            manualCheckBox.Enabled = !enable;
            addButton.Enabled = !enable;
            deleteButton.Enabled = !enable; 
            device_table.ReadOnly = enable;
            connectToolStripMenuItem.Enabled = !enable;
            disconnectToolStripMenuItem.Enabled = enable ;
        }

        //<===========================  Thread   ===========================>

        
        //reset with new Date (this automatically checked in Machines)
        private void resetWithNewDate()
        {
            stopDeviceThread();
            startConnectionChecker();
            enableConnectButtonStatus(true);
            renewDatabase();
        }
        private void renewDatabase()
        {
            database = new Database(this);
            database.start();
        }
        //connecting or disconnecting
        private void Connect_device(object sender, EventArgs e)
        {
            startEverything(true);
        }
        private void startDeviceThread()
        {
            int loop = 0;
            while (true)
            {
                try
                {
                    loop = refreshCheck(loop);
                    if (dateChangeAutomated)
                    {
                        dateChecker();
                    }
                    checkDeviceConnection();
                    updateDataTable();
                    updateErrorCounter();
                    Thread.Sleep(1200);
                }
                catch(Exception e)
                {
                    addErrorLog("Device Thread display loop error");
                    continue;
                }
            }
        }
        private void updateErrorCounter()
        {
            try
            {
                errorCount.Invoke(new Action(delegate ()
                {
                    errorCount.Text = errorLogs.Count.ToString();
                }));
            }
            catch(Exception e)
            {
                addErrorLog("Error Counter Error");
            }
        }

        private void startEverything(bool everything)
        {
            dateChangeAutomated = manualCheckBox.Checked;
            karyawan_table.Invoke(new Action(delegate ()
            {
                karyawan_table.Rows.Clear();
            }));
            dataInTable.Clear();
            dateUsed = date_used.Value.Date;
            startConnectionChecker();
            if (everything)
            {
                startDB();
            }
            //startResetChecker();
            enableConnectButtonStatus(true);
        }
        private void startDB()
        {
            Thread.Sleep(1000);
            database = new Database(this);
            database.start();

        }
        private void Disconnect_device(object sender, EventArgs e)
        {
            stopDeviceThread();
            if (database != null)
            {
                database.stop();
            }
            enableConnectButtonStatus(false);
        }

        //runtime thread for device conn/data checker
       

        //checker for refresh data, if loop is a certain value,
        //it will erase record in machines object and clear table and dataintable checker
        private int refreshCheck(int loop)
        {
            if (loop == 120)
            {
                dateUsed = date_used.Value.Date;
                if (Properties.Settings.Default.autoloop)
                {
                    timeControl.Invoke(new Action(delegate ()
                    {
                        timeControl.Value = 350;
                    }));
                }
                //reset();
                return 0;
            }
            return loop += 1;
        }

        private void dateChecker()
        {
            date_used.Invoke(new Action(delegate ()
            {
                date_used.Value = DateTime.Now.Date;
            }));

            if (dateUsed.Date != date_used.Value.Date)
            {
                dateUsed = date_used.Value.Date;
                restartProgram();
                reset();
            }
        }

        private void reset()
        {
            
            foreach(Machine machine in Machines)
            {
                machine.stopConnection();
                machine.startConnection();
            }
            database.stop();
            database.start();
            dataInTable.Clear();
            karyawan_table.Invoke(new Action(delegate ()
            {
                karyawan_table.Rows.Clear();
            }));

        }

        //functions-calls shorten
        private void checkDeviceConnection()
        {
            foreach(DataGridViewRow row in device_table.Rows)
            {
                if (row != null)
                {
                    row.Cells[2].Value = Machines[Machines.FindIndex(x => x.DeviceName == row.Cells[0].Value.ToString())].Status;
                }
            }
        }
        public void addErrorLog(String message)
        {
            try
            {
                if (!errorLogs.ContainsKey(DateTime.Now))
                {
                    errorLogs.Add(DateTime.Now, message);
                }
                else
                {
                    errorLogs.Add(DateTime.Now + TimeSpan.FromSeconds(random.Next(1,999)), message + "(Retried)"); ;
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(1000);
                addErrorLog(message);
            }

        }
        //get data from main record
        private void updateDataTable()
        {
            foreach(Machine machine in Machines)
            {
                foreach (Karyawan karyawan in machine.Record.Values)
                {
                    try
                    {
                        insertToKaryawanTable(karyawan);
                    }
                    catch (Exception e)
                    {
                        addErrorLog("Update Data Table Error");
                        continue;
                    }                
                }
            }
        }


        //checker for data and presentation
        private void insertToKaryawanTable(Karyawan karyawan)
        {
            try
            {
                String Roster = (karyawan.Roster != null) ? karyawan.Roster : " ";
                String IN = (karyawan.inTime != DateTime.MaxValue) ? karyawan.inTime.ToString("HH:mm") : " ";
                String OUT = (karyawan.outTime != DateTime.MinValue) ? karyawan.outTime.ToString("HH:mm") : (karyawan.prevOutTime != DateTime.MinValue) ? karyawan.prevOutTime.ToString("HH:mm") : " ";
                karyawan_table.Invoke(new Action(delegate ()
                {
                    if (!(IN == " " && OUT == " "))
                    {
                        try
                        {
                            if (!dataInTable.ContainsKey(karyawan.NIK))
                            {

                                addNewRow(karyawan, IN, OUT, displayIndex[karyawan.INValidation], displayIndex[karyawan.OUTValidation], displayIndex[karyawan.prevOUTValidation], Roster);
                            }
                            else
                            {
                                int index = findIndexInTable(karyawan.NIK);
                                if (index >= 0)
                                {
                                    updateRow(index, IN, OUT, displayIndex[karyawan.INValidation], displayIndex[karyawan.OUTValidation], displayIndex[karyawan.prevOUTValidation], Roster);
                                }
                                else
                                {
                                    addNewRow(karyawan, IN, OUT, displayIndex[karyawan.INValidation], displayIndex[karyawan.OUTValidation], displayIndex[karyawan.prevOUTValidation], Roster);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            addErrorLog("Karyawan Table failed");
                            return;
                        }
                    }
                }));

            }
            catch(Exception e)
            {
                addErrorLog("Karyawan Table insert failure");           
            }
        }

        //table to update Row
        private void updateRow(int index, String IN, String OUT, String inValid, String outValid, String prevOutValid, String Roster)
        {
            try
            {
                karyawan_table.Rows[index].Cells["IN_karyawan"].Value = IN;
                karyawan_table.Rows[index].Cells["OUT_karyawan"].Value = OUT;
                karyawan_table.Rows[index].Cells["in_valid"].Value = inValid;
                karyawan_table.Rows[index].Cells["out_valid"].Value = outValid;
                karyawan_table.Rows[index].Cells["prevout_valid"].Value = prevOutValid;
                karyawan_table.Rows[index].Cells["roster_karyawan"].Value = Roster;

            }
            catch(Exception ex)
            {
                addErrorLog("Table Update Failure");
            }
        }

        //table add new row
        private void addNewRow(Karyawan karyawan, String IN, String OUT, String inValid, String outValid, String prevOutValid, String Roster)
        {
            try
            {
                karyawan_table.Rows.Add(karyawan.NIK, karyawan.Nama, IN, OUT, karyawan.Device, inValid, outValid, prevOutValid, Roster);
                List<String> dataTime = new List<String>() { IN, OUT };
                dataInTable.Add(karyawan.NIK, dataTime);
            }
            catch(Exception ex)
            {
                addErrorLog("New Row Failure");
            }
        }

        //clear karyawan table and refresh data
        private void refreshData()
        {
            dataInTable.Clear();
            karyawan_table.Invoke(new Action(delegate ()
            {
                karyawan_table.Rows.Clear();
            }));
            foreach(Machine machine in Machines)
            {
                machine.refreshData();
            }
            if (database != null)
            {
                database.DBRecord.Clear();
                database.stop();
                database = new Database(this);
                database.start();
            }
        }
        //check index manually because its sortable
        private int findIndexInTable(String NIK)
        {
            try
            {
                if (karyawan_table.RowCount > 0)
                {
                    for (int i = 0; i < karyawan_table.RowCount; i++)
                    {
                        if (karyawan_table.Rows[i].Cells["NIK_karyawan"].Value.ToString().Equals(NIK))
                        {
                            return i;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return -1;
            }
            return -1;
        }
        //clear Selected device
        private void clearSelectedDevice(object sender, EventArgs e)
        {
            String DeviceName = device_table.Rows[device_table.SelectedCells[0].RowIndex].Cells[0].Value.ToString();
            Machine machine = Machines.Single(x=> x.DeviceName == DeviceName);
            //machine.clearLog();
        }

        //STOP ALL ACTIVITIES
        private void stopDeviceThread()
        {
            karyawan_table.Invoke(new Action(delegate ()
            {
                karyawan_table.Rows.Clear();
            }));
            dataInTable.Clear();
            if (database != null) database.stop();
            if (Machines != null)
            {
                foreach (Machine machine in Machines)
                {
                    machine.stopConnection();
                }
            }

            stopDataThread();

        }

        //<===========================  DATABASE   ===========================>
        private void openDatabaseSettings(object sender, EventArgs e)
        {
            databaseSettings = new DatabaseSettings();
            databaseSettings.Show();
        }

        private void connectDatabase(object sender, EventArgs e)
        {
            database = new Database(this);
            database.start();
        }

        //<===========================  FORM CLEANUP   ===========================>
        private void formClosing(object sender, FormClosingEventArgs e)
        {
            stopDeviceThread();
        }

        private void stopDataThread()
        {
            if(dataThread != null)
            {
                dataThread.Abort();
            }
        }

        //Paint Event for device table
        private void deviceCellPaint(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex < 2)
            {
                return;
            }
            if (e.Value!=null)
            {
                if (colorRef.ContainsKey(e.Value.ToString()))
                {
                    device_table.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = colorRef[e.Value.ToString()];
                    device_table.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.SelectionForeColor = colorRef[e.Value.ToString()];
                }
            }
        }

        private void cellPaint(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if(e.ColumnIndex>=5 && e.ColumnIndex <= 7 && e.RowIndex>-1)
            {
                if (e.Value != null)
                {
                    e.CellStyle.ForeColor = colorMapping[e.Value.ToString()];
                    e.CellStyle.SelectionForeColor = colorMapping[e.Value.ToString()];
                }
            }
        }

        private void changeManual(object sender, EventArgs e)
        {
            dateChangeAutomated = !manualCheckBox.Checked;
        }

        private void firstShown(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.autostart)
            {
                startEverything(true);
            }
        }

        private void errorLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (errorlog != null)
            {
                errorlog.Close();
            }
            errorlog = new ErrorLog(errorLogs);
            errorlog.Show();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void restartDevice(object sender, EventArgs e)
        {
            restartProgram();
        }
        public void restartProgram()
        {
            Application.Restart();
            Environment.Exit(0);
        }
    }
}
