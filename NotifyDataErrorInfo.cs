using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System;

namespace Mx.Models.NotifyDataErrors
{
	/// <summary>
	/// Implements the INotifyDataErrorInfo interface.
	/// </summary>
	public abstract class NotifyDataErrorInfo : INotifyDataErrorInfo
	{
		#region Private/Protected Member Variables
		/// <summary>
		/// Set of rules for the properties.
		/// </summary>
		private readonly Dictionary<string, List<PropertyErrorRule>> validationRules;
		#endregion

		#region Private/Protected Properties
		/// <summary>
		/// Initialize or gets the set of rules of a property.
		/// </summary>
		/// <typeparam name="TValue">Type of the property</typeparam>
		/// <param name="memberExpression"></param>
		/// <returns>Set of rules</returns>
		protected List<PropertyErrorRule> RuleFor<TValue>(Expression<Func<TValue>> memberExpression)
		{
			if (memberExpression == null)
				throw new ArgumentNullException("memberExpression", "Member Expression can not be null.");

			var expressionBody = memberExpression.Body as MemberExpression;

			if (expressionBody == null)
				throw new ArgumentException("Expression is not Member type.", "memberExpression");

			string propertyName = (expressionBody.Member as PropertyInfo)?.Name;

			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentException("Member Expression does not contains property name.", "memberExpression");

			if (!validationRules.ContainsKey(propertyName))
				validationRules.Add(propertyName, new List<PropertyErrorRule>(1));

			return validationRules[propertyName];
		}
		/// <summary>
		/// Initialize or gets the set of rules of a property.
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>Set of rules</returns>
		protected List<PropertyErrorRule> RuleFor(string propertyName)
		{
			if (!validationRules.ContainsKey(propertyName))
				validationRules.Add(propertyName, new List<PropertyErrorRule>(1));

			return validationRules[propertyName];
		}
		#endregion

		#region Private/Protected Methods
		/// <summary>
		/// Notify subscribers property errors changed.
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		protected void RaisePropertyErrors([CallerMemberName] string propertyName = "")
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName", "Property name can not be null.");

			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
		}
		/// <summary>
		/// Define the rules of the properties to validate.
		/// </summary>
		protected abstract void SetErrorRules();
		#endregion

		#region Constructor
		public NotifyDataErrorInfo()
		{
			validationRules = new Dictionary<string, List<PropertyErrorRule>>(3);
			SetErrorRules();

			if (validationRules.Count != 0)
				foreach (var rules in validationRules)
					rules.Value.TrimExcess();
		}
		public NotifyDataErrorInfo(int capacity)
		{
			validationRules = new Dictionary<string, List<PropertyErrorRule>>(capacity);
			SetErrorRules();

			if (validationRules.Count != 0)
				foreach (var rules in validationRules)
					rules.Value.TrimExcess();
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Enables or disable the validations
		/// </summary>
		public bool CanValidate { get; set; }
		/// <summary>
		/// Check if the object has validation erros.
		/// </summary>
		public bool HasErrors
		{
			get
			{
				if (CanValidate && validationRules.Count != 0)
				{
					foreach (var propertyRules in validationRules)
						if (propertyRules.Value.AnyFails())
							return true;
				}

				return false;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Check if the property of the object has validation erros.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>Property has erros</returns>
		public bool PropertyHasErrors(string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName", "Property name can not be null.");

			if (validationRules.Count != 0)
			{
				List<PropertyErrorRule> propertyRules;

				if (validationRules.TryGetValue(propertyName, out propertyRules))
					return propertyRules?.AnyFails() ?? false;
			}

			return false;
		}
		/// <summary>
		/// Get the messages of all failed validations.
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>Set of messages</returns>
		public string[] GetErrorsMessages(string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName", "Property name can not be null.");

			List<PropertyErrorRule> propertyRules;

			if (CanValidate && validationRules.TryGetValue(propertyName, out propertyRules))
			{
				int rulesCount = propertyRules?.Count ?? 0;

				if (rulesCount != 0)
				{
					var messages = new string[rulesCount];
					PropertyErrorRule rule;
					int messagesCount = 0;

					for (int index = 0; index < rulesCount; index++)
					{
						rule = propertyRules[index];

						if (rule.FailWith())
							messages[messagesCount++] = rule.Message;
					}

					if (messagesCount == 0)
						return null;

					else if (messagesCount < messages.Length)
						Array.Resize(ref messages, messagesCount);
					
					return messages;
				}
			}

			return null;
		}
		/// <summary>
		/// Get the messages of all failed validations.
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>Set of messages</returns>
		public IEnumerable GetErrors(string propertyName)
		{
			return GetErrorsMessages(propertyName);
		}
		#endregion

		#region Events
		/// <summary>
		/// Notify all subscribers when a property error validation changed.
		/// </summary>
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
		#endregion
	}

	public static class ExtensionsPropertyErrorRule
	{
		/// <summary>
		/// Add a rule to the set of the rules of a property.
		/// </summary>
		/// <param name="collection">Set of rules</param>
		/// <param name="failWith">Action to test</param>
		/// <param name="message">Message to show when test fails</param>
		/// <returns>Set of rules</returns>
		public static List<PropertyErrorRule> AddRule(this List<PropertyErrorRule> collection, Func<bool> failWith, string message)
		{
			if (failWith == null)
				throw new ArgumentNullException("failWith", "FailWith action can not be null.");

			if (message == null)
				throw new ArgumentNullException("rules", "Rule message can not be null.");

			collection.Add(new PropertyErrorRule(failWith, message));

			return collection;
		}

		/// <summary>
		/// Add a collection of rules to the set of the rules of a property.
		/// </summary>
		/// <param name="collection">Set of rules</param>
		/// <param name="rules">Collection of rules</param>
		/// <returns>Set of rules</returns>
		public static List<PropertyErrorRule> AddRules(this List<PropertyErrorRule> collection, PropertyErrorRule[] rules)
		{
			if (rules == null)
				throw new ArgumentNullException("rules", "Set of rules can not be null.");

			else if (rules.Length != 0)
				collection.AddRange(rules);

			return collection;
		}

		/// <summary>
		/// Tests if all set of rules pass.
		/// </summary>
		/// <returns>If a rule fails.</returns>
		public static bool AnyFails(this IList<PropertyErrorRule> collection)
		{
			int rulesCount = collection.Count;

			if (rulesCount != 0)
			{
				for (int index = 0; index < rulesCount; index++)
					if (collection[index].FailWith())
						return true;
			}

			return false;
		}
	}
}
