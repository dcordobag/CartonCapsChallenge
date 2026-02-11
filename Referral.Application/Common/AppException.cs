namespace Referrals.Application.Common
{
    /// <summary>
    /// App Exception
    /// </summary>
    public sealed class AppException(string code, string message) : Exception(message)
    {
        public string Code { get; } = code;
    }

}
