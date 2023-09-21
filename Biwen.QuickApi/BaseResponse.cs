namespace Biwen.QuickApi
{
    public abstract class BaseResponse
    {

    }

    /// <summary>
    /// 空输出
    /// </summary>
    public class EmptyResponse : BaseResponse
    {
        public static EmptyResponse New => new();
    }
}