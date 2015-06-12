using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;

using CurrencyExchangeMVC.Models;

namespace CurrencyExchangeMVC.Repository
{
    /// <summary>
    /// Fetches data from the European Central Bank web service.
    /// The web service link is specified in the Web.config file in the ASP.NET project.
    /// </summary>
    public class ECBWebService
    {
        /// <summary>
        /// The URL of the web-service to fetch data from. It is initialized in the Web.Config.
        /// </summary>
        private static readonly string _dataWebServiceConnectionString = ConfigurationManager.AppSettings["DataWebServiceConnectionString"];


        /// <summary>
        /// A LINQ-based XML Parser. Returns the data indexed by date and currency name.
        /// </summary>
        public static Dictionary<string, List<ReferenceRate>> FetchData()
        {
            var data = new Dictionary<string, List<ReferenceRate>>();

            var xDoc = ECBWebService.MakeRequest();

            var nodes = xDoc.DescendantNodes().ToList();

            // Used to keep track of the last added date to the dictionary
            string lastDataKey = string.Empty;

            for (int i = 0; i < nodes.Count; ++i)
            {
                if (nodes[i] is XText)
                    continue;

                var node = (XElement)nodes[i];
                var name = node.Name.ToString();

                if (!string.IsNullOrEmpty(name) && name.Contains("Cube"))
                {
                    if (node.Attributes().Count() == 1 && node.FirstAttribute.Name == "time")
                    {
                        var dateString = node.FirstAttribute.Value.ToString();

                        if (!string.IsNullOrEmpty(dateString) && !data.Keys.Contains(dateString))
                            data.Add(dateString, new List<ReferenceRate>());

                        lastDataKey = dateString;
                    }
                    else if (node.Attributes().Count() == 2 && node.FirstAttribute.Name == "currency" && node.FirstAttribute.NextAttribute.Name == "rate")
                    {
                        var currency = node.FirstAttribute.Value;
                        var rate = Convert.ToDecimal(node.FirstAttribute.NextAttribute.Value);

                        if (false == data[lastDataKey].Exists(item => item.Currency == currency))
                        {
                            var referenceRate = new ReferenceRate()
                            {
                                Currency = currency,
                                Rate = rate
                            };

                            data[lastDataKey].Add(referenceRate);
                        }
                    }
                }//end-outer-if
            }//end-for

            return data;
        }

        /// <summary>
        /// Creates a web request and returns the XMLDocument of the response.
        /// </summary>
        public static XDocument MakeRequest()
        {
            try
            {
                return XDocument.Load(_dataWebServiceConnectionString);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}