using MySql.Data.MySqlClient;
using System.Data;
using System.Reflection;

namespace TFUserver{
    public class DBManager
    {
        private static DBManager? instance;
        public static DBManager Instance { get { if(instance == null) instance = new DBManager(); return instance; } }

        private MySqlConnection OpenMariaDB()
        {
            string connstr = string.Format("Server={0};Database={1};Uid ={2};Pwd={3};", "127.0.0.1", "tripforu", "root", "tfupw");
            MySqlConnection mariaDB = new MySqlConnection(connstr);
            mariaDB.Open();
            return mariaDB;
        }

        public bool ConnectionTest()
        {
            try
            {
                using (MySqlConnection mariaDB = OpenMariaDB())
                {
                    mariaDB.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        
        public DataSet Select(string query)
        {
            using (MySqlConnection mariaDB = OpenMariaDB())
            {
                try
                {
                    DataSet ds = new DataSet();
                    MySqlDataAdapter dataAdepter = new MySqlDataAdapter(query, mariaDB);
                    dataAdepter.Fill(ds);
                    mariaDB.Close();
                    return ds;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    mariaDB.Close();
                    return new DataSet();
                }
            }
        }

        public void ExecuteQuery(string query)
        {
            using (MySqlConnection mariaDB = OpenMariaDB())
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, mariaDB);

                    int result = cmd.ExecuteNonQuery();
                    Console.WriteLine("[{0}] Query Executed: result={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), result);

                    mariaDB.Close();
                }
                catch (Exception ex)
                {
                    mariaDB.Close();
                    Console.WriteLine("[{0}] error: {1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), ex.ToString());
                }
            }
        }

        public void insertWaypointIntoDB(){
            
        }
    }
}