namespace Rezolver
{
	public class NamedRezolverScope : ChildRezolverScope, INamedRezolverScope
	{
		private readonly string _name;

		public NamedRezolverScope(IRezolverScope parentScope, string name) 
			: base(parentScope)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}
	}
}