using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sqor.Utils.Json
{
    public class JsonArray : JsonValue
    {
        private List<JsonValue> array;

        public JsonArray() : base(JsonType.Array)
        {
            array = new List<JsonValue>();
        }
        
        public JsonArray(IEnumerable<JsonValue> array) : base(JsonType.Array)
        {
            this.array = array.ToList();
        }

        public int Count
        {
            get { return array.Count; }
        }

        public override JsonValue this[int index]
        {
            get { return array[index]; }    
        }
        
        public override IEnumerator<JsonValue> GetEnumerator()
        {
            return ((IEnumerable<JsonValue>)array).GetEnumerator();
        }
        
        public void Add(JsonValue value)
        {
            array.Add(value);
        }
    }
}

