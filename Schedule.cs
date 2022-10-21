namespace TFUserver{
    public class Schedule{
        private int id;
        private string name = "";
        private string destination = "";
        private int days;
        private string startDate = "";
        private List<Waypoint> waypoints = new List<Waypoint>();
        private int memberGroupId;
        private bool isShared = false;

        public int ID {get { return id;} set{ this.id = value;}}
        public string Name {get { return name;} set{ this.name = value;}}
        public string Destination {get { return destination;} set{ this.destination = value;}}
        public int Days {get { return days;} set{ this.days = value;}}
        public string StartDate {get { return startDate;} set{ this.startDate = value;}}
        public List<Waypoint> Waypoints {get { return waypoints;} set{ this.waypoints = value;}}
        public int MemberGroupId {get { return memberGroupId;} set{ this.memberGroupId = value;}}
        public bool IsShared {get { return isShared;} set{ this.isShared = value;}}
    }
}