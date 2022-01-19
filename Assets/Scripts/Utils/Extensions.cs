public static class Extensions
{
    public static int Hash(this string some)
    {
        return some.GetHashCode();
    }
}
