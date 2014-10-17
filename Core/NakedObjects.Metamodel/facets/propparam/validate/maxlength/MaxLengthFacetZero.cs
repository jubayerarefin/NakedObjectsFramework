// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using NakedObjects.Architecture.Interactions;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Architecture.Facets.Propparam.Validate.MaxLength {
    public class MaxLengthFacetZero : MaxLengthFacetAbstract {
        private const int NoLimit = 0;

        public MaxLengthFacetZero(ISpecification holder)
            : base(NoLimit, holder) {}

        public override bool IsNoOp {
            get { return true; }
        }

        /// <summary>
        ///     No limit to maximum length
        /// </summary>
        public override string Invalidates(InteractionContext interactionContext) {
            return null;
        }
    }


    // Copyright (c) Naked Objects Group Ltd.
}