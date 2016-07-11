using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.AsyncProg
{
    public class ProgressString : IProgress<string>
    {
        private string progressValue;
        public void Report(string value)
        {
            System.Console.WriteLine(value);
            progressValue = value;
        }

        public string GetProgressValue() { return progressValue; }
    }
}
