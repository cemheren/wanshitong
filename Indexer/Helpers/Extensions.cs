using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Extensions
{
    public static IEnumerable<T> Coalesce<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
        {
            return Enumerable.Empty<T>();
        }

        return enumerable;
    }
}