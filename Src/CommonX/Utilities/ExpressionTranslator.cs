using CommonX.Components;
using CommonX.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CommonX.Utilities
{
    /// <summary>
    /// translate expression to string 
    /// </summary>
    public class ExpressionTranslator : ExpressionVisitor
    {
        private StringBuilder sb;
        private string _orderBy = string.Empty;
        private int? _skip = null;
        private int? _take = null;
        private string _whereClause = string.Empty;
        private bool isLike = false;

        private ILogger _logger;
        public ExpressionTranslator()
        {
            using (var scope = ObjectContainer.BeginLifetimeScope())
            {
                _logger = scope.Resolve<ILoggerFactory>().Create(nameof(ExpressionTranslator));
            }
        }
        public int? Skip
        {
            get
            {
                return _skip;
            }
        }

        public int? Take
        {
            get
            {
                return _take;
            }
        }

        public string OrderBy
        {
            get
            {
                return _orderBy;
            }
        }

        public string WhereClause
        {
            get
            {
                return _whereClause;
            }
        }

        public string Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            _whereClause = this.sb.ToString();
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(_whereClause);
            }
            return _whereClause;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                this.Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }
            else if (m.Method.Name == "Take")
            {
                if (this.ParseTakeExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Skip")
            {
                if (this.ParseSkipExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderBy")
            {
                if (this.ParseOrderByExpression(m, "ASC"))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderByDescending")
            {
                if (this.ParseOrderByExpression(m, "DESC"))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Equals")
            {
                this.Visit(m.Object);
                sb.Append(" = ");
                return this.Visit(m.Arguments[0]);
            }
            else if (m.Method.Name == "Contains")
            {
                if (m.Arguments.Count == 1)
                {
                    sb.Append((m.Object as MemberExpression)?.Member.Name);
                    sb.Append(" like '%");
                    isLike = true;
                    //this.Visit(m.Arguments[0]);
                    this.Visit(m.Object);
                    isLike = false;
                    sb.Append("%'");
                    return m;
                }
                else
                {

                    this.Visit(m.Arguments[1]);
                    sb.Append(" in ");
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OwnedCollection")
            {
                Expression nextExpression = m.Arguments[0];
                return this.Visit(nextExpression);
            }
            //不支持 不缓存
            else if (m.Method.Name == "Any")
            {
                sb.Append($" {DateTime.Now.Ticks} = {DateTime.Now.Ticks} ");
                return m;
            }
            else if (m.Method.Name == "IsNullOrEmpty")
            {
                sb.Append($"IsNullOrEmpty({m.Arguments[0]})");
                return m;
            }
            //else
            //{

            //    return this.Visit(m);
            //}
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("(");
            this.Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(b.Right))
                    {
                        sb.Append(" IS ");
                    }
                    else
                    {
                        sb.Append(" = ");
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right))
                    {
                        sb.Append(" IS NOT ");
                    }
                    else
                    {
                        sb.Append(" <> ");
                    }
                    break;

                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));

            }

            this.Visit(b.Right);
            sb.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q == null && c.Value == null)
            {
                sb.Append("NULL");
            }
            else if (q == null)
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;

                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;

                    case TypeCode.DateTime:
                        sb.Append("to_date('");
                        var date = ((DateTime)c.Value).ToString("yyyy/MM/dd HH:mm:ss");
                        sb.Append(date);
                        sb.Append("', 'yyyy/MM/dd hh24:mi:ss')");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));

                    default:
                        sb.Append(c.Value);
                        break;
                }
            }

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                sb.Append(m.Member.Name);
                return m;
            }
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Constant)
            {
                var c = m.Expression as ConstantExpression;
                if (c != null)
                {
                    Type t = c.Value.GetType();
                    var x = t.InvokeMember(m.Member.Name, BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                        null, c.Value, null);
                    if (m.Type == typeof(DateTime))
                    {
                        sb.Append("to_date('");
                        var date = ((DateTime)x).ToString("yyyy/MM/dd HH:mm:ss");
                        sb.Append(date);
                        sb.Append("', 'yyyy/MM/dd hh24:mi:ss')");
                    }
                    else if (m.Type == typeof(IEnumerable<string>) || m.Type == typeof(IList<string>) || m.Type == typeof(List<string>))
                    {
                        sb.Append("(");
                        List<string> strs = (from object v in (IEnumerable)x select "'" + v + "'").ToList();
                        if (strs.Any())
                        {
                            sb.Append(string.Join(",", strs));
                        }
                        else
                        {
                            sb.Append("''");
                        }
                        sb.Append(")");
                    }
                    else if (isLike)
                    {
                        sb.Append(x);
                    }
                    else
                    {
                        sb.Append("'");
                        sb.Append(x);
                        sb.Append("'");
                    }
                }
                return m;
            }
            if (m.NodeType == ExpressionType.MemberAccess)
            {
                object x = Expression.Lambda<Func<object>>(Expression.Convert(m, typeof(object))).Compile().Invoke();
                if (m.Type == typeof(DateTime))
                {
                    sb.Append("to_date('");
                    var date = ((DateTime)x).ToString("yyyy/MM/dd HH:mm:ss");
                    sb.Append(date);
                    sb.Append("', 'yyyy/MM/dd hh24:mi:ss')");
                }
                else
                {
                    sb.Append("'");
                    sb.Append(x);
                    sb.Append("'");
                }
                return m;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        protected bool IsNullConstant(Expression exp)
        {
            return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        }

        private bool ParseOrderByExpression(MethodCallExpression expression, string order)
        {
            UnaryExpression unary = (UnaryExpression)expression.Arguments[1];
            LambdaExpression lambdaExpression = (LambdaExpression)unary.Operand;

            lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

            MemberExpression body = lambdaExpression.Body as MemberExpression;
            if (body != null)
            {
                if (string.IsNullOrEmpty(_orderBy))
                {
                    _orderBy = string.Format("{0} {1}", body.Member.Name, order);
                }
                else
                {
                    _orderBy = string.Format("{0}, {1} {2}", _orderBy, body.Member.Name, order);
                }

                return true;
            }

            return false;
        }

        private bool ParseTakeExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                _take = size;
                return true;
            }

            return false;
        }

        private bool ParseSkipExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                _skip = size;
                return true;
            }

            return false;
        }
    }
}
