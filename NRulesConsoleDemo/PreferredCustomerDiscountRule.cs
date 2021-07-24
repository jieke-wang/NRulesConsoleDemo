using NRules.Fluent.Dsl;
using NRules.RuleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRulesConsoleDemo
{
    [Name("Preferred customer discount")]
    //[Name("MyRule"), Description("Test rule that demonstrates metadata usage")] // 定义规则名称、描述
    //[Tag("Test"), Tag("Metadata")] // 定义标签
    //[Priority(10)] // 定义优先级
    //[Repeatability(RuleRepeatability.Repeatable)] // 定义是否可重入
    public class PreferredCustomerDiscountRule : Rule // 请确保规则类是公开的，否则引擎将无法找到它们。
    {
        public override void Define()
        {
            Customer customer = null;
            IEnumerable<Order> orders = null;
            //double total = 0;

            When()
                .Match<Customer>(() => customer, c => c.IsPreferred) // 模型匹配

                //.Exists<Order>(o => o.Customer == customer) // 存在规则

                //.Not<Order>(o => o.IsOpen == false) // 否定规则

                //.All<Order>(o => o.IsOpen)  // 通用限定符

                //.Or(x => x.Match<Customer>(() => customer, c => c.IsPreferred)
                //    .And(xx => xx.Match<Customer>(() => customer, c => c.IsPreferred == false)
                //        .Exists<Order>(o => o.Customer == customer, o => o.Price > 1000))) // 分组模式

                .Query(() => orders, x => x
                    .Match<Order>(
                        o => o.Customer == customer,
                        o => o.IsOpen,
                        o => !o.IsDiscounted)
                    .Collect()
                    .Where(c => c.Any())) // 复杂逻辑的规则

                //.Let(() => total, () => orders.Sum(o => o.UnitPrice * o.Quantity)) // 给变量绑值
                //.Having(() => total > 100) // 使用已存在的变量添加条件
                ;

            Then()
                .Do(ctx => ApplyDiscount(orders, 10.0)) // 执行特定业务
                .Do(ctx => ctx.UpdateAll(orders)); // 将事实更新到引擎内存中，以触发其它规则
        }

        private static void ApplyDiscount(IEnumerable<Order> orders, double discount)
        {
            foreach (var order in orders)
            {
                Console.WriteLine($"Apply discount {discount} to order {order.Id}");
                order.ApplyDiscount(discount);
            }
        }
    }
}

/*
响应式 LINQ 查询
Match - 通过匹配规则引擎内存中的事实来启动查询
Where - 按一组条件过滤源元素
GroupBy - 将源元素聚合到组中
Collect - 将源元素聚合到集合中
Select - 呈现源集合
SelectMany - 展开源集合
*/
