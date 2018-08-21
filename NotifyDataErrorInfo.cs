using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System;

namespace Mx.CustomNotifyDataError
{
    public abstract class NotifyDataErrorInfo : INotifyDataErrorInfo
    {
        #region Private/Protected Member Variables
        private readonly Dictionary<string, List<PropertyErrorRule>> rulesDiccionary;
        #endregion

        #region Private/Protected Properties
        protected ICollection<PropertyErrorRule> RuleFor<TValue>(Expression<Func<TValue>> memberExpression)
        {
            string propertyName = string.Empty;

            if (memberExpression != null && memberExpression.Body is MemberExpression)
            {
                var property = (memberExpression.Body as MemberExpression).Member as PropertyInfo;
                propertyName = property.Name;
            }
            else
                return null;

            if (!rulesDiccionary.ContainsKey(propertyName))
                rulesDiccionary.Add(propertyName, new List<PropertyErrorRule>());

            return rulesDiccionary[propertyName];
        }
        protected ICollection<PropertyErrorRule> RuleFor(string propertyName)
        {
            if(!rulesDiccionary.ContainsKey(propertyName))
                rulesDiccionary.Add(propertyName, new List<PropertyErrorRule>());

            return rulesDiccionary[propertyName];
        }
        #endregion

        #region Private/Protected Methods
        private bool RulesFail(IReadOnlyList<PropertyErrorRule> rules)
        {
            if (rules != null && rules.Count != 0)
            {
                for (int index = 0; index < rules.Count; index++)
                    if (rules[index].FailWith())
                        return true;
            }

            return false;
        }

        protected void RaiseErrorsChanged([CallerMemberName] string propertyName = "")
        {
            if (!string.IsNullOrWhiteSpace(propertyName))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        protected abstract void SetErrorRules();
        #endregion

        #region Constructor
        public NotifyDataErrorInfo()
        {
            rulesDiccionary = new Dictionary<string, List<PropertyErrorRule>>(3);
            SetErrorRules();

            if (rulesDiccionary.Count != 0)
                foreach (var rules in rulesDiccionary)
                    rules.Value.TrimExcess();
        }
        #endregion

        #region Public Properties
        public bool CanValidate { get; set; }
        public bool HasErrors
        {
            get
            {
                if (CanValidate && rulesDiccionary != null && rulesDiccionary.Count != 0)
                {
                    foreach (var propertyRules in rulesDiccionary)
                        if (RulesFail(propertyRules.Value))
                            return true;
                }

                return false;
            }
        }
        #endregion

        #region Public Methods
        public bool PropertyHasErrors(string propertyName)
        {
            bool hasPropertyRules = !string.IsNullOrWhiteSpace(propertyName)
                && rulesDiccionary != null
                && rulesDiccionary.Count != 0
                && rulesDiccionary.ContainsKey(propertyName);

            return hasPropertyRules ? RulesFail(rulesDiccionary[propertyName]) : false;
        }
        public IEnumerable GetErrors(string propertyName)
        {
            if (CanValidate && !string.IsNullOrWhiteSpace(propertyName) && rulesDiccionary.ContainsKey(propertyName))
            {
                var rules = rulesDiccionary[propertyName];
                int rulesCount = rules?.Count ?? 0;

                if (rulesCount != 0)
                {
                    var messages = new string[rulesCount];
                    PropertyErrorRule rule;
                    int messagesCount = 0;

                    for (int index = 0; index < rulesCount; index++)
                    {
                        rule = rules[index];

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
        #endregion

        #region Events
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        #endregion
    }

    public static class ExtensionsPropertyErrorRule
    {
        public static ICollection<PropertyErrorRule> AddRule(this ICollection<PropertyErrorRule> collection, Func<bool> failWith, string message)
        {
            collection.Add(new PropertyErrorRule(failWith, message));
            return collection;
        }
    }
}
