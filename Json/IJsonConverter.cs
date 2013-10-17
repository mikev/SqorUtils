namespace Sqor.Utils.Json
{
    public interface IJsonConverter
    {
        JsonValue ToJson(object o);
        object FromJson(JsonValue json);
    }
}