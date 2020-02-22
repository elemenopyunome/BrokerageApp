using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YahooFinance.NET;
using System.Net;
using System.Web;
using System.Collections;
using System.Reflection;

namespace BrokerageApp
{
    public partial class BrokerageMainForm : Form
    {
        public BrokerageMainForm()
        {
            InitializeComponent();
        }
        public static string HttpGet(string URI)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
            //req.Proxy = new System.Net.WebProxy(ProxyString, true); //true means no proxy
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }
        public List<Cookie> GetCookieListFromHTTPWebRequest(CookieContainer container)
        {
            var cookies = new List<Cookie>();
            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                BindingFlags.NonPublic |
                BindingFlags.GetField |
                BindingFlags.Instance,
                null,
                container,
                null);

            foreach (string key in table.Keys)
            {
                var item = table[key];
                var items = (ICollection)item.GetType().GetProperty("Values").GetGetMethod().Invoke(item, null);
                foreach (CookieCollection cc in items)
                {
                    foreach (Cookie cookie in cc)
                    {
                        cookies.Add(cookie);
                    }
                }
            }

            return cookies;
        }
        /// <summary>
        /// This will get the Yahoo Finance B Value cookie needed for query - Elemenopy
        /// </summary>
        /// <returns></returns>
        public string GetYahooBCookieValueByName()
        {
            string urlToCheck = "https://finance.yahoo.com/quote/WFC?p=WFC&.tsrc=fin-srch";
            //string urlToCheck = "https://www.google.com";
            var request = (HttpWebRequest)HttpWebRequest.Create(urlToCheck);
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)";
            request.CookieContainer = new CookieContainer();

            var response = request.GetResponse();
            CookieContainer cookies = request.CookieContainer;
            List<Cookie> cookielist = GetCookieListFromHTTPWebRequest(cookies);
            
            string ValueToReturn = "";
            foreach (Cookie checkcookie in cookielist)
            {
                if (checkcookie.Name == "B")
                {
                    ValueToReturn = checkcookie.Value;
                }
            }
            return ValueToReturn;
        }
        private void button1_Click(object sender, EventArgs e)
        {

            string cookie = GetYahooBCookieValueByName();
            string crumb = "enp5cLfls8Q";


            string exchange = "NYSE";
            string symbol = "enp5cLfls8Q";

            YahooFinanceClient yahooFinance = new YahooFinanceClient(cookie, crumb);
            string yahooStockCode = yahooFinance.GetYahooStockCode(exchange, symbol);
            List<YahooHistoricalPriceData> yahooPriceHistory = yahooFinance.GetDailyHistoricalPriceData(yahooStockCode);
            List<YahooHistoricalDividendData> yahooDividendHistory = yahooFinance.GetHistoricalDividendData(yahooStockCode);
            YahooRealTimeData yahooRealTimeData = yahooFinance.GetRealTimeData(yahooStockCode);
        }
    }
}
