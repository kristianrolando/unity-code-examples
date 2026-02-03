using System;
using System.Collections.Generic;

namespace Game.Systems.SaveSystem
{
    /// <summary>
    /// Root data that is actually written to disk.
    /// Contains version and a dictionary of state blobs (each is a JSON string).
    /// </summary>
    [Serializable]
    public class SaveContainer
    {
        public int version = 1;
        public DateTime createdUtc;
        public DateTime lastModifiedUtc;

        public Dictionary<string, string> states = new Dictionary<string, string>();
    }
}
