﻿using Certify.Management;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Certify.Core.Tests
{
    [TestClass]
    /// <summary>
    /// Integration tests for IIS Manager 
    /// </summary>
    public class IISManagerTests : IntegrationTestBase, IDisposable
    {
        private IISManager iisManager;
        private string testSiteName = "Test2CertRequest";
        private string testSiteDomain = "integration2." + PrimaryTestDomain;

        public IISManagerTests()
        {
            iisManager = new IISManager();

            //perform setup for IIS
            SetupIIS();
        }

        /// <summary>
        /// Perform teardown for IIS 
        /// </summary>
        public void Dispose()
        {
            TeardownIIS();
        }

        public void SetupIIS()
        {
            if (iisManager.SiteExists(testSiteName))
            {
                iisManager.DeleteSite(testSiteName);
            }
            iisManager.CreateSite(testSiteName, testSiteDomain, PrimaryIISRoot, "DefaultAppPool");
            Assert.IsTrue(iisManager.SiteExists(testSiteName));
        }

        public void TeardownIIS()
        {
            iisManager.DeleteSite(testSiteName);
            Assert.IsFalse(iisManager.SiteExists(testSiteName));
        }

        [TestMethod]
        public void TestSiteExists()
        {
            //site exists and matches required domain
            var site = iisManager.GetSiteByDomain(testSiteDomain);
            Assert.AreEqual(site.Name, testSiteName);
        }

        [TestMethod]
        public void TestIISVersionCheck()
        {
            var version = iisManager.GetIisVersion();
            Assert.IsTrue(version.Major >= 7);
        }

        [TestMethod]
        public void TestIISSiteRunning()
        {
            var site = iisManager.GetSiteByDomain(testSiteDomain);

            //this site should be running
            bool isRunning = iisManager.IsSiteRunning(site.Id.ToString());
            Assert.IsTrue(isRunning);

            //this site should not be running
            isRunning = iisManager.IsSiteRunning("MadeUpSiteName");
            Assert.IsFalse(isRunning);
        }

        [TestMethod]
        public void TestGetBinding()
        {
            var b = iisManager.GetSiteBindingByDomain(testSiteDomain);
            Assert.AreEqual(b.Host, testSiteDomain);

            b = iisManager.GetSiteBindingByDomain("randomdomain.com");
            Assert.IsNull(b);
        }

        [TestMethod]
        public void TestCreateUnusalBindings()
        {
            //delete test if it exists
            iisManager.DeleteSite("MSMQTest");

            // create net.msmq://localhost binding, no port or ip
            iisManager.CreateSite("MSMQTest", "localhost", PrimaryIISRoot, null, protocol: "net.msmq", ipAddress: null, port: null);

            var sites = iisManager.GetSiteBindingList(false);
        }

        [TestMethod]
        public void TestTooManyBindings()
        {
            //delete test if it exists
            if (iisManager.SiteExists("ManyBindings"))
            {
                iisManager.DeleteSite("ManyBindings");
            }

            // create net.msmq://localhost binding, no port or ip
            iisManager.CreateSite("ManyBindings", "toomany.com", PrimaryIISRoot, null, protocol: "http");
            var site = iisManager.GetSiteBindingByDomain("toomany.com");
            List<string> domains = new List<string>();
            for (var i = 0; i < 10000; i++)
            {
                domains.Add(Guid.NewGuid().ToString() + ".toomany.com");
            }
            iisManager.AddSiteBindings(site.SiteId, domains);
        }

        [TestMethod]
        public void TestPrimarySites()
        {
            //get all sites
            var sites = iisManager.GetPrimarySites(includeOnlyStartedSites: false);
            Assert.IsTrue(sites.Any());

            //get all sites excluding stopped sites
            sites = iisManager.GetPrimarySites(includeOnlyStartedSites: true);
            Assert.IsTrue(sites.Any());
        }
    }
}