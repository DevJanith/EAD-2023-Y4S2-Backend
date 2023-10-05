namespace Rest.Configurations
{
    public class ProductDBSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string ProductCollectionName { get; set; }
        public string ReservationCollectionName { get; set; }
        public string ScheduleCollectionName { get; set; }
        public string TrainCollectionName { get; set; }
        public string UserCollectionName { get; set; } 
    }

}
