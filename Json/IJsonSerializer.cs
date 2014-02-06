using System.Collections.Generic;
using Sqor.Utils.Json;

namespace Sqor.Utils.Json
{
    public interface IJsonSerializer
    {
        JsonValue Parse(string input);
        T Parse<T>(string input);
        string Serialize(object o);
    }
}
