using System.Collections.Concurrent;

namespace TFUserver{
    internal class ImageManager{
        private static ImageManager? instance;
        public static ImageManager Instance { get { if(instance == null) instance = new ImageManager(); return instance; } }
        public ConcurrentDictionary<int, string> imageDictionary = new ConcurrentDictionary<int, string>();
        string baseUrl = "https://tripforu.s3.ap-northeast-2.amazonaws.com/";

        private ImageManager(){
            syncStorageToDictionary();
        }

        public string this[int key]
        {
            get { return imageDictionary[key]; }
            set { imageDictionary[key] = baseUrl + value; }
        }

        public int getEmptyKey(){
            using(System.Data.DataSet ds = DBManager.Instance.Select("select * from S3 where status = 0;"))
                foreach(System.Data.DataRow row in ds.Tables[0].Rows)
                    if(row[1].Equals("0")){
                        imageAdded((int) row[0]);
                        return (int) row[0];
                    }
            int id = DBManager.Instance.Select("select * from S3 where status = 1;").Tables[0].Rows.Count;
            imageAdded(id);
            return id;
        }

        private void syncStorageToDictionary(){
            Console.WriteLine("[{0}] 이미지 목록 동기화.", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"));

            imageDictionary = new ConcurrentDictionary<int, string>();

            using(System.Data.DataSet ds = DBManager.Instance.Select("select * from S3 where status = 1;"))
                foreach(System.Data.DataRow row in ds.Tables[0].Rows)
                    if(row[1].Equals("1"))
                        imageDictionary[(int) row[0]] = (int) row[0] + ".jpg";

            Console.WriteLine("[{0}] 이미지 목록 동기화 완료.", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"));
        }

        public void imageAdded(int id){
            imageDictionary[id] = id + ".jpg";

            if(DBManager.Instance.Select("select * from S3 where id = " + id + ";").Tables[0].Rows.Count > 0)
                DBManager.Instance.ExecuteQuery("Update S3 SET status = 1 where id = " + id + ";");
            else
                DBManager.Instance.ExecuteQuery("Insert into S3 values(" + id + ", 1);");

            Console.WriteLine("[{0}] 이미지 추가됨 : {1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), id);
        }
    }
}