using Microsoft.Extensions.DependencyInjection;
using NRules;
using NRules.Fluent;
using NRules.Fluent.Dsl;
using System;
using System.Linq;
using System.Reflection;

namespace NRulesConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test1();
            Test2();
        }

        // 普通使用方式
        static void Test1()
        {
            //Load rules，载入规则
            var repository = new RuleRepository(); // 构建规则仓库
            repository.Load(x => x.From(typeof(PreferredCustomerDiscountRule).Assembly));

            #region 可以按需加载指定规则库，并添加到指定规则集中
            //repository.Load(x => x // 载入规则
            //    .From(Assembly.GetExecutingAssembly()) // 定义规则源
            //    .Where(rule => rule.Name.StartsWith("Test") || rule.IsTagged("Test")) // 过滤规则
            //    .To("MyRuleSet")); // 将规则加到规则集中 如果具有给定名称的规则集已经存在，那么Load方法只是向其添加规则。
            #endregion

            //Compile rules，编译规则
            var factory = repository.Compile();

            #region 可以只编译一部分
            //var ruleSets = repository.GetRuleSets();
            //var compiler = new RuleCompiler();
            //ISessionFactory factory = compiler.Compile(ruleSets.Where(x => x.Name == "MyRuleSet")); // 根据需要编译规则库
            #endregion

            //Create a working session，创建工作会话
            var session = factory.CreateSession();

            //Load domain model，载入领域模型
            var customer = new Customer("John Doe") { IsPreferred = true };
            var order1 = new Order(123456, customer, 2, 25.0);
            var order2 = new Order(123457, customer, 1, 100.0);

            //Insert facts into rules engine's memory，将事实放入引擎内存中
            session.Insert(customer);
            session.Insert(order1);
            session.Insert(order2);

            //Start match/resolve/act cycle，运行匹配、断言、执行的循环流程
            session.Fire();
        }

        // 使用ioc容器
        static void Test2()
        {
            IServiceCollection services = new ServiceCollection(); // 构建服务容器
            RegisterRules(services); // 注册规则类
            services.AddTransient<MyRuleActivator>(); // 注册规则创建类
            services.AddTransient<InjectService>();
            services.AddTransient<MyDependencyResolver>();
            IServiceProvider serviceProvider = services.BuildServiceProvider(); // 构造服务提供程序

            var ruleRepository = new RuleRepository(); // 构建规则仓库
            ruleRepository.Activator = serviceProvider.GetRequiredService<MyRuleActivator>(); // 替换默认规则激活器（用于构建规则对象），默认使用System.Activator
            ruleRepository.Load(r => r.From(Assembly.GetExecutingAssembly())); // 载入规则库

            var factory = ruleRepository.Compile(); // 编译等到会话工厂，同时构建了rete网络

            factory.DependencyResolver = serviceProvider.GetRequiredService<MyDependencyResolver>(); // 用于解析依赖服务

            var session = factory.CreateSession(); // 使用会话工厂构建会话实例

            //session.DependencyResolver

            //Load domain model，载入领域模型，即事实
            var customer = new Customer("John Doe") { IsPreferred = true };
            var order1 = new Order(123456, customer, 2, 25.0);
            var order2 = new Order(123457, customer, 1, 100.0);

            //Insert facts into rules engine's memory，将事实放入引擎内存中
            session.Insert(customer);
            session.Insert(order1);
            session.Insert(order2);

            //Start match/resolve/act cycle，运行匹配、解析、执行的循环流程
            session.Fire();
        }

        static void RegisterRules(IServiceCollection services)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type ruleType in assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Rule))))
            {
                services.AddTransient(ruleType);
            }
        }
    }
}
