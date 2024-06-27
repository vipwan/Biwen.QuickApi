using Biwen.QuickApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Biwen.QuickApi.Service
{
    /// <summary>
    /// 业务服务基类
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public abstract class BuinessServiceBase<TDbContext> where TDbContext : DbContext
    {
        /// <summary>
        /// Uow
        /// </summary>
        protected IUnitOfWork<TDbContext> Uow { get; }

        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger Logger { get; }

        public BuinessServiceBase(IUnitOfWork<TDbContext> uow, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(uow, nameof(uow));
            Uow = uow;
            Logger = logger ?? NullLogger.Instance;
        }
    }


    public abstract class BuinessServiceBase<TDbContext1, TDbContext2> : BuinessServiceBase<TDbContext1>
        where TDbContext1 : DbContext
        where TDbContext2 : DbContext
    {
        protected IUnitOfWork<TDbContext2> Uow2 { get; }

        public BuinessServiceBase(IUnitOfWork<TDbContext1> uow1, IUnitOfWork<TDbContext2> uow2, ILogger? logger = null) : base(uow1, logger)
        {
            ArgumentNullException.ThrowIfNull(uow2, nameof(uow2));
            Uow2 = uow2;
        }
    }

    public abstract class BuinessServiceBase<TDbContext1, TDbContext2, TDbContext3> : BuinessServiceBase<TDbContext1, TDbContext2>
        where TDbContext1 : DbContext
        where TDbContext2 : DbContext
        where TDbContext3 : DbContext
    {
        protected IUnitOfWork<TDbContext3> Uow3 { get; }

        public BuinessServiceBase(IUnitOfWork<TDbContext1> uow1, IUnitOfWork<TDbContext2> uow2, IUnitOfWork<TDbContext3> uow3, ILogger? logger) : base(uow1, uow2, logger)
        {
            ArgumentNullException.ThrowIfNull(uow3, nameof(uow3));
            Uow3 = uow3;
        }
    }
}