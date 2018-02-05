using Apache.Ignite.Core.Cache.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Apache.Ignite.Linq;
using System.Diagnostics;

namespace CommonX.Cache.Ignite.Linq
{
    public class OnKey
    {
        public string L { get; set; }
        public string R { get; set; }
    }
    public enum SqlJoinType
    {
        Inner,
        Left
    }
    [Serializable]
    public class IgniteSqlQueryBuilder<Tkey, Tval>
    {
        private static Dictionary<string, Dictionary<string, MethodInfo>> EntityPropertyVisitors = new Dictionary<string, Dictionary<string, MethodInfo>>();
        private static Dictionary<string, ConstructorInfo> EntityCtorVisitors = new Dictionary<string, ConstructorInfo>();
        private string typeName;
        private string shortTypeName;
        private Type entityType;
        private List<string> joinClauses = new List<string>();
        private List<string> whereAndClauses = new List<string>();
        private List<string> whereOrClauses = new List<string>();
        private List<object> parameters = new List<object>();
        private Apache.Ignite.Core.Cache.ICache<Tkey, Tval> cache;
        public IgniteSqlQueryBuilder(Apache.Ignite.Core.Cache.ICache<Tkey, Tval> cache)
        {
            this.cache = cache;
            var types = cache.GetType().GetGenericArguments();
            this.typeName = types[1].FullName;
            this.shortTypeName = types[1].Name;
            this.entityType = types[1];
        }
        public IgniteSqlQueryBuilder<Tkey, Tval> Join(string joinEntityTypeName, KeyValuePair<string, string>[] onKeys, SqlJoinType joinType, params object[] parameters)
        {
            StringBuilder join = new StringBuilder();
            string joinContent = null;
            switch (joinType)
            {
                case SqlJoinType.Left:
                    joinContent = "left outer join";
                    break;
                case SqlJoinType.Inner:
                    joinContent = "inner join";
                    break;
                default:
                    joinContent = "inner join";
                    break;
            }
            join.Append(" " + joinContent + " " + joinEntityTypeName);
            join.Append(" on ");
            List<string> joins = new List<string>();
            foreach (var key in onKeys)
            {
                joins.Add(string.Format(" {0}.{1}={2}.{3} ", this.shortTypeName, key.Key, joinEntityTypeName, key.Value));
            }
            join.Append(string.Join(" and ", joins.ToArray()));
            joinClauses.Add(join.ToString());
            this.parameters.AddRange(parameters);
            return this;
        }
        List<string> containJoinClauses = new List<string>();
        List<object> containJoinParameters = new List<object>();
        public IgniteSqlQueryBuilder<Tkey, Tval> ForContain(string entityKey, string keyType, object[] list)
        {
            var joinEntityName = "fc" + (this.containJoinParameters.Count + 1).ToString();
            string joinClause = null;
            if (this.containJoinClauses.Count > 0)
                joinClause = string.Format("table(id {0}=?) {1} on {1}.id={2}.{3}", keyType, joinEntityName, this.shortTypeName, entityKey);
            else
                joinClause = string.Format("table(id {0}=?) {1} inner join {2} on {1}.id={2}.{3}", keyType, joinEntityName, this.shortTypeName, entityKey);
            this.containJoinParameters.Add(list);
            this.containJoinClauses.Add(joinClause);
            return this;
        }

        public IgniteSqlQueryBuilder<Tkey, Tval> ForContain2(string entityKey, string keyType, object[] list)
        {
            var al = new List<object>(list);
            if (keyType.ToLower() == "varchar")
            {
                WhereAnd(this.shortTypeName + "." + entityKey +
                    " in (" + string.Join(",", al.Select(x => "'" + x.ToString() + "'")) + ")");
            }
            else
            {
                WhereAnd(this.shortTypeName + "." + entityKey +
                    " in (" + string.Join(",", al.Select(x => x.ToString())) + ")");
            }
            return this;
        }
        public IgniteSqlQueryBuilder<Tkey, Tval> Join(Type joinEntityType, KeyValuePair<string, string>[] onKeys, SqlJoinType joinType, params object[] parameters)
        {
            return Join(joinEntityType.Name, onKeys, joinType, parameters);
        }

        public IgniteSqlQueryBuilder<Tkey, Tval> Join(Type joinEntityType, OnKey[] onKeys, SqlJoinType joinType, params object[] parameters)
        {
            return Join(joinEntityType.Name, onKeys.Select(x => new KeyValuePair<string, string>(x.L, x.R)).ToArray(), joinType, parameters);
        }

        /// <summary>
        /// 例如instr(#prodName, 'asd')>0
        /// </summary>
        /// <param name="where">其中表示字段的部分一定要加上#, 參考summary </param>
        /// <returns></returns>
        private IgniteSqlQueryBuilder<Tkey, Tval> Where(string where, List<string> whereClause, params object[] parameters)
        {
            whereClause.Add(System.Text.RegularExpressions.Regex.Replace(where, @"#(\S+)(?<=[\s,=]*)", this.shortTypeName + ".$1"));
            this.parameters.AddRange(parameters);
            return this;
        }

        /// <summary>
        /// 例如instr(#prodName, 'asd')>0
        /// </summary>
        /// <param name="where">其中表示字段的部分一定要加上#, 參考summary </param>
        /// <returns></returns>
        public IgniteSqlQueryBuilder<Tkey, Tval> WhereOr(string where, params object[] parameters)
        {
            return Where(where, whereOrClauses, parameters);
        }

        /// <summary>
        /// 例如instr(#prodName, 'asd')>0
        /// </summary>
        /// <param name="where">其中表示字段的部分一定要加上#, 參考summary </param>
        /// <returns></returns>
        public IgniteSqlQueryBuilder<Tkey, Tval> WhereAnd(string where, params object[] parameters)
        {
            return Where(where, whereAndClauses, parameters);
        }
        public SqlQuery Build()
        {
            StringBuilder where = new StringBuilder();
            List<object> pps = new List<object>();
            where.Append(this.whereAndClauses.Count > 0 ? " Where " + string.Join(" And ", this.whereAndClauses.ToArray()) : "");
            if (this.whereOrClauses.Count > 0 && where.Length > 0)
            {
                where.AppendFormat(" AND ({0})", string.Join(" OR ", this.whereOrClauses.ToArray()));
            }
            else if (this.whereOrClauses.Count > 0 && where.Length == 0)
            {
                where.AppendFormat(" Where {0}", string.Join(" OR ", this.whereOrClauses.ToArray()));
            }
            StringBuilder from = new StringBuilder();
            if (this.containJoinClauses.Count > 0)
            {
                pps.AddRange(this.containJoinParameters);
                pps.AddRange(this.parameters);
                from.Append(" from ");
                from.Append(string.Join(" inner join ", this.containJoinClauses));
                from.Append(string.Join(" ", this.joinClauses));
            }
            else
            {
                from.AppendFormat(" from {0} {1} ", this.shortTypeName, string.Join(" ", this.joinClauses));
                pps.AddRange(this.parameters);
            }
            var query =  new SqlQuery(this.shortTypeName,
                string.Format("{0} {1}",
                from.ToString(),
                where.ToString()
                ), pps.ToArray());
            Debug.WriteLine(query.Sql);
            return query;
        }

        private MethodInfo GetSetter(string fieldName)
        {
            Dictionary<string, MethodInfo> setters = null;
            if (!EntityPropertyVisitors.TryGetValue(typeName, out setters))
            {
                setters = new Dictionary<string, MethodInfo>();
                EntityPropertyVisitors.Add(typeName, setters);
            }
            MethodInfo setter = null;
            if (!setters.TryGetValue(fieldName, out setter))
            {
                var prop = entityType.GetProperty(fieldName);
                if (prop != null)
                {
                    setter = prop.GetSetMethod();
                    if (setter != null)
                    {
                        setters.Add(fieldName, setter);
                    }
                    else
                    {
                        throw new Exception(string.Format("Entity {0} doesn't have property setter for {1}", typeName, fieldName));
                    }
                }
                else
                {
                    throw new Exception(string.Format("Entity {0} doesn't have property for {1}", typeName, fieldName));
                }
            }
            return setter;
        }

        private ConstructorInfo GetConstructor()
        {
            ConstructorInfo ctor = null;
            if (!EntityCtorVisitors.TryGetValue(this.typeName, out ctor))
            {
                ctor = this.entityType.GetConstructor(new Type[] { });
                EntityCtorVisitors.Add(this.typeName, ctor);
            }
            return ctor;
        }
        /// <summary>
        /// 可以传入一个evaluator来收集结果，例如(list)=>resultCollector.Add(list[0])
        /// </summary>
        /// <param name="resultEvaluator"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public IEnumerable<Tval> FieldsQuery(Func<IList, Tval> resultEvaluator, params string[] fields)
        {
            SqlQuery sqlQuery = this.Build();
            if (fields.Length == 0)
            {
                return cache.Query(sqlQuery).Select(x => x.Value);
            }
            var regexp = new Regex(@"(?<=\S+\.)\S+", RegexOptions.Compiled);
            string[] formalizedFields = fields.Select(x => regexp.IsMatch(x) ? x : this.shortTypeName + "." + x).ToArray();
            StringBuilder selectClause = new StringBuilder();
            selectClause.Append("select " + string.Join(", ", formalizedFields));
            selectClause.Append(" " + sqlQuery.Sql);
            SqlFieldsQuery fieldQuery = new SqlFieldsQuery(selectClause.ToString(), sqlQuery.Arguments);
            return new FieldQuerableWrap<Tkey, Tval>(cache.QueryFields(fieldQuery), resultEvaluator);
        }


        public IEnumerable<Tval> FieldsQuery(params string[] fields)
        {
            Func<IList, Tval> resultEvaluator = (list) =>
            {
                var ctor = GetConstructor();
                Tval ins = (Tval)ctor.Invoke(new object[] { });
                for (int i = 0; i < fields.Length; i++)
                {
                    var val = list[i];
                    var setter = GetSetter(fields[i]);
                    setter.Invoke(ins, new object[] { val });
                }
                return ins;
            };
            return FieldsQuery(resultEvaluator, fields);
        }
    }

    public class FieldQuerableWrap<Tkey, Tval> : IEnumerable<Tval>
    {
        IQueryCursor<System.Collections.IList> cursor;
        Func<System.Collections.IList, Tval> evaluator;
        DefaultEnumerator innerEnu;
        public FieldQuerableWrap(IQueryCursor<System.Collections.IList> cursor, Func<System.Collections.IList, Tval> evaluator)
        {
            this.cursor = cursor;
            this.evaluator = evaluator;
            innerEnu = new DefaultEnumerator(cursor, evaluator);
        }

        public IEnumerator<Tval> GetEnumerator()
        {
            return innerEnu;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerEnu;
        }
        internal class DefaultEnumerator : IEnumerator<Tval>
        {
            internal IQueryCursor<IList> cursor;
            internal IEnumerator<IList> _innerEnu;
            internal Func<System.Collections.IList, Tval> evaluator;
            public DefaultEnumerator(IQueryCursor<IList> cursor, Func<System.Collections.IList, Tval> evaluator)
            {
                this.cursor = cursor;
                this.evaluator = evaluator;
            }
            public Tval Current
            {
                get
                {
                    return (Tval)(this as IEnumerator).Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    var list = innerEnu.Current;
                    if (list != null)
                    {
                        return evaluator(list);
                    }
                    else
                    {
                        return default(Tval);
                    }
                }
            }

            public void Dispose()
            {
                this.cursor.Dispose();
            }

            public bool MoveNext()
            {
                return this.innerEnu.MoveNext();
            }
            private IEnumerator<IList> innerEnu
            {
                get
                {
                    if (_innerEnu == null) _innerEnu = this.cursor.GetEnumerator();
                    return _innerEnu;
                }
            }
            public void Reset()
            {
                this.innerEnu.Reset();
            }
        }
    }
}
