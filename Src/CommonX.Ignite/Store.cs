//using System;
//using System.Collections;
//using System.Linq;
//using ECommon.Cache.Ignite.Interface;
//using ECommon.Components;
//using JZTERP.Common.Shared.Entities.Common;
//using JZTERP.Common.Shared.Entities.Common.Common;
//using JZTERP.Frameworks.Common.Cache.Ignite.Interface;
//using JZTERP.Frameworks.Common.Components;
//using JZTERP.Frameworks.DataAccess;

//namespace ECommon.Cache.Ignite
//{
//    [Serializable]
//    public class Store<TEntity> : IStore<TEntity> where TEntity : BaseObject, new()
//    {
//        private readonly string carrierCode = "CachingInterceptor";

//        public void Delete(object key)
//        {
//            throw new NotImplementedException();
//        }

//        public void DeleteAll(ICollection keys)
//        {
//            throw new NotImplementedException();
//        }

//        public virtual object Load(object key)
//        {
//            //using (var scope = ObjectContainer.Current.BeginLifetimeScope())
//            //{
//            //    var service = scope.Resolve<IRepository<TEntity, long>>();
//            //    var longKey = long.Parse(key.ToString());
//            //    return service.Get(key);
//            //}
//        }

//        public virtual IDictionary LoadAll(ICollection keys)
//        {
//            //using (var scope = ObjectContainer.BeginLifetimeScope())
//            //{
//            //    var service = scope.Resolve<IRepository<TEntity, long>>();
//            //    var lstKey = keys.Cast<long>().ToList();
//            //    return service.Find(t => lstKey.Contains(t.PK)).ToDictionary(r => r.PK);
//            //}
//        }

//        public virtual void LoadCache(Action<object, object> act, params object[] args)
//        {

//            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
//            {

//                //var service = scope.Resolve<IRepository<TEntity, long>>();
//                //var itemArray = service.GetAll(); //.ToList();

//                //foreach (var ent in itemArray)
//                //{
//                //    act(ent.PK, ent);
//                //}
//            }
//        }

//        public void SessionEnd(bool commit)
//        {
//        }

//        public void Write(object key, object val)
//        {
//            throw new NotImplementedException();
//        }

//        public void WriteAll(IDictionary entries)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}