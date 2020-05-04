using System.Collections.Generic;

namespace GameHub_Console
{
	/// <summary>
	/// Collection of extension methods
	/// </summary>
	public static class CExtensions
	{
		/// <summary>
		/// Return a new HashSet of type T from the IEnumerable
		/// </summary>
		/// <typeparam name="T">Type specifier for source and return</typeparam>
		/// <param name="source">Source of data to be saves as a HashSet</param>
		/// <param name="comparer">Hashset comparison function. Defaults to null</param>
		/// <returns>New HashSet of type T containing source data</returns>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
		{
			return new HashSet<T>(source, comparer);
		}
	}
}
