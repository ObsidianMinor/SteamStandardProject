using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Steam.KeyValues.Utilities
{
    internal static class DynamicUtils
    {
        internal static class BinderWrapper
        {
            public static CallSiteBinder GetMember(string name, Type context)
            {
                return Binder.GetMember(
                    CSharpBinderFlags.None, name, context, new[] {CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)});
            }

            public static CallSiteBinder SetMember(string name, Type context)
            {
                return Binder.SetMember(
                    CSharpBinderFlags.None, name, context, new[]
                                                               {
                                                                   CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                                                   CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
                                                               });
            }
        }

        public static IEnumerable<string> GetDynamicMemberNames(this IDynamicMetaObjectProvider dynamicProvider)
        {
            DynamicMetaObject metaObject = dynamicProvider.GetMetaObject(Expression.Constant(dynamicProvider));
            return metaObject.GetDynamicMemberNames();
        }
    }

    internal class NoThrowGetBinderMember : GetMemberBinder
    {
        private readonly GetMemberBinder _innerBinder;

        public NoThrowGetBinderMember(GetMemberBinder innerBinder)
            : base(innerBinder.Name, innerBinder.IgnoreCase)
        {
            _innerBinder = innerBinder;
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            DynamicMetaObject retMetaObject = _innerBinder.Bind(target, CollectionUtils.ArrayEmpty<DynamicMetaObject>());

            NoThrowExpressionVisitor noThrowVisitor = new NoThrowExpressionVisitor();
            Expression resultExpression = noThrowVisitor.Visit(retMetaObject.Expression);

            DynamicMetaObject finalMetaObject = new DynamicMetaObject(resultExpression, retMetaObject.Restrictions);
            return finalMetaObject;
        }
    }

    internal class NoThrowSetBinderMember : SetMemberBinder
    {
        private readonly SetMemberBinder _innerBinder;

        public NoThrowSetBinderMember(SetMemberBinder innerBinder)
            : base(innerBinder.Name, innerBinder.IgnoreCase)
        {
            _innerBinder = innerBinder;
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            DynamicMetaObject retMetaObject = _innerBinder.Bind(target, new DynamicMetaObject[] { value });

            NoThrowExpressionVisitor noThrowVisitor = new NoThrowExpressionVisitor();
            Expression resultExpression = noThrowVisitor.Visit(retMetaObject.Expression);

            DynamicMetaObject finalMetaObject = new DynamicMetaObject(resultExpression, retMetaObject.Restrictions);
            return finalMetaObject;
        }
    }

    internal class NoThrowExpressionVisitor : ExpressionVisitor
    {
        internal static readonly object ErrorResult = new object();

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            // if the result of a test is to throw an error, rewrite to result an error result value
            if (node.IfFalse.NodeType == ExpressionType.Throw)
            {
                return Expression.Condition(node.Test, node.IfTrue, Expression.Constant(ErrorResult));
            }

            return base.VisitConditional(node);
        }
    }
}
