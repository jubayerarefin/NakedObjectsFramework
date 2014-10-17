// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Reflector.DotNet.Value {
    public class LongValueTypeFacetFactory : ValueUsingValueSemanticsProviderFacetFactory<long> {
        public LongValueTypeFacetFactory(INakedObjectReflector reflector)
            : base(reflector, typeof (ILongValueFacet)) {}

        public override bool Process(Type type, IMethodRemover methodRemover, ISpecification specification) {
            if (LongValueSemanticsProvider.IsAdaptedType(type)) {
                var spec = Reflector.LoadSpecification(LongValueSemanticsProvider.AdaptedType);
                AddFacets(new LongValueSemanticsProvider(spec, specification));
                return true;
            }
            return false;
        }
    }
}