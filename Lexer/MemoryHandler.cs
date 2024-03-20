namespace Lexer
{
    public static unsafe class MemoryHandler
    {

        public static Dictionary<string, dynamic> Memorys = [];
        private static readonly object obj = new();
        public static bool StopWatcher;
        private static List<string>? keys;
        public static void WatchForChanges()
        {
            StopWatcher = false;
            keys = [];
            int lastCount = Memorys.Count;
            new Thread(() =>
            {
                while (!StopWatcher)
                {
                    if (Memorys.Count > lastCount)
                    {
                        keys.Add(Memorys.ToList().Last().Key);
                        lastCount = Memorys.Count;
                    }
                }
            }).Start();
        }
        public static void StopWatching()
        {
            lock (obj)
            {
                StopWatcher = true;
            }
        }
        public static void RemoveLastChangesFromMemory()
        {
            if (!StopWatcher)
            {
                throw new Exception("Watcher must be stopped first");
            }
            else
            {

                foreach (string key in keys)
                {
                    if (Memorys.ContainsKey(key))
                    {
                        _ = Memorys.Remove(key);
                    }
                }


            }
        }

    }
}
