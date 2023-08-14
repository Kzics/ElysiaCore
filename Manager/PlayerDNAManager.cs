using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElysiaInteractMenu.Features.ADN;

namespace ElysiaInteractMenu.Manager
{
    public class PlayerDNAManager
    {
        public List<PlayerDNA> playerDNAS = new List<PlayerDNA>();
        public ElysiaDB ElysiaDB;

        public PlayerDNAManager(ElysiaDB elysiaDB)
        {
            ElysiaDB = elysiaDB;
            LoadDnas();
        }


        public PlayerDNA getDna(int characterId) => playerDNAS.FirstOrDefault(dna => dna.CharacterId == characterId);
        public List<PlayerDNA> GetFoundDna() => playerDNAS.Where(dna => dna.IsFound).ToList();
        
        public PlayerDNA getDna(string dnaCode) => playerDNAS.FirstOrDefault(dna => dna.DnaCode == dnaCode);

        public bool IsFound(string dnaCode)
        {
            PlayerDNA playerDna = playerDNAS.Where(dna => dna.DnaCode == dnaCode).FirstOrDefault();

            if (playerDna != null)
            {
                return playerDna.IsFound;
            }

            return false;
        }

        public void FindDna(string dnaCode) => playerDNAS[0].IsFound = true;
        protected async Task LoadDnas()
        {
            List<PlayerDNAS> playerDnasAsync = await ElysiaDB.LoadPlayerDnas();
            foreach (var playerDnaAsync in playerDnasAsync)
            {
                PlayerDNA dna = new PlayerDNA
                {
                    DnaCode = playerDnaAsync.DnaCode,
                    CharacterId = playerDnaAsync.CharacterId,
                    IsFound = playerDnaAsync.IsFound
                };

                playerDNAS.Add(dna);
            }
        }
    }
}