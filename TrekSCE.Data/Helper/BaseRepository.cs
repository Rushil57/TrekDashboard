using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using Dapper;
using System.IO;
using System.Text;


namespace SCE.Data.Helper
{
    public class BaseRepository
    {
        private bool connection_open;
        protected MySqlConnection connection;
        
        protected MySqlConnection Get_Connection()
        {
            connection_open = false;
            connection = new MySqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["connstring"].ConnectionString; 
                //.GetConnectionString("SCE_ConnectionString");
            if (Open_Connection())
            {
                connection_open = true;
                return connection;
            }
            else
            {
                throw new Exception("Error");
            }
        }

        private bool Open_Connection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
