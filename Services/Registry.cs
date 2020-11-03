using System;
using System.IO;

namespace AzureContainerRegistry.CLI.Services
{
    class Registry
    {
        private readonly string _registry;
        private readonly string _username;
        private readonly string _password;

        public string LoginUrl => _registry;

        public string UserName => _username;

        public string Password => _password;

        public Registry(string registry, string username, string password)
        {
            if (string.IsNullOrEmpty(registry))
                throw new ArgumentNullException(nameof(registry));

            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            _registry = registry;
            _username = username;
            _password = password;
        }
    }
}