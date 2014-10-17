// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Linq;
using System.Reflection;
using NakedObjects.Architecture;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facets.AutoComplete;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.Util;
using NakedObjects.Reflector.DotNet.Reflect.Util;

namespace NakedObjects.Reflector.DotNet.Facets.AutoComplete {
    public class AutoCompleteFacetViaMethod : AutoCompleteFacetAbstract, IImperativeFacet {
        private readonly MethodInfo method;

        public AutoCompleteFacetViaMethod(MethodInfo autoCompleteMethod, int pageSize, int minLength, ISpecification holder)
            : base(holder) {
            method = autoCompleteMethod;
            PageSize = pageSize == 0 ? DefaultPageSize : pageSize;
            MinLength = minLength;
        }

        #region IImperativeFacet Members

        public MethodInfo GetMethod() {
            return method;
        }

        #endregion

        public override object[] GetCompletions(INakedObject inObject, string autoCompleteParm) {
            try {
                object autoComplete = InvokeUtils.Invoke(method, inObject.GetDomainObject(), new object[] {autoCompleteParm});
                if (autoComplete is IQueryable) {
                    return ((IQueryable) autoComplete).Take(PageSize).ToArray();
                }
                throw new NakedObjectDomainException("Must return IQueryable from autoComplete method: " + method.Name);
            }
            catch (ArgumentException ae) {
                string msg = string.Format("autoComplete exception: {0} has mismatched parameter type - must be string", method.Name);
                throw new InvokeException(msg, ae);
            }
        }

        protected override string ToStringValues() {
            return "method=" + method;
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}