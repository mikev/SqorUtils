namespace Sqor.Utils.Objects
{
    public static class ObjectExtensions
    {
        public static bool SafeEquals<T>(this T o1, T o2)
        {
            bool o1Null = o1 == null;//EqualityComparer<T>.Default.Equals(o1, default(T));
            bool o2Null = o2 == null;//EqualityComparer<T>.Default.Equals(o2, default(T));
            if (o1Null && o2Null)
                return true;
            if (o1Null)
                return false;

            return o1.Equals(o2);
        }         
    }
}