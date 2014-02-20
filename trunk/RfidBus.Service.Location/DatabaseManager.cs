using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using DevExpress.Xpo;
using DevExpress.Xpo.DB;

using RfidBus.Primitives;
using RfidBus.Service.Location.database;

namespace RfidBus.Service.Location
{
    internal sealed class DatabaseManager
    {
        #region single instance
        private static readonly DatabaseManager _instance;

        static DatabaseManager()
        {
            _instance = new DatabaseManager();
        }

        private DatabaseManager()
        {
        }

        public static DatabaseManager Instance
        {
            get { return _instance; }
        }
        #endregion

        private readonly ConcurrentDictionary<string, TagsHistory> _tagsCache = new ConcurrentDictionary<string, TagsHistory>();

        public void Initialize()
        {
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(Properties.Settings.Default.ConnectionString, AutoCreateOption.DatabaseAndSchema);
            XpoDefault.Session = new Session();
        }

        private void FinishHistoryRecord(TagsHistory record)
        {
            record.LeaveTime = DateTime.UtcNow;
            record.Save();
        }

        public void FoundTransponders(string reader, IEnumerable<Transponder> transponders)
        {
            var addNew = new Action<Transponder>(delegate(Transponder transponder)
                                                 {
                                                     var historyRecord = new TagsHistory(XpoDefault.Session)
                                                                         {
                                                                             Tid = transponder.IdAsString,
                                                                             Reader = reader,
                                                                             Antenna = transponder.Antenna,
                                                                             EntryTime = DateTime.UtcNow
                                                                         };
                                                     historyRecord.Save();

                                                     this._tagsCache[transponder.IdAsString] = historyRecord;
                                                 });

            foreach (var transponder in transponders)
            {
                TagsHistory historyRecord;
                if (this._tagsCache.TryGetValue(transponder.IdAsString, out historyRecord))
                {
                    if (!string.Equals(historyRecord.Reader, reader, StringComparison.InvariantCultureIgnoreCase)
                        || (historyRecord.Antenna != transponder.Antenna))
                    {
                        this.FinishHistoryRecord(historyRecord);

                        addNew(transponder);
                    }
                }
                else
                    addNew(transponder);
            }
        }

        public void LostTransponders(string reader, IEnumerable<Transponder> transponders)
        {
            foreach (var transponder in transponders)
            {
                TagsHistory historyRecord;
                if (this._tagsCache.TryGetValue(transponder.IdAsString, out historyRecord))
                {
                    if (string.Equals(historyRecord.Reader, reader, StringComparison.InvariantCultureIgnoreCase)
                        && (historyRecord.Antenna == transponder.Antenna))
                        this.FinishHistoryRecord(historyRecord);
                }
            }
        }
    }
}