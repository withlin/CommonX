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

        //ObjectContext 在EF7废除
        //public static string GetTableName(this ObjectContext context, Type t)
        //{
        //    return GetTableName(context, t.Name);
        //}

        //ObjectContext 在EF7废除
        //public static string GetTableName(this ObjectContext context, string typeName)
        //{
        //    string result;

        //    if (!TableNames.TryGetValue(typeName, out result))
        //    {
        //        lock (TableNames)
        //        {
        //            if (!TableNames.TryGetValue(typeName, out result))
        //            {

        //                string entityName = typeName;

        //                ReadOnlyCollection<EntityContainerMapping> storageMetadata = context.MetadataWorkspace.GetItems<EntityContainerMapping>(DataSpace.CSSpace);

        //                foreach (EntityContainerMapping ecm in storageMetadata)
        //                {
        //                    EntitySet entitySet;
        //                    if (ecm.StoreEntityContainer.TryGetEntitySetByName(entityName, true, out entitySet))
        //                    {
        //                        result = entitySet.Table;
        //                        break;
        //                    }
        //                }

        //                TableNames.Add(typeName, result);
        //            }
        //        }
        //    }
        //    return result;
        //}

        public static IEnumerable<PropertyInfo> GetPrimaryKeyFieldsFor(this DbContext context, Type entityType)
        {
            return context.Model.FindEntityType(entityType).FindPrimaryKey().Properties.ToList() as IEnumerable<PropertyInfo>;
            //var metadata = context.ObjectContext.MetadataWorkspace
            //        .GetItems<EntityType>(DataSpace.OSpace)
            //        .SingleOrDefault(p => p.FullName == entityType.FullName);

            //if (metadata == null)
            //{
            //    throw new InvalidOperationException(String.Format("The type {0} is not known to the DbContext.", entityType.FullName));
            //}

            //return metadata.KeyMembers.Select(k => entityType.GetProperty(k.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)).ToList();
        }
        
    }
}
