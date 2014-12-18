﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Expenses.ExpenseClaims;
using Expenses.ExpenseEmployees;
using Expenses.Fixtures;
using Expenses.RecordedActions;
using Expenses.Services;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcTestApp.Tests.Util;
using NakedObjects.Mvc.Test.Data;
using NakedObjects.Persistor.Entity.Configuration;
using NakedObjects.Web.Mvc.Html;
using NakedObjects.Xat;

namespace MvcTestApp.Tests.Helpers {
    [TestClass]
    public class SystemHelperTest : AcceptanceTestCase {
        #region Setup/Teardown

        private static bool runFixtures;

        private void RunFixturesOnce() {
            if (!runFixtures) {
                RunFixtures();
                runFixtures = true;
            }
        }

        [TestInitialize]
        public void SetupTest() {
            InitializeNakedObjectsFramework(this);
            RunFixturesOnce();
            StartTest();
            controller = new DummyController();
            mocks = new ContextMocks(controller);
            SetUser("sven");
            SetupViewData();
        }

        #endregion

        protected override void RegisterTypes(IUnityContainer container) {
            base.RegisterTypes(container);
            var config = new EntityObjectStoreConfiguration {EnforceProxies = false};
            config.UsingCodeFirstContext(() => new MvcTestContext("SystemHelperTest"));
            container.RegisterInstance<IEntityObjectStoreConfiguration>(config, (new ContainerControlledLifetimeManager()));
        }

        [ClassInitialize]
        public static void SetupTestFixture(TestContext tc) {
            Database.SetInitializer(new DatabaseInitializer());
        }

        [ClassCleanup]
        public static void TearDownTest() {
            Database.Delete("SystemHelperTest");
        }

        private DummyController controller;
        private ContextMocks mocks;

        private void SetupViewData() {
            mocks.ViewDataContainer.Object.ViewData[IdHelper.NofServices] = NakedObjectsFramework.GetServices();
            mocks.ViewDataContainer.Object.ViewData[IdHelper.NoFramework] = NakedObjectsFramework;
        }

        protected override Type[] Types {
            get {
                var types1 = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "NakedObjects.Mvc.Test.Data").
                    GetTypes().Where(t => t.FullName.StartsWith("Expenses") && !t.FullName.Contains("Repository")).ToArray();

                var types2 = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "NakedObjects.Mvc.Test.Data").
                    GetTypes().Where(t => t.FullName.StartsWith("MvcTestApp.Tests.Helpers") && t.IsPublic).ToArray();

                return types1.Union(types2).ToArray();
            }
        }

        protected override object[] MenuServices {
            get { return (DemoServicesSet.ServicesSet()); }
        }

        protected override object[] ContributedActions {
            get { return (new object[] {new RecordedActionContributedActions()}); }
        }

        protected override object[] Fixtures {
            get { return (DemoFixtureSet.FixtureSet()); }
        }

     

        private class DummyController : Controller {}


        private static string GetTestData(string name) {
            var file = Path.Combine(@"..\..\Generated Html reference files", name) + ".htm";
            return File.ReadAllText(file);
        }

        private static bool writeTest = false;

        private static void WriteTestData(string name, string data) {
            string file = Path.Combine(@"..\..\Generated Html reference files", name) + ".htm";
            File.WriteAllText(file, data);
        }

        private static void CheckResults(string resultsFile, string s) {
            if (writeTest) {
                WriteTestData(resultsFile, s);
            }
            else {
                string actionView = GetTestData(resultsFile).StripWhiteSpace();
                Assert.AreEqual(actionView, s.StripWhiteSpace());
            }
        }

        [TestMethod]
        [Ignore] // doesn't work now uses urls which are empty in tests
        public void Cancel() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();
            Employee emp = NakedObjectsFramework.Persistor.Instances<Employee>().First();


            mocks.HtmlHelper.History(claim);
            mocks.HtmlHelper.History(emp);
            string s = mocks.HtmlHelper.CancelButton(null).ToString();
            string fieldView = GetTestData("Cancel");
            Assert.AreEqual(fieldView, s);
        }

        [TestMethod]
        public void History() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();
            Employee emp = NakedObjectsFramework.Persistor.Instances<Employee>().First();

            mocks.HtmlHelper.History(claim);
            mocks.HtmlHelper.History(emp);
            string s = mocks.HtmlHelper.History().StripWhiteSpace();
            CheckResults("History", s);
        }

        [TestMethod]
        public void HistoryWithCount1() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();

            Employee emp1 = NakedObjectsFramework.Persistor.Instances<Employee>().OrderBy(c => c.Id).First();
            Employee emp2 = NakedObjectsFramework.Persistor.Instances<Employee>().OrderByDescending(c => c.Id).First();


            mocks.HtmlHelper.History(emp2);
            mocks.HtmlHelper.History(claim);
            mocks.HtmlHelper.History(emp1);

            string s = mocks.HtmlHelper.History(3).StripWhiteSpace();
            CheckResults("HistoryWithCount", s);
        }

        [TestMethod]
        public void HistoryWithCount2() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();
            Employee emp1 = NakedObjectsFramework.Persistor.Instances<Employee>().OrderBy(c => c.Id).First();
            Employee emp2 = NakedObjectsFramework.Persistor.Instances<Employee>().OrderByDescending(c => c.Id).First();


            mocks.HtmlHelper.History(emp2);
            mocks.HtmlHelper.History(claim);
            mocks.HtmlHelper.History(emp1);

            string s = mocks.HtmlHelper.History(2).StripWhiteSpace();
            CheckResults("History", s);
        }

        // too hard to mock appropriately - rely on selenium tests
        [TestMethod, Ignore]
        public void TabbedHistory() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();
            Employee emp = NakedObjectsFramework.Persistor.Instances<Employee>().First();

            mocks.HtmlHelper.TabbedHistory(claim);
            mocks.HtmlHelper.TabbedHistory(emp);
            string s = mocks.HtmlHelper.TabbedHistory().StripWhiteSpace();
            CheckResults("TabbedHistory", s);
        }

        [TestMethod, Ignore]
        public void TabbedHistoryWithCount1() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();
            Employee emp1 = NakedObjectsFramework.Persistor.Instances<Employee>().OrderBy(c => c.Id).First();
            Employee emp2 = NakedObjectsFramework.Persistor.Instances<Employee>().OrderByDescending(c => c.Id).First();


            mocks.HtmlHelper.TabbedHistory(emp2);
            mocks.HtmlHelper.TabbedHistory(claim);
            mocks.HtmlHelper.TabbedHistory(emp1);

            string s = mocks.HtmlHelper.TabbedHistory(3).StripWhiteSpace();
            CheckResults("TabbedHistoryWithCount", s);
        }

        [TestMethod, Ignore]
        public void TabbedHistoryWithCount2() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();
            Employee emp1 = NakedObjectsFramework.Persistor.Instances<Employee>().OrderBy(c => c.Id).First();
            Employee emp2 = NakedObjectsFramework.Persistor.Instances<Employee>().OrderByDescending(c => c.Id).First();


            mocks.HtmlHelper.TabbedHistory(emp2);
            mocks.HtmlHelper.TabbedHistory(claim);
            mocks.HtmlHelper.TabbedHistory(emp1);

            string s = mocks.HtmlHelper.TabbedHistory(2).StripWhiteSpace();
            CheckResults("TabbedHistory", s);
        }
    }
}