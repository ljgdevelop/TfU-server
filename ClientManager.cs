using System.Collections.Concurrent;
using System.Text;

namespace TFUserver{
    internal class ClientManager{
        private static ClientManager? instance;
        public static ClientManager Instance { get { if(instance == null) instance = new ClientManager(); return instance; } }
        public ConcurrentDictionary<long, Client> clientDictionary = new ConcurrentDictionary<long, Client>();


        public void addClient(Client clt){
            clientDictionary[clt.uid] = clt;
            StringBuilder query = new StringBuilder("");

            if(DBManager.Instance.Select("select * from USER where uid = " + clt.uid + ";").Tables[0].Rows.Count > 0){
                query.Append("Update USER SET name = '");
                query.Append(clt.name.ToString());
                query.Append("', profileUrl = '");
                query.Append(clt.profileUrl.ToString());
                query.Append("' WHERE uid = ");
                query.Append(clt.uid.ToString());
                query.Append(";");
            }
            else{
                query.Append("Insert into USER values(");
                query.Append(clt.uid.ToString());
                query.Append(", '");
                query.Append(clt.name.ToString());
                query.Append("', '");
                query.Append(clt.profileUrl.ToString());
                query.Append("');");
            }

            DBManager.Instance.ExecuteQuery(query.ToString());
            Console.WriteLine("[{0}] 사용자 추가됨 : query={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), query.ToString());
        }
    }
}