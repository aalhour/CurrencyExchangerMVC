using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurrencyExchangeMVC.Helpers
{
    public class Misc
    {
        public static string GetBaseUrl()
        {
            var request = HttpContext.Current.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;

            if (!string.IsNullOrWhiteSpace(appUrl) && appUrl != "/") appUrl += "/";

            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }


        /*
         * To reslove relative paths to absolute ones
         * example: ResolveServerUrl(VirtualPathUtility.ToAbsolute("~/images/image1.gif"), false))
         * return: http://www.yourdomainname.com/images/image1.gif
         * source: http://stackoverflow.com/questions/2069922/getting-full-url-of-any-file-in-asp-net-mvc
         **/
        public static string ResolveServerUrl(string serverUrl, bool forceHttps)
        {
            if (serverUrl.IndexOf("://") > -1)
                return serverUrl;

            string newUrl = serverUrl;
            Uri originalUri = System.Web.HttpContext.Current.Request.Url;
            newUrl = (forceHttps ? "https" : originalUri.Scheme) +
                "://" + originalUri.Authority + newUrl;
            return newUrl;
        }
    }
}