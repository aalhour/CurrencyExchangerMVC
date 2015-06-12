//
// GLOBAL VARIABLES USED ONLY IN DATA BINDING AT THE UI
window.GLOBAL_FromCurrency = "";
window.GLOBAL_ToCurrency = "";


//
// AJAX REQUEST FUNCTION
function makeExchangeRateRequest() {
    var fromCurrency = $('#SourceCurrency').val();
    var toCurrency = $('#DestinationCurrency').val();
    var exchangeAmount = parseFloat($('#ExchangeAmount').val());

    window.GLOBAL_FromCurrency = fromCurrency;
    window.GLOBAL_ToCurrency = toCurrency;

    $.ajax({
        type: "GET",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        url: costructWebApiCallUrl(fromCurrency, toCurrency, exchangeAmount),
        data: "",
        // ON SUCCESS
        success: ajaxOnSuccess,
        // ON ERROR
        error: ajaxOnError
    });
}


//
// Constructs the ASP.NET Web API 2 URL
function costructWebApiCallUrl(fromCurrency, toCurrency, exchangeAmount) {
    var url = $('#ExchangeUtilityAPIHiddenField').val();
    url += "/" + fromCurrency;
    url += "/" + toCurrency;
    url += "/" + exchangeAmount;

    return url;
}


//
// AJAX ON SUCCESS FUNCTION
function ajaxOnSuccess(jsonResponse) {
    data = $.parseJSON(jsonResponse);
    console.log(data);

    var exchangeRate = data["ExchangeRate"];
    var history = data["History"];

    $('#CalculatedExchangeRate span').html(exchangeRate);
    $('#historyFromCurrency').html("<em>" + window.GLOBAL_FromCurrency + "</em>");
    $('#historyToCurrency').html("<em>" + window.GLOBAL_ToCurrency + "</em>");

    drawRickShawGraph(history);

    $('#ExchangeRateResultsContainer').removeClass('hidden');
}


//
// AJAX ON ERROR FUNCTION
function ajaxOnError(jsonError) {
    var error = $.parseJSON(jsonError);
    console.log(error);

    $('#validation-error span#error-message').html(error);
    $('#validation-error').removeClass("hidden");
    $('#ExchangeRateResultsContainer').addClass('hidden');
}


//
// FORM VALIDATOR
function validateForm() {
    var exchangeAmount = $("#ExchangeAmount").val();
    var errorMessage = "Please insert a valid amount number.";

    if (exchangeAmount == undefined || exchangeAmount == "") {
        $('#validation-error span#error-message').html(errorMessage);
        $('#validation-error').removeClass("hidden");
        $('#ExchangeRateResultsContainer').addClass('hidden');
        return false;
    } else if (parseFloat(exchangeAmount) <= 0) {
        $('#validation-error span#error-message').html(errorMessage);
        $('#validation-error').removeClass("hidden");
        $('#ExchangeRateResultsContainer').addClass('hidden');
        return false;
    } else {
        $('#validation-error').addClass("hidden");
        return true;
    }
}


//
// RICK SHAW GRAPH
function drawRickShawGraph(historyOfRates) {
    if (historyOfRates == undefined || historyOfRates.length == 0)
        return;

    //
    // Set up our data series collection
    var seriesData = [[], []];
    seriesData = prepareDataForRickshaw(historyOfRates);


    //
    // Clear the chart and legend containers
    var chartElement = document.getElementById("chart");
    var chartLegendElement = document.getElementById('legend');
    chartElement.innerHTML = "";
    chartLegendElement.innerHTML = "";


    //
    // Instantiate our graph!
    var graph = new Rickshaw.Graph({
        element: chartElement,
        width: 870,
        height: 500,
        renderer: 'line',
        series: [
            {
                color: "#30c020",
                data: seriesData[0],
                name: window.GLOBAL_FromCurrency
            }, {
                color: "#6060c0",
                data: seriesData[1],
                name: window.GLOBAL_ToCurrency
            }
        ]
    });

    graph.render();

    var hoverDetail = new Rickshaw.Graph.HoverDetail({
        graph: graph
    });

    var legend = new Rickshaw.Graph.Legend({
        graph: graph,
        element: chartLegendElement

    });

    var shelving = new Rickshaw.Graph.Behavior.Series.Toggle({
        graph: graph,
        legend: legend
    });

    var axes = new Rickshaw.Graph.Axis.Time({
        graph: graph
    });

    axes.render();
}


//
// PREPARE THE API DATA FOR RICKSHAW
//
// Return an array of two internal arrays. These arrays correspond to FromCurrency and ToCurrecy respectively.
function prepareDataForRickshaw(historyOfRates) {
    //
    // Set up our data series collection
    var seriesData = [[], []];

    // The data is sorted in descending order, we need to insert it backwards.
    // We also need to insert the epoch data as an attribute 'x' in the object, and the currency rate as attribute 'y' in the object.
    var j = 0;
    for (var i = historyOfRates.length - 1; i >= 0; i--) {
        seriesData[0][j] = {
            x: historyOfRates[i]["EpochDate"],
            y: historyOfRates[i]["FromCurrencyRate"]
        }

        seriesData[1][j] = {
            x: historyOfRates[i]["EpochDate"],
            y: historyOfRates[i]["ToCurrencyRate"]
        }

        j++;

        if (j > historyOfRates.length)
            break;
    } //end-for

    return seriesData;
}