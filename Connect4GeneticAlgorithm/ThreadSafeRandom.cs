#nullable enable
using System;
using System.Threading.Tasks;

internal static class ThreadSafeRandom
{
    [ThreadStatic]
    private static Random? _local;
    private static readonly Random Global = new((int)DateTime.Now.Ticks); // 👈 Global instance used to generate seeds

    private static Random Instance
    {
        get
        {
            if (_local is null)
            {
                int seed;
                lock (Global) // 👈 Ensure no concurrent access to Global
                {
                    seed = Global.Next();
                }

                _local = new Random(seed); // 👈 Create [ThreadStatic] instance with specific seed
            }

            return _local;
        }
    }

    public static int Next() => Instance.Next();
    public static int Next(int maxValue) => Instance.Next(maxValue);

    public static double NextDouble() => Instance.NextDouble();
}