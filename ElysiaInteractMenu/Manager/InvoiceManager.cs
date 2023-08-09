using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Life.Network;

namespace ElysiaInteractMenu
{
    public class InvoiceManager
    {
        private ElysiaDB _elysiaDB;
        private List<Invoice> Invoices = new List<Invoice>();
        public InvoiceManager(ElysiaDB elysiaDB)
        {
            _elysiaDB = elysiaDB;
            LoadInvoices();
        }

        public void SendInvoice(int bizId, int characterId, int amount, string reason) =>
            this.Invoices.Add(new Invoice(){BizId = bizId,CharacterId = characterId,Amount = amount,Reason = reason,IsPaid = false});

        public bool TryPayInvoice(Player player,Invoice invoice)
        {
            int amountBank = player.character.Bank;

            if (!(amountBank >= invoice.Amount))
            {
                return false;
            }

            player.character.Bank -= invoice.Amount;
            invoice.IsPaid = true;
            return true;
        }

        private List<Invoice> GetSentBizInvoices(int bizId)
        {
            return this.Invoices.Where(invoice => invoice.BizId == bizId).ToList();
        }

        public List<Invoice> GetUnPaidBizSentInvoices(int bizId)
        {
            return this.GetSentBizInvoices(bizId).Where(invoice => !invoice.IsPaid).ToList();
        }
        
        public List<Invoice> GetPaidBizSentInvoices(int bizId)
        {
            return this.GetSentBizInvoices(bizId).Where(invoice => invoice.IsPaid).ToList();
        }

        public List<Invoice> GetUnPaidInvoices(int characterId)
        {
            return this.Invoices.Where(invoice => invoice.CharacterId == characterId).Where(invoice => !invoice.IsPaid).ToList();
        }
        
        public List<Invoice> GetPaidInvoices(int characterId)
        {
            return this.Invoices.Where(invoice => invoice.CharacterId == characterId).Where(invoice => invoice.IsPaid).ToList();
        }
        
        public List<Invoice> GetPlayerInvoices(int characterId) =>
            this.Invoices.Where(invoice => invoice.CharacterId == characterId).ToList();

        private async void LoadInvoices()
        {
            List<Invoices> invoicesAsync = await _elysiaDB.LoadInvoices();
            List<Invoice> invoices = new List<Invoice>();

            foreach (Invoices invoiceA in invoicesAsync)
            {
                Invoice invoice = new Invoice()
                {
                    InvoiceId = invoiceA.InvoiceId,
                    BizId = invoiceA.BizId,
                    Amount = invoiceA.Amount,
                    CharacterId = invoiceA.CharacterId,
                    IsPaid = invoiceA.IsPaid,
                    Reason = invoiceA.Reason
                };
                
                invoices.Add(invoice);
            }

            this.Invoices = invoices;
        }
        
        public async Task Save()
        {
            foreach (Invoice invoice in Invoices)
            {
                await _elysiaDB.db.InsertOrReplaceAsync(new Invoices()
                {
                    InvoiceId = invoice.InvoiceId,Amount = invoice.Amount,
                    BizId = invoice.BizId,CharacterId = invoice.CharacterId,
                    IsPaid = invoice.IsPaid,Reason = invoice.Reason
                }); 
            }
            
        }
    }
}