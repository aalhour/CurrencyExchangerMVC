using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CurrencyExchangeMVC.Models;
using CurrencyExchangeMVC.Repository;
using CurrencyExchangeMVC.Helpers;

namespace CurrencyExchangeMVC.Controllers
{
    public class HomeController : Controller
    {
        private static ECBRatesRepository _ratesRepo = ECBRatesRepository.Instance;
        private static List<string> _currenciesList = new List<string>();

        /// <summary>
        /// Initializes the currencies list.
        /// </summary>
        private void _ensureCurrenciesAvailability()
        {
            if (_currenciesList == null || _currenciesList.Count == 0)
            {
                _currenciesList = _ratesRepo.GetLatestRates().Select(item => item.Currency).Distinct().ToList<string>();
            }
        }


        //
        // GET /home/index
        public ActionResult Index()
        {
            return View();
        }


        //
        // GET /home/exchange
        public ActionResult Exchange()
        {
            _ensureCurrenciesAvailability();

            ViewBag.Currencies = _currenciesList;
            ViewBag.ExchangeUtilityAPI = Misc.ResolveServerUrl(VirtualPathUtility.ToAbsolute("/exchange"), false);
            return View();
        }


        //
        // GET /home/about
        public ActionResult About()
        {
            return View();
        }
    }
}