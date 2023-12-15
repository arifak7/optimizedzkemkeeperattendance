using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceOptimized.Objects
{
    public class Karyawan
    {
        public String NIK;
        public String Nama;
        public String Device;

        public DateTime inTime;
        public DateTime outTime;
        public DateTime prevOutTime;

        public String Roster;
        public String prevRoster;
        

        public int prevOUTValidation;
        public int INValidation;
        public int OUTValidation;  

        // 0 = No Record 1 = Record Not Database  2 = New Record 3 = Processing 4= In Database


        public Karyawan(String NIK, String Nama)
        {
            this.NIK = NIK;
            this.Nama = Nama;
            INValidation = 0;
            OUTValidation = 0;
            prevOUTValidation = 0;
            inTime = DateTime.MaxValue;
            outTime = DateTime.MinValue;
            prevOutTime = DateTime.MinValue;
        }

        public void AddTime(DateTime time, String recordTag, String device)
        {
            this.Device = device;
            recordTag = (recordTag=="OUT" && time.TimeOfDay < TimeSpan.FromHours(9))? "PREVOUT" : recordTag;
            switch (recordTag)
            {
                case "IN":
                    inTime = (time < inTime) ? time : inTime;
                    break;
                case "OUT":
                    outTime = (time > outTime) ? time : outTime;
                    break;
                case "PREVOUT":
                    prevOutTime = (time > prevOutTime) ? time : prevOutTime;
                    break;
                default:
                    break;
            }
        }
    }
}


