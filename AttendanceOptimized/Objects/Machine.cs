using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using zkemkeeper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace AttendanceOptimized.Objects
{
    public class Machine
    {
        List<int> entryValid = new List<int>() { 0, 3, 4 };
        public Dictionary<String,Karyawan> Record;
        public String IPAddress;
        public String DeviceName;
        public String Status;
        Dictionary<String, String> nameRef;
        public zkemkeeper.CZKEMClass device;
        Main _Main;
        public Thread machineThread;
        DateTime dateUsed;

        public Machine(String IPAddress, String Name, Main main)
        {
            this.IPAddress = IPAddress;
            this.DeviceName = Name;
            this._Main = main;
            this.Status = "Idle";
            nameRef = new Dictionary<String, String>();
            device = new zkemkeeper.CZKEMClass();
            Record = new Dictionary<String, Karyawan>();
            this.dateUsed = main.dateUsed;
        }

        public void startMachine()
        {
            startConnection();
        }


        //<===========================  Zkem Interaction   ===========================>

        private void readUserInfoFromMachine()
        {
            device.ReadAllUserID(1);
            int machineNumber = 1;
            string enroll = ""; string name = ""; string password = "";
            int privilige = 0; bool enabled = false;
            while (device.SSR_GetAllUserInfo(machineNumber, out enroll, out name, out password, out privilige, out enabled))
            {
               if (!nameRef.ContainsKey(enroll)) nameRef.Add(enroll, name);
            }
        }
        private void readDataFromMachine(zkemkeeper.CZKEM device)
        {
            device.ReadGeneralLogData(1);
            
            String enroll = "";
            int verify = 0; int inOut = 0; int year = 0; int month = 0; int workCode = 0;
            int day = 0; int hour = 0; int minute = 0; int second = 0;
            while (device.SSR_GetGeneralLogData(0, out enroll, out verify, out inOut, out year, out month, out day, out hour, out minute, out second, ref workCode))
            {                
                DateTime time = new DateTime(year, month, day, hour, minute, second);
                if (time.Date == dateUsed.Date)
                {
                    try
                    {
                        insertRecord(enroll, inOut, time, DeviceName);
                    }
                    catch (Exception e)
                    {
                        _Main.errorLogs.Add(DateTime.Now, e.Message + " (Machine - Read Data From Machine)");
                        continue;
                    }
                }
            }
        }


        //<===========================  Runtime   ===========================>

        //Shortened-functions / Record Interactions (Optimize this)
        private void insertRecord(String enroll, int inOut, DateTime time, String DevName)
        {
            if (!Record.ContainsKey(enroll))
            {
                String nama = (nameRef.ContainsKey(enroll)) ? nameRef[enroll] : " "; 
                Record.Add(enroll, new Karyawan(enroll, nama));
            }
            String entryType = (entryValid.Contains(inOut)) ? "IN" : "OUT";            
            Record[enroll].AddTime(time, entryType, DeviceName);            
        }

        //Thread Init
        public void startConnection()
        {
            machineThread = new Thread(() => connectionThread()) { IsBackground = true };
            machineThread.Start();
        }
        public void stopConnection()
        {
            if(machineThread!= null)
            {
                machineThread.Abort();
            }
            Record.Clear();
            nameRef.Clear();
        }

        //refresh Data in existing table
        public void refreshData()
        {
            Record.Clear();
        }
        private void connectDevice()
        {
            Status = "Trying...";
            bool connection = device.Connect_Net(IPAddress, 4370);
            Status = (connection)? "Connected": "Disconnected";
        }

        private bool isConnected()
        {
            return Status == "Connected";
        }

        public void clearLog()
        {
            if(isConnected())
            {
                device.ClearGLog(1);
            }
        }
        //<===========================  Thread   ===========================>


        private void connectionThread()
        {
            dateUsed = _Main.date_used.Value;
            device = new zkemkeeper.CZKEMClass();
            connectDevice();
            if (isConnected()) readUserInfoFromMachine();
            while (true)
            {
                try
                {
                    connectDevice();
                    if (isConnected())
                    {
                        readDataFromMachine(device);
                    }
                }
                catch(Exception e)
                {
                    _Main.errorLogs.Add(DateTime.Now, e.Message + " (Machine - Connection Thread)");
                    continue;
                }
                Thread.Sleep(Convert.ToInt16(_Main.timeControl.Value)*1000);
            }
        }
        public void initDevice()
        {
            dateUsed = _Main.date_used.Value;
            device = new zkemkeeper.CZKEMClass();
            connectDevice();
            if (isConnected()) readUserInfoFromMachine();
        }
        public void connectLoop()
        {
            connectDevice();
            if (isConnected())
            {
                readDataFromMachine(device);
            }
        }

        


    }
}
