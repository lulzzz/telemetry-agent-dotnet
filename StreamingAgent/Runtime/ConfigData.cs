// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime
{
    public interface IConfigData
    {
        string GetString(string key);
        int GetInt(string key);
        TimeSpan GetTimeSpan(string key);
        bool GetBoolean(string key);
    }

    public class ConfigData : IConfigData
    {
        private readonly IConfigurationRoot configuration;

        public ConfigData()
        {
            // More info about configuration at
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            this.configuration = configurationBuilder.Build();
        }

        public string GetString(string key)
        {
            var value = this.configuration.GetValue<string>(key);
            return ReplaceEnvironmentVariables(value);
        }

        public int GetInt(string key)
        {
            try
            {
                return Convert.ToInt32(this.GetString(key));
            }
            catch (Exception e)
            {
                throw new InvalidConfigurationException($"Unable to load configuration value for '{key}'", e);
            }
        }

        public TimeSpan GetTimeSpan(string key)
        {
            try
            {
                return TimeSpan.Parse(this.GetString(key));
            }
            catch (Exception e)
            {
                throw new InvalidConfigurationException($"Unable to load configuration value for '{key}'", e);
            }
        }

        public bool GetBoolean(string key)
        {
            try
            {
                return Convert.ToBoolean(this.GetString(key));
            }
            catch (Exception e)
            {
                throw new InvalidConfigurationException($"Unable to load configuration value for '{key}'", e);
            }
        }

        private static string ReplaceEnvironmentVariables(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            // Extract the name of all the substitutions required
            // using the following pattern, e.g. ${VAR_NAME}
            const string pattern = @"\${(?'key'[a-zA-Z_][a-zA-Z0-9_]*)}";
            var keys = (from Match m
                        in Regex.Matches(value, pattern)
                        select m.Groups[1].Value).ToArray();

            foreach (DictionaryEntry x in Environment.GetEnvironmentVariables())
            {
                if (keys.Contains(x.Key))
                {
                    value = value.Replace("${" + x.Key + "}", x.Value.ToString());
                }
            }

            return value;
        }
    }
}
