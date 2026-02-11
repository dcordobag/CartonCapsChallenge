namespace Referrals.Domain.Exceptions
{
    /// <summary>
    /// Domain Exception
    /// </summary>
    public sealed class DomainException : Exception
    {
        /// <summary>
        /// Domain Exception.
        /// </summary>
        public DomainException(string code, string message) : base(message)
        {
            Code = code;
        }

        public string Code { get; }
    }
}
