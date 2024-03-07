namespace Biwen.QuickApi
{
    /// <summary>
    /// QuickApi异常
    /// </summary>
    public class QuickApiExcetion(string message) : Exception(message)
    {
    }
}