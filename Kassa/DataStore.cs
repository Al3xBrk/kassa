using System.Collections.Generic;

namespace Kassa
{
    public static class DataStore
    {
        public static Dictionary<string, List<(string Name, decimal Price)>> GroupsData { get; } = new();
    }
}
