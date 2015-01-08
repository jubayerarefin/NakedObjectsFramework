// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Spec;
using NakedObjects.Meta.Facet;

namespace NakedObjects.Meta.Authorization {
    [Serializable]
    public class AuthorizationDisableForSessionFacet : DisableForSessionFacetAbstract {
        private readonly IAuthorizationManager authorizationManager;
        private readonly IIdentifier identifier;

        public AuthorizationDisableForSessionFacet(IIdentifier identifier,
                                                   IAuthorizationManager authorizationManager,
                                                   ISpecification holder)
            : base(holder) {
            this.authorizationManager = authorizationManager;
            this.identifier = identifier;
        }

        public override string DisabledReason(ISession session, INakedObject target, ILifecycleManager lifecycleManager, IMetamodelManager manager) {
            return authorizationManager.IsEditable(session, lifecycleManager, manager, target, identifier)
                ? null
                : "Not authorized to edit";
        }
    }
}