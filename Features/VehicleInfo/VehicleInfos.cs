using SQLite;

namespace ElysiaInteractMenu
{
    public class VehicleInfos
    {
        public int CharacterId { get; set; }
        
        [PrimaryKey]
        public int VehicleId { get; set; }
        public long DateDriving { get; set; }
        public double Kilometer { get; set; }
    }
}