using System;

using RfidBus.Primitives;

namespace RfidBus.Service.Location
{
    internal sealed class TranspondersEventArgs : EventArgs
    {
        public TranspondersEventArgs(string readerId, Transponder[] transponders)
        {
            this.ReaderId = readerId;
            this.Transponders = transponders;
        }

        public string ReaderId { get; private set; }

        public Transponder[] Transponders { get; private set; }
    }
}