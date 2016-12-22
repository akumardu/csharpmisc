using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csharpmisctest
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Runtime.Remoting;
    using System.Text;

    [TestClass]
    public class Dynamic
    {
        [TestMethod]
        public void TestExpandoObject()
        {
            dynamic rd = new ExpandoObject();

            // Add properties dynamically to the object
            rd.Id = 42;
            rd.sb = new StringBuilder("a string builder");

            // Can also cast to Dictionary and add key/value pairs
            var c = (IDictionary<string, object>)rd;
            c.Add("FirstName", "Amar");
            c.Add("SecondName", "Dubedy");

            // Access keys as properties
            Console.WriteLine(rd.FirstName);

            Console.WriteLine("Properties");
            // Iterate over all keys
            foreach (var item in rd)
            {
                Console.WriteLine($"{item.Key} : {item.Value}");
            }

            // Can also add functions dynamically to the object
            rd.Print = (Action)(() =>
                {
                    Console.WriteLine("Properties in action");
                    foreach (var item in rd)
                    {
                        Console.WriteLine($"{item.Key} : {item.Value}");
                    }
                });

            // Call the dynamically added functions
            rd.Print();
        }
    }
}
