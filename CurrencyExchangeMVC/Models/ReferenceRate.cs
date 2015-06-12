using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurrencyExchangeMVC.Models
{
    public class ReferenceRate
    {
        public string Currency { get; set; }
        public decimal Rate { get; set; }
        public double EpochDate { get; set; }
    }
}