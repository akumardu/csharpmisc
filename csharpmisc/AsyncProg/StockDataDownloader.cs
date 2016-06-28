using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.AsyncProg
{
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading;

    // TODO: Test Exception rethrowing when task is garbage-collected
    // TODO: Test TaskUnobservedException event handler
    // TODO: Verify exception hierarchy flattenning
    // TODO: Test WaitAllOneByOne Pattern with exception handling
    // TODO: Test parameter passing via closure and as task parameter
    public class StockDataDownloader
    {
        /// <summary>
        /// External method for checking internet access:
        /// </summary>
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);


        /// <summary>
        /// C# callable method to check internet access
        /// </summary>
        public static bool IsConnectedToInternet()
        {
            int Description;
            return InternetGetConnectedState(out Description, 0);
        }


        /// <summary>
        /// Returns last 10 years of historical data for given stock symbol.
        /// </summary>
        /// <param name="symbol">stock ticker symbol, e.g. 'msft'</param>
        /// <returns></returns>
        public static StockData GetHistoricalData(string symbol, int numYearsOfHistory)
        {
            //
            // If we have an internet connection, download data live, otherwise check the cache
            // and see if we have the data available...
            //
            if (IsConnectedToInternet())
                return GetDataFromInternet(symbol, numYearsOfHistory);
            else
                return GetDataFromFileCache(symbol, numYearsOfHistory);
        }


        /// <summary>
        /// Tries to read stock data from file cache, presumably because internet is not available.
        /// 
        /// NOTE: file cache is a sub-folder "\cache" under the .exe.  The assumption is that it
        /// holds CSV files from http://finance.yahoo.com.
        /// </summary>
        private static StockData GetDataFromFileCache(string symbol, int numYearsOfHistory)
        {
            // simulate a web delay:
            Random random = new Random();
            int secs = random.Next(3);  // returns 0..2:
            secs += 3;  // 3..5:

            Thread.Sleep(secs * 1000);  // delay...

            // now retrieve from file cache:
            string url = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");
            url = Path.Combine(url, symbol + ".csv");

            if (!File.Exists(url))
                throw new FileNotFoundException("Internet access not available, and stock info not found in file cache");

            FileWebRequest FileRequestObject = (FileWebRequest)FileWebRequest.Create(url);
            WebResponse Response = FileRequestObject.GetResponse();

            //
            // cached finance.yahoo.com, data format:
            //
            //   Date (YYYY-MM-DD),Open,High,Low,Close,Volume,Adj Close
            //
            string dataSource = string.Format("file cache (http://finance.yahoo.com), daily Adj Close, {0} years",
                numYearsOfHistory);

            List<decimal> prices = GetData(Response, new char[] { ',' }, 6 /*Adj Close*/);

            return new StockData(dataSource, prices);
        }


        /// <summary>
        /// Tries to download historial data from 3 different web sites, and takes the data
        /// from the first site that responds.  Sites used:  nasdaq, yahoo, and msn (although
        /// msn only provides a year of weekly data, so others are preferred).
        ///
        /// NOTE: we use the async web methods BeginGetResponse and EndGetResponse to access 
        /// the web sites asynchronously, with a Task-based facade on top.  This has the advantages
        /// that (1) worker thread is not dedicated to request (i.e. different threads can
        /// initiate vs. handle callback), and (2) easier to cancel outstanding requests.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="numYearsOfHistory"></param>
        /// <returns></returns>
        private static StockData GetDataFromInternet(string symbol, int numYearsOfHistory)
        {
            //
            // initiate web requests:
            //
            var taskList = new List<Task<StockData>>();
            try
            {
                var tYahoo = GetDataFromYahooAsync(symbol, numYearsOfHistory);
                var tNasdaq = GetDataFromNasdaqAsync(symbol, numYearsOfHistory);
                var tMsn = GetDataFromMsnAsync(symbol, numYearsOfHistory);
                
                taskList.Add(tYahoo);
                taskList.Add(tNasdaq);
                taskList.Add(tMsn);

                while (taskList.Count > 0)
                {
                    int index = Task.WaitAny(taskList.ToArray());
                    if (taskList[index].Exception == null)
                    {
                        return taskList[index].Result;
                    }

                    taskList.RemoveAt(index);
                }

            }
            catch (AggregateException ae)
            {
                string msg = "";
                ae.Flatten();
                foreach(Exception ex in ae.InnerExceptions)
                {
                    msg += string.Format("Unable to initiate set of web requests ('{0}')", ex.Message);
                    System.Diagnostics.Debug.WriteLine(msg);
                }
                //throw new ApplicationException(msg);
            }

            // WaitAllOneByOne
            

            return null;
        }


        //
        // Infrastructure for async web processing:
        //
        class RequestState
        {
            //
            // Inputs:
            //
            public HttpWebRequest Request;
            public string DataSource;
            public char[] Separators;
            public int DataIndex;
            //
            // Outputs:
            //
            public StockData Result;
            public Exception Exception;
            public ManualResetEvent Done;

            public RequestState(HttpWebRequest req, string src, char[] seps, int index)
            {
                this.Request = req;
                this.DataSource = src;
                this.Separators = seps;
                this.DataIndex = index;

                this.Result = null;
                this.Exception = null;
                this.Done = new ManualResetEvent(false);
            }
        }

        static void WebResponseCallback(IAsyncResult iar)
        {
            RequestState state = (RequestState)iar.AsyncState;

            if (state.DataSource.Contains("yahoo"))
                System.Diagnostics.Debug.WriteLine("Yahoo callback on thread {0}.", Thread.CurrentThread.ManagedThreadId);
            else if (state.DataSource.Contains("nasdaq"))
                System.Diagnostics.Debug.WriteLine("Nasdaq callback on thread {0}.", Thread.CurrentThread.ManagedThreadId);
            else if (state.DataSource.Contains("msn"))
                System.Diagnostics.Debug.WriteLine("MSN callback on thread {0}.", Thread.CurrentThread.ManagedThreadId);

            //
            // Request has finished, let's process results:
            //
            try
            {
                WebResponse response = state.Request.EndGetResponse(iar);

                List<decimal> prices = GetData(response, state.Separators, state.DataIndex);

                if (prices.Count == 0)
                    throw new ApplicationException("site returned no data");

                state.Result = new StockData(state.DataSource, prices);
            }
            catch (Exception ex)
            {
                state.Exception = ex;
            }
            finally
            {
                // no matter what, make sure we release caller who is waiting:
                state.Done.Set();
            }
        }


        /// <summary>
        /// Tries to download data from Yahoo; this is an ASYNC method, caller must wait for it
        /// to complete. 
        /// </summary>
        private static Task<StockData> GetDataFromYahooAsync(string symbol, int numYearsOfHistory)
        {
            return Task.Factory.StartNew<StockData>(() =>
            {
                System.Diagnostics.Debug.WriteLine("Yahoo initiated on thread {0}.", Thread.CurrentThread.ManagedThreadId);

                //
                // finance.yahoo.com, data format:
                //
                //   Date (YYYY-MM-DD),Open,High,Low,Close,Volume,Adj Close
                //
                DateTime today = DateTime.Now;

                string url = string.Format("http://ichart.finance.yahoo.com/table.csv?s={0}&d={1}&e={2}&f={3}&g=d&a={1}&b={2}&c={4}&ignore=.csv",
                    symbol,
                    today.Month - 1,
                    today.Day - 1,
                    today.Year,
                    today.Year - numYearsOfHistory);

                //
                // Fire off web request:
                //
                HttpWebRequest WebRequestObject = (HttpWebRequest)HttpWebRequest.Create(url);

                RequestState state = new RequestState(
                    WebRequestObject,
                    string.Format("http://finance.yahoo.com, daily Adj Close, {0} years", numYearsOfHistory),
                    new char[] { ',' },
                    6 /*Adj Close*/
                );

                var webResponse = WebRequestObject.GetResponse();
                List<decimal> prices = GetData(webResponse, state.Separators, state.DataIndex);
                if (prices.Count == 0)
                    throw new ApplicationException("site returned no data");

                return new StockData(state.DataSource, prices);
            });
        }


        /// <summary>
        /// Tries to download data from Nasdaq; this is an ASYNC method, caller must wait for it
        /// to complete.
        /// </summary>
        private static Task<StockData> GetDataFromNasdaqAsync(string symbol, int numYearsOfHistory)
        {
            return Task.Factory.StartNew<StockData>(() =>
            {
                System.Diagnostics.Debug.WriteLine("Nasdaq initiated on thread {0}.", Thread.CurrentThread.ManagedThreadId);

                //
                // nasdaq.com, data format:
                //
                //   Date (MM-DD-YYYY)\tOpen\tHigh\tLow\tClose\tVolume\t
                //
                string url = string.Format("http://charting.nasdaq.com/ext/charts.dll?2-1-14-0-0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0|0,0,0,0,0-5120-03NA000000{0}-&SF:4|5-WD=539-HT=395--XXCL-",
                    symbol);

                //
                // Fire off web request:
                //
                HttpWebRequest WebRequestObject = (HttpWebRequest)HttpWebRequest.Create(url);

                RequestState state = new RequestState(
                    WebRequestObject,
                    string.Format("http://nasdaq.com, daily Close, {0} years", numYearsOfHistory),
                    new char[] { '\t' },
                    4 /*Close*/
                );

                var webResponse = WebRequestObject.GetResponse();
                List<decimal> prices = GetData(webResponse, state.Separators, state.DataIndex);
                if (prices.Count == 0)
                    throw new ApplicationException("site returned no data");

                return new StockData(state.DataSource, prices);
            });
            
        }


        /// <summary>
        /// Tries to download data from MSN; this is an ASYNC method, caller must wait for it
        /// to complete.
        /// 
        /// NOTE: MSN only returns 1 year of data, and weekly, so this result is not preferred.
        /// </summary>
        private static Task<StockData> GetDataFromMsnAsync(string symbol, int numYearsOfHistory)
        {
            return Task.Factory.StartNew<StockData>(() =>
            {
                System.Diagnostics.Debug.WriteLine("MSN initiated on thread {0}.", Thread.CurrentThread.ManagedThreadId);

                //
                // MSN, data format:
                //
                //   Date (MM-DD-YYYY),Open,High,Low,Close,Volume
                //
                // NOTE: MSN only provides one year of historical data, and only by week.
                //
                string url = string.Format("http://moneycentral.msn.com/investor/charts/chartdl.aspx?C1=0&C2=1&height=258&width=612&CE=0&symbol={0}&filedownloadbt.x=1",
                    symbol);

                //
                // Fire off web request:
                //
                HttpWebRequest WebRequestObject = (HttpWebRequest)HttpWebRequest.Create(url);

                RequestState state = new RequestState(
                    WebRequestObject,
                    "http://moneycentral.msn.com, weekly Close, 1 year",
                    new char[] { ',' },
                    4 /*Close*/
                );

                var webResponse = WebRequestObject.GetResponse();
                List<decimal> prices = GetData(webResponse, state.Separators, state.DataIndex);
                if (prices.Count == 0)
                    throw new ApplicationException("site returned no data");

                return new StockData(state.DataSource, prices);
            });
        }


        /// <summary>
        /// Opens given data stream and reads the data; could be from the web, or a local file.
        /// Note that the given Response stream is closed for you before returning.
        /// </summary>
        /// <param name="Response">stream to read (closed upon completion)</param>
        /// <param name="separators">char(s) that delimit data fields</param>
        /// <param name="dataIndex">0-based index of price field of interest (open, close, etc.)</param>
        /// <returns></returns>
        private static List<decimal> GetData(WebResponse Response, char[] separators, int dataIndex)
        {
            //
            // Open data stream and download/read the data:
            //
            try
            {
                List<decimal> prices = new List<decimal>();

                using (Stream WebStream = Response.GetResponseStream())
                {
                    using (StreamReader Reader = new StreamReader(WebStream))
                    {

                        //
                        // Read data stream:
                        //
                        while (!Reader.EndOfStream)
                        {
                            string record = Reader.ReadLine();
                            string[] tokens = record.Split(separators);

                            //
                            // valid records start with a date:
                            //
                            DateTime date;
                            decimal data;

                            if (DateTime.TryParse(tokens[0], out date))
                                if (Decimal.TryParse(tokens[dataIndex], out data))
                                    prices.Add(data);
                        }//while

                    }//using--Reader
                }//using--WebStream

                //
                // return list of historical prices:
                //
                return prices;

            }
            finally
            {
                try // ensure response stream is closed before return:
                {
                    Response.Close();
                }
                catch
                { /*ignore*/ }
            }
        }

    }

    public class StockData
    {
        public string DataSource { get; private set; }
        public List<decimal> Prices { get; private set; }

        public StockData(string dataSource, List<decimal> prices)
        {
            this.DataSource = dataSource;
            this.Prices = prices;
        }
    }
}
