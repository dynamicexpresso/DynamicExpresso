using System;

namespace DynamicExpresso.Parsing
{
	internal class ParseSignatures
	{
		public interface ILogicalSignatures
		{
			void F(bool x, bool y);
			void F(bool? x, bool? y);
		}

		public interface IArithmeticSignatures
		{
			void F(int x, int y);
			void F(uint x, uint y);
			void F(long x, long y);
			void F(ulong x, ulong y);
			void F(float x, float y);
			void F(double x, double y);
			void F(decimal x, decimal y);
			void F(int? x, int? y);
			void F(uint? x, uint? y);
			void F(long? x, long? y);
			void F(ulong? x, ulong? y);
			void F(float? x, float? y);
			void F(double? x, double? y);
			void F(decimal? x, decimal? y);
		}

		public interface IRelationalSignatures : IArithmeticSignatures
		{
			void F(string x, string y);
			void F(char x, char y);
			void F(DateTime x, DateTime y);
			void F(TimeSpan x, TimeSpan y);
			void F(char? x, char? y);
			void F(DateTime? x, DateTime? y);
			void F(TimeSpan? x, TimeSpan? y);
		}

		public interface IEqualitySignatures : IRelationalSignatures
		{
			void F(bool x, bool y);
			void F(bool? x, bool? y);
		}

		public interface IAddSignatures : IArithmeticSignatures
		{
			void F(DateTime x, TimeSpan y);
			void F(TimeSpan x, TimeSpan y);
			void F(DateTime? x, TimeSpan? y);
			void F(TimeSpan? x, TimeSpan? y);
		}

		public interface ISubtractSignatures : IAddSignatures
		{
			void F(DateTime x, DateTime y);
			void F(DateTime? x, DateTime? y);
		}

		public interface INegationSignatures
		{
			void F(int x);
			void F(long x);
			void F(float x);
			void F(double x);
			void F(decimal x);
			void F(int? x);
			void F(long? x);
			void F(float? x);
			void F(double? x);
			void F(decimal? x);
		}

		public interface INotSignatures
		{
			void F(bool x);
			void F(bool? x);
		}

		//interface IEnumerableSignatures
		//{
		//    void Where(bool predicate);
		//    void Any();
		//    void Any(bool predicate);
		//    void All(bool predicate);
		//    void Count();
		//    void Count(bool predicate);
		//    void Min(object selector);
		//    void Max(object selector);
		//    void Sum(int selector);
		//    void Sum(int? selector);
		//    void Sum(long selector);
		//    void Sum(long? selector);
		//    void Sum(float selector);
		//    void Sum(float? selector);
		//    void Sum(double selector);
		//    void Sum(double? selector);
		//    void Sum(decimal selector);
		//    void Sum(decimal? selector);
		//    void Average(int selector);
		//    void Average(int? selector);
		//    void Average(long selector);
		//    void Average(long? selector);
		//    void Average(float selector);
		//    void Average(float? selector);
		//    void Average(double selector);
		//    void Average(double? selector);
		//    void Average(decimal selector);
		//    void Average(decimal? selector);
		//}
	}
}
