using NRules.Extensibility;
using NRules.Fluent;
using NRules.Fluent.Dsl;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRulesConsoleDemo
{
    public class MyRuleActivator : IRuleActivator // 自定义规则创建器，可对接ioc容器
    {
        private IServiceProvider _serviceProvider;
        public MyRuleActivator(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public IEnumerable<Rule> Activate(Type type)
        {
            yield return this._serviceProvider.GetService(type) as Rule;
        }
    }

    public class MyDependencyResolver : IDependencyResolver // 自定义服务解析器
    {
        private IServiceProvider _serviceProvider;
        public MyDependencyResolver(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public object Resolve(IResolutionContext context, Type serviceType)
        {
            return this._serviceProvider.GetService(serviceType);
        }
    }
}
