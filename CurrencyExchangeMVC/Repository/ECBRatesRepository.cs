using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CurrencyExchangeMVC.Models;

namespace CurrencyExchangeMVC.Repository
{
    public class ECBRatesRepository
    {
        /******************************************************************/
        /**
         * Implementing the Singleton Design Pattern
         */

        // internal ECBRatesRepository singleton container
        private static ECBRatesRepository _instance;


        // lock for thread-safety laziness
        private static readonly object Mutex = new object();


        // empty constuctor
        private ECBRatesRepository()
        {
        }

        //The only public method, used to obtain an instance of DataStorage
        public static ECBRatesRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Mutex)
                    {
                        if (_instance == null)
                        {
                            _instance = new ECBRatesRepository();
                        }
                    }
                }

                return _instance;
            }
        }


        /******************************************************************/
        /**
        * Implementing the Repository of Reference Rates
        */

        /// <summary>
        /// An internal cache for keeping the data in memory.
        ///
        /// Data Organization, a dictionary of lists of ReferenceRate objects:
        /// The list consists of all the days (first dictionary) of all the currency rates (second dictionary).
        /// DATA:
        /// {
        ///     "2015-05-30" => { 
        ///             USD_ReferenceRate object, 
        ///             GBP_ReferenceRate object,
        ///             ... etc
        ///     },
        ///     ... etc
        /// }
        /// </summary>
        private static Dictionary<string, List<ReferenceRate>> EXCHANGE_RATES_CACHE = new Dictionary<string, List<ReferenceRate>>();


        /// <summary>
        /// Fills the EpochDate attribute in all the reference rate objects
        /// </summary>
        private static void _fillEpochDateForRates(ref Dictionary<string, List<ReferenceRate>> exchangeRatesCollection)
        {
            foreach(var keyValue in exchangeRatesCollection)
            {
                var keyDate = keyValue.Key;

                foreach(var rate in keyValue.Value) {
                    DateTime dateTime;
                    DateTime.TryParse(keyDate, out dateTime);
                    
                    if(dateTime != DateTime.MinValue) {
                        TimeSpan span = dateTime.Subtract(new DateTime(1970,1,1,0,0,0));
                        rate.EpochDate = span.TotalSeconds;
                    }
                }
            }
        }


        /// <summary>
        /// Syncs the internal data-cache with the web-service.
        /// </summary>
        /// <returns></returns>
        private static bool _syncDataWithWebService()
        {
            try
            {
                EXCHANGE_RATES_CACHE = ECBWebService.FetchData();
                _fillEpochDateForRates(ref EXCHANGE_RATES_CACHE);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Ensures that the internal data-cache is always available.
        /// </summary>
        private void _ensureDataIsSynced()
        {
            // Get the date string of today. Example: "2015-06-10"
            var todayDateString = (DateTime.Now).ToString("yyyy-MM-dd");

            // Check if the cache is null, empty, or doesn't contain yesterday's reference rates
            bool mustSync = (EXCHANGE_RATES_CACHE == null
                || (EXCHANGE_RATES_CACHE.Count == 0)
                || (EXCHANGE_RATES_CACHE.Count > 0 && false == EXCHANGE_RATES_CACHE.Keys.Contains(todayDateString)));

            // Sync if the above condition is true
            if (mustSync == true)
            {
                _syncDataWithWebService();
            }
        }


        /// <summary>
        /// Returns the complete collection of the reference rates.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<ReferenceRate>> GetReferenceRates()
        {
            _ensureDataIsSynced();

            return EXCHANGE_RATES_CACHE;
        }


        /// <summary>
        /// Returns the latest collection of rates (the last day).
        /// </summary>
        public List<ReferenceRate> GetLatestRates()
        {
            _ensureDataIsSynced();

            List<ReferenceRate> latestReferenceRates;

            // Construct the date string of today and yesterday. Example: "2015-06-10"
            var todayDateString = (DateTime.Now).ToString("yyyy-MM-dd");
            var yesterdayDateString = (DateTime.Now.AddDays(-1)).ToString("yyyy-MM-dd");

            // Get which ever list of reference rates available.
            if (EXCHANGE_RATES_CACHE.ContainsKey(todayDateString))
            {
                latestReferenceRates = EXCHANGE_RATES_CACHE[todayDateString];
            }
            else
            {
                latestReferenceRates = EXCHANGE_RATES_CACHE[yesterdayDateString];
            }

            return latestReferenceRates;
        }


        /// <summary>
        /// Returns the history of exchange rates between two currencies.
        /// </summary>
        public Dictionary<string, List<ReferenceRate>> GetHistoryOfExchangeRates(string fromCurrency, string toCurrency)
        {
            var data = new Dictionary<string, List<ReferenceRate>>();
            decimal oneUnit = Convert.ToDecimal(1);

            foreach (var keyValue in EXCHANGE_RATES_CACHE)
            {
                var fromCurrencyRate = keyValue.Value.Find(item => item.Currency.ToLower() == fromCurrency.ToLower());
                var toCurrencyRate = keyValue.Value.Find(item => item.Currency.ToLower() == toCurrency.ToLower());
                //var exchangeRate = CalculateExchangeRate (fromCurrency.Rate, oneUnit, toCurrencyRate.Rate, oneUnit);

                data.Add(keyValue.Key, new List<ReferenceRate>() { fromCurrencyRate, toCurrencyRate });
            }

            return data;
        }


        /// <summary>
        /// Calculates the exchange rate between two currencies given their Exchange Rates to EURO and their specified amounts.
        /// </summary>
        /// <returns>The exchange rate.</returns>
        public decimal CalculateExchangeRate(decimal fromCurrencyRate, decimal toCurrencyRate, decimal exchangeAmount)
        {
            return Decimal.Divide((exchangeAmount * toCurrencyRate), (exchangeAmount * fromCurrencyRate));
        }
    }
}