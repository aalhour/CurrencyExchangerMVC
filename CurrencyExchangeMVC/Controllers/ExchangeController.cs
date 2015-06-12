using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

using CurrencyExchangeMVC.Models;
using CurrencyExchangeMVC.Repository;

namespace CurrencyExchangeMVC.Controllers
{
    public class ExchangeController : ApiController
    {
        private static ECBRatesRepository _ratesRepo = ECBRatesRepository.Instance;


        //
        // GET exchange/USD/GBP/1.0
        // Returns JSON string.
        // Returns the exchange value from one currency to another, alongside the history of the currencies' reference-rates.
        [HttpGet]
        [Route("exchange/{fromCurrencyName}/{toCurrencyName}/{exchangeAmount}")]
        public string Get(string fromCurrencyName, string toCurrencyName, decimal exchangeAmount)
        {
            var today = (DateTime.Now).ToString("yyyy-MM-dd");
            var yesterday = (DateTime.Now.AddDays(-1)).ToString("yyyy-MM-dd");

            //
            // Get the latest exchange rates from repository
            var latestRates = _ratesRepo.GetLatestRates();

            //
            // Get the history of exchange rates for these two currencies and cast them into a list of anonymous typed objects.
            // See the return statement at the end for an explanation.
            var exchangeRatesHistory = _ratesRepo.GetHistoryOfExchangeRates(fromCurrencyName, toCurrencyName)
                .Select(item => new
                {
                    Date = item.Key,
                    EpochDate = item.Value[0].EpochDate,
                    FromCurrencyName = item.Value[0].Currency,
                    FromCurrencyRate = item.Value[0].Rate,
                    ToCurrencyName = item.Value[1].Currency,
                    ToCurrencyRate = item.Value[1].Rate
                })
                .ToList();

            // Get the corresponding reference rates objects
            var fromCurrencyRate = latestRates.Find(item => item.Currency.ToLower() == fromCurrencyName.ToLower());
            var toCurrencyRate = latestRates.Find(item => item.Currency.ToLower() == toCurrencyName.ToLower());

            // Calculate the exchange rate
            var exchangeRate = _ratesRepo.CalculateExchangeRate(fromCurrencyRate.Rate, toCurrencyRate.Rate, exchangeAmount);

            //
            // Return a new anonymous type object.
            // Automatically serialized and deserialzed by javascript
            // Object { ExchangeRate: "...", History: Array[...] }
            return JsonConvert.SerializeObject(new
            {
                ExchangeRate = exchangeRate,
                History = exchangeRatesHistory
            });
        }
    }
}
