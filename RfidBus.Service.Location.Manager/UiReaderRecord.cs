using System;

using RfidBus.Primitives.Messages.Readers;

namespace RfidBus.Service.Location.Manager
{
    public sealed class UiReaderRecord
    {
        private readonly ReaderRecord _record;

        public UiReaderRecord(ReaderRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("record");

            this._record = record;
        }

        public string Name
        {
            get { return this._record.Name; }
        }

        public bool IsOpen
        {
            get { return this._record.IsOpen; }
        }

        public bool IsActive
        {
            get { return this._record.IsActive; }
        }

        public string IdAsString
        {
            get { return this._record.Id.ToString(); }
        }
    }
}