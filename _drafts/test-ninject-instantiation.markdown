---
layout: post
title: "Test Ninject instantiation"
date: 2015-02-06
comments: true
categories: [tfs,git]
---
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Impact.Application.Contracts.Services;
using Impact.API;
using Impact.API.Controllers;
using Impact.IoC;
using Impact.Utils.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace Impact.Ioc.Tests
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
            IKernel kernel = new NinjectKernelBuilder(new FakeConfigurationManager()).BuildKernel();
            kernel.Load<MvcNinjectModule>();

            IEnumerable<Type> controllerTypes = 
                typeof(BaseController)
                .Assembly
                .GetTypes()
                .Where(t => 
                    typeof(BaseController).IsAssignableFrom(t) && 
                    !t.IsAbstract && 
                    t != typeof(ErrorController));

            foreach (Type controllerType in controllerTypes)
            {
                kernel.Get(controllerType);
            }
        }

        [TestMethod]
        public void ApplicationServiceTest()
        {
            IKernel kernel = new NinjectKernelBuilder(new FakeConfigurationManager()).BuildKernel();

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

        private class FakeConfigurationManager : IConfigurationManager
        {
            public string GetAppSetting(string key)
            {
                switch (key)
                {
                    case "ExternalServicesPitsBasePath":
                        return "http://example.com";
                    default:
                        return "fakesetting";
                }
            }

            public ConnectionStringSettings GetConnectionString(string key)
            {
                return new ConnectionStringSettings(key, "fakeconnectionstring");
            }
        }
    }
}