using System;
using System.Reflection;

namespace DynamicExpresso.Reflection
{
	internal class IndexerData : MethodData
	{
		public readonly PropertyInfo Indexer;

		public IndexerData(PropertyInfo indexer)
		{
			Indexer = indexer;

			var method = indexer.GetGetMethod();
			if (method != null)
			{
				Parameters = method.GetParameters();
			}
			else
			{
				method = indexer.GetSetMethod();
				Parameters = RemoveLast(method.GetParameters());
			}
		}

		private static T[] RemoveLast<T>(T[] array)
		{
			var result = new T[array.Length - 1];
			Array.Copy(array, 0, result, 0, result.Length);
			return result;
		}
	}
}
