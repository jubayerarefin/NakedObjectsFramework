﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Xat {
    internal class TestAction : ITestAction {
        private readonly IActionSpec actionSpec;
        private readonly ITestObjectFactory factory;
        private readonly ILifecycleManager lifecycleManager;
        private readonly ITransactionManager transactionManager;
        private readonly INakedObjectManager manager;
        private readonly IMetamodelManager metamodelManager;
        private readonly IMessageBroker messageBroker;
        private readonly ITestHasActions owningObject;
        private readonly ISession session;

        public TestAction(IMetamodelManager metamodelManager, ISession session, ILifecycleManager lifecycleManager, ITransactionManager transactionManager, IActionSpec actionSpec, ITestHasActions owningObject, ITestObjectFactory factory, INakedObjectManager manager, IMessageBroker messageBroker)
            : this(metamodelManager, session, lifecycleManager, transactionManager, string.Empty, actionSpec, owningObject, factory, manager, messageBroker) {}

        public TestAction(IMetamodelManager metamodelManager, ISession session, ILifecycleManager lifecycleManager, ITransactionManager transactionManager, string contributor, IActionSpec actionSpec, ITestHasActions owningObject, ITestObjectFactory factory, INakedObjectManager manager, IMessageBroker messageBroker) {
            SubMenu = contributor;
            this.metamodelManager = metamodelManager;
            this.session = session;
            this.lifecycleManager = lifecycleManager;
            this.transactionManager = transactionManager;
            this.messageBroker = messageBroker;
            this.owningObject = owningObject;
            this.factory = factory;
            this.manager = manager;
            this.actionSpec = actionSpec;
        }

        #region ITestAction Members

        public string Name {
            get { return actionSpec.Name; }
        }

        public string SubMenu { get; private set; }
        public string LastMessage { get; private set; }

        public ITestParameter[] Parameters {
            get { return actionSpec.Parameters.Select(x => factory.CreateTestParameter(actionSpec, x, owningObject)).ToArray(); }
        }

        public bool MatchParameters(Type[] typestoMatch) {
            if (actionSpec.Parameters.Count() == typestoMatch.Length) {
                int i = 0;
                return actionSpec.Parameters.All(x => x.Spec.IsOfType(metamodelManager.GetSpecification(typestoMatch[i++])));
            }
            return false;
        }

        public ITestObject InvokeReturnObject(params object[] parameters) {
            try {
                transactionManager.StartTransaction();
                return (ITestObject) DoInvoke(ParsedParameters(parameters));
            }
            finally {
                transactionManager.EndTransaction();
            }
        }

        public ITestCollection InvokeReturnCollection(params object[] parameters) {
            try {
                transactionManager.StartTransaction();
                return (ITestCollection) DoInvoke(ParsedParameters(parameters));
            }
            finally {
                transactionManager.EndTransaction();
            }
        }

        public void Invoke(params object[] parameters) {
            try {
                transactionManager.StartTransaction();
                DoInvoke(ParsedParameters(parameters));
            }
            finally {
                transactionManager.EndTransaction();
            }
        }

        public ITestCollection InvokeReturnPagedCollection(int page, params object[] parameters) {
            try {
                transactionManager.StartTransaction();
                return (ITestCollection) DoInvoke(page, ParsedParameters(parameters));
            }
            finally {
                transactionManager.EndTransaction();
            }
        }

        #endregion

        private ITestNaked DoInvoke(int page, params object[] parameters) {
            ResetLastMessage();
            AssertIsValidWithParms(parameters);
            INakedObjectAdapter[] parameterObjectsAdapter = parameters.AsTestNakedArray(manager).Select(x => x.NakedObject).ToArray();

            INakedObjectAdapter[] parms = actionSpec.RealParameters(owningObject.NakedObject, parameterObjectsAdapter);
            INakedObjectAdapter target = actionSpec.RealTarget(owningObject.NakedObject);
            INakedObjectAdapter result = actionSpec.GetFacet<IActionInvocationFacet>().Invoke(target, parms, page, lifecycleManager, metamodelManager, session, manager, messageBroker, transactionManager);

            if (result == null) {
                return null;
            }
            if (result.Spec.IsCollection) {
                return factory.CreateTestCollection(result);
            }
            return factory.CreateTestObject(result);
        }

        private ITestNaked DoInvoke(params object[] parameters) {
            ResetLastMessage();
            AssertIsValidWithParms(parameters);
            INakedObjectAdapter[] parameterObjectsAdapter = parameters.AsTestNakedArray(manager).Select(x => x.NakedObject).ToArray();
            INakedObjectAdapter result = null;
            try {
                result = actionSpec.Execute(owningObject.NakedObject, parameterObjectsAdapter);
            }
            catch (ArgumentException) {
                Assert.Fail("Invalid Argument(s)");
            }
            catch (InvalidCastException) {
                Assert.Fail("Invalid Argument(s)");
            }

            if (result == null) {
                return null;
            }
            if (result.Spec.IsCollection && !result.Spec.IsParseable) {
                return factory.CreateTestCollection(result);
            }
            return factory.CreateTestObject(result);
        }

        private void ResetLastMessage() {
            LastMessage = string.Empty;
        }

        #region Asserts

        public ITestAction AssertIsDisabled() {
            ResetLastMessage();
            if (actionSpec.IsVisible(owningObject.NakedObject)) {
                IConsent canUse = actionSpec.IsUsable(owningObject.NakedObject);
                LastMessage = canUse.Reason;
                Assert.IsFalse(canUse.IsAllowed, "Action '" + Name + "' is usable: " + canUse.Reason);
            }
            return this;
        }

        public ITestAction AssertIsEnabled() {
            ResetLastMessage();
            AssertIsVisible();
            IConsent canUse = actionSpec.IsUsable(owningObject.NakedObject);
            LastMessage = canUse.Reason;
            Assert.IsTrue(canUse.IsAllowed, "Action '" + Name + "' is disabled: " + canUse.Reason);
            return this;
        }

        public ITestAction AssertIsInvalidWithParms(params object[] parameters) {
            ResetLastMessage();

            object[] parsedParameters = ParsedParameters(parameters);

            if (actionSpec.IsVisible(owningObject.NakedObject)) {
                IConsent canUse = actionSpec.IsUsable(owningObject.NakedObject);
                LastMessage = canUse.Reason;
                if (canUse.IsAllowed) {
                    INakedObjectAdapter[] parameterObjectsAdapter = parsedParameters.AsTestNakedArray(manager).Select(x => x == null ? null : x.NakedObject).ToArray();
                    IConsent canExecute = actionSpec.IsParameterSetValid(owningObject.NakedObject, parameterObjectsAdapter);
                    LastMessage = canExecute.Reason;
                    Assert.IsFalse(canExecute.IsAllowed, "Action '" + Name + "' is usable and executable");
                }
            }
            return this;
        }

        public ITestAction AssertIsValidWithParms(params object[] parameters) {
            ResetLastMessage();
            AssertIsVisible();
            AssertIsEnabled();

            object[] parsedParameters = ParsedParameters(parameters);

            INakedObjectAdapter[] parameterObjectsAdapter = parsedParameters.AsTestNakedArray(manager).Select(x => x == null ? null : x.NakedObject).ToArray();
            IConsent canExecute = actionSpec.IsParameterSetValid(owningObject.NakedObject, parameterObjectsAdapter);
            Assert.IsTrue(canExecute.IsAllowed, "Action '" + Name + "' is unusable: " + canExecute.Reason);
            return this;
        }

        public ITestAction AssertIsVisible() {
            ResetLastMessage();
            Assert.IsTrue(actionSpec.IsVisible(owningObject.NakedObject), "Action '" + Name + "' is hidden");
            return this;
        }

        public ITestAction AssertIsInvisible() {
            ResetLastMessage();
            Assert.IsFalse(actionSpec.IsVisible(owningObject.NakedObject), "Action '" + Name + "' is visible");
            return this;
        }

        public ITestAction AssertIsDescribedAs(string expected) {
            Assert.IsTrue(expected.Equals(actionSpec.Description), "Description expected: '" + expected + "' actual: '" + actionSpec.Description + "'");
            return this;
        }

        public ITestAction AssertLastMessageIs(string message) {
            Assert.IsTrue(message.Equals(LastMessage), "Last message expected: '" + message + "' actual: '" + LastMessage + "'");
            return this;
        }

        public ITestAction AssertLastMessageContains(string message) {
            Assert.IsTrue(LastMessage.Contains(message), "Last message expected to contain: '" + message + "' actual: '" + LastMessage + "'");
            return this;
        }

        private object[] ParsedParameters(params object[] parameters) {
            var parsedParameters = new List<object>();

            Assert.IsTrue(parameters.Count() == actionSpec.Parameters.Count(), String.Format("Action '{0}' is unusable: wrong number of parameters, got {1}, expect {2}", Name, parameters.Count(), actionSpec.Parameters.Count()));

            int i = 0;
            foreach (IActionParameterSpec parm in actionSpec.Parameters) {
                object value = parameters[i++];

                var valueAsString = value as string;
                if (valueAsString != null && parm.Spec.IsParseable) {
                    parsedParameters.Add(parm.Spec.GetFacet<IParseableFacet>().ParseTextEntry(valueAsString, manager).Object);
                }
                else {
                    parsedParameters.Add(value);
                }
            }

            return parsedParameters.ToArray();
        }

        public ITestAction AssertHasFriendlyName(string friendlyName) {
            Assert.AreEqual(friendlyName, this.Name);
            return this;
        }

        #endregion
    }
}