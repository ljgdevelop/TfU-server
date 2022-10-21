namespace TFUserver{
    public class SharedSchedule{
        private int scheduleId;
        private int ownerId;
        private float rating;
        private int likes;
        private int sharedCount;
        private int titleImgId;
        private string? titleText = "";
        private string? descriptionText ="";
        private List<WaypointDescription> descriptionList = new List<WaypointDescription>();

        public int ScheduleId {get { return scheduleId;} set{ this.scheduleId = value;}}
        public int OwnerId {get { return ownerId;} set{ this.ownerId = value;}}
        public float Rating {get { return rating;} set{ this.rating = value;}}
        public int Likes {get { return likes;} set{ this.likes = value;}}
        public int SharedCount {get { return sharedCount;} set{ this.sharedCount = value;}}
        public int TitleImgId {get { return titleImgId;} set{ this.titleImgId = value;}}
        public string? TitleText {get { return titleText;} set{ this.titleText = value;}}
        public string? DescriptionText {get { return descriptionText;} set{ this.descriptionText = value;}}
        public List<WaypointDescription> DescriptionList {get { return descriptionList;} set{ this.descriptionList = value;}}

        public class WaypointDescription{
            private int waypointId;
            private int[] waypointImgId = new int[3];
            private string waypointContent = "";

            public int WaypointId {get { return waypointId;} set{ this.waypointId = value;}}
            public int[] WaypointImgId {get { return waypointImgId;} set{ this.waypointImgId = value;}}
            public string WaypointContent {get { return waypointContent;} set{ this.waypointContent = value;}}
        }
    }
}