using System.Data;
using System.Text;

namespace TFUserver
{
    internal class CommandManager
    {
        public static bool isWorking = false;

        public void startCommandListener(){
            isWorking = true;
            Thread commandThread = new Thread(() => commandListener());
            commandThread.Start();
        }

        private void commandListener(){
            while(isWorking){
                string cmd = Console.ReadLine() + "";
                if( cmd == null)
                    continue;
                
                if(!cmd.Contains(" "))
                    switch(cmd){
                        case "start":
                            Console.WriteLine("[{0}] 서버를 시작합니다.", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"));
                            Server.Instance.serverInit();
                            Server.Instance.startServer();
                            break;
                        case "stop":
                            Console.WriteLine("[{0}] 서버를 종료합니다.", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"));
                            Server.Instance.stopServer();
                            break;
                        case "exit":
                            Console.WriteLine("[{0}] 프로그램을 종료합니다.", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"));
                            Server.Instance.stopServer();
                            isWorking = false;
                            break;
                        case "upload waypoint":
                            Console.WriteLine("[{0}] 관광지 정보를 로드합니다. 파일 위치 = /data", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"));

                            break;
                        default:
                            Console.WriteLine("[{0}] 잘못된 명령어.", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"));
                            break;
                    }

                else if(cmd.StartsWith("send query ")){
                    string query = cmd.Replace("send query ", "");
                    if(query.Split(" ")[0].Equals("select", StringComparison.CurrentCultureIgnoreCase)){
                        Console.WriteLine("[{0}] 쿼리문 실행: {1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), query);
                        DataSet data = DBManager.Instance.Select(query);
                        Console.WriteLine("[{0}] 결과:", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"));
                        Console.WriteLine(data.ToPrettyString());
                    }
                    else{
                        Console.WriteLine("[{0}] 쿼리문 실행: {1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), query);
                        DBManager.Instance.ExecuteQuery(query);
                    }
                }
            }
        }
    }


    /*
    * https://stackoverflow.com/questions/33662631/using-dataset-in-console-application
    * using DataSet in Console Application? - Stack Overflow
    */
    static class ExtensionMethods
    {    
        public static string ToPrettyString(this DataSet ds)
        {
            var sb = new StringBuilder();
            foreach (var table in ds.Tables.ToList())
            {
                sb.AppendLine("--" + table.TableName + "--");
                sb.AppendLine(String.Join("\t | ", table.Columns.ToList()));
                foreach (DataRow row in table.Rows)
                {
                    sb.AppendLine(String.Join("\t | ", row.ItemArray));
                }
            }
            return sb.ToString();
        }

        public static void AddRange(this DataColumnCollection collection, params string[] columns)
        {
            foreach (var column in columns)
            {
                collection.Add(column);
            }
        }       

        public static List<DataTable> ToList(this DataTableCollection collection)
        {
            var list = new List<DataTable>();
            foreach (var table in collection)
            {
                list.Add((DataTable)table);
            }
            return list;
        }

        public static List<DataColumn> ToList(this DataColumnCollection collection)
        {
            var list = new List<DataColumn>();
            foreach (var column in collection)
            {
                list.Add((DataColumn)column);
            }
            return list;
        }
    }
}