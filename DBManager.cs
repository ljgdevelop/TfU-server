using MySql.Data.MySqlClient;
using System.Data;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    if(query.LastIndexOf(';') != query.Length -1)
                        query += ";";

                    int result = cmd.ExecuteNonQuery();
                    Console.WriteLine("[{0}] Query Executed: type={2} result={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), result, query.Split(" ")[0]);

                    mariaDB.Close();
                }
                catch (Exception ex)
                {
                    mariaDB.Close();
                    Console.WriteLine("[{0}] error: {1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), ex.ToString());
                }
            }
        }

        public void ExecuteQueryNoSpace(string query, bool debug)
        {
            using (MySqlConnection mariaDB = OpenMariaDB())
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, mariaDB);
                    if(query.LastIndexOf(';') != query.Length -1)
                        query += ";";

                    int result = cmd.ExecuteNonQuery();
                    if(debug)
                        Console.Write("[{0}] Query Executed: type={2} result={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), result, query.Split(" ")[0]);

                    mariaDB.Close();
                }
                catch (Exception ex)
                {
                    mariaDB.Close();
                    Console.WriteLine("[{0}] error at query: {1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), query);
                }
            }
        }

        public void insertWaypointIntoDB(){
            Console.WriteLine("[{0}] 로드 시작.", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"));
            string path = @"/home/ubuntu/TfU-server/dataset/waypoints.json";
            if(File.Exists(path)){
                String file = File.ReadAllText(path, Encoding.Default).Replace(@"'", "u0027");
                JArray? jArr = JObject.Parse(file)["items"] as JArray;
                int progress = 0;
                if(jArr != null)
                foreach(JObject jObj in jArr){
                    drawTextProgressBar(++progress, jArr.Count);

                    //임시 테이블에 삽입
                    StringBuilder insertQuery = new StringBuilder("Insert Into WAYPOINT Values(");
                    insertQuery.Append(jObj["id"]?.Value<string>() ?? "0");
                    insertQuery.Append(@", '");
                    insertQuery.Append(jObj["name"]?.Value<string>() ?? "");
                    insertQuery.Append(@"', ");
                    insertQuery.Append(jObj["posX"]?.Value<string>() ?? "0.0");
                    insertQuery.Append(@", ");
                    insertQuery.Append(jObj["posY"]?.Value<string>() ?? "0.0");
                    insertQuery.Append(@", ");
                    insertQuery.Append(jObj["rating"]?.Value<string>() ?? "0");
                    insertQuery.Append(@", ");
                    insertQuery.Append(jObj["reviewCount"]?.Value<string>() ?? "0");
                    insertQuery.Append(@", ");
                    insertQuery.Append(jObj["type"]?.Value<string>() ?? "0");
                    insertQuery.Append(@", '");
                    insertQuery.Append(jObj["originLink"]?.Value<string>() ?? "");
                    insertQuery.Append(@"', ");
                    insertQuery.Append(jObj["time"]?.Value<string>() ?? "0");
                    insertQuery.Append(@") ON DUPLICATE KEY UPDATE name = '");

                    insertQuery.Append(jObj["name"]?.Value<string>() ?? "");
                    insertQuery.Append(@"', posX = ");
                    insertQuery.Append(jObj["posX"]?.Value<string>() ?? "0.0");
                    insertQuery.Append(@", posY = ");
                    insertQuery.Append(jObj["posY"]?.Value<string>() ?? "0.0");
                    insertQuery.Append(@", rating = ");
                    insertQuery.Append(jObj["rating"]?.Value<string>() ?? "0");
                    insertQuery.Append(@", reviewCount = ");
                    insertQuery.Append(jObj["reviewCount"]?.Value<string>() ?? "0");
                    insertQuery.Append(@", type = ");
                    insertQuery.Append(jObj["type"]?.Value<string>() ?? "0");
                    insertQuery.Append(@", originLink = '");
                    insertQuery.Append(jObj["originLink"]?.Value<string>() ?? "");
                    insertQuery.Append(@"', time = ");
                    insertQuery.Append(jObj["time"]?.Value<string>() ?? "0");
                    insertQuery.Append(@";");
                    DBManager.Instance.ExecuteQueryNoSpace(insertQuery.ToString(), false);
                }
                Console.WriteLine("[{0}] 관광지 목록 로드 완료 : Waypoint Count={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), DBManager.Instance.Select(@"Select id from WAYPOINT;").Tables[0].Rows.Count);
            }
        }

        /*
        * https://m.blog.naver.com/PostView.naver?isHttpsRedirect=true&blogId=gboarder&logNo=90015857948
        *
        */
        private void drawTextProgressBar(int progress, int total)
        {
            int percent = progress * 100 / total;
            if(progress < total - 1){
                Console.CursorVisible = false;

                int count = Console.CursorLeft;
                for(int i = 0; i < count; i++)
                    Console.Write("\b");

                Console.Write("[");
                for(int i = 0; i < 10; i++)
                    if(i < percent / 10)
                        Console.Write("=");
                    else
                        Console.Write(" ");
                Console.Write("] ");

                Console.Write("{0}%, {1}files", percent, progress);
            }
            else{
                Console.CursorVisible = true;

                int count = Console.CursorLeft;
                for(int i = 0; i < count; i++)
                    Console.Write("\b");
            }
            
        }
    }
}