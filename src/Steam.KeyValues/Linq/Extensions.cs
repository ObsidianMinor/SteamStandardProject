using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Contains the LINQ to JSON extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns a collection of tokens that contains the ancestors of every token in the source collection.
        /// </summary>
        /// <typeparam name="T">The type of the objects in source, constrained to <see cref="KVToken"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the ancestors of every token in the source collection.</returns>
        public static IKVEnumerable<KVToken> Ancestors<T>(this IEnumerable<T> source) where T : KVToken
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(j => j.Ancestors()).AsKVEnumerable();
        }

        /// <summary>
        /// Returns a collection of tokens that contains every token in the source collection, and the ancestors of every token in the source collection.
        /// </summary>
        /// <typeparam name="T">The type of the objects in source, constrained to <see cref="KVToken"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains every token in the source collection, the ancestors of every token in the source collection.</returns>
        public static IKVEnumerable<KVToken> AncestorsAndSelf<T>(this IEnumerable<T> source) where T : KVToken
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(j => j.AncestorsAndSelf()).AsKVEnumerable();
        }

        /// <summary>
        /// Returns a collection of tokens that contains the descendants of every token in the source collection.
        /// </summary>
        /// <typeparam name="T">The type of the objects in source, constrained to <see cref="KVContainer"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the descendants of every token in the source collection.</returns>
        public static IKVEnumerable<KVToken> Descendants<T>(this IEnumerable<T> source) where T : KVContainer
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(j => j.Descendants()).AsKVEnumerable();
        }

        /// <summary>
        /// Returns a collection of tokens that contains every token in the source collection, and the descendants of every token in the source collection.
        /// </summary>
        /// <typeparam name="T">The type of the objects in source, constrained to <see cref="KVContainer"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains every token in the source collection, and the descendants of every token in the source collection.</returns>
        public static IKVEnumerable<KVToken> DescendantsAndSelf<T>(this IEnumerable<T> source) where T : KVContainer
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(j => j.DescendantsAndSelf()).AsKVEnumerable();
        }

        /// <summary>
        /// Returns a collection of child properties of every object in the source collection.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVObject"/> that contains the source collection.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVProperty"/> that contains the properties of every object in the source collection.</returns>
        public static IKVEnumerable<KVProperty> Properties(this IEnumerable<KVObject> source)
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(d => d.Properties()).AsJEnumerable();
        }

        /// <summary>
        /// Returns a collection of child values of every object in the source collection with the given key.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <param name="key">The token key.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the values of every token in the source collection with the given key.</returns>
        public static IKVEnumerable<KVToken> Values(this IEnumerable<KVToken> source, object key)
        {
            return Values<KVToken, KVToken>(source, key).AsKVEnumerable();
        }

        /// <summary>
        /// Returns a collection of child values of every object in the source collection.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the values of every token in the source collection.</returns>
        public static IKVEnumerable<KVToken> Values(this IEnumerable<KVToken> source)
        {
            return source.Values(null);
        }

        /// <summary>
        /// Returns a collection of converted child values of every object in the source collection with the given key.
        /// </summary>
        /// <typeparam name="U">The type to convert the values to.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <param name="key">The token key.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains the converted values of every token in the source collection with the given key.</returns>
        public static IEnumerable<U> Values<U>(this IEnumerable<KVToken> source, object key)
        {
            return Values<KVToken, U>(source, key);
        }

        /// <summary>
        /// Returns a collection of converted child values of every object in the source collection.
        /// </summary>
        /// <typeparam name="U">The type to convert the values to.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains the converted values of every token in the source collection.</returns>
        public static IEnumerable<U> Values<U>(this IEnumerable<KVToken> source)
        {
            return Values<KVToken, U>(source, null);
        }

        /// <summary>
        /// Converts the value.
        /// </summary>
        /// <typeparam name="U">The type to convert the value to.</typeparam>
        /// <param name="value">A <see cref="KVToken"/> cast as a <see cref="IEnumerable{T}"/> of <see cref="KVToken"/>.</param>
        /// <returns>A converted value.</returns>
        public static U Value<U>(this IEnumerable<KVToken> value)
        {
            return value.Value<KVToken, U>();
        }

        /// <summary>
        /// Converts the value.
        /// </summary>
        /// <typeparam name="T">The source collection type.</typeparam>
        /// <typeparam name="U">The type to convert the value to.</typeparam>
        /// <param name="value">A <see cref="KVToken"/> cast as a <see cref="IEnumerable{T}"/> of <see cref="KVToken"/>.</param>
        /// <returns>A converted value.</returns>
        public static U Value<T, U>(this IEnumerable<T> value) where T : KVToken
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));

            KVToken token = value as KVToken;
            if (token == null)
            {
                throw new ArgumentException("Source value must be a KVToken.");
            }

            return token.Convert<KVToken, U>();
        }

        internal static IEnumerable<U> Values<T, U>(this IEnumerable<T> source, object key) where T : KVToken
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            if (key == null)
            {
                foreach (T token in source)
                {
                    if (token is KVValue value)
                    {
                        yield return Convert<KVValue, U>(value);
                    }
                    else
                    {
                        foreach (KVToken t in token.Children())
                        {
                            yield return t.Convert<KVToken, U>();
                        }
                    }
                }
            }
            else
            {
                foreach (T token in source)
                {
                    KVToken value = token[key];
                    if (value != null)
                    {
                        yield return value.Convert<KVToken, U>();
                    }
                }
            }
        }

        //TODO
        //public static IEnumerable<T> InDocumentOrder<T>(this IEnumerable<T> source) where T : JObject;

        /// <summary>
        /// Returns a collection of child tokens of every array in the source collection.
        /// </summary>
        /// <typeparam name="T">The source collection type.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the values of every token in the source collection.</returns>
        public static IKVEnumerable<KVToken> Children<T>(this IEnumerable<T> source) where T : KVToken
        {
            return Children<T, KVToken>(source).AsKVEnumerable();
        }

        /// <summary>
        /// Returns a collection of converted child tokens of every array in the source collection.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <typeparam name="U">The type to convert the values to.</typeparam>
        /// <typeparam name="T">The source collection type.</typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains the converted values of every token in the source collection.</returns>
        public static IEnumerable<U> Children<T, U>(this IEnumerable<T> source) where T : KVToken
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(c => c.Children()).Convert<KVToken, U>();
        }

        internal static IEnumerable<U> Convert<T, U>(this IEnumerable<T> source) where T : KVToken
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            foreach (T token in source)
            {
                yield return Convert<KVToken, U>(token);
            }
        }

        internal static U Convert<T, U>(this T token) where T : KVToken
        {
            if (token == null)
            {
                return default;
            }

            if (token is U
                // don't want to cast KVValue to its interfaces, want to get the internal value
                && typeof(U) != typeof(IComparable) && typeof(U) != typeof(IFormattable))
            {
                // HACK
                return (U)(object)token;
            }
            else
            {
                KVValue value = token as KVValue;
                if (value == null)
                {
                    throw new InvalidCastException("Cannot cast {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, token.GetType(), typeof(T)));
                }

                if (value.Value is U)
                {
                    return (U)value.Value;
                }

                Type targetType = typeof(U);

                if (ReflectionUtils.IsNullableType(targetType))
                {
                    if (value.Value == null)
                    {
                        return default;
                    }

                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                return (U)System.Convert.ChangeType(value.Value, targetType, CultureInfo.InvariantCulture);
            }
        }

        //TODO
        //public static void Remove<T>(this IEnumerable<T> source) where T : JContainer;

        /// <summary>
        /// Returns the input typed as <see cref="IKVEnumerable{T}"/>.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <returns>The input typed as <see cref="IKVEnumerable{T}"/>.</returns>
        public static IKVEnumerable<KVToken> AsKVEnumerable(this IEnumerable<KVToken> source)
        {
            return source.AsJEnumerable<KVToken>();
        }

        /// <summary>
        /// Returns the input typed as <see cref="IKVEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The source collection type.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the source collection.</param>
        /// <returns>The input typed as <see cref="IKVEnumerable{T}"/>.</returns>
        public static IKVEnumerable<T> AsJEnumerable<T>(this IEnumerable<T> source) where T : KVToken
        {
            if (source == null)
            {
                return null;
            }
            else if (source is IKVEnumerable<T>)
            {
                return (IKVEnumerable<T>)source;
            }
            else
            {
                return new KVEnumerable<T>(source);
            }
        }
    }
}
