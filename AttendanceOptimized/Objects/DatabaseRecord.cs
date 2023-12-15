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

        public DatabaseRecord()
        {
            todayIN = todayOUT = yesterdayIN = yesterdayOUT = String.Empty;
        }

        public void addIN(DateTime date, String IN, DateTime dateUsed)
        {
            if(date.Date == dateUsed.Date)
            {
                todayIN = IN;
            }
            else if(date.Date == dateUsed.Date - TimeSpan.FromDays(1))
            {
                yesterdayIN = IN;
            }
        }

        public void addOUT(DateTime date, String OUT, DateTime dateUsed)
        {
            if (date.Date == dateUsed.Date)
            {
                todayOUT = OUT;
            }
            else if (date.Date == dateUsed.Date - TimeSpan.FromDays(1))
            {
                yesterdayOUT = OUT;
            }
        }
        public void AddRoster(DateTime date, String roster, DateTime dateUsed)
        {

            if (date.Date == dateUsed.Date)
            {
                todayRoster = roster;
            }
            else if (date.Date == dateUsed.Date - TimeSpan.FromDays(1))
            {
                yesterdayRoster = roster; 
            }
        }

    }


}
