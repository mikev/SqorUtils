using System;
using System.Web.Mvc;

namespace Sqor.Utils.Web
{
    public static class ModelExtensions
    {
        public static HtmlHelper<TSubModel> SubModel<TModel, TSubModel>(this HtmlHelper<TModel> html, Func<TModel, TSubModel> subModel)
        {
            return new HtmlHelper<TSubModel>(html.ViewContext, new SimpleViewDataContainer(subModel(html.ViewData.Model)));
        }

        public static HtmlHelper<TSubModel> SubModel<TModel, TSubModel>(this HtmlHelper<TModel> html, TSubModel subModel)
        {
            return new HtmlHelper<TSubModel>(html.ViewContext, new SimpleViewDataContainer(subModel));
        }
    }

    public class SimpleViewDataContainer : IViewDataContainer
    {
        public ViewDataDictionary ViewData { get; set; }

        public SimpleViewDataContainer(object model)
        {
            ViewData = new ViewDataDictionary(model);
        }
    }
}