using System;
using System.IO;
using System.Web.Mvc;
using Sqor.Utils.Json;

namespace Sqor.Utils.Web
{
    public class JsonModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.RequestContext.HttpContext.Request;
            var inputStream = request.InputStream;
            inputStream.Position = 0;
            var content = new StreamReader(inputStream).ReadToEnd();
            var json = content.FromJson();

            if (bindingContext.ModelType == typeof(JsonArray) && !(json is JsonArray))
                throw new InvalidOperationException("Trying to bind a JsonArray to JSON content that does not connotate an array");
            if (bindingContext.ModelType == typeof(JsonObject) && !(json is JsonObject))
                throw new InvalidOperationException("Trying to bind a JsonObject to JSON content that does not connotate an object");

            return json;

/*
            var collection = bindingContext.ValueProvider as ValueProviderCollection;
            if (collection == null)
                throw new InvalidOperationException("Cannot use JSON model binding when the value provider is not a ValueProviderCollection");

            var dictionaryProvider = collection.SingleOrDefault(x => x is DictionaryValueProvider<object>);
            if (dictionaryProvider == null)
                throw new InvalidOperationException("Cannot use JSON model binding when the value provider does not contain a dictionary value provider.");

            // If JSON array
            if (dictionaryProvider.ContainsPrefix("[0]"))
            {
                
            }
            // Else JSON object
            else
            {
                
            }
//            bindingContext.ValueProvider.GetValue(0)
            throw new System.NotImplementedException();
*/
        }
    }
}