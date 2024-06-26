public static class ArrayUtils
{
    public static bool Contains<T>(this T[] array, T itemToCheck)
    {
        foreach(var item in array)
        {
            if (item.Equals(itemToCheck))
            {
                return true;
            }
        }
        return false;
    }

}

