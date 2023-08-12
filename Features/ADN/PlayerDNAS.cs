using SQLite;

namespace ElysiaInteractMenu.Features.ADN
{
    public class PlayerDNAS
    {
        
        [PrimaryKey]
        public string DnaCode { get; set; }
        public int CharacterId { get; set; }
        public bool IsFound { get; set; }
    }
}