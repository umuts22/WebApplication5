using System;
using System.Collections.Generic;

namespace WebApplication5.ViewModels
{
    public class DashboardViewModel
    {
       
        
        public decimal TotalSaving { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal TotalCredit { get; set; }

        public string CustomerName { get; set; }


        public List<TransactionViewModel> Transactions { get; set; }
        public List<InvoiceViewModel> Invoices { get; set; }
        public DebitCreditChartDataViewModel DebitCreditChartData { get; set; }
    }

    public class TransactionViewModel
    {
        public string Description { get; set; }
        public string Category { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }

        public decimal Balance { get; set; }
        public int TransactionID { get; set; }
    }

    public class InvoiceViewModel
    {
        public string ClientName { get; set; }
        public string TimeAgo { get; set; }
        public decimal Amount { get; set; }
    }

    public class DebitCreditChartDataViewModel
    {
        public List<string> Labels { get; set; }
        public List<decimal> DebitData { get; set; }
        public List<decimal> CreditData { get; set; }
    }

}
