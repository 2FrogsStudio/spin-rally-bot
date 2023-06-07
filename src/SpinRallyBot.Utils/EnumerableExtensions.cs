namespace SpinRallyBot;

public static class EnumerableExtensions {
    public static IEnumerable<T[]> Split<T>(this IEnumerable<T> arr, int size) {
        var enumerable = arr as T[] ?? arr.ToArray();
        for (var i = 0; i < enumerable.Length / size + 1; i++)
            yield return enumerable.Skip(i * size).Take(size).ToArray();
    }
}