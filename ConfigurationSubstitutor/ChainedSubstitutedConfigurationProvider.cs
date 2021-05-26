﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace ConfigurationSubstitution
{
    /// This class is inspired from Microsoft.Extensions.Configuration.ChainedConfigurationProvider
    public class ChainedSubstitutedConfigurationProvider: IConfigurationProvider
    {
        private readonly IConfiguration _config;
        private readonly ConfigurationSubstitutor _substitutor;

        /// <summary>
        /// Initialize a new instance from the configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public ChainedSubstitutedConfigurationProvider(IConfiguration config, ConfigurationSubstitutor substitutor)
        {
            _config = config;
            _substitutor = substitutor;
        }

        /// <summary>
        /// Tries to get a configuration value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>True</c> if a value for the specified key was found, otherwise <c>false</c>.</returns>
        public bool TryGet(string key, out string value)
        {
            if (_substitutor.ConfigurationExists(_config, key))
                value = _substitutor.ApplySubstitutionFromKey(_config, key);
            else
                value = null;
            
            return true;
        }

        /// <summary>
        /// Sets a configuration value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(string key, string value) => _config[key] = value;

        /// <summary>
        /// Returns a change token if this provider supports change tracking, null otherwise.
        /// </summary>
        /// <returns>The change token.</returns>
        public IChangeToken GetReloadToken() => _config.GetReloadToken();

        /// <summary>
        /// Loads configuration values from the source represented by this <see cref="IConfigurationProvider"/>.
        /// </summary>
        public void Load() { }

        /// <summary>
        /// Returns the immediate descendant configuration keys for a given parent path based on this
        /// <see cref="IConfigurationProvider"/>s data and the set of keys returned by all the preceding
        /// <see cref="IConfigurationProvider"/>s.
        /// </summary>
        /// <param name="earlierKeys">The child keys returned by the preceding providers for the same parent path.</param>
        /// <param name="parentPath">The parent path.</param>
        /// <returns>The child keys.</returns>
        public IEnumerable<string> GetChildKeys(
            IEnumerable<string> earlierKeys,
            string parentPath)
        {
            var section = parentPath == null ? _config : _config.GetSection(parentPath);
            var children = section.GetChildren();
            var keys = new List<string>();
            keys.AddRange(children.Select(c => c.Key));
            return keys.Concat(earlierKeys)
                .OrderBy(k => k, ConfigurationKeyComparer.Instance);
        }
    }
}
