namespace Sqor.Utils.Json
{
    public interface IJsonConverter
    {
        string TypeDescription { get; }
        JsonValue ToJson(object o);
        object FromJson(JsonValue json);
    }
}