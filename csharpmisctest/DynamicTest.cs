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
    using Microsoft.CSharp.RuntimeBinder;
    using Newtonsoft.Json;

    using IronPython.Hosting;
    using Microsoft.Scripting;
    using Microsoft.Scripting.Hosting;

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
            // string result = CommonMathDynamic.Add(1,4); //because it can't convert dynamic int result to string

            Console.WriteLine($"CommonMathDynamic basic version{CommonMathDynamicWithGenerics.Add(double1, double2)}");
            Console.WriteLine($"CommonMathDynamic basic version{CommonMathDynamicWithGenerics.Add(int1, int2)}");

            // Throws compile-time error
            // string result = CommonMathDynamicWithGenerics.Add(1, 4); // because it can't convert dynamic int result to string
            //short result = CommonMathDynamicWithGenericsExplicitCast.Add((short)1, (short)4); // runtime error because short is implicitly converted to int

            Console.WriteLine($"CommonMathDynamic basic version{CommonMathDynamicWithGenericsExplicitCast.Add(double1, double2)}");
            Console.WriteLine($"CommonMathDynamic basic version{CommonMathDynamicWithGenericsExplicitCast.Add(int1, int2)}");

            short result = CommonMathDynamicWithGenericsExplicitCast.Add((short)1, (short)4); 
        }

        [TestMethod]
        public void TestComInterop()
        {
            Type excelType = Type.GetTypeFromProgID("Excel.Application", true);
            dynamic excel = Activator.CreateInstance(excelType);
            excel.Visible = true;
            excel.Workbooks.Add();

            dynamic defaultWorksheet = excel.ActiveSheet;

            defaultWorksheet.Cells[1, "A"] = "This is the Name column";
            defaultWorksheet.Columns[1].AutoFit();
        }

        [TestMethod]
        public void TestDynamicJson()
        {
            string customerJson = "{'FirstName': 'Parul', 'SecondName': 'Mishra'}";

            dynamic c = JsonConvert.DeserializeObject(customerJson);
            Console.WriteLine($"Customer is: {c.FirstName} {c.SecondName}");
        }

        [TestMethod]
        public void TestShouldStoreTagName()
        {
            var image = new HtmlElement("img");

            Assert.AreEqual("img", image.TagName);
        }

        [TestMethod]
        public void TestShouldAddAttributeNameAndValueDynamically()
        {
            dynamic image = new HtmlElement("img");

            image.src = "car.png";

            Assert.AreEqual("car.png", image.src);
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException), "RuntimeBinderException expected")]
        public void TestShouldErrorIfAttributeNotSet()
        {
            dynamic image = new HtmlElement("img");

            var value = image.src;
        }

        [TestMethod]
        public void TestShouldReturnDynamicMemberNames()
        {
            dynamic img = new HtmlElement("img");
            img.src = "car.png";
            img.alt = "a blue car";
            string[] members = img.GetDynamicMemberNames();

            Assert.AreEqual(2, members.Length);
            Assert.AreEqual("src", members[0]);
            Assert.AreEqual("alt", members[1]);
        }

        [TestMethod]
        public void TestShouldOutputTagHtml()
        {
            dynamic img = new HtmlElement("img");
            img.src = "car.png";
            img.alt = "a blue car";

            var html = img.ToString();

            Assert.AreEqual(html, "<img src='car.png' alt='a blue car' />");
        }

        [TestMethod]
        public void TestShouldRenderHtml()
        {
            dynamic img = new HtmlElement("img");
            img.src = "car.png";
            img.alt = "a blue car";

            var html = img.Render();

            Assert.AreEqual(html, "<img src='car.png' alt='a blue car' />");
        }

        [TestMethod]
        public void TestBasicPythonInterop()
        {
            ScriptEngine engine = Python.CreateEngine();

            string simpleExpression = "2+2";

            dynamic dynamicResult = engine.Execute(simpleExpression);

            Assert.AreEqual(dynamicResult, 4);
        }

        [TestMethod]
        public void TestScopedPythonInterop()
        {
            ScriptEngine engine = Python.CreateEngine();
            int age = 42;

            ScriptScope scope = engine.CreateScope();
            scope.SetVariable("a", age);

            string expression = "1 < a < 50";

            ScriptSource source = engine.CreateScriptSourceFromString(expression, SourceCodeKind.Expression);

            dynamic result = source.Execute(scope);

            Assert.IsTrue(result);
        }
    }
}
