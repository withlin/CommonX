using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace CommonX.DataAccess.EF
{
    public static partial class DbContextExtentions
    {
        public static string GetTableName<T>(this DbContext context) where T : class
        {
            return context.Model.FindEntityType(typeof(T)).Relational().TableName;
        }

        public static string GetTableName(this DbContext context, Type t)
        {
            return context.Model.FindEntityType(t).Relational().TableName;
        }

        public static string GetTableName(this DbContext context, string t)
        {
            return context.Model.FindEntityType(t).Relational().TableName;
        }

        private static readonly Dictionary<string, string> TableNames = new Dictionary<string, string>();


        public static IEnumerable<PropertyInfo> GetPrimaryKeyFieldsFor(this DbContext context, Type entityType)
        {
            return context.Model.FindEntityType(entityType).FindPrimaryKey().Properties.ToList() as IEnumerable<PropertyInfo>;
        }
        
    }
}
