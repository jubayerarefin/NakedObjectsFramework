// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Util;
using NakedObjects.Core.Util.Query;
using NakedObjects.Facade.Contexts;
using NakedObjects.Facade.Impl.Utility;

namespace NakedObjects.Facade.Impl {
    public class AssociationFacade : IAssociationFacade {
        private readonly INakedObjectsFramework framework;

        public AssociationFacade(IAssociationSpec assoc, IFrameworkFacade frameworkFacade, INakedObjectsFramework framework) {
            FacadeUtils.AssertNotNull(assoc, "Assoc is null");
            FacadeUtils.AssertNotNull(framework, "framework is null");
            FacadeUtils.AssertNotNull(frameworkFacade, "FrameworkFacade is null");

            WrappedSpec = assoc;
            this.framework = framework;
            FrameworkFacade = frameworkFacade;
        }

        public IAssociationSpec WrappedSpec { get; }

        #region IAssociationFacade Members

        public string Name => WrappedSpec.Name;

        public string Description => WrappedSpec.Description;

        public bool IsMandatory => WrappedSpec.IsMandatory;

        public bool IsCollection => WrappedSpec is IOneToManyAssociationSpec;

        public bool IsObject => WrappedSpec is IOneToOneAssociationSpec;

        public bool IsConcurrency => WrappedSpec.ContainsFacet<IConcurrencyCheckFacet>();

        public bool NotNavigable => WrappedSpec.ContainsFacet<INotNavigableFacet>();

        public int? MaxLength => WrappedSpec.GetMaxLength();

        public string Pattern => WrappedSpec.GetPattern();

        public int MemberOrder => WrappedSpec.GetMemberOrder();

        public bool IsASet
        {
            get
            {
                var collection = WrappedSpec as IOneToManyAssociationSpec;
                return collection != null && collection.IsASet;
            }
        }

        public bool IsInline => WrappedSpec.IsInline;

        public string Mask => WrappedSpec.GetMask();

        public int AutoCompleteMinLength => WrappedSpec.GetAutoCompleteMinLength();

        public ITypeFacade Specification => new TypeFacade(WrappedSpec.ReturnSpec, FrameworkFacade, framework);

        public ITypeFacade ElementSpecification
        {
            get
            {
                var coll = WrappedSpec as IOneToManyAssociationSpec;
                var elementSpec = coll == null ? null : coll.ElementSpec;
                return elementSpec == null ? null : new TypeFacade(elementSpec, FrameworkFacade, framework);
            }
        }

        public string Id => WrappedSpec.Id;

        public Choices IsChoicesEnabled
        {
            get
            {
                var oneToOneFeature = WrappedSpec as IOneToOneFeatureSpec;
                return oneToOneFeature != null && oneToOneFeature.IsChoicesEnabled ? Choices.Single : Choices.NotEnabled;
            }
        }

        public bool IsAutoCompleteEnabled
        {
            get
            {
                var single = WrappedSpec as IOneToOneFeatureSpec;
                return single != null && single.IsAutoCompleteEnabled;
            }
        }

        public IConsentFacade IsUsable(IObjectFacade target) {
            IConsent consent = WrappedSpec.IsUsable(((ObjectFacade)target).WrappedNakedObject);
            return new ConsentFacade(consent);
        }

        public IObjectFacade GetValue(IObjectFacade target) {
            INakedObjectAdapter result = WrappedSpec.GetNakedObject(((ObjectFacade)target).WrappedNakedObject);
            return ObjectFacade.Wrap(result, FrameworkFacade, framework);
        }

        public bool IsVisible(IObjectFacade objectFacade) {
            return WrappedSpec.IsVisible(((ObjectFacade)objectFacade).WrappedNakedObject);
        }

        public bool IsEager(IObjectFacade objectFacade) {
            return ((TypeFacade)objectFacade.Specification).WrappedValue.ContainsFacet<IEagerlyFacet>() ||
                   WrappedSpec.ContainsFacet<IEagerlyFacet>();
        }

        public DataType? DataType => WrappedSpec.GetFacet<IDataTypeFacet>()?.DataType();

        public IObjectFacade[] GetChoices(IObjectFacade target, IDictionary<string, object> parameterNameValues) {
            var oneToOneFeature = WrappedSpec as IOneToOneFeatureSpec;
            var pnv = parameterNameValues == null ? null : parameterNameValues.ToDictionary(kvp => kvp.Key, kvp => framework.GetNakedObject(kvp.Value));
            return oneToOneFeature != null ? oneToOneFeature.GetChoices(((ObjectFacade)target).WrappedNakedObject, pnv).Select(no => ObjectFacade.Wrap(no, FrameworkFacade, framework)).Cast<IObjectFacade>().ToArray() : null;
        }

        public Tuple<string, ITypeFacade>[] GetChoicesParameters() {
            var oneToOneFeature = WrappedSpec as IOneToOneFeatureSpec;
            return oneToOneFeature != null ? oneToOneFeature.GetChoicesParameters().Select(WrapChoiceParm).ToArray() : new Tuple<string, ITypeFacade>[0];
        }

        public Tuple<IObjectFacade, string>[] GetChoicesAndTitles(IObjectFacade target, IDictionary<string, object> parameterNameValues) {
            var choices = GetChoices(target, parameterNameValues);
            return choices.Select(c => new Tuple<IObjectFacade, string>(c, c.TitleString)).ToArray();
        }

        public IObjectFacade[] GetCompletions(IObjectFacade target, string autoCompleteParm) {
            var oneToOneFeature = WrappedSpec as IOneToOneFeatureSpec;
            return oneToOneFeature != null ? oneToOneFeature.GetCompletions(((ObjectFacade)target).WrappedNakedObject, autoCompleteParm).Select(no => ObjectFacade.Wrap(no, FrameworkFacade, framework)).Cast<IObjectFacade>().ToArray() : null;
        }

        public int Count(IObjectFacade target) {
            return IsCollection ? framework.Persistor.CountField(((ObjectFacade)target).WrappedNakedObject, Id) : 0;
        }

        public bool IsSetToImplicitDefault(IObjectFacade objectFacade) {
            // return true if it's scalar and and still set to its implicit default value (eg 0 for an int)
            if (!DefaultTypeIsExplicit(objectFacade) && WrappedSpec.ReturnSpec.IsParseable) {
                var dflt = WrappedSpec.GetDefault(objectFacade.WrappedAdapter());
                var currentValue = GetValue(objectFacade);

                return dflt?.Object == currentValue?.Object;

            }

            return false;
        }

        public string GetTitle(IObjectFacade objectFacade) {
            var enumFacet = WrappedSpec.GetFacet<IEnumFacet>();

            if (enumFacet != null) {
                return enumFacet.GetTitle(((ObjectFacade)objectFacade).WrappedNakedObject);
            }

            var mask = WrappedSpec.GetFacet<IMaskFacet>();
            if (mask == null) {
                return objectFacade.TitleString;
            }
            var titleFacet = ((TypeFacade)objectFacade.Specification).WrappedValue.GetFacet<ITitleFacet>();
            return titleFacet.GetTitleWithMask(mask.Value, ((ObjectFacade)objectFacade).WrappedNakedObject, framework.NakedObjectManager);
        }

        public IFrameworkFacade FrameworkFacade { get; set; }

        public bool IsFile => WrappedSpec.IsFile(framework);

        public bool IsDateOnly => WrappedSpec.ContainsFacet<IDateOnlyFacet>();

        public bool IsEnum => WrappedSpec.ContainsFacet<IEnumFacet>();

        public bool IsFindMenuEnabled => WrappedSpec is IOneToOneAssociationSpec && ((IOneToOneAssociationSpec)WrappedSpec).IsFindMenuEnabled;

        public Tuple<Regex, string> RegEx => WrappedSpec.GetRegEx();

        public Tuple<IConvertible, IConvertible, bool> Range => WrappedSpec.GetRange();

        public bool IsAjax => !WrappedSpec.ContainsFacet<IAjaxFacet>();

        public bool DoNotCount => WrappedSpec.ContainsFacet<INotCountedFacet>();

        public int Width => WrappedSpec.GetWidth();

        public string PresentationHint => WrappedSpec.GetPresentationHint();

        public string GetMaskedValue(IObjectFacade objectFacade) {
            return WrappedSpec.GetMaskedValue(objectFacade, framework.NakedObjectManager);
        }

        public bool DefaultTypeIsExplicit(IObjectFacade objectFacade) {
            var no = ((ObjectFacade)objectFacade).WrappedNakedObject;
            return WrappedSpec.GetDefaultType(no) == TypeOfDefaultValue.Explicit;
        }

        public int TypicalLength => WrappedSpec.GetTypicalLength();

        public int NumberOfLines => WrappedSpec.GetNumberOfLinesWithDefault();

        public Tuple<bool, string[]> TableViewData => WrappedSpec.GetTableViewData();

        public bool RenderEagerly => WrappedSpec.GetRenderEagerly();

        public bool IsPassword => WrappedSpec.ContainsFacet<IPasswordFacet>();

        public bool IsNullable => WrappedSpec.ContainsFacet<INullableFacet>();

        #endregion

        private Tuple<string, ITypeFacade> WrapChoiceParm(Tuple<string, IObjectSpec> parm) {
            return new Tuple<string, ITypeFacade>(parm.Item1, new TypeFacade(parm.Item2, FrameworkFacade, framework));
        }

        public override bool Equals(object obj) {
            var nakedObjectAssociationWrapper = obj as AssociationFacade;
            if (nakedObjectAssociationWrapper != null) {
                return Equals(nakedObjectAssociationWrapper);
            }
            return false;
        }

        public bool Equals(AssociationFacade other) {
            if (ReferenceEquals(null, other)) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            return Equals(other.WrappedSpec, WrappedSpec);
        }

        public override int GetHashCode() {
            return (WrappedSpec != null ? WrappedSpec.GetHashCode() : 0);
        }
    }
}