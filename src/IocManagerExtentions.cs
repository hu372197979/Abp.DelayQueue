using Abp.Dependency;
using Abp.Modules;
using Castle.MicroKernel.Registration;
using System;

namespace Abp.DelayQueue
{
    /// <summary>
    /// IocManager扩展类
    /// </summary>
    public static class IocManagerExtentions
    {
        /// <summary>
        /// 依赖注入拓展（跨模块注入处理）
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <param name="iocmanager"></param>
        /// <param name="basedOnType"></param>
        public static void RegisterAssembly<TModule>(this IIocManager iocmanager,Type basedOnType)
            where TModule:AbpModule
        {
            var container = iocmanager.IocContainer;
            // 注册命令处理类
            container.Register(
                Classes.FromAssemblyContaining<TModule>()
                    .BasedOn(basedOnType)
                    .WithServiceSelf()
                    .WithServiceAllInterfaces()
                    .AllowMultipleMatches()
            );
            // 注册事件处理类
            container.Register(
                Classes.FromAssemblyContaining<TModule>()
                    .BasedOn(basedOnType)
                    .WithServiceSelf()
                    .WithServiceAllInterfaces()
                    .AllowMultipleMatches()
            );
            
        }
    }
}