using System.Data;

namespace TFUserver{
    public class Waypoint{
        private int id;
        private string name = "";
        private double posX;
        private double posY;
        private decimal rating;
        private int reviewCount;
        private int type;
        private string originLink = "";
        private int time;

        public int ID {get { return id;} set{ this.id = value;}}
        public string Name {get { return name;} set{ this.name = value;}}
        public double PosX {get { return posX;} set{ this.posX = value;}}
        public double PosY {get { return posY;} set{ this.posY = value;}}
        public decimal Rating {get { return rating;} set{ this.rating = value;}}
        public int ReviewCount {get { return reviewCount;} set{ this.reviewCount = value;}}
        public int Type {get { return type;} set{ this.type = value;}}
        public string OriginLink {get { return originLink;} set{ this.originLink = value;}}
        public int Time {get { return time;} set{ this.time = value;}}

        public Waypoint convertDataRowToWaypoint(DataRow dataRow){
            this.id = (int) dataRow["id"];
            this.name = (string) dataRow["name"];
            this.posX = double.Parse(dataRow["posX"].ToString() ?? "0.0");
            this.posY = double.Parse(dataRow["posY"].ToString() ?? "0.0");
            this.rating = (decimal) dataRow["rating"];
            this.reviewCount = (int) dataRow["reviewCount"];
            this.type = (int) dataRow["type"];
            this.originLink = (string) dataRow["originLink"];
            this.time = 0;
            return this;
        }
    }
}