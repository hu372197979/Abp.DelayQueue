using Abp.Modules;
using Abp.Runtime.Caching.Redis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Abp.Reflection.Extensions;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor.Installer;
using Abp.DelayQueue.Abstractions;

namespace Abp.DelayQueue
{
 
    /// <summary>
    /// 模块功能
    /// </summary>
    [DependsOn(typeof(AbpKernelModule), 
        typeof(AbpRedisCacheModule))]
    public class DelayQueusModule : AbpModule
    {
        public override void PreInitialize()
        {
            base.PreInitialize();
        }
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
