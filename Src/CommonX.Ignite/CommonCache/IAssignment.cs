
using CommonX.Components;
using FastMember;
using System;
using System.Collections;
using System.Linq;

namespace CommonX.Cache.Ignite.CommonCache
{
    public interface IAssignment
    {
        TEntity Assign<TEntity>(TEntity entity, string[] parameterArray, IList value)
            where TEntity : class;
    }

    [Component(LifeStyle.Transient)]
    public class Assignment : IAssignment
    {
        public TEntity Assign<TEntity>(TEntity entity, string[] parameterArray, IList value) where TEntity : class
        {
            if (parameterArray?.Count() == 0 || value?.Count == 0)
                return entity;

            var access = TypeAccessor.Create(typeof(TEntity));
            MemberSet members = access.GetMembers();
            var newEntity = entity ?? access.CreateNew();

            for (int i = 0; i < parameterArray.Count(); i++)
            {
                if (value[i] != null && !string.IsNullOrEmpty(parameterArray[i]))
                {
                    try
                    {
                        access[newEntity, parameterArray[i]] = value[i];
                    }
                    catch(InvalidCastException ex)
                    {
                        Member member = members.FirstOrDefault(m => m.Name.ToUpper() == parameterArray[i].ToUpper());
                        Type propertyType = member.Type;

                        if (propertyType == typeof(long?) || propertyType == typeof(long))
                            access[newEntity, parameterArray[i]] = Int64.Parse(value[i].ToString());
                        if (propertyType == typeof(Decimal?) || propertyType == typeof(Decimal))
                            access[newEntity, parameterArray[i]] = Decimal.Parse(value[i].ToString());
                        if (propertyType == typeof(Double?) || propertyType == typeof(Double))
                            access[newEntity, parameterArray[i]] = Double.Parse(value[i].ToString());
                        if (propertyType == typeof(Int32?) || propertyType == typeof(Int32))
                            access[newEntity, parameterArray[i]] = Int32.Parse(value[i].ToString());
                        if (propertyType == typeof(Boolean?) || propertyType == typeof(Boolean))
                            access[newEntity, parameterArray[i]] = Boolean.Parse(value[i].ToString());
                        if (propertyType == typeof(DateTime?) || propertyType == typeof(DateTime))
                        {
                            int separate_index = value[i].ToString().IndexOf(':'), length = value[i].ToString().Length;
                            string date_part = value[i].ToString().Substring(0, separate_index);
                            string time_part = value[i].ToString().Substring(separate_index + 1, length - separate_index - 1);

                            access[newEntity, parameterArray[i]] = Convert.ToDateTime(string.Format("{0} {1}", date_part, time_part));
                        }
                    }
                }
                    
            }

            return (TEntity)newEntity;
        }
    }
}
