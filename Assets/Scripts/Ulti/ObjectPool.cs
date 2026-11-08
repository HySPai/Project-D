using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reuses inactive instances per prefab. An instance goes back to the pool simply by deactivating itself, so pooled
/// objects need no reference to the pool. Typed lookups return the component directly, with no GetComponent call.
/// </summary>
public static class ObjectPool
{
    private interface IPool
    {
        void DeactivateAll();
        void Clear();
    }

    private static readonly List<IPool> pools = new List<IPool>();

    /// <summary>An active instance of prefab, reused when an inactive one exists.</summary>
    public static GameObject Get(GameObject prefab) => Pool<GameObject>.Get(prefab);

    /// <summary>An active instance of the prefab that owns component, returned as that component type.</summary>
    public static T Get<T>(T prefab) where T : Component => Pool<T>.Get(prefab);

    /// <summary>Detaches and hides every pooled instance, e.g. on a world reset.</summary>
    public static void DeactivateAll()
    {
        for (int i = 0; i < pools.Count; i++) pools[i].DeactivateAll();
    }

    /// <summary>Forgets every pooled instance, e.g. when the scene that owns them unloads.</summary>
    public static void Clear()
    {
        for (int i = 0; i < pools.Count; i++) pools[i].Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Clear();

    private static GameObject ToGameObject(Object item) => item is GameObject go ? go : ((Component)item).gameObject;

    private sealed class Pool<T> : IPool where T : Object
    {
        private sealed class Entry
        {
            public readonly List<T> items = new List<T>();
            public int cursor;
        }

        private static Pool<T> instance;
        private readonly Dictionary<T, Entry> entries = new Dictionary<T, Entry>();

        public static T Get(T prefab)
        {
            if (prefab == null) return null;

            if (instance == null)
            {
                instance = new Pool<T>();
                pools.Add(instance);
            }
            return instance.GetFree(prefab);
        }

        private T GetFree(T prefab)
        {
            if (!entries.TryGetValue(prefab, out Entry entry))
            {
                entry = new Entry();
                entries.Add(prefab, entry);
            }

            List<T> items = entry.items;
            int count = items.Count;
            for (int i = 0; i < count; i++)
            {
                int index = (entry.cursor + i) % count;
                T item = items[index];

                if (item == null)
                {
                    item = Object.Instantiate(prefab);
                    items[index] = item;
                }
                else if (ToGameObject(item).activeSelf)
                {
                    continue;
                }

                entry.cursor = index + 1;
                ToGameObject(item).SetActive(true);
                return item;
            }

            T created = Object.Instantiate(prefab);
            items.Add(created);
            entry.cursor = 0;
            ToGameObject(created).SetActive(true);
            return created;
        }

        public void DeactivateAll()
        {
            foreach (Entry entry in entries.Values)
            {
                foreach (T item in entry.items)
                {
                    if (item == null) continue;

                    GameObject go = ToGameObject(item);
                    go.transform.SetParent(null);
                    go.SetActive(false);
                }
            }
        }

        public void Clear() => entries.Clear();
    }
}
