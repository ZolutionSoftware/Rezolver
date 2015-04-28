using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rezolver.Resources;

namespace Rezolver
{
	/// <summary>
	/// Represents a path to a named rezolver Builder.  Optimised for walking the path in both directions after creation.
	/// 
	/// Please note - this class is not thread-safe.
	/// </summary>
	public sealed class RezolverPath : ICloneable
	{
		public const string DefaultPathSeparator = ".";
		private static readonly Regex RxNoWhitespace = new Regex(@"\s");

		public string Path { get; private set; }
		public string PathSeparator { get; private set; }

		private string[] Items { get; set; }

		public string Current
		{
			get
			{
				if (_currentItem < 0 || _currentItem >= Items.Length)
					throw new InvalidOperationException(/*TODO: add resourced exception*/);
				return Items[_currentItem];
			}
		}

		/// <summary>
		/// used when walking the path
		/// </summary>
		private int _currentItem = -1;

		private string _next;

		public RezolverPath(string path, string pathSeparator = null)
		{
			PathSeparator = pathSeparator ?? DefaultPathSeparator;
			path.MustNotBeNull("path");
			Path = path;

			var items = path.Split(PathSeparator.ToCharArray());
			foreach (var item in items)
			{
				if (string.IsNullOrWhiteSpace(item))
					throw new ArgumentException(Exceptions.PathIsInvalid, "path");
				if (item.Length == 0)
					throw new ArgumentException(Exceptions.PathIsInvalid, "path");
				if (RxNoWhitespace.IsMatch(item))
					throw new ArgumentException(Exceptions.PathIsInvalid, "path");
			}

			Items = items;
		}

		public RezolverPath(RezolverPath source)
		{
			source.MustNotBeNull("source");
			Path = source.Path;
			Items = source.Items;
			PathSeparator = source.PathSeparator;
		}

		/// <summary>
		/// Creates a ResolverPath from the given string, using the default
		/// path separator.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static implicit operator RezolverPath(string source)
		{
			return new RezolverPath(source);
		}

		public bool MoveNext()
		{
			//invalidate next
			_next = null;
			return ++_currentItem < Items.Length;
		}

		public string Next
		{
			get
			{
				if (_next != null) return _next;

				if (_currentItem < 0)
					return _next = Items[0];
				else if (_currentItem >= Items.Length - 1)
					return null;
				return _next = Items[_currentItem + 1];
			}
		}

		public override string ToString()
		{
			if (Next != null)
				return string.Join(PathSeparator, Items.Skip(_currentItem + 1));
			return "";
		}

		/// <summary>
		/// creates a clone of this path, reset to the start.
		/// </summary>
		/// <returns></returns>
		public RezolverPath Clone()
		{
			return new RezolverPath(this);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}