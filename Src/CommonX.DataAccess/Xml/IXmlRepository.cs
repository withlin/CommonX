#region



#endregion

namespace CommonX.DataAccess.Xml
{
    public partial interface IXmlRepository<TEntity,TKey> : IRepository<TEntity,long> where TEntity : class, new()
    {
        string Path { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string FileName { get; set; }
    }

    
}