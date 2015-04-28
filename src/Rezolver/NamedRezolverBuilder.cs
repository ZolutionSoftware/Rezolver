namespace Rezolver
{
	public class NamedRezolverBuilder : ChildRezolverBuilder, INamedRezolverBuilder
	{
		private readonly string _name;

		public NamedRezolverBuilder(IRezolverBuilder parentBuilder, string name) 
			: base(parentBuilder)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}
	}
}