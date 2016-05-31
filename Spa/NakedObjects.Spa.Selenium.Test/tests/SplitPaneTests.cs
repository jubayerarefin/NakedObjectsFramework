﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System.Threading;

namespace NakedObjects.Web.UnitTests.Selenium
{
    public abstract class SplitPaneTestsRoot : AWTest
    {

        #region Actions that go from single to split panes

        public virtual void RightClickActionReturningObjectFromHomeSingle()
        {
            Url(CustomersMenuUrl);
            WaitForView(Pane.Single, PaneType.Home, "Home");
            wait.Until(d => d.FindElements(By.CssSelector(".action")).Count == CustomerServiceActions);
            OpenActionDialog("Find Customer By Account Number");
            ClearFieldThenType(".value  input","AW00022262");
            RightClick(OKButton());
            WaitForView(Pane.Left, PaneType.Home, "Home");
            WaitForView(Pane.Right, PaneType.Object, "Marcus Collins, AW00022262");
            //Check that dialog is still open on the left:
            WaitForCss("#pane1 .dialog");
        }

        public virtual void RightClickActionReturningListFromHomeSingle()
        {
            Url(OrdersMenuUrl);
            WaitForView(Pane.Single, PaneType.Home, "Home");
            RightClick(GetObjectAction("Highest Value Orders"));
            WaitForView(Pane.Left, PaneType.Home, "Home");
            WaitForView(Pane.Right, PaneType.List, "Highest Value Orders");
        }


        public virtual void RightClickReferenceFromListSingle()
        {
            Url(OrdersMenuUrl);
            Click(GetObjectAction("Highest Value Orders"));
            WaitForView(Pane.Single, PaneType.List, "Highest Value Orders");
            var row = WaitForCss("table .reference");
            Assert.AreEqual("SO51131", row.Text);
            RightClick(row);
            WaitForView(Pane.Left, PaneType.List, "Highest Value Orders");
            WaitForView(Pane.Right, PaneType.Object, "SO51131");
        }


        public virtual void RightClickReferencePropertyFromObjectSingle()
        {
            GeminiUrl("object?o1=___1.Store--350&as1=open");
            WaitForView(Pane.Single, PaneType.Object, "Twin Cycles");
            var reference = GetReferenceProperty("Sales Person", "Lynn Tsoflias");
            RightClick(reference);
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles");
            WaitForView(Pane.Right, PaneType.Object, "Lynn Tsoflias");
        }


        public virtual void RightClickActionFromObjectSingle()
        {
            GeminiUrl("object?o1=___1.Customer--30116&as1=open");
            WaitForView(Pane.Single, PaneType.Object, "Technical Parts Manufacturing, AW00030116");
            OpenSubMenu("Orders");
            RightClick(GetObjectAction("Last Order"));
            WaitForView(Pane.Left, PaneType.Object, "Technical Parts Manufacturing, AW00030116");
            WaitForView(Pane.Right, PaneType.Object, "SO67279");
        }


        public virtual void RightClickHomeIconFromObjectSingle()
        {
            GeminiUrl("object?o1=___1.Store--350&as1=open");
            WaitForView(Pane.Single, PaneType.Object, "Twin Cycles");
            RightClick(HomeIcon());
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles");
            WaitForView(Pane.Right, PaneType.Home, "Home");
        }


        public virtual void SwapPanesIconFromSingleOpensHomeOnLeft()
        {
            GeminiUrl("object?o1=___1.Store--350&as1=open");
            WaitForView(Pane.Single, PaneType.Object, "Twin Cycles");
            Click(SwapIcon());
            WaitForView(Pane.Left, PaneType.Home, "Home");
            WaitForView(Pane.Right, PaneType.Object, "Twin Cycles");
        }
        #endregion

        #region Actions within split panes
        private const string twoObjects = GeminiBaseUrl + "object/object?o1=___1.Customer--555&as1=open&o2=___1.SalesOrderHeader--71926&as2=open";
        private const string twoObjectsB = GeminiBaseUrl + "object/object?o1=___1.Store--350&as1=open&o2=___1.SalesOrderHeader--71926&as2=open";


        public virtual void RightClickReferenceInLeftPaneObject()
        {
            Url(twoObjects);
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles, AW00000555");
            WaitForView(Pane.Right, PaneType.Object, "SO71926");
            var reference = GetReferenceProperty("Sales Territory", "Australia", Pane.Left);
            RightClick(reference);
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles, AW00000555");
            WaitForView(Pane.Right, PaneType.Object, "Australia");
        }


        public virtual void ClickReferenceInLeftPaneObject()
        {
            Url(twoObjects);
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles, AW00000555");
            WaitForView(Pane.Right, PaneType.Object, "SO71926");

            var reference = GetReferenceProperty("Sales Territory", "Australia", Pane.Left);
            Click(reference);

            WaitForView(Pane.Left, PaneType.Object, "Australia");
            WaitForView(Pane.Right, PaneType.Object, "SO71926");
        }


        public virtual void ClickReferenceInRightPaneObject()
        {
            Url(twoObjectsB);
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles");
            WaitForView(Pane.Right, PaneType.Object, "SO71926");

            var reference = GetReferenceProperty("Billing Address", "2253-217 Palmer Street ...", Pane.Right);
            Click(reference);

            WaitForView(Pane.Right, PaneType.Object, "2253-217 Palmer Street ...");
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles");
        }


        public virtual void RightClickReferenceInRightPaneObject()
        {
            Url(twoObjects);
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles, AW00000555");
            WaitForView(Pane.Right, PaneType.Object, "SO71926");
            var reference = GetReferenceProperty("Ship Method", "CARGO TRANSPORT 5", Pane.Right);
            RightClick(reference);
            WaitForView(Pane.Right, PaneType.Object, "SO71926");
            WaitForView(Pane.Left, PaneType.Object, "CARGO TRANSPORT 5");
        }


        public virtual void LeftClickHomeIconFromSplitObjectObject()
        {
            Url(twoObjects);
            Click(HomeIcon());
            WaitForView(Pane.Left, PaneType.Home, "Home");
            WaitForView(Pane.Right, PaneType.Object, "SO71926");
        }


        public virtual void RightClickHomeIconFromSplitObjectObject()
        {
            Url(twoObjects);
            RightClick(HomeIcon());
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles, AW00000555");
            WaitForView(Pane.Right, PaneType.Home, "Home");
        }


        public virtual void ActionDialogOpensInCorrectPane()
        {
            GeminiUrl("object/object?i1=View&o1=___1.Customer--543&i2=View&o2=___1.SalesOrderHeader--56672&as1=open&as2=open");
            WaitForView(Pane.Left, PaneType.Object, "Friendly Neighborhood Bikes, AW00000543");
            WaitForView(Pane.Right, PaneType.Object, "SO56672");
            OpenSubMenu("Orders", Pane.Left);
            OpenActionDialog("Create New Order", Pane.Left);
            Thread.Sleep(500);
            CancelDialog(Pane.Left);
            OpenActionDialog("Add New Sales Reasons", Pane.Right);
            Thread.Sleep(500);
            CancelDialog(Pane.Right);
        }


        public virtual void RightClickIsSameAsLeftClickForOpeningDialog()
        {
            Url(twoObjects);
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles, AW00000555");
            WaitForView(Pane.Right, PaneType.Object, "SO71926");
            OpenSubMenu("Orders", Pane.Left);
            RightClick(GetObjectAction("Create New Order", Pane.Left));
            var selector = CssSelectorFor(Pane.Left) + " .dialog ";
            var dialog = wait.Until(d => d.FindElement(By.CssSelector(selector)));
        }


        public virtual void SwapPanes()
        {
            Url(twoObjects);
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles, AW00000555");
            WaitForView(Pane.Right, PaneType.Object, "SO71926");
            Click(SwapIcon());
            WaitForView(Pane.Left, PaneType.Object, "SO71926");
            WaitForView(Pane.Right, PaneType.Object, "Twin Cycles, AW00000555");
        }
        public virtual void FullPaneFromLeft()
        {
            Url(twoObjects);
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles, AW00000555");
            WaitForView(Pane.Right, PaneType.Object, "SO71926");
            Click(FullIcon());
            WaitForView(Pane.Single, PaneType.Object, "Twin Cycles, AW00000555");
        }


        public virtual void FullPaneFromRight()
        {
            Url(twoObjects);
            WaitForView(Pane.Left, PaneType.Object, "Twin Cycles, AW00000555");
            WaitForView(Pane.Right, PaneType.Object, "SO71926");
            RightClick(FullIcon());
            WaitForView(Pane.Single, PaneType.Object, "SO71926");
        }


        #endregion

    }
    public abstract class SplitPaneTests : SplitPaneTestsRoot
    {
        #region Actions that go from single to split panes
        [TestMethod]
        public override void RightClickActionReturningObjectFromHomeSingle()
        {
            base.RightClickActionReturningObjectFromHomeSingle();
        }

        [TestMethod]
        public override void RightClickActionReturningListFromHomeSingle()
        {
            base.RightClickActionReturningListFromHomeSingle();
        }

        [TestMethod]
        public override void RightClickReferenceFromListSingle()
        {
            base.RightClickReferenceFromListSingle();
        }

        [TestMethod]
        public override void RightClickReferencePropertyFromObjectSingle()
        {
            base.RightClickReferencePropertyFromObjectSingle();
        }

        [TestMethod]
        public override void RightClickActionFromObjectSingle()
        {
            base.RightClickActionFromObjectSingle();
        }

        [TestMethod]
        public override void RightClickHomeIconFromObjectSingle()
        {
            base.RightClickHomeIconFromObjectSingle();
        }

        [TestMethod]
        public override void SwapPanesIconFromSingleOpensHomeOnLeft()
        {
            base.SwapPanesIconFromSingleOpensHomeOnLeft();
        }
        #endregion

        #region Actions within split panes
        private const string twoObjects = GeminiBaseUrl + "object/object?o1=___1.Customer--555&as1=open&o2=___1.SalesOrderHeader--71926&as2=open";
        private const string twoObjectsB = GeminiBaseUrl + "object/object?o1=___1.Store--350&as1=open&o2=___1.SalesOrderHeader--71926&as2=open";

        [TestMethod]
        public override void RightClickReferenceInLeftPaneObject()
        {
            base.RightClickReferenceInLeftPaneObject();
        }

        [TestMethod]
        public override void ClickReferenceInLeftPaneObject()
        {
            base.ClickReferenceInLeftPaneObject();
        }

        [TestMethod]
        public override void ClickReferenceInRightPaneObject()
        {
            base.ClickReferenceInRightPaneObject();
        }

        [TestMethod]
        public override void RightClickReferenceInRightPaneObject()
        {
            base.RightClickReferenceInRightPaneObject();
        }

        [TestMethod]
        public override void LeftClickHomeIconFromSplitObjectObject()
        {
            base.LeftClickHomeIconFromSplitObjectObject();
        }

        [TestMethod]
        public override void RightClickHomeIconFromSplitObjectObject()
        {
            base.RightClickHomeIconFromSplitObjectObject();
        }

        [TestMethod]
        public override void ActionDialogOpensInCorrectPane()
        {
            base.ActionDialogOpensInCorrectPane();
        }

        [TestMethod]
        public override void RightClickIsSameAsLeftClickForOpeningDialog()
        {
            base.RightClickIsSameAsLeftClickForOpeningDialog();
        }

        [TestMethod]
        public override void SwapPanes()
        {
            base.SwapPanes();
        }

        [TestMethod]
        public override void FullPaneFromLeft()
        {
            base.FullPaneFromLeft();
        }

        [TestMethod]
        public override void FullPaneFromRight()
        {
            base.FullPaneFromRight();
        }
        #endregion
    }

    #region browser specific subclasses

    //[TestClass, Ignore]
    public class SplitPaneTestsIe : SplitPaneTests
    {
        [ClassInitialize]
        public new static void InitialiseClass(TestContext context)
        {
            FilePath(@"drivers.IEDriverServer.exe");
            AWTest.InitialiseClass(context);
        }

        [TestInitialize]
        public virtual void InitializeTest()
        {
            InitIeDriver();
            Url(BaseUrl);
        }

        [TestCleanup]
        public virtual void CleanupTest()
        {
            base.CleanUpTest();
        }
    }

   // [TestClass]
    public class SplitPaneTestsFirefox : SplitPaneTests
    {
        [ClassInitialize]
        public new static void InitialiseClass(TestContext context)
        {
            AWTest.InitialiseClass(context);
        }

        [TestInitialize]
        public virtual void InitializeTest()
        {
            InitFirefoxDriver();
        }

        [TestCleanup]
        public virtual void CleanupTest()
        {
            base.CleanUpTest();
        }

        protected override void ScrollTo(IWebElement element)
        {
            string script = string.Format("window.scrollTo({0}, {1});return true;", element.Location.X, element.Location.Y);
            ((IJavaScriptExecutor)br).ExecuteScript(script);
        }
    }

    //[TestClass, Ignore]
    public class SplitPaneTestsChrome : SplitPaneTests
    {
        [ClassInitialize]
        public new static void InitialiseClass(TestContext context)
        {
            FilePath(@"drivers.chromedriver.exe");
            AWTest.InitialiseClass(context);
        }

        [TestInitialize]
        public virtual void InitializeTest()
        {
            InitChromeDriver();
        }

        [TestCleanup]
        public virtual void CleanupTest()
        {
            base.CleanUpTest();
        }
    }

    #endregion
    #region Mega tests
    [TestClass]
    public class MegaSplitPaneTestFirefox : SplitPaneTestsRoot
    {
        [ClassInitialize]
        public new static void InitialiseClass(TestContext context)
        {
            AWTest.InitialiseClass(context);
        }

        [TestInitialize]
        public virtual void InitializeTest()
        {
            InitFirefoxDriver();
        }

        [TestCleanup]
        public virtual void CleanupTest()
        {
            base.CleanUpTest();
        }

        protected override void ScrollTo(IWebElement element)
        {
            string script = string.Format("window.scrollTo({0}, {1});return true;", element.Location.X, element.Location.Y);
            ((IJavaScriptExecutor)br).ExecuteScript(script);
        }

        [TestMethod]
        public virtual void MegaSplitPaneTest()
        {
            RightClickActionReturningObjectFromHomeSingle();
            RightClickActionReturningListFromHomeSingle();
            RightClickReferenceFromListSingle();
            RightClickReferencePropertyFromObjectSingle();
            RightClickActionFromObjectSingle();
            RightClickHomeIconFromObjectSingle();
            SwapPanesIconFromSingleOpensHomeOnLeft();
            ClickReferenceInLeftPaneObject();
            ClickReferenceInRightPaneObject();
            RightClickReferenceInRightPaneObject();
            LeftClickHomeIconFromSplitObjectObject();
            RightClickHomeIconFromSplitObjectObject();
            ActionDialogOpensInCorrectPane();
            RightClickIsSameAsLeftClickForOpeningDialog();
            SwapPanes();
            FullPaneFromLeft();
            FullPaneFromRight();
        }
    }
    #endregion
}