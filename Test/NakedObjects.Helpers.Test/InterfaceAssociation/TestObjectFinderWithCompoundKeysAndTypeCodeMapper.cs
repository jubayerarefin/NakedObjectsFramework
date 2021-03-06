﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Boot;
using NakedObjects.Core.NakedObjectsSystem;
using NakedObjects.EntityObjectStore;
using NakedObjects.Helpers.Test.ViewModel;
using NakedObjects.Services;
using NakedObjects.Xat;
using NakedObjects.Helpers.Test;
using NakedObjects.SystemTest.ObjectFinderCompoundKeys;

namespace NakedObjects.SystemTest.TestObjectFinderWithCompoundKeysAndTypeCodeMapper {

    [TestClass]
    public class TestObjectFinderWithCompoundKeysAndTypeCodeMapper : TestObjectFinderWithCompoundKeysAbstract {

        protected override IServicesInstaller MenuServices {
            get {
                return new ServicesInstaller(new object[] {
                    new ObjectFinderWithTypeCodeMapper(),
                    new SimpleRepository<Payment>(),
                    new SimpleRepository<CustomerOne>(),
                    new SimpleRepository<CustomerTwo>(),
                    new SimpleRepository<CustomerThree>(),
                    new SimpleRepository<Supplier>(),
                    new SimpleRepository<Employee>(),
                    new SimpleTypeCodeMapper()
                });
            }
        }


        [ClassInitialize]
        public static void SetupTestFixture(TestContext tc)
        {
            InitializeNakedObjectsFramework(new TestObjectFinderWithCompoundKeysAndTypeCodeMapper());
        }

        [ClassCleanup]
        public  static void TearDownTest()
        {
            CleanupNakedObjectsFramework(new TestObjectFinderWithCompoundKeysAndTypeCodeMapper());
            Database.Delete(PaymentContext.DatabaseName);
        }

    

        [TestMethod]
        public  void SetAssociatedObject()
        {
            payee1.SetObject(customer2a);
            key1.AssertValueIsEqual("CU2|1|1001");

            payee1.SetObject(customer2b);
            Assert.AreEqual(payee1.ContentAsObject, customer2b);

            key1.AssertValueIsEqual("CU2|2|1002");
        }

        [TestMethod]
        public   void WorksWithASingleIntegerKey() {
            payee1.SetObject(customer1);
            key1.AssertValueIsEqual("CU1|1");
            payee1.ClearObject();

            key1.SetValue("CU1|1");
            payee1.AssertIsNotEmpty();
            payee1.AssertObjectIsEqual(customer1);
        }

        [TestMethod]
        public  void WorksWithTripleIntegerKey()
        {
            payee1.SetObject(customer3);
            key1.AssertValueIsEqual("CU3|1|1001|2001");
            payee1.ClearObject();

            key1.SetValue("CU3|1|1001|2001");
            payee1.AssertIsNotEmpty();
            payee1.AssertObjectIsEqual(customer3);
        }

        [TestMethod]
        public  void FailsIfTypeNameIsEmpty()
        {
            key1.SetValue("|1|1001|2001");
            try {
                payee1.AssertIsNotEmpty();
                throw new AssertFailedException("Exception should have been thrown");
            }
            catch (Exception ex) {
                Assert.AreEqual("Compound key: |1|1001|2001 does not contain an object type", ex.Message);
            }
        }

        [TestMethod]
        public  void FailsIfCodeNotRecognised() {
            key1.SetValue("EMP|1");
            try {
                payee1.AssertIsNotEmpty();
                throw new AssertFailedException("Exception should have been thrown");
            }
            catch (Exception ex) {
                Assert.AreEqual("Code not recognised: EMP", ex.Message);
            }
        }

        [TestMethod]
        public void FailsIfTypeNotRecognisedByEncodingService()
        {
            try {
                payee1.SetObject(emp1);
                throw new AssertFailedException("Exception should have been thrown");
            }
            catch (Exception ex) {
                Assert.AreEqual("Type not recognised: NakedObjects.SystemTest.ObjectFinderCompoundKeys.Employee", ex.Message);
            }
        }

        [TestMethod]
        public  void FailsIfTooFewKeysSupplied()
        {
            key1.SetValue("CU3|1|1001");
            try {
                payee1.AssertIsNotEmpty();
                throw new AssertFailedException("Exception should have been thrown");
            }
            catch (Exception ex) {
                Assert.AreEqual("Number of keys provided does not match the number of keys specified for type: NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerThree", ex.Message);
            }
        }


        [TestMethod]
        public  void FailsIfTooManyKeysSupplied()
        {
            key1.SetValue("CU2|1|1001|2001");
            try {
                payee1.AssertIsNotEmpty();
                throw new AssertFailedException("Exception should have been thrown");
            }
            catch (Exception ex) {
                Assert.AreEqual("Number of keys provided does not match the number of keys specified for type: NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerTwo", ex.Message);
            }
        }


        [TestMethod]
        public  void ChangeAssociatedObjectType() {
            payee1.SetObject(customer2a);
            key1.AssertValueIsEqual("CU2|1|1001");
            payee1.SetObject(supplier1);
            Assert.AreEqual(payee1.ContentAsObject, supplier1);

            key1.AssertValueIsEqual("SUP|1|2001");
        }


        [TestMethod]
        public  void ClearAssociatedObject()
        {
            payee1.SetObject(customer2a);
            payee1.ClearObject();
            key1.AssertIsEmpty();
        }


        [TestMethod]
        public void GetAssociatedObject()
        {
            key1.SetValue("CU2|1|1001");
            payee1.AssertIsNotEmpty();
            payee1.ContentAsObject.GetPropertyByName("Id").AssertValueIsEqual("1");

            payee1.ClearObject();

            key1.SetValue("CU2|2|1002");
            payee1.AssertIsNotEmpty();
            payee1.ContentAsObject.GetPropertyByName("Id").AssertValueIsEqual("2");
        }

        [TestMethod]
        public void NoAssociatedObject()
        {
            key1.AssertIsEmpty();
        }
    }

    #region Classes used by test

    public class SimpleTypeCodeMapper : ITypeCodeMapper {
        #region ITypeCodeMapper Members

        public Type TypeFromCode(string code) {
            if (code == "CU1") return typeof (CustomerOne);
            if (code == "CU2") return typeof (CustomerTwo);
            if (code == "CU3") return typeof (CustomerThree);
            if (code == "SUP") return typeof (Supplier);
            throw new DomainException("Code not recognised: " + code);
        }

        public string CodeFromType(Type type) {
            if (type == typeof (CustomerOne)) return "CU1";
            if (type == typeof (CustomerTwo)) return "CU2";
            if (type == typeof (CustomerThree)) return "CU3";
            if (type == typeof (Supplier)) return "SUP";
            throw new DomainException("Type not recognised: " + type);
        }

        #endregion
    }

    #endregion
}