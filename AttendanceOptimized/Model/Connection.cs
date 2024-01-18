using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Attendance;
using System.Data.SqlClient;

namespace Attendance
{
    public class Connection
    {
        public SqlConnection conn { get; set; }
        public String connstring { get; set; }
        public String state;

        public Connection()
        {
            setConnString();
            this.conn = new SqlConnection(connstring);
        }
        public Connection(String connString)
        {
            this.connstring = connString;
            this.conn = new SqlConnection(connstring);
        }
        public Connection(String dbServer, String username, String password, String initCatalog)
        {
            setConnString(dbServer, username, password, initCatalog);
            this.conn = new SqlConnection(connstring);
        }

        public void setConnString()
        {
            String dbServer = @"10.2.168.119";
            String username = "DBUNAME";
            String password = "DBPASSWORD";
            String initCatalog = "DBCATALOG";
            setConnString(dbServer, username, password, initCatalog);  
        }
        public void setConnString(String dbServer, String username, String password, String initCatalog)
        {
            connstring = "Data source=" + dbServer +";"+ "initial catalog=" + initCatalog + ";" + "user id=" + username +";" + "password=" + password + ";TrustServerCertificate=true";
        }

        public void open()
        {
            try
            {
                state = "Closed";
                conn.Open();
                updateState();
            }
            catch (Exception e)
            {
                updateState();
            }
            
        }
        public void updateState()
        {
            state = conn.State.ToString();
        }

        public String refreshConnection()
        {
            conn = new SqlConnection(connstring);
            try
            {
                state = "Closed";
                conn.Open();
                updateState();
            }
            catch (Exception ex)
            {
                updateState();
                return ex.Message;
            }
            return conn.State.ToString();
        }
        public void close()
        {
            if (conn.State.ToString() == "Open")
            {
                conn.Close();
                updateState();
            }
        }

        public SqlDataReader Reader(String query)
        {
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                return reader;            
        }

        public String executeQuery(String query)
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            return cmd.ExecuteNonQuery().ToString();
        }
        

    }
}
