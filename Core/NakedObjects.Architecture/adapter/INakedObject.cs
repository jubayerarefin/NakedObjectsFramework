// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using NakedObjects.Architecture.Facets.Actcoll.Typeof;
using NakedObjects.Architecture.Persist;
using NakedObjects.Architecture.Resolve;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Architecture.Adapter {
    /// <summary>
    ///     Naked objects are adapters to domain objects, where the application is written in terms of domain objects
    ///     and those objects are represented within the NOF through these adapter, and not directly. Objects with the
    ///     NOF are divided into two distinct groups: value object and reference objects.
    /// </summary>
    public interface INakedObject {
        /// <summary>
        ///     Returns the adapted domain object, the POCO, that this adapter represents with the NOF
        /// </summary>
        /// <seealso cref="AdapterUtils.GetDomainObject{T}" />
        object Object { get; }

        /// <summary>
        ///     Returns the specification that details the structure of this object's adapted domain object.
        ///     Specifically, this specification provides the mechanism to access and manipulate the domain object.
        /// </summary>
        INakedObjectSpecification Specification { get; }

        /// <summary>
        ///     The objects unique id. This id allows the object to added to, stored by, and retrieved from the object
        ///     store
        /// </summary>
        IOid Oid { get; }

        /// <summary>
        ///     Determines what 'lazy loaded' state the domain object is in
        /// </summary>
        ResolveStateMachine ResolveState { get; }

        /// <summary>
        ///     Returns the current version of the domain object
        /// </summary>
        IVersion Version { get; }

        /// <summary>
        ///     Sets the versions of the domain object
        /// </summary>
        IVersion OptimisticLock { set; }

        /// <summary>
        ///     Gets and sets the element type facet, typically by copying it from the method or association
        ///     information, defaulting to the facet held by the underlying specification
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is needed by collections that can have an element type but is not held by the collection
        ///     </para>
        ///     <para>
        ///         REVIEW should this be more generic allowing other facets to be added to adapters
        ///     </para>
        /// </remarks>
        ITypeOfFacet TypeOfFacet { get; set; }

        /// <summary>
        ///     Returns a list in priority order of names of icons to use if this object is to be displayed graphically
        /// </summary>
        /// <para>
        ///     Should always return at lesat one item
        /// </para>
        string IconName();

        /// <summary>
        ///     Returns the title to display this object with, which is usually got from the
        ///     wrapped <see cref="Object" /> domain object
        /// </summary>
        string TitleString();

        /// <summary>
        ///     Returns a local independent string for this object 
        /// </summary>
        string InvariantString();


        /// <summary>
        ///     Checks the version of this adapter to make sure that it does not differ from the specified
        ///     version
        /// </summary>
        /// <exception cref="ConcurrencyException">
        ///     If the specified version differs from the version held this adapter
        /// </exception>
        void CheckLock(IVersion otherVersion);

        /// <summary>
        ///     Sometimes it is necessary to manage the replacement of the underlying domain object (by another
        ///     component such as an object store). This method allows the adapter to be kept while the domain object
        ///     is replaced
        /// </summary>
        void ReplacePoco(object poco);

        /// <summary>
        ///     Checks that a transient object is in a valid state to be persisted. Returns reason that it cannot be persisted, or null if it can be persisted.
        /// </summary>
        string ValidToPersist();

        /// <summary>
        ///     Sets the oid if the oid is currently null and new new oid is transient. Used to make the oid a place
        ///     holder for custom mmechanisms to recover the object.
        /// </summary>
        /// <param name="oid"></param>
        void SetATransientOid(IOid oid);
    }

    // Copyright (c) Naked Objects Group Ltd.
}