using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceOptimized.Objects
{
    public class DatabaseRecord
    {
        public String todayIN;
        public String todayOUT;
        public String yesterdayIN;
        public String yesterdayOUT;
        public String todayRoster;
        public String yesterdayRoster;
        public DateTime IN;
        public DateTime OUT;
        public DateTime prevOUT;
        public DateTime prevIN;
        public DatabaseRecord()
        {
            todayIN = todayOUT = yesterdayIN = yesterdayOUT = String.Empty;
            IN = DateTime.MaxValue;
            prevIN = DateTime.MaxValue;
            OUT = DateTime.MinValue; 
            prevOUT = DateTime.MinValue;
        }

        public void addIN(DateTime date, String IN, DateTime dateUsed)
        {
            if(date.Date == dateUsed.Date)
            {
                todayIN = (IN is null) ? "" : IN;
                this.IN = date;
            }
            else if(date.Date == dateUsed.Date - TimeSpan.FromDays(1))
            {
                yesterdayIN = (IN is null) ? "" : IN;
                this.prevIN = date;
            }
        }

        public void addOUT(DateTime date, String OUT, DateTime dateUsed)
        {
            if (date.Date == dateUsed.Date)
            {
                todayOUT = (OUT is null) ? "" : OUT;
                this.OUT = date;
            }
            else if (date.Date == dateUsed.Date - TimeSpan.FromDays(1))
            {
                yesterdayOUT = (OUT is null)? "": OUT;
                this.prevOUT = date;
            }
        }
        public void AddRoster(DateTime date, String roster, DateTime dateUsed)
        {

            if (date.Date == dateUsed.Date)
            {
                todayRoster = (roster is null)? "": roster;
            }
            else if (date.Date == dateUsed.Date - TimeSpan.FromDays(1))
            {
                yesterdayRoster =(roster is null)?"": roster; 
            }
        }

    }


}
