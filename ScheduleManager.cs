using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Data;
using Newtonsoft.Json.Linq;

namespace TFUserver{
    internal class ScheduleManager{
        private static ScheduleManager? instance;
        public static ScheduleManager Instance { get { if(instance == null) instance = new ScheduleManager(); return instance; } }
        public ConcurrentDictionary<int, SharedSchedule> sharedScheduleDic = new ConcurrentDictionary<int, SharedSchedule>();
        public ConcurrentDictionary<int, Schedule> scheduleDic = new ConcurrentDictionary<int, Schedule>();

        public void syncDBToDictionary(){
            scheduleDic = new ConcurrentDictionary<int, Schedule>();
            DataSet data = DBManager.Instance.Select("SELECT * FROM SCHEDULE;");
            if(data.Tables.Count > 0)
                foreach(DataRow row in data.Tables[0].Rows){
                    Schedule sch = new Schedule();
                    sch.ID = (int) row["id"];
                    sch.Name = (string) row["name"];
                    sch.Destination = (string) row["destination"];
                    sch.Days = (int) row["days"];
                    sch.StartDate = ((DateTime) row["startDate"]).ToString("yyyy-MM-dd");
                    sch.MemberGroupId = (int) row["memberGroupId"];
                    sch.IsShared = (bool) row["isShared"];
                    List<Waypoint> list = new List<Waypoint>();
                    JArray jarr = JArray.Parse((string) row["waypoints"]);
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
                    sch.Waypoints = list;
                    scheduleDic[sch.ID] = sch;
                }
            
            Console.WriteLine("[{0}] 일정 목록 로드 완료 : Dic Count={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), scheduleDic.Count);
        }

        public void addSchedule(Schedule sch){
            StringBuilder query = new StringBuilder("SELECT * FROM SCHEDULE WHERE id = ");
            query.Append(sch.ID);
            query.Append(";");
            if(DBManager.Instance.Select(query.ToString()).Tables.Count > 0 && DBManager.Instance.Select(query.ToString()).Tables[0].Rows.Count < 1){//신규 업로드
                scheduleDic[sch.ID] = sch;
                
                query = new StringBuilder("INSERT INTO SCHEDULE Values(");
                query.Append(sch.ID);
                query.Append(", '");
                query.Append(sch.Name);
                query.Append("', '");
                query.Append(sch.Destination);
                query.Append("', ");
                query.Append(sch.Days);
                query.Append(", '");
                query.Append(sch.StartDate);
                query.Append("', '");
                var option = new JsonSerializerOptions{
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
                };
                query.Append(JsonSerializer.Serialize<List<Waypoint>>(sch.Waypoints, option));
                query.Append("', ");
                query.Append(sch.MemberGroupId);
                query.Append(", ");
                query.Append(sch.IsShared);
                query.Append(");");

                DBManager.Instance.ExecuteQuery(query.ToString());
                Console.WriteLine("[{0}] 일정 추가됨 : query={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), query.ToString());
            }
            else{
                scheduleDic[sch.ID] = sch;
                
                query = new StringBuilder("UPDATE SCHEDULE SET ");
                query.Append("name = '");
                query.Append(sch.Name);
                query.Append("', destination = '");
                query.Append(sch.Destination);
                query.Append("', days = ");
                query.Append(sch.Days);
                query.Append(", startDate = '");
                query.Append(sch.StartDate);
                query.Append("', waypoints = '");
                query.Append(JsonSerializer.Serialize<List<Waypoint>>(sch.Waypoints));
                query.Append("', memberGroupId = ");
                query.Append(sch.MemberGroupId);
                query.Append(", isShared = ");
                query.Append(sch.IsShared);
                query.Append(" WHERE id = ");
                query.Append(sch.ID);
                query.Append(";");

                DBManager.Instance.ExecuteQuery(query.ToString());
                Console.WriteLine("[{0}] 일정 수정됨 : query={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), query.ToString());
            }
        }

        public void addSharedSchedule(SharedSchedule sch){
            StringBuilder query = new StringBuilder("SELECT * FROM SHAREDSCHEDULE WHERE scheduleId = ");
            query.Append(sch.ScheduleId);
            query.Append(";");
            if(DBManager.Instance.Select(query.ToString()).Tables.Count > 0 && DBManager.Instance.Select(query.ToString()).Tables[0].Rows.Count < 1){//신규 업로드
                query = new StringBuilder("INSERT INTO SHAREDSCHEDULE Values(");
                query.Append(sch.ScheduleId);
                query.Append(", ");
                query.Append(sch.OwnerId);
                query.Append(", ");
                query.Append(sch.Rating);
                query.Append(", ");
                query.Append(sch.Likes);
                query.Append(", ");
                query.Append(sch.SharedCount);
                query.Append(", ");
                query.Append(sch.TitleImgId);
                query.Append(", '");
                query.Append(sch.TitleText);
                query.Append("', '");
                query.Append(sch.DescriptionText);
                query.Append("', '");
                var option = new JsonSerializerOptions{
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All), WriteIndented = true
                };
                query.Append(JsonSerializer.Serialize<List<SharedSchedule.WaypointDescription>>(sch.DescriptionList, option));
                query.Append("');");

                sharedScheduleDic[sch.ScheduleId] = sch;
                DBManager.Instance.ExecuteQuery(query.ToString());
                Console.WriteLine("[{0}] 일정 공유됨 : query={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), query.ToString());
            }
            else{// 업로드 된 내용 업데이트
                query = new StringBuilder("UPDATE SHAREDSCHEDULE SET ");
                query.Append("scheduleId = ");
                query.Append(sch.ScheduleId);
                query.Append(", ownerId = ");
                query.Append(sch.OwnerId);
                query.Append(", rating = ");
                query.Append(sch.Rating);
                query.Append(", likes = ");
                query.Append(sch.Likes);
                query.Append(", sharedCount = ");
                query.Append(sch.SharedCount);
                query.Append(", titleImgId = ");
                query.Append(sch.TitleImgId);
                query.Append(", titleText = '");
                query.Append(sch.TitleText);
                query.Append("', descriptionText = '");
                query.Append(sch.DescriptionText);
                query.Append("', waypointDescription = '");
                var option = new JsonSerializerOptions{
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
                };
                query.Append(JsonSerializer.Serialize<List<SharedSchedule.WaypointDescription>>(sch.DescriptionList, option));
                query.Append("' WHERE scheduleId = ");
                query.Append(sch.ScheduleId);
                query.Append(";");

                sharedScheduleDic[sch.ScheduleId] = sch;
                DBManager.Instance.ExecuteQuery(query.ToString());
                Console.WriteLine("[{0}] 공유 일정 수정됨 : query={1}", DateTime.Now.ToString("yy-MM-dd hh:mm:ss"), query.ToString());
            }
        }

        public SharedSchedule convertDataRowToSharedSchedule(DataRow dataRow){
            SharedSchedule sch = new SharedSchedule();
            sch.ScheduleId = (int) dataRow["scheduleId"];
            sch.OwnerId = (long) dataRow["ownerId"];
            sch.Rating = float.Parse(dataRow["rating"].ToString() ?? "0.0");
            sch.Likes = (int) dataRow["likes"];
            sch.SharedCount = (int) dataRow["likes"];
            sch.TitleImgId = (int) dataRow["titleImgId"];
            sch.TitleText = (string) dataRow["titleText"];
            sch.DescriptionText = (string) dataRow["descriptionText"];
            List<SharedSchedule.WaypointDescription> list = new List<SharedSchedule.WaypointDescription>();
            JArray jarr = JArray.Parse((string) dataRow["waypointDescription"]);
            if(jarr != null)
                foreach(JObject obj in jarr){
                    Object[] wAttrs = {"WaypointId", "WaypointContent"};
                    JValue?[] wResult = new JValue[2];
                    for(int i = 0; i < 2; i++){
                        JToken? token = obj[wAttrs[i]];
                        if(token != null)
                            wResult[i] = token.Value<JValue>();
                    }

                    SharedSchedule.WaypointDescription waypointDescription = new SharedSchedule.WaypointDescription();
                    
                    waypointDescription.WaypointId = wResult[0]?.Value<int?>() ?? 0;
                    
                    int[] waypointImgId = new int[3];
                    JArray? wArr = obj["WaypointImgId"] as JArray;
                    if(wArr != null)
                        for(int i = 0; i < 3; i++)
                            waypointImgId[i] = int.Parse(wArr[i].ToString());
                            
                    waypointDescription.WaypointImgId = waypointImgId;
                    waypointDescription.WaypointContent = wResult[1]?.Value<string?>() ?? "";

                    list.Add(waypointDescription);
                }
            sch.DescriptionList = list;
            return sch;
        }
    }
}