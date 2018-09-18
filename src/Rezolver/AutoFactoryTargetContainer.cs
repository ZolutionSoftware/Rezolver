using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    // TODO: move to shared extensions class
    public static class AutoFactoryRegistrationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="targets"></param>
        public static void EnableAutoFactory<TService>(this IRootTargetContainer targets)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            Type funcType = typeof(Func<>).MakeGenericType(typeof(TService));

            var existingContainer = targets.FetchContainer(funcType);
        }
    }

    /// <summary>
    /// Target dictionary container which provides support for automatic production of delegates
    /// based on the <see cref="Func{TResult}"/> generic delegate type and its non-unary variants (i.e.
    /// <see cref="Func{T, TResult}"/> and up).
    /// </summary>
    /// <remarks>
    /// The container binds to the open generic delegate type, effectively decorating the container's own
    /// GenericTargetContainer for that type.
    /// </remarks>
    internal class AutoFactoryTargetContainer : ITargetContainer
    {
        /// <summary>
        /// Will be an open generic delegate type (such as <see cref="Func{TResult}"/>)
        /// </summary>
        public Type GenericFactoryTypeDefinition { get; }

        private ITargetContainer Inner { get; set; }

        private IRootTargetContainer Root { get; }


        //private Type[] _objectArgsForFunc;
        internal AutoFactoryTargetContainer(IRootTargetContainer root, Type factoryType)
        {
            Root = root;
            GenericFactoryTypeDefinition = factoryType;

        }

        private ITargetContainer EnsureInner()
        {
            // This reuses the same logic that TargetContainer employs via these extension methods
            return this.Inner ??
                (this.Inner = this.Root.CreateChildContainer(
                            this.Root.GetChildContainerType(this.GenericFactoryTypeDefinition)));
        }

        public ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            if (existing is AutoFactoryTargetContainer)
                return existing;
            else if (Inner != null)
                throw new NotSupportedException("This target container has already been combined with another");

            Inner = existing ?? throw new ArgumentNullException(nameof(existing));
            return this;
        }

        //private void Root_TargetRegistered(object sender, Events.TargetRegisteredEventArgs e)
        //{

        //    if (!(e.Target is AutoFactoryTarget))
        //    {
        //        // bit squeaky, this - will slow up registrations, hence why the configuration will split the auto 
        //        // func registration between 'common' funcs and 'extended' funcs.
        //        var type = GenericType.MakeGenericType(_objectArgsForFunc.Concat(new[] { e.Type }).ToArray());
        //        var newTarget = new AutoFactoryTarget(e.Target, type, e.Type, _objectArgsForFunc);
        //        this.Register(newTarget);
        //        //Root.AddKnownType(type);
        //    }
        //}

        public ITarget Fetch(Type type)
        {
            Type genericType;
            if (!TypeHelpers.IsGenericType(type) || (genericType = type.GetGenericTypeDefinition()) != GenericFactoryTypeDefinition)
            {
                throw new ArgumentException($"Only {GenericFactoryTypeDefinition} is supported by this container", nameof(type));
            }

            // just like EnumerableTargetContainer, we allow for specific Func<T> registrations
            var result = Inner.Fetch(type);

            if (result != null)
                return result;



            var typeArgs = TypeHelpers.GetGenericArguments(type);

            var requiredReturnType = typeArgs[typeArgs.Length - 1];

            var innerTarget = Root.Fetch(requiredReturnType);
            if (innerTarget == null)
                innerTarget = new ResolvedTarget(requiredReturnType);

            // create a func target (new type) which wraps the inner target
            return new AutoFactoryTarget(innerTarget, type, requiredReturnType, typeArgs.Take(typeArgs.Length - 1).ToArray());
        }

        public IEnumerable<ITarget> FetchAll(Type type)
        {
            // required to allow interoperability with the FetchAll() functionality; because the targets we return are
            // not in the underlying dictionary, so we have to 
            var baseResult = Inner.FetchAll(type);
            if (!baseResult.Any())
            {
                var individual = Fetch(type);
                if (individual != null)
                    return new[] { individual };

                return Enumerable.Empty<ITarget>();
            }

            return baseResult;
        }

        public ITargetContainer FetchContainer(Type type)
        {
            throw new NotImplementedException();
        }

        public void Register(ITarget target, Type serviceType = null)
        {
            throw new NotImplementedException();
        }

        public void RegisterContainer(Type type, ITargetContainer container)
        {
            throw new NotImplementedException();
        }
    }
}
