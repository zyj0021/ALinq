using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using ALinq;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public class MySqlTest : SqlTest
    {
        private static TextWriter writer;

        public override NorthwindDatabase CreateDataBaseInstace()
        {
            writer = Console.Out;
            var xmlMapping = XmlMappingSource.FromStream(GetType().Assembly.GetManifestResourceStream("Test.Northwind.MySql.map"));
            return new MySqlNorthwind(CreateConnection().ConnectionString) { Log = writer };//xmlMapping
        }

        protected DbConnection CreateConnection()
        {
            //throw new NotImplementedException();
            return MySqlNorthwind.CreateConnection("root", "test", "Northwind", "localhost", 3306);
        }

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            //var type = typeof(SQLiteTest);
            //var path = type.Module.FullyQualifiedName;
            //var filePath = Path.GetDirectoryName(path) + @"\ALinq.MySQL.lic";
            //File.Copy(@"E:\ALinqs\ALinq1.8\Test\ALinq.MySQL.lic", filePath);
            //filePath = Path.GetDirectoryName(path) + @"\Northwind.MySql.map";
            //File.Copy(@"E:\ALinqs\ALinq1.8\Test\Northwind.MySql.map", filePath);

            //writer = new StreamWriter("c:/MySql.txt", false);
            //var database = new MySqlNorthwind(Cr) { Log = writer };
            //if (!database.DatabaseExists())
            //{
            //    database.CreateDatabase();
            //    database.Connection.Close();
            //}
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            writer.Flush();
            writer.Close();
        }

        public static Func<MySqlNorthwind, string, IQueryable<Customer>> CustomersByCity =
            ALinq.CompiledQuery.Compile((MySqlNorthwind db, string city) =>
                                        from c in db.Customers
                                        where c.City == city
                                        select c);
        public static Func<MySqlNorthwind, string, Customer> CustomersById =
            ALinq.CompiledQuery.Compile((MySqlNorthwind db, string id) =>
                                               Enumerable.Where(db.Customers, c => c.CustomerID == id).First());

        [TestMethod]
        public void StoreAndReuseQuery()
        {
            var customers = CustomersByCity((MySqlNorthwind)db, "London").ToList();
            Assert.IsTrue(customers.Count() > 0);

            var id = customers.First().CustomerID;
            var customer = CustomersById((MySqlNorthwind)db, id);
            Assert.AreEqual("London", customer.City);
        }

        [TestMethod]
        public void StringConnect()
        {
            var connstr = CreateConnection().ConnectionString;
            var context = new ALinq.DataContext(connstr, typeof(ALinq.MySQL.MySqlProvider));
            context.Connection.Open();
            context.Connection.Close();
        }

        //存储过程
        //1、标量返回
        //[TestMethod]
        //public void Procedure_GetCustomersCountByRegion()
        //{
        //    var regions = db.Regions.ToList();
        //    var groups = db.Customers.GroupBy(o => o.Region).Select(g => new { Count = g.Count(), Region = g.Key }).ToArray();
        //    foreach (var group in groups)
        //    {
        //        if (group.Region == null) continue;
        //        var count1 = group.Count;
        //        var count2 = ((MySqlNorthwind)db).GetCustomersCountByRegion(group.Region);
        //        Assert.AreEqual(count1, count2);
        //        Console.WriteLine(group.Region + ":" + count1);
        //    }
        //}


        //2、单一结果集返回
        //[TestMethod]
        //public void Procedure_GetCustomersByCity()
        //{
        //    var groups = db.Customers.GroupBy(o => o.City).Select(o => new { o.Key, Count = o.Count() }).ToList();
        //    foreach (var group in groups)
        //    {
        //        if (group.Key == null) continue;

        //        var result = ((MySqlNorthwind)db).GetCustomersByCity(group.Key);
        //        Assert.AreEqual(group.Count, result.Count());
        //    }
        //}

        //3.多个可能形状的单一结果集
        //[TestMethod]
        //public void Procedure_SingleRowset_MultiShape()
        //{
        //    //返回全部Customer结果集
        //    var result = ((MySqlNorthwind)db).SingleRowset_MultiShape(1);
        //    var shape1 = result.GetResult<Customer>();
        //    foreach (var compName in shape1)
        //    {
        //        Console.WriteLine(compName.CompanyName);
        //    }

        //    //返回部分Customer结果集
        //    result = ((MySqlNorthwind)db).SingleRowset_MultiShape(2);
        //    var shape2 = result.GetResult<PartialCustomersSetResult>();
        //    foreach (var con in shape2)
        //    {
        //        Console.WriteLine(con.ContactName);
        //    }
        //}

        //4.多个结果集
        //[TestMethod]
        //public void Procedure_GetCustomerAndOrders()
        //{
        //    var result = ((MySqlNorthwind)db).GetCustomerAndOrders("SEVES");
        //    //返回Customer结果集
        //    var customers = result.GetResult<Customer>();

        //    //返回Orders结果集
        //    var orders = result.GetResult<Order>();

        //    //在这里，我们读取CustomerResultSet中的数据
        //    foreach (var cust in customers)
        //    {
        //        Console.WriteLine(cust.CustomerID);
        //    }

        //    foreach (var order in orders)
        //    {
        //        Console.WriteLine(order.OrderID);
        //    }
        //}


        //[TestMethod]
        //public void Procedures_GetEmployee()
        //{
        //    db.Log = Console.Out;
        //    var result = ((MySqlNorthwind)db).GetEmployee(1).SingleOrDefault();
        //    Assert.IsNotNull(result);
        //    var employee = db.Employees.Where(o => o.EmployeeID == 1).SingleOrDefault();
        //    Assert.AreEqual(result.FirstName, employee.FirstName);
        //    Assert.AreEqual(result.LastName, employee.LastName);
        //    Assert.AreEqual(result.Title, employee.Title);
        //}

        [TestMethod]
        public void T()
        {
            db.Log = Console.Out;
            var item = db.GetTable<MyOrder>().Where(o => o.OrderID == 11080);
     
            Console.Out.Flush();
        }

        [Table(Name = "Orders")]
        public partial class MyOrder
        {
            public MyOrder()
            {

            }

            [Column(CanBeNull = false, IsPrimaryKey = true, UpdateCheck = UpdateCheck.Never)]
            public int OrderID
            {
                get;
                set;
            }

            //[Column(DbType = "VarChar(5)", CanBeNull = false, UpdateCheck = UpdateCheck.Never)]
            //public string CustomerID
            //{
            //    get;
            //    set;
            //}

            //[Column(CanBeNull = false, UpdateCheck = UpdateCheck.Never)]
            //public int EmployeeID
            //{
            //    get;
            //    set;
            //}

            [Column(UpdateCheck = UpdateCheck.Never)]
            public DateTime? OrderDate
            {
                get;
                set;
            }

            [Column(UpdateCheck = UpdateCheck.Never)]
            public DateTime? RequiredDate
            {
                get;
                set;
            }




        }
    }
}