using System;

namespace SecureFolderFS.Sdk.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class InjectAttribute<T> : Attribute
        where T : notnull
    {
        /// <summary>
        /// Gets or sets the value that represents the property name of the injected class.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the value that represents the visibility of the injected class.
        /// </summary>
        public string? Visibility { get; set; }
    }
}
