namespace SecureFolderFS.Cli.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ValidOptionsAttribute : Attribute
    {
        public IEnumerable<string> Values { get; }

        public ValidOptionsAttribute(params string[] values)
        {
            Values = values;
        }
    }
}