using System.Web.Mvc;

namespace Sqor.Utils.Web
{
    /// <summary>
    /// T denotes the expected return type (assuming there is no error, etc.)  Standard result types
    /// are mirrored to use this base interface.  This facilitates auto documentation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ActionResult<T> : ActionResult {
    }
}
