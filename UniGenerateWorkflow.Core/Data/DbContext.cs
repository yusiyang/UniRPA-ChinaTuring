using SqlSugar;
using System;
using System.Linq;
using System.Reflection;
using Uni.Entity;

namespace Uni.Core
{
    /// <summary>
    /// 数据上下文
    /// </summary>
    public class DbContext : IDisposable
    {
        #region Fields

        /// <summary>
        /// 所有/复杂查询
        /// </summary>
        public SqlSugarClient Client;

        /// <summary>
        /// 活动表
        /// </summary>
        public SimpleClient<Activity> Activity => new SimpleClient<Activity>(Client);

        /// <summary>
        /// 活动属性表
        /// </summary>
        public SimpleClient<ActivityProperty> ActivityProperty => new SimpleClient<ActivityProperty>(Client);

        /// <summary>
        /// 行为映射表
        /// </summary>
        public SimpleClient<Behavior> Behavior => new SimpleClient<Behavior>(Client);

        /// <summary>
        /// 操作关键字表
        /// </summary>
        public SimpleClient<ActionKeyword> ActionKeyword => new SimpleClient<ActionKeyword>(Client);

        /// <summary>
        /// 对象关键字表
        /// </summary>
        public SimpleClient<ObjectKeyword> ObjectKeyword => new SimpleClient<ObjectKeyword>(Client);

        /// <summary>
        /// 属性关键字表
        /// </summary>
        public SimpleClient<ParameterKeyword> ParameterKeyword => new SimpleClient<ParameterKeyword>(Client);
          
        /// <summary>
        /// 属性关键字活动属性映射表
        /// </summary>
        public SimpleClient<ParameterKeywordActivityPropertyMapping> ParameterKeywordActivityPropertyMapping => new SimpleClient<ParameterKeywordActivityPropertyMapping>(Client);

        #endregion

        #region Ctor

        /// <summary>
        /// 构造函数
        /// </summary>
        public DbContext()
        {
            Client = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = @"Data Source = " + AppDomain.CurrentDomain.BaseDirectory + "\\wf.sqlite;Pooling=true;FailIfMissing=false;Version=3;",
                DbType = DbType.Sqlite,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true
            });
        }

        #endregion

        #region Initialize

        /// <summary>
        /// 数据库初始化
        /// </summary>
        public static void Initialize(bool isInitData = false)
        {
            using (DbContext dbContext = new DbContext())
            {
                //生成数据库表
                var types = Assembly.GetAssembly(typeof(EntityBase)).GetTypes().Where(t => !t.IsAbstract && t.IsClass).ToArray();
                dbContext.Client.CodeFirst.InitTables(types);
                if (isInitData == false)
                {
                    return;
                }
                new DbDataInitializer(dbContext.Client).Initialize();
            }
        }

        /// <summary>
        /// 释放托管资源
        /// </summary>
        public void Dispose()
        {
            if (Client != null)
            {
                Client.Dispose();
            }
        }
        #endregion

    }
}
