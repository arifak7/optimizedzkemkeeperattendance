using Attendance;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AttendanceOptimized.Objects
{
    internal class Database
    {
        Connection connection;
        Connection newDB;
        Model.Model model;
        Main main;
        Thread thread;
        public Dictionary<String, DatabaseRecord> DBRecord;
        List<String> validShisft2 = new List<String>() { "2", "L2" };
        DateTime dateUsed;
        public Database(Main main)
        {
            this.main = main;
            init();
        }

        private void init()
        {
            DBRecord = new Dictionary<String, DatabaseRecord>();
            connection = new Connection(Properties.Settings.Default.db1connstring);
            newDB = new Connection(Properties.Settings.Default.db2connstring);
            dateUsed = main.dateUsed;
            model = new Model.Model();
        }

        //<===========================  THREAD   ===========================>

        public void start()
        {   
            init();
            stop();
            thread = new Thread(() => DBconnectionChecker());
            thread.Start();
        }

        public void stop()
        {
            if (thread != null)
            {
                thread.Abort();
                connection.close();
                newDB.close();
                DBRecord.Clear();
            }
        }

        private void recordRefresh()
        {
            DBRecord = model.getRecord(newDB, dateUsed);
        }


        private void DBconnectionChecker()
        {
            init();

            connectDatabases();
            while (true)
            {
                try
                {
                    checkConnection();
                    recordRefresh();
                    verifyMainRecord();
                    syncDatabaseDataWithMachine();
                    Thread.Sleep(Convert.ToInt16(main.timeControl.Value) * 1500);
                }
                catch (Exception e)
                {

                    main.errorLogs.Add(DateTime.Now, e.Message + " (Database - DBConnectionChecker)");
                    continue;
                }
            }
        }

        public void initDB()
        {
            init();
            connectDatabases();
        }

        public void loopDB()
        {
            checkConnection();
            recordRefresh();
            verifyMainRecord();
            syncDatabaseDataWithMachine();
        }

        private void syncDatabaseDataWithMachine()
        {
            List<int> validForDB = new List<int>() { 1, 2 };
            foreach (Machine machine in main.Machines)
            {
                foreach(Karyawan karyawan in machine.Record.Values.ToList())
                {
                    if (validForDB.Contains(karyawan.INValidation))
                    {
                        karyawan.INValidation = 3;
                        model.insertRecordToDatabase(connection, karyawan, "IN", "MRB", "H");
                    }
                    if (validForDB.Contains(karyawan.OUTValidation))
                    {
                        karyawan.OUTValidation = 3;
                        model.insertRecordToDatabase(connection, karyawan, "OUT", "MRB", "H");
                    }
                    if (validForDB.Contains(karyawan.prevOUTValidation))
                    {
                        karyawan.prevOUTValidation = 3;
                        model.insertRecordToDatabase(connection, karyawan, "PREVOUT", "MRB", "H");
                    }
                }
            }
        }

        private void verifyMainRecord()
        {
            foreach(Machine machine in main.Machines)
            {
                checkMachineRecordinDB(machine);
            }

        }

        private void checkMachineRecordinDB(Machine machine)
        {
            foreach(Karyawan karyawan in machine.Record.Values.ToList()) 
            {
                try
                {
                    String NIK = karyawan.NIK;
                    if (DBRecord.ContainsKey(NIK))
                    {
                        karyawan.Roster = DBRecord[NIK].todayRoster;
                        karyawan.prevRoster = DBRecord[NIK].yesterdayRoster;
                        karyawan.INValidation = (DBRecord[NIK].todayIN == karyawan.inTime.ToString("HH:mm")) ? 4 : (DBRecord[NIK].todayIN != String.Empty && karyawan.inTime != DateTime.MaxValue) ? 2 : (karyawan.inTime == DateTime.MaxValue) ? 0 : 1;
                        karyawan.OUTValidation = (DBRecord[NIK].todayOUT == karyawan.outTime.ToString("HH:mm")) ? 4 : (DBRecord[NIK].todayOUT != String.Empty && karyawan.outTime != DateTime.MinValue) ? 2 : (karyawan.outTime == DateTime.MinValue) ? 0 : 1;
                        karyawan.prevOUTValidation = ((DBRecord[NIK].yesterdayOUT != String.Empty && validShisft2.Contains(karyawan.prevRoster)) || !validShisft2.Contains(karyawan.prevRoster)) ? 4 : (DBRecord[NIK].yesterdayOUT != String.Empty && karyawan.prevOutTime != DateTime.MinValue && validShisft2.Contains(karyawan.prevRoster)) ? 2 : (karyawan.prevOutTime == DateTime.MinValue) ? 0 : 1;
                        /*if(karyawan.INValidation == 2)
                            {
                                MessageBox.Show(karyawan.NIK + "(" + karyawan.inTime.ToString() + karyawan.outTime.ToString() + karyawan.prevOutTime.ToString()+")" + DBRecord[NIK].todayIN + DBRecord[NIK].todayOUT + DBRecord[NIK].yesterdayIN + DBRecord[NIK].yesterdayOUT);
                            }*/
                    }
                    else
                    {
                        karyawan.INValidation = karyawan.OUTValidation = karyawan.prevOUTValidation = 1;
                    }

                }
                catch(Exception e)
                {
                    main.errorLogs.Add(DateTime.Now, e.Message + " (Database - DBConnectionChecker)");
                    continue;
                }
            }
        }




        //<===========================  CONNECTION CHECKER   ===========================>
        private void connectDatabases()
        {
            try
            {

                connection.conn.Open();
            }
            catch (Exception ex)
            {
                main.errorLogs.Add(DateTime.Now, ex.Message + " (Database - DBConnectionChecker)");
                MessageBox.Show(ex.Message);
            }
            try
            {
                newDB.conn.Open();
            }
            catch (Exception ex)
            {
                main.errorLogs.Add(DateTime.Now, ex.Message + " (Database - DBConnectionChecker)");
                MessageBox.Show(ex.Message);
            }
            main.ipaddr1.Invoke(new Action(delegate ()
            {
                main.ipaddr1.Text =  connection.conn.DataSource;
            }));
            main.ipaddr2.Invoke(new Action(delegate ()
            {
                main.ipaddr2.Text =  newDB.conn.DataSource;
            }));
        }

        private void checkConnection()
        {
            main.db1status.Invoke(new Action(delegate ()
            {
                main.db1status.Text = connection.refreshConnection();
            }));
            main.db2status.Invoke(new Action(delegate ()
            {
                main.db2status.Text = newDB.refreshConnection();
            }));
        }
    }


}
