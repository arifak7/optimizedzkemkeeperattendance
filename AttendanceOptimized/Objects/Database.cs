using Attendance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
                    main.addErrorLog("Database Timeout");
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
            try
            {

            }
            catch(Exception ex)
            {

            }
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
                crossRefrenceMachine(machine);
            }

        }

        private void crossRefrenceMachine(Machine machine)
        {
            foreach(Karyawan karyawan in machine.Record.Values.ToList())
            {
                String NIK = karyawan.NIK;
                if (!DBRecord.ContainsKey(NIK))
                {
                    continue;
                }
                karyawan.prevOUTValidation = validatePrevOut(karyawan, NIK);
                karyawan.OUTValidation = validateOUT(karyawan, NIK);
                karyawan.INValidation = validateIN(karyawan, NIK);
            }
        }
        private int validateOUT(Karyawan karyawan, String NIK)
        {
            DatabaseRecord karyawanDB = DBRecord[NIK];
            int validation = 0;
            if (karyawanDB.todayOUT == String.Empty  )
            {
                if(karyawan.outTime != DateTime.MinValue)
                {
                    return 1;
                }
                else if(karyawan.prevOUTValidation == 5 && karyawan.prevOutTime!=DateTime.MinValue)
                {
                    karyawan.outTime = karyawan.prevOutTime;
                    return 1;
                }
            }
            else
            {
                validation = (replaceTimeFormat(karyawanDB.todayOUT) !=
                    karyawan.outTime.ToString("HH:mm") && karyawan.retriedOUT < 5) ? 2 : 4;
                if (validation == 2) karyawan.retriedOUT++;
            }
            return validation;
        }
        private int validateIN(Karyawan karyawan, String NIK)
        {
            DatabaseRecord karyawanDB = DBRecord[NIK];
            int validation = 0;
            if (karyawanDB.todayIN == String.Empty )
            {
                if (karyawan.inTime != DateTime.MaxValue)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                validation = (replaceTimeFormat(karyawanDB.todayIN) !=
                    karyawan.inTime.ToString("HH:mm") && karyawan.retriedIN < 5) ? 2 : 4;
                if (validation == 2) karyawan.retriedIN++;
            }

            return validation;
        }
        private String replaceTimeFormat(String time)
        {
            return time.Replace(".", ":");
        }

        private int validatePrevOut(Karyawan karyawan, String NIK)
        {
            DatabaseRecord karyawanDB = DBRecord[NIK];
            int validation = 0;
            if(karyawanDB.yesterdayOUT != String.Empty)
            {
                return 4;
            }
            else 
            {
                validation = (karyawan.prevOutTime != DateTime.MinValue) ? 1 : 0;
                if (validation == 1)
                {
                    validation = (validShisft2.Contains(karyawan.prevRoster) 
                        || getHour(karyawanDB.yesterdayIN) > 16) ? 1 : 5;
                }
            }
            if (karyawan.prevOutTime.ToString("HH:mm") == replaceTimeFormat(karyawanDB.yesterdayOUT))
            {
                validation = 4;
            }

            return validation;
        }

        private int getHour(String hour)
        {
            if (hour == String.Empty) return 0;
            try
            {
                String[] delimiters = { ":", "." };
                return Convert
                    .ToInt32(hour
                    .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)[0]);
            }
            catch (Exception ex)
            {
                return 0;
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
                        int prevInHour = 0;
                      
                        karyawan.Roster = DBRecord[NIK].todayRoster;
                        karyawan.prevRoster = DBRecord[NIK].yesterdayRoster;
                        karyawan.INValidation = (DBRecord[NIK].todayIN == karyawan.inTime.ToString("HH:mm")) ? 4
                            : (DBRecord[NIK].todayIN != String.Empty && karyawan.inTime != DateTime.MaxValue) ? 2
                            : (karyawan.inTime == DateTime.MaxValue) ? 0 : 1;
                        karyawan.prevOUTValidation =
                            (DBRecord[NIK].yesterdayOUT != String.Empty) ? 4 :
                            /*
                            (DBRecord[NIK].yesterdayOUT != String.Empty && 
                            karyawan.prevOutTime != DateTime.MinValue &&
                            karyawan.prevOutTime.ToString("HH:mm")!= DBRecord[NIK].yesterdayOUT &&
                            validShisft2.Contains(karyawan.prevRoster)) ? 2 :
                            */
                            (DBRecord[NIK].yesterdayIN != String.Empty &&
                            DBRecord[NIK].yesterdayOUT == String.Empty &&
                            karyawan.prevOutTime != DateTime.MinValue &&
                            (karyawan.prevOutTime.ToString("HH:mm") != DBRecord[NIK].yesterdayOUT ||
                            karyawan.prevOutTime.ToString("HH.mm") != DBRecord[NIK].yesterdayOUT) &&
                            prevInHour > 15
                            ) ? 2 :
                            (DBRecord[NIK].yesterdayOUT == String.Empty &&
                            karyawan.prevOutTime != DateTime.MinValue &&
                            (karyawan.prevOutTime.ToString("HH:mm") != DBRecord[NIK].yesterdayOUT ||
                            karyawan.prevOutTime.ToString("HH.mm") != DBRecord[NIK].yesterdayOUT) &&
                            validShisft2.Contains(karyawan.prevRoster)
                            ) ? 2 :
                            (karyawan.prevOutTime == DateTime.MinValue) ? 0 : 5;
                        if (karyawan.prevOUTValidation != 5)
                        {
                            karyawan.OUTValidation = (DBRecord[NIK].todayOUT == karyawan.outTime.ToString("HH:mm")) ? 4
                            : (DBRecord[NIK].todayOUT != String.Empty && karyawan.outTime != DateTime.MinValue) ? 2
                            : (karyawan.outTime == DateTime.MinValue) ? 0 : 1;
                        }
                        else
                        {
                            karyawan.OUTValidation = (karyawan.OUTValidation == 0) ? 2 : karyawan.OUTValidation;
                        }
                        if (karyawan.OUTValidation==3 )
                        {
                            karyawan.OUTValidation = (DBRecord[NIK].todayOUT != String.Empty) ? 4 : karyawan.OUTValidation;
                        }
                        if (karyawan.INValidation == 3)
                        {
                            karyawan.INValidation = (DBRecord[NIK].todayOUT != String.Empty) ? 4 : karyawan.INValidation;
                        }
                        if (karyawan.prevOUTValidation == 3)
                        {
                            karyawan.prevOUTValidation = (DBRecord[NIK].yesterdayOUT != String.Empty) ? 4 : karyawan.prevOUTValidation;
                        }
                        /*if(karyawan.INValidation == 2)
                          {
                              MessageBox.Show(karyawan.NIK + "(" + karyawan.inTime.ToString() + karyawan.outTime.ToString() + karyawan.prevOutTime.ToString()+")" + DBRecord[NIK].todayIN + DBRecord[NIK].todayOUT + DBRecord[NIK].yesterdayIN + DBRecord[NIK].yesterdayOUT);
                          }*/
                        // if (karyawan.prevOUTValidation == 2) MessageBox.Show(NIK+"|"+DBRecord[NIK].yesterdayIN +"|"+ DBRecord[NIK].yesterdayOUT +"|"+ prevInHour+"|"+ DBRecord[NIK].yesterdayRoster);
                    }
                    else
                    {
                        karyawan.INValidation = karyawan.OUTValidation = karyawan.prevOUTValidation = 1;
                    }

                }
                catch(Exception e)
                {
                    main.addErrorLog("Database Connection Error");
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
                main.addErrorLog("Connection open error");
            }
            try
            {
                newDB.conn.Open();
            }
            catch (Exception ex)
            {
                main.addErrorLog("Connection open error");
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
            try
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
            catch(Exception ex)
            {
                main.addErrorLog("DB String CONN update Error");
                return;
            }
        }
    }


}
