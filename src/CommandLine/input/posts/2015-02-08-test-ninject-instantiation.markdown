Here's a version of [Rob Moore's class](https://robdmoore.id.au/blog/2012/05/29/controller-instantiation-testing/) for testing dependency resolution for MVC controllers.

In our team at [Frontiers](https://frontiersin.org), we have an Application [Service Layer](https://martinfowler.com/eaaCatalog/serviceLayer.html) that is partially shared between a console application and a REST API built with ASP.NET MVC. So for us (and I think it's a common need) it is important also to test that the dependencies of the application layer itself are correctly resolved.

Since we didn't have any common ancestor for our Application Services, we created an empty interface; but then the Code Analysis complained that [empty interfaces are not a good thing][CA1040], so we ended up using an attribute.

[CA1040]: https://msdn.microsoft.com/en-us/library/ms182128(v=VS.100).aspx

We also use MsTest (business rule, alleviated with [Fluent Assertions](https://www.fluentassertions.com/)), and [Ninject](https://www.ninject.org/). And this is the result:

```csharp
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace Frontiers.Ioc.Tests
{
    /// <summary>
    /// This tests try to instantiate all available controllers and application services using Ninject.
    /// The tests will fail if some binding is missing or incorrectly configured.
    /// </summary>
    /// <remarks>
    /// We are instantiating controllers and application services because these constitute the
    /// surface of our dependency injection calls, that is, Ninject is only called directly to
    /// instantiate these classes. If Ninject were to be called to instantiate other classes,
    /// those should be also included here.
    /// </remarks>
    [TestClass]
    public class IocTests
    {
        [TestMethod]
        public void MvcControllerTest()
        {
            IKernel kernel = BuildKernel();

            IEnumerable<Type> controllerTypes = 
                typeof(BaseController)
                .Assembly
                .GetTypes()
                .Where(t => 
                    typeof(BaseController).IsAssignableFrom(t) && 
                    !t.IsAbstract);

            foreach (Type controllerType in controllerTypes)
            {
                kernel.Get(controllerType);
            }
        }

        [TestMethod]
        public void ApplicationServiceTest()
        {
            IKernel kernel = BuildKernel();

            var controllerTypes = typeof(ApplicationServiceAttribute).Assembly.GetTypes()
                .Where(t => 
                    HasAttribute<ApplicationServiceAttribute>(t) &&
                    !t.IsAbstract);

            foreach (Type controllerType in controllerTypes)
            {
                kernel.Get(controllerType);
            }
        }

        private static bool HasAttribute<T>(Type t) where T : Attribute
        {
            return Attribute.GetCustomAttribute(
                t,
                typeof(T)) != null;
        }

		private IKernel BuildKernel()
		{
			// Here you must build your kernel and load any necessary modules.
		}
    }
}
```
