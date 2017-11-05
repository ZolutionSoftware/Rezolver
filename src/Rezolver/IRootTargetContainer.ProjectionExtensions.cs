using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    public static partial class RootTargetContainerExtensions
    {
        public static void RegisterProjection<TFrom, TTo>(this IRootTargetContainer targets)
        {
            RegisterProjection<TFrom, TTo, TTo>(targets);
        }

        public static void RegisterProjection<TFrom, TTo, TImplementation>(this IRootTargetContainer targets)
        {
            RegisterProjection(targets, typeof(TFrom), typeof(TTo), typeof(TImplementation));
        }

        public static void RegisterProjection<TFrom, TTo>(this IRootTargetContainer targets, Func<IRootTargetContainer, ITarget, Type> implementationTypeSelector)
        {
            RegisterProjection(targets, typeof(TFrom), typeof(TTo), implementationTypeSelector);
        }

        public static void RegisterProjection(this IRootTargetContainer targets, Type fromService, Type toService)
        {
            RegisterProjection(targets, fromService, toService, toService);
        }

        public static void RegisterProjection(this IRootTargetContainer targets, Type fromType, Type toType, Type implementationType)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            RegisterProjection(targets, fromType, toType, (r, t) => implementationType);
        }

        public static void RegisterProjection(this IRootTargetContainer targets, Type fromType, Type toType, Func<IRootTargetContainer, ITarget, Type> implementationTypeSelector)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));
            if (fromType == null)
                throw new ArgumentNullException(nameof(fromType));
            if (toType == null)
                throw new ArgumentNullException(nameof(toType));

            if (implementationTypeSelector == null)
                implementationTypeSelector = (r, t) => toType;

            RegisterProjectionInternal(targets, fromType, toType, (r, t) =>
            {
                var implementationType = implementationTypeSelector(r, t);
                if (implementationType == null)
                    throw new InvalidOperationException($"Implementation type returned for projection from { fromType } to { toType } for target { t } returned null");
                // REVIEW: Cache the .ForType result on a per-type basis? It's container-agnostic.
                var target = r.Fetch(implementationType);
                return target != null && !target.UseFallback ? target : Target.ForType(implementationType);
            });
        }

        public static void RegisterProjection<TFrom, TTo>(this IRootTargetContainer targets, Func<IRootTargetContainer, ITarget, ITarget> implementationTargetFactory)
        {
            RegisterProjection(targets, typeof(TFrom), typeof(TTo), implementationTargetFactory);
        }

        public static void RegisterProjection(this IRootTargetContainer targets, Type fromType, Type toType, Func<IRootTargetContainer, ITarget, ITarget> implementationTargetFactory)
        {
            RegisterProjectionInternal(targets ?? throw new ArgumentNullException(nameof(targets)),
                fromType ?? throw new ArgumentNullException(nameof(fromType)),
                toType ?? throw new ArgumentNullException(nameof(toType)),
                implementationTargetFactory ?? throw new ArgumentNullException(nameof(implementationTargetFactory)));
        }

        private static void RegisterProjectionInternal(this IRootTargetContainer targets, Type fromType, Type toType, Func<IRootTargetContainer, ITarget, ITarget> implementationTargetFactory)
        {
            targets.RegisterContainer(
                typeof(IEnumerable<>).MakeGenericType(toType),
                new ProjectionTargetContainer(targets, fromType, toType, implementationTargetFactory));
        }
    }
}
