// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Meta.Facet;
using NakedObjects.ParallelReflect.FacetFactory;

namespace NakedObjects.ParallelReflect.Test.FacetFactory {
    [TestClass]
    public class MenuFacetFactoryTest : AbstractFacetFactoryTest {
        private MenuFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof(IMenuFacet)}; }
        }

        protected override IFacetFactory FacetFactory => facetFactory;

        [TestMethod]
        public override void TestFeatureTypes() {
            FeatureType featureTypes = facetFactory.FeatureTypes;
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Objects));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Properties));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Collections));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Actions));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.ActionParameters));
        }

        [TestMethod]
        public void TestDefaultMenuPickedUp() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            metamodel = facetFactory.Process(Reflector, typeof(Class1), MethodRemover, Specification, metamodel);
            IFacet facet = Specification.GetFacet(typeof(IMenuFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is MenuFacetDefault);
            Assert.IsNotNull(metamodel);
        }

        [TestMethod]
        public void TestMethodMenuPickedUp() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            var class2Type = typeof(Class2);
            metamodel = facetFactory.Process(Reflector, class2Type, MethodRemover, Specification, metamodel);
            IFacet facet = Specification.GetFacet(typeof(IMenuFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is MenuFacetViaMethod);
            MethodInfo m1 = class2Type.GetMethod("Menu");
            AssertMethodRemoved(m1);
            Assert.IsNotNull(metamodel);
        }

        #region Nested type: Class1

        private class Class1 { }

        #endregion

        #region Nested type: Class2

        private class Class2 {
// ReSharper disable once UnusedMember.Local
            public static void Menu() { }
        }

        #endregion

        #region Setup/Teardown

        [TestInitialize]
        public override void SetUp() {
            base.SetUp();

            facetFactory = new MenuFacetFactory(0);
        }

        [TestCleanup]
        public new void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion
    }
}