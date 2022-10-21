using System;
using System.Net;
using Newtonsoft.Json.Linq;

namespace TFUserver
{
    internal class Server
    {
        private static Server? instance;
        public static Server Instance { get { if(instance == null) instance = new Server(); return instance; } }
        HttpListener httpListener = new HttpListener();

        public void serverInit(){
            if(!httpListener.IsListening){
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(string.Format("http://+:6059/"));
                httpListener.Prefixes.Add(string.Format("https://+:6060/"));
                httpListener.AuthenticationSchemes = AuthenticationSchemes.Basic | AuthenticationSchemes.Anonymous;
                ScheduleManager.Instance.syncDBToDictionary();
            }
        }

        public void startServer(){
            if(!httpListener.IsListening){
                string dns = "ec2-3-34-196-61.ap-northeast-2.compute.amazonaws.com";
                Console.WriteLine("[{0}] {1} 주소로 서버를 실행합니다.", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), dns);

                httpListener.Start();

                
                Task.Factory.StartNew(() => {
                    while(httpListener != null){
                        HttpListenerContext context = httpListener.GetContext();

                        string httpMethod = context.Request.HttpMethod;
                        Console.WriteLine("[{0}] 명령 수신: {1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), httpMethod);

                        dataReceived(httpMethod, context);

                        // HttpListenerRequest request = context.Request;
                        // HttpListenerResponse response = context.Response;
                        // string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
                        // byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        //response.ContentLength64 = buffer.Length;
                        //System.IO.Stream output = response.OutputStream;
                        //output.Write(buffer,0,buffer.Length);

                        context.Response.Close();
                    }
                });
            }
        }

        public void stopServer(){
            if(httpListener.IsListening){
                httpListener.Stop();
                httpListener.Close();
            }
        }

        private void dataReceived(string method, HttpListenerContext context){
            switch(method){
                case "GET"://자원 조회 = READ (입력한 값이 쿼리 스트링으로 전달)
                    clientGet(context);
                    break;
                case "POST"://자원 생성 = CREATE (입력한 값이 요청 Body에 담겨 전달, 보안상 더 안전하다, 글자 수 제한이 없다.)
                    clientPost(context);
                    break;
                case "PUT"://자원 정보 업데이트 = UPDATE
                    clientPut(context);
                    break;
                case "DELETE"://자원 삭제 = DELETE
                    clientDelete(context);
                    break;
                default:
                break;
            }
        }

        private void clientGet(HttpListenerContext context){
            HttpListenerResponse response = context.Response;
            string responseString = "";
            string rawUrl = "" + context.Request.RawUrl;
            Console.WriteLine("[{0}] GET Received: rawUrl={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), rawUrl);

            if(rawUrl.StartsWith("/image")){//클라이언트 요청 : 유효한 이미지 ID 값 -> 서버 전송 : imageDictionary 비어있는 Key값
                int emptyKey = ImageManager.Instance.getEmptyKey();
                ImageManager.Instance.imageDictionary[emptyKey] = emptyKey + ".jpg";
                responseString = "{\n\t\"key\": " + emptyKey + "\n}";
            }
            else if(rawUrl.StartsWith("/recommend")){
                List<SharedSchedule> recommendList = new List<SharedSchedule>();
                int count = 0;
                Console.WriteLine(DBManager.Instance.Select("SELECT * FROM SHAREDSCHEDULE order by rand();").Tables[0].Rows.Count);
                foreach(System.Data.DataRow row in DBManager.Instance.Select("SELECT * FROM SHAREDSCHEDULE order by rand();").Tables[0].Rows){
                    recommendList.Add(ScheduleManager.Instance.convertDataRowToSharedSchedule(row));
                    if(count++ > 20)
                        break;
                }
                
                var option = new System.Text.Json.JsonSerializerOptions{
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
                };
                responseString += System.Text.Json.JsonSerializer.Serialize<List<SharedSchedule>>(recommendList, option).ToString();

                Console.WriteLine("[{0}] 추천 일정 전송: count={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), recommendList.Count);
            }
            else{
                responseString = "unknownUrlAccess";
            }
            
            responseString = responseString.Replace("u0027", "\'");
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer,0,buffer.Length);
            Console.WriteLine("[{0}] Server Wrote: body={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), responseString.Replace("\n", @"\n").Replace("\t", @"\t"));
        }

        private void clientPost(HttpListenerContext context){
            string rawUrl = "" + context.Request.RawUrl;
            string body = new StreamReader(context.Request.InputStream).ReadToEnd().Replace("u0027", "\'");

            Console.WriteLine("[{0}] POST Received: rawUrl={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), rawUrl);
            Console.WriteLine("[{0}] POST Received: body={1}ws", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), body.Length);

            JObject? json = new JObject();
            try{
                if(body.StartsWith("{\"body\""))
                    json = JObject.Parse((JObject.Parse(body)?["body"]?.Value<string?>() ?? "").Trim('"'));
                else
                    json = JObject.Parse(body);
            }catch(Exception e){
                Console.WriteLine("[{0}] error at: clientPost={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), e);
            }


            if(rawUrl.StartsWith("/users")){
                Object[] attrs = {"uid", "name", "profileUrl"};
                JValue?[] result = new JValue[3];
                for(int i = 0; i < 3; i++){
                    JToken? token = json[attrs[i]];
                    if(token != null)
                        result[i] = token.Value<JValue>();
                }

                Client client = new Client();
                client.uid = result[0]?.Value<long?>() ?? 0;
                client.name = result[1]?.Value<string?>() ?? "";
                client.profileUrl = result[2]?.Value<string?>() ?? "";

                ClientManager.Instance.addClient(client);
            }
            else if(rawUrl.StartsWith("/schedule")){
                Object[] attrs = {"id", "name", "destination", "days", "startDate", "memberGroupId", "isShared"};
                JValue?[] result = new JValue[7];
                for(int i = 0; i < 7; i++){
                    JToken? token = json[attrs[i]];
                    if(token != null)
                        result[i] = token.Value<JValue>();
                }
                Schedule schedule = new Schedule();
                schedule.ID = result[0]?.Value<int?>() ?? 0;
                schedule.Name = result[1]?.Value<string?>() ?? "";
                schedule.Destination = result[2]?.Value<string?>() ?? "0";
                schedule.Days = result[3]?.Value<int?>() ?? 0;
                schedule.StartDate = result[4]?.Value<string?>() ?? "1900-00-00";
                schedule.MemberGroupId = result[5]?.Value<int?>() ?? 0;
                schedule.IsShared = result[6]?.Value<bool?>() ?? false;

                List<Waypoint> list = new List<Waypoint>();
                JArray? jarr = json["wayPointList"] as JArray;
                if(jarr != null)
                    foreach(JObject obj in jarr){
                        Object[] wAttrs = {"id", "name", "posX", "posY", "rating", "reviewCount", "type", "originLink", "time"};
                        JValue?[] wResult = new JValue[9];
                        for(int i = 0; i < 9; i++){
                            JToken? token = obj[wAttrs[i]];
                            if(token != null)
                                wResult[i] = token.Value<JValue>();
                        }
                        Waypoint waypoint = new Waypoint();
                        waypoint.ID = wResult[0]?.Value<int?>() ?? 0;
                        waypoint.Name = wResult[1]?.Value<string?>() ?? "";
                        waypoint.PosX = wResult[2]?.Value<double?>() ?? 0.0;
                        waypoint.PosY = wResult[3]?.Value<double?>() ?? 0.0;
                        waypoint.Rating = wResult[4]?.Value<byte?>() ?? 0;
                        waypoint.ReviewCount = wResult[5]?.Value<int?>() ?? 0;
                        waypoint.Type = wResult[6]?.Value<int?>() ?? 0;
                        waypoint.OriginLink = wResult[7]?.Value<string?>() ?? "";
                        waypoint.Time = wResult[8]?.Value<int?>() ?? 0;

                        list.Add(waypoint);
                    }
                schedule.Waypoints = list;

                ScheduleManager.Instance.addSchedule(schedule);
                Console.WriteLine("[{0}] Schedule Added: id={1}, Dic Count={2}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), schedule.ID, ScheduleManager.Instance.scheduleDic.Count);
            }
            else if(rawUrl.StartsWith("/sharedschedule")){
                Object[] attrs = {"scheduleId", "ownerId", "rating", "likes", "sharedCount", "titleImgId", "titleText", "descriptionText"};
                JValue?[] result = new JValue[8];
                for(int i = 0; i < 8; i++){
                    JToken? token = json[attrs[i]];
                    if(token != null)
                        result[i] = token.Value<JValue>();
                }
                
                SharedSchedule schedule = new SharedSchedule();
                schedule.ScheduleId = result[0]?.Value<int?>() ?? 0;
                schedule.OwnerId = result[1]?.Value<int?>() ?? 0;
                schedule.Rating = result[2]?.Value<float?>() ?? 0;
                schedule.Likes = result[3]?.Value<int?>() ?? 0;
                schedule.SharedCount = result[4]?.Value<int?>() ?? 0;
                schedule.TitleImgId = result[5]?.Value<int?>() ?? 0;
                schedule.TitleText = result[6]?.Value<string?>() ?? "";
                schedule.DescriptionText = result[7]?.Value<string?>() ?? "";

                List<SharedSchedule.WaypointDescription> list = new List<SharedSchedule.WaypointDescription>();
                JArray? jarr = json["descriptionList"] as JArray;
                if(jarr != null)
                    foreach(JObject obj in jarr){
                        Object[] wAttrs = {"waypointId", "waypointContent"};
                        JValue?[] wResult = new JValue[2];
                        for(int i = 0; i < 2; i++){
                            JToken? token = obj[wAttrs[i]];
                            if(token != null)
                                wResult[i] = token.Value<JValue>();
                        }

                        SharedSchedule.WaypointDescription waypoint = new SharedSchedule.WaypointDescription();
                        waypoint.WaypointId = wResult[0]?.Value<int?>() ?? 0;
                        int[] waypointImgId = new int[3];
                        JArray? imgArray = obj["waypointImgId"] as JArray;
                        if(imgArray != null)
                            for(int i = 0; i < 3; i++)
                                waypointImgId[i] = int.Parse(imgArray[i].ToString());
                        waypoint.WaypointImgId = waypointImgId;
                        waypoint.WaypointContent = wResult[1]?.Value<string?>() ?? "";

                        list.Add(waypoint);
                    }
                schedule.DescriptionList = list;

                ScheduleManager.Instance.addSharedSchedule(schedule);
                Console.WriteLine("[{0}] Shared Schedule Added: id={1}, Dic Count={2}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), schedule.ScheduleId, ScheduleManager.Instance.sharedScheduleDic.Count);
            }
            else if(rawUrl.StartsWith("/imageupload")){
                string[] attrs = {"result", "id"};
                JValue?[] result = {new JValue("failure"), new JValue(0)};
                for(int i = 0; i < 2; i++){
                    JToken? token = json[attrs[i]];
                    if(token != null)
                        result[i] = token.Value<JValue>();
                }

                if(result[0]?.ToString()?.Equals("success") == true){
                    ImageManager.Instance.imageAdded(result[1]?.Value<int?>() ?? 0);
                }

                context.Response.Close();
            }
        }

        private void clientPut(HttpListenerContext context){

        }

        private void clientDelete(HttpListenerContext context){

        }
    }
}