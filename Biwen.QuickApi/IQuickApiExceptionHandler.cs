using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.QuickApi
{
    /// <summary>
    /// 全局QuickApi异常处理
    /// </summary>
    public interface IQuickApiExceptionHandler
    {
        Task HandleAsync(Exception exception);
    }
}