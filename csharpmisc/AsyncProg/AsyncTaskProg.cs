using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.AsyncProg
{
    using System.Threading;

    public class AsyncTaskProg
    {
        public static void FindStockInfo(string symbol)
        {
            
            var stockData = StockDataDownloader.GetHistoricalData(symbol, 10);

            decimal max = stockData.Prices.Max();
            decimal min = stockData.Prices.Min();

            decimal avg = 0;
            int count = 0;
            foreach (var price in stockData.Prices)
            {
                avg = (avg * count + price) / (count + 1);
                count++;
            }

            decimal stddev = 0;
            foreach (var price in stockData.Prices)
            {
                stddev += Math.Abs(avg - price) / count;
            }

            Console.WriteLine("Max: {0}", max);
            Console.WriteLine("Min: {0}", min);
            Console.WriteLine("Avg: {0}", avg);
            Console.WriteLine("Std Dev: {0}", stddev);

        }

    }
}