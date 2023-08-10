using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElysiaInteractMenu.Manager
{
    public class PlayerFineManager
    {
        public List<PlayerFine> PlayerFines { get; }

        private ElysiaDB ElysiaDB;
        
        public PlayerFineManager(ElysiaDB elysiaDB)
        {
            ElysiaDB = elysiaDB;
            LoadFines();
        }


        public List<PlayerFine> GetPlayerFines(int characterId)
        {
            return PlayerFines.Where(fine => fine.ReceiverId == characterId).ToList();
        }
        protected async Task LoadFines()
        {
            List<PlayerFines> playerFinesAsync = await ElysiaDB.LoadPlayerFines();
            foreach (var playerFineAsync in playerFinesAsync)
            {
                PlayerFine playerFine = new PlayerFine
                {
                    FineId = playerFineAsync.FineId,
                    Amount = playerFineAsync.Amount,
                    IsPaid = playerFineAsync.IsPaid,
                    Reason = playerFineAsync.Reason,
                    ReceiverId = playerFineAsync.ReceiverId,
                    SenderId = playerFineAsync.SenderId,
                    SentTimestamp = playerFineAsync.SentTimestamp
                };

                PlayerFines.Add(playerFine);
            }
        }
        
        
    }
}