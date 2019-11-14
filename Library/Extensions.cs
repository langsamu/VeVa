namespace langsamu.VeVa
{
    using System;
    using System.Collections.Generic;

    public static class Extensions
    {
        public static Func<T, TResult> Memoize<T, TResult>(this Func<T, TResult> operation)
        {
            var dictionary = new Dictionary<T, TResult>();

            return item =>
            {
                TResult result;
                var exists = dictionary.TryGetValue(item, out result);

                if (!exists)
                {
                    result = operation(item);

                    dictionary.Add(item, result);
                }

                return result;
            };
        }
    }
}
