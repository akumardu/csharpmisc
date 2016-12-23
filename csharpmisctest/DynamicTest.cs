using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csharpmisctest
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Text;

    using csharpmisc.Dynamic;

    [TestClass]
    public class DynamicTest
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

        [TestMethod]
        public void TestDynamicVsReflection()
        {
            // Using reflection
            StringBuilder sb = new StringBuilder();
            sb.GetType()
                .InvokeMember("AppendLine", BindingFlags.InvokeMethod, null, sb, new object[] { "Reflection !" });

            Console.WriteLine(sb);

            // using Dynamic
            StringBuilder dsb = new StringBuilder();
            dynamic dynamicsb = (dynamic)dsb;
            dynamicsb.AppendLine("Dynamic !");
            Console.WriteLine(dsb);
        }

        [TestMethod]
        public void TestUnifiedNumericMethods()
        {
            double double1 = 10, double2 = 20;
            int int1 = 11, int2 = 22;

            Console.WriteLine($"CommonMathDynamic basic version{CommonMathDynamic.Add(double1, double2)}");
            Console.WriteLine($"CommonMathDynamic basic version{CommonMathDynamic.Add(int1, int2)}");

            // Throws run-time error
            // string result = CommonMathDynamic.Add(1,4) ; because it can't convert dynamic int result to string


        }
    }
}
