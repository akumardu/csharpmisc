using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWcfService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting tests");
            using (var client = new IISHostedService.Service1Client())
            {
                int i = 0;
                while (i < 1)
                {
                    try
                    {
                        IISHostedService.CompositeType t = new IISHostedService.CompositeType { BoolValue = true, StringValue = "hello" };
                        //var result = client.GetDataUsingDataContract(t);
                        //Console.WriteLine(result.StringValue);

                        var result2 = client.GetDataUsingDataContract(t);
                        Console.WriteLine(result2.StringValue);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    i++;
                }
                while (i < 2)
                {
                    try
                    {
                        IISHostedService.CompositeType t = new IISHostedService.CompositeType { BoolValue = true, StringValue = "hello" };
                        //var result = client.GetDataUsingDataContract(t);
                        //Console.WriteLine(result.StringValue);

                        t.BoolValue = false;
                        var result2 = client.GetDataUsingDataContract(t);
                        Console.WriteLine(result2.StringValue);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    i++;
                }
            }
        }
    }
}
