﻿using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConfigurationSubstitution
{
    public class ConfigurationSubstitutor
    {
        private readonly string _startsWith;
        private readonly string _endsWith;
        private readonly Regex _findSubstitutions;
        private readonly bool _exceptionOnMissingVariables;

        public ConfigurationSubstitutor(bool exceptionOnMissingVariables = true) : this("{", "}", exceptionOnMissingVariables)
        {
        }

        public ConfigurationSubstitutor(string substitutableStartsWith, string substitutableEndsWith, bool exceptionOnMissingVariables = true)
        {
            _startsWith = substitutableStartsWith;
            _endsWith = substitutableEndsWith;
            var escapedStart = Regex.Escape(_startsWith);
            var escapedEnd = Regex.Escape(_endsWith);
            _findSubstitutions = new Regex(@"(?<=" + escapedStart + @")[^" + escapedStart + escapedEnd + "]*(?=" + escapedEnd + @")",
                RegexOptions.Compiled);
            _exceptionOnMissingVariables = exceptionOnMissingVariables;
        }

        public bool ConfigurationExists(IConfiguration configuration, string key)
        {   
            var value = configuration[key];
            return value != null;
        }

        public string ApplySubstitutionFromKey(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value)) return value;

            return ApplySubstitution(configuration, value);
        }

        private string ApplySubstitution(IConfiguration configuration, string value)
        {
            var captures = _findSubstitutions.Matches(value).Cast<Match>().SelectMany(m => m.Captures.Cast<Capture>());
            foreach (var capture in captures)
            {
                var substitutedValue = configuration[capture.Value];

                if (substitutedValue == null && _exceptionOnMissingVariables)
                {
                    throw new UndefinedConfigVariableException($"{_startsWith}{capture.Value}{_endsWith}");
                }

                value = value.Replace(_startsWith + capture.Value + _endsWith, substitutedValue);
            }
            return value;
        }
    }
}
