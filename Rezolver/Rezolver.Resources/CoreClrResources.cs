

namespace Rezolver.Resources
{
#if ASPNETCORE50
	/// <summary>
	/// Produced by a text template from the Resx file used by the original Rezolver library.
	///	This is here to provide pseudo-resourced exception messages in english only, until
	/// the localisation story is complete for vNext projects.
	/// </summary>
	internal static class Exceptions
	{
		internal static string DeclaredTypeIsNotCompatible_Format
		{
			get { return @"The declared type {0} is not compatible with the type {1}"; }
		}
		internal static string PathIsInvalid
		{
			get { return @"The path {0} is invalid.  All path steps must contain non-whitespace characters and be at least one character in length"; }
		}
		internal static string TargetDoesntSupportType_Format
		{
			get { return @"The target does not support the type {0}"; }
		}
		internal static string TargetIsNullButTypeIsNotNullable_Format
		{
			get { return @"The type {0} is not a nullable type"; }
		}
		internal static string TypeIsAlreadyRegistered
		{
			get { return @"The type {0} has already been registered"; }
		}
		internal static string PathIsAtEnd
		{
			get { return @"path's Next must not be null - pass path as null once it's reached the last item"; }
		}
		internal static string LambdaBodyIsNotNewExpressionFormat
		{
			get { return @"The body of the lambda ""{0}"" is not a NewExpression"; }
		}
		internal static string NoDefaultOrAllOptionalConstructorFormat
		{
			get { return @"The type {0} has no default constructor, nor any constructors where all the parameters are optional."; }
		}
		internal static string NotRuntimeMethod
		{
			get { return @"This method is not to be called at run-time - it is only used for static expression analysis in creating IRezolveTargets for an IRezolveBuilder"; }
		}
		internal static string UnableToResolveTypeFromBuilderFormat
		{
			get { return @"Unable to resolve type {0} from builder"; }
		}
		internal static string NoPublicConstructorsDefinedFormat
		{
			get { return @"No public constructors declared on the type {0}"; }
		}
		internal static string MoreThanOneConstructorFormat
		{
			get { return @"More than one constructor for {0} qualifies as a target for Auto construction"; }
		}
		internal static string NoConstructorSetOnNewExpression
		{
			get { return @"No constructor has been set on the NewExpression - this is not allowed."; }
		}
		internal static string CyclicDependencyDetectedInTargetFormat
		{
			get { return @"Cyclic dependency detected in targets - current target of type {0} with DeclaredType of {1} has tried to include itself in its expression."; }
		}
		internal static string MoreThanOneObjectFoundInScope
		{
			get { return @"More than one matching object was found in the scope"; }
		}
		internal static string ScopedSingletonRequiresAScope
		{
			get { return @"A lifetime scope is required for a scoped singleton"; }
		}
		internal static string LambdaBodyNewExpressionIsWrongTypeFormat
		{
			get { return @"The expression {0} does not represent calling a constructor of the type {1}"; }
		}
	}
#endif	
}