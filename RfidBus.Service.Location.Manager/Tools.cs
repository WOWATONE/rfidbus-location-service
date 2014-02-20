using System.IO;
using System.Linq;
using System.Reflection;

using DevExpress.Data.Filtering;
using DevExpress.Xpo;

namespace RfidBus.Service.Location.Manager
{
    public static class Tools
    {
        static Tools()
        {
            CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public static string CurrentDirectory { get; private set; }

        public static TResult GetUncachedXPObject<TResult>(Session session, TResult xpObject) where TResult : XPObject
        {
            var collection = new XPCollection<TResult>(session, new BinaryOperator("Oid", xpObject.Oid));
            return collection.FirstOrDefault();
        }
    }
}