using System;

namespace Sqor.Utils.Json
{
    public class JsonGroupAttribute : Attribute
    {
        public string GroupName { get; private set; }

        public JsonGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}
