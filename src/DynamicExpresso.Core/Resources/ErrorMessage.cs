using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicExpresso.Resources
{
	/// <summary>
	/// Used to provide a lazily accessed Error Message.
	/// </summary>
	public class ErrorMessage
	{
		private string _message;
		private Func<string> _getMessage;

		/// <summary>
		/// Initializes this instance to return the provided message.
		/// </summary>
		/// <param name="message"></param>
		public ErrorMessage(string message)
		{
			_message = message;
			_getMessage = null;
		}

		/// <summary>
		/// Initializes this instance to call the provided method to retrieve the message.
		/// </summary>
		/// <param name="message"></param>
		public ErrorMessage(Func<string> message)
		{
			_message = null;
			_getMessage = message;
		}

		/// <summary>
		/// Returns the message this instance was initialized with.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return _message ?? _getMessage();
		}

		public static implicit operator string(ErrorMessage message)
		{
			return message.ToString();
		}

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.OpenCurlyBracketExpected"/>.
		/// </summary>
		public static readonly ErrorMessage OpenCurlyBracketExpected = new ErrorMessage(() => ErrorMessages.OpenCurlyBracketExpected);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.OpenParenExpected"/>.
		/// </summary>
		public static readonly ErrorMessage OpenParenExpected = new ErrorMessage(() => ErrorMessages.OpenParenExpected);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.CloseCurlyBracketExpected"/>.
		/// </summary>
		public static readonly ErrorMessage CloseCurlyBracketExpected = new ErrorMessage(() => ErrorMessages.CloseCurlyBracketExpected);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.CloseParenOrCommaExpected"/>.
		/// </summary>
		public static readonly ErrorMessage CloseParenOrCommaExpected = new ErrorMessage(() => ErrorMessages.CloseParenOrCommaExpected);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.CloseBracketOrCommaExpected"/>.
		/// </summary>
		public static readonly ErrorMessage CloseBracketOrCommaExpected = new ErrorMessage(() => ErrorMessages.CloseBracketOrCommaExpected);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.SyntaxError"/>.
		/// </summary>
		public static readonly ErrorMessage SyntaxError = new ErrorMessage(() => ErrorMessages.SyntaxError);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.ColonExpected"/>.
		/// </summary>
		public static readonly ErrorMessage ColonExpected = new ErrorMessage(() => ErrorMessages.ColonExpected);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.CloseParenOrOperatorExpected"/>.
		/// </summary>
		public static readonly ErrorMessage CloseParenOrOperatorExpected = new ErrorMessage(() => ErrorMessages.CloseParenOrOperatorExpected);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.IdentifierExpected"/>.
		/// </summary>
		public static readonly ErrorMessage IdentifierExpected = new ErrorMessage(() => ErrorMessages.IdentifierExpected);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.EqualExpected"/>.
		/// </summary>
		public static readonly ErrorMessage EqualExpected = new ErrorMessage(() => ErrorMessages.EqualExpected);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.CloseTypeArgumentListExpected"/>.
		/// </summary>
		public static readonly ErrorMessage CloseTypeArgumentListExpected = new ErrorMessage(() => ErrorMessages.CloseTypeArgumentListExpected);

		/// <summary>
		/// The singleton instance that returns a call to <seealso cref="ErrorMessages.DotOrOpenParenExpected"/>.
		/// </summary>
		public static readonly ErrorMessage DotOrOpenParenExpected = new ErrorMessage(() => ErrorMessages.DotOrOpenParenExpected);
	}
}
