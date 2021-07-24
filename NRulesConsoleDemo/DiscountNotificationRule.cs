using NRules.Fluent.Dsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRulesConsoleDemo
{
    [Priority(1)]
    public class DiscountNotificationRule : Rule
    {
        public override void Define()
        {
            Customer customer = null;

            When()
                .Match<Customer>(() => customer)
                .Exists<Order>(o => o.Customer == customer, o => o.PercentDiscount > 0.0);

            Then()
                .Do(_ => customer.NotifyAboutDiscount());
        }
    }
    
    [Name("Orders by customer")]
    public class OrdersByCustomerRule : Rule
    {
        public override void Define()
        {
            IGrouping<Customer, Order> group = null;
            InjectService injectService = null;

            Dependency().Resolve(() => injectService); // 使用容器注入对象，规则依赖项不能在规则条件中使用（即不能在When中使用）。需要在session factory或session上配置DependencyResolver

            When()
                .Query(() => group, q =>
                    from o in q.Match<Order>()
                    where o.IsOpen
                    group o by o.Customer into g
                    select g);

            Then()
                .Do(ctx => Console.WriteLine("Customer {0} has {1} open order(s)",
                    group.Key.Name, group.Count()))
                .Do(ctx => injectService.DoSomething());
        }
    }
}
