using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.AsyncProg
{
    public class AsyncTaskProg
    {
        public static void FindStockInfo(string symbol)
        {
            
            var stockData = StockDataDownloader.GetHistoricalData(symbol, 10);

            decimal max = stockData.Prices.Max();
            decimal min = stockData.Prices.Min();

        }

    }
}