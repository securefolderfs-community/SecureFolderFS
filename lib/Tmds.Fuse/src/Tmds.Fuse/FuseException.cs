namespace Tmds.Fuse
{
    [System.Serializable]
    public class FuseException : System.Exception
    {
        public FuseException() { }
        public FuseException(string message) : base(message) { }
        public FuseException(string message, System.Exception inner) : base(message, inner) { }
        protected FuseException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}