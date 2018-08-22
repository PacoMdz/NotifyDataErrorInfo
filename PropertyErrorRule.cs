using System;

namespace Mx.Models.NotifyDataErrors
{
	/// <summary>
	/// Define a rule for property validation.
	/// </summary>
	public struct PropertyErrorRule
	{
		/// <summary>
		/// Define a rule for property validation, with a message.
		/// </summary>
		/// <param name="failWith">Rule condition to test the property value</param>
		/// <param name="message">Message to show when the rule fails</param>
		public PropertyErrorRule(Func<bool> failWith, string message)
		{
			FailWith = failWith;
			Message = message;
		}

		/// <summary>
		/// Rule condition to test the property value.
		/// </summary>
		public Func<bool> FailWith { get; }
		/// <summary>
		/// Message to show when the rule fails.
		/// </summary>
		public string Message { get; }
	}
}
