using SQLite;

namespace ElysiaInteractMenu
{
    public class PlayerFines
    {
        [PrimaryKey,AutoIncrement]
        public int FineId { get; set;}
        public int SenderId { get; set;}
        public int ReceiverId { get; set;}
        public string Reason { get; set;}
        public int Amount { get; set;}
        public bool IsPaid { get; set;}
        public long SentTimestamp { get; set;}
        public string Type { get; set; }
        public int VehicleId { get; set; }
    }
}