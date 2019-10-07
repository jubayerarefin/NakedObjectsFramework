// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

namespace NakedObjects.Core.Reflect {
    /// <summary>
    ///     An instance of this type is used to allow something
    /// </summary>
    public sealed class Allow : ConsentAbstract {
        /// <summary>
        ///     An Allow object with no reason
        /// </summary>
        public static readonly Allow Default = new Allow();

        public Allow() {}

        public Allow(string reason)
            : base(reason) {}

        /// <summary>
        ///     Returns <c>true</c>
        /// </summary>
        public override bool IsAllowed {
            get { return true; }
        }

        /// <summary>
        ///     Returns <c>false</c>
        /// </summary>
        public override bool IsVetoed {
            get { return false; }
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}