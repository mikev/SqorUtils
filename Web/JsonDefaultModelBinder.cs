using System;
using System.IO;
using System.Web.Mvc;
using Sqor.Utils.Json;

namespace Sqor.Utils.Web
{
    public class JsonDefaultModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (IsJsonType(bindingContext.ModelType))
            {
                var request = controllerContext.RequestContext.HttpContext.Request;
                var inputStream = request.InputStream;
                inputStream.Position = 0;
                var content = new StreamReader(inputStream).ReadToEnd();
                var json = content.FromJson();

                if (bindingContext.ModelType.IsArray)
                {
                    var jsonArray = (JsonArray)json;
                    var elementType = bindingContext.ModelType.GetElementType();
                    var array = Array.CreateInstance(elementType, jsonArray.Count);
                    for (var i = 0; i < array.Length; i++)
                    {
                        var jsonObject = (JsonObject)jsonArray[i];
                        var element = jsonObject.To(elementType);
                        array.SetValue(element, i);
                    }
                    return array;
                }
                else
                {
                    var jsonObject = (JsonObject)json;
                    var result = jsonObject.To(bindingContext.ModelType);
                    return result;
                }
            }

            return base.BindModel(controllerContext, bindingContext);
        }

        public bool IsJsonType(Type type)
        {
            if (type.IsArray)
                return IsJsonType(type.GetElementType());
            else if (type.IsValueType)
                return false;
            else if (type == typeof(string))
                return false;
            else
                return true;
        }
    }
}