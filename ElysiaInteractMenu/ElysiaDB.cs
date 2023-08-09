using System.Collections.Generic;
using System.Threading.Tasks;
using Life.DB;
using SQLite;

namespace ElysiaInteractMenu
{
    public class ElysiaDB
    {
        public SQLiteAsyncConnection db;
        public ElysiaDB()
        {
            db = LifeDB.db;
            db.CreateTableAsync<Invoices>();
            db.CreateTableAsync<VehicleInfos>();
        }
        
        public async Task<List<Invoices>> LoadInvoices() => await db.Table<Invoices>().ToListAsync();

        public async Task<List<VehicleInfos>> LoadVehicleInfos() => await db.Table<VehicleInfos>().ToListAsync();

        public async Task CreateVehicleInfo(VehicleInfo vehicleInfo)
        {
            await db.InsertAsync(new VehicleInfo()
            {
                CharacterId = vehicleInfo.CharacterId,DateDriving = vehicleInfo.DateDriving,
                Kilometer = vehicleInfo.Kilometer,VehicleId = vehicleInfo.VehicleId
            });
        }
        
        public async Task DeleteVehicleInfo(VehicleInfo vehicleInfo)
        {
            await db.DeleteAsync(vehicleInfo);
        }

         public async Task CreateInvoice(int characterId, int bizId, int amount,string reason)
         {
             Invoices invoices = new Invoices
             {
                 BizId = bizId,
                 CharacterId = characterId,
                 Amount = amount,
                 Reason = reason,
                 IsPaid = false
             };
 
             await db.InsertAsync(invoices);
         }
 
         public async Task<List<Invoice>> FetchCharacterInvoices(int characterId)
         {
             List<Invoices> invoicesListAsync = await db.Table<Invoices>().Where(invoices => invoices.CharacterId == characterId).ToListAsync();
             List<Invoice> invoiceList = new List<Invoice>();
 
             foreach (Invoices invoices in invoicesListAsync)
             {
                 Invoice invoice = new Invoice
                 {
                     InvoiceId = invoices.InvoiceId,
                     BizId = invoices.BizId,
                     CharacterId = invoices.CharacterId,
                     Amount = invoices.Amount,
                     IsPaid = invoices.IsPaid
                 };
                 invoiceList.Add(invoice);
             }
 
             return invoiceList;
         }
    }
}