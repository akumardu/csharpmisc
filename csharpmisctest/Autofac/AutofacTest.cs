using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csharpmisctest.Autofac
{
    [TestClass]
    public class AutofacTest
    {
        [TestMethod]
        public void BasicTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MultiConstrSameParamComponent>().As<IComponent>();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var component = container.Resolve<IComponent>();
                var str = component.SomeRandomMethod1("a","b");
                Assert.IsNotNull(str);
            }
        }
    }
}
