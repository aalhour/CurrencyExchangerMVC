# Currency Exchanger

This is an interview assignment project. It was completed in a total of 18 hours of work.

## Description

This is an MVC application that reads *Curreny Reference-Rates* from the **ECB** (European Central Bank) [Web-Service](http://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist-90d.xml), stores it in an in-memory Data Structure and serves it to the front-end.

The project consists of two controllers: Home, and Exchange. The Home controller represent the front-end which consists of three pages: Home page, Exchange utility page, and About page. The actual service is in the Exchange utility page.

The Exchange controller is a simple API controller with one publicly-exposed method: GET. Given two currencies and a financial amount (number), it returns an object of 1) the currency-exchange, and 2) a history-array of all changes in the reference-rates of these two currencies.

``` javascript
// a sample api response object
data = {
  "ExchangeRate": 12.1231342,
  "History": Array[60]
}

// a sample from the data-history
data["History"] = [
  0: Object {
      Date: "2015-06-12",
      EpochDate: 141234123123123
      FromCurrencyName: "USD",
      FromCurrencyRate: 1.1232,
      ToCurrencyName: "GBP",
      ToCurrencyRate: 0.82922
  },
  1: Object {
      ...
  },
  ...
]
```

The Exchange utility page reads the list of currencies from the internal API, and renders them on page-load into HTML select-inputs.

The calculation of currency-exchanges and the rendering of the histogram (RichShaw Graph) happens asynchronously via jQuery. The calculation event calls the ExchangeController#Get method and processes the data.

All the javascript code for the Exchange Utility page is written in a separate file under the Scripts directory, titles "home_exchange_js_functions.js".

## The Repository and In-Memory Data Structure

#### [ECB Web Service](/CurrencyExchangeMVC/Repository/ECBWebService.cs)

This class implements a simple xml-web-service parser. It requests the web-service, converts the response to an XDocument and parses it using foreach-loops and LINQ.

It fetches the Web-Service URL from the Web.config file.

#### [ECB Rates Repository](/CurrencyExchangeMVC/Repository/ECBRatesRepository.cs)

This class implements the Singleton design pattern and keeps in memory one instance of itself, each instance has an internal cache of the Rates returned from ECB Web Service.

When a page or a controller-action asks this class for data, it keeps checking if it has the latest data or not, and in case of any need for syncing, it does so without breaking the logic of seemlessly providing data to the requester.

#### In-Memory Data Structure

The in-memory cache or collection of data in this app is basically a dictionary of lists. The key for every entry is string-formatted date (i.e. "2015-05-29"). The value of every entry is a list of [ReferenceRate](/CurrencyExchangeMVC/Models/ReferenceRate.cs) objects.

Accessing the reference-rates of any date (if available) has a worst-case asymptotic complexity of O(1). Which is very important for this application.

## Technologies

  * C# 4.5
  * ASP.NET MVC 5.2.3
  * ASP.NET Web API 2.2
  * .NET Framework 4.5.1,
  * Newtonsoft.Json
  * jQuery 2.1.4
  * jQuery Validation 1.13.1
  * Twitter Bootstrap 3.3.4
  * D3.js
  * RickShaw.js
