using Steam.KeyValues.Utilities;
using System;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Steam.KeyValues.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="Type"/> used by the <see cref="KeyValueSerializer"/>.
    /// </summary>
    public class KeyValueDynamicContract : KeyValueContainerContract
    {
        /// <summary>
        /// Gets the object's properties.
        /// </summary>
        /// <value>The object's properties.</value>
        public KeyValuePropertyCollection Properties { get; }

        /// <summary>
        /// Gets or sets the property name resolver.
        /// </summary>
        /// <value>The property name resolver.</value>
        public Func<string, string> PropertyNameResolver { get; set; }

        private readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>> _callSiteGetters =
            new ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>>(CreateCallSiteGetter);

        private readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object, object>>> _callSiteSetters =
            new ThreadSafeStore<string, CallSite<Func<CallSite, object, object, object>>>(CreateCallSiteSetter);

        private static CallSite<Func<CallSite, object, object>> CreateCallSiteGetter(string name)
        {
            GetMemberBinder getMemberBinder = (GetMemberBinder)DynamicUtils.BinderWrapper.GetMember(name, typeof(DynamicUtils));

            return CallSite<Func<CallSite, object, object>>.Create(new NoThrowGetBinderMember(getMemberBinder));
        }

        private static CallSite<Func<CallSite, object, object, object>> CreateCallSiteSetter(string name)
        {
            SetMemberBinder binder = (SetMemberBinder)DynamicUtils.BinderWrapper.SetMember(name, typeof(DynamicUtils));

            return CallSite<Func<CallSite, object, object, object>>.Create(new NoThrowSetBinderMember(binder));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueDynamicContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        public KeyValueDynamicContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = KeyValueContractType.Dynamic;

            Properties = new KeyValuePropertyCollection(UnderlyingType);
        }

        internal bool TryGetMember(IDynamicMetaObjectProvider dynamicProvider, string name, out object value)
        {
            ValidationUtils.ArgumentNotNull(dynamicProvider, nameof(dynamicProvider));

            CallSite<Func<CallSite, object, object>> callSite = _callSiteGetters.Get(name);

            object result = callSite.Target(callSite, dynamicProvider);

            if (!ReferenceEquals(result, NoThrowExpressionVisitor.ErrorResult))
            {
                value = result;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        internal bool TrySetMember(IDynamicMetaObjectProvider dynamicProvider, string name, object value)
        {
            ValidationUtils.ArgumentNotNull(dynamicProvider, nameof(dynamicProvider));

            CallSite<Func<CallSite, object, object, object>> callSite = _callSiteSetters.Get(name);

            object result = callSite.Target(callSite, dynamicProvider, value);

            return !ReferenceEquals(result, NoThrowExpressionVisitor.ErrorResult);
        }
    }
}
