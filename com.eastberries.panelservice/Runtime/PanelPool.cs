using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PanelService
{
    public interface IPanelPool
    {
        IPanel Get(GameObject prefab, Transform parent);
        void Return(IPanel panel);
        void PrewarmPool(GameObject prefab, int count);
    }

    public class PanelPool : IPanelPool
    {
        private readonly IObjectResolver _container;
        private readonly Dictionary<string, Queue<IPanel>> _pools = new();
        private readonly Dictionary<string, GameObject> _prefabs = new();

        public PanelPool(IObjectResolver container)
        {
            _container = container;
        }

        public IPanel Get(GameObject prefab, Transform parent)
        {
            var prefabId = prefab.name;

            if (!_pools.TryGetValue(prefabId, out var pool))
            {
                pool = new Queue<IPanel>();
                _pools[prefabId] = pool;
                _prefabs[prefabId] = prefab;
            }

            IPanel panel;
            if (pool.Count > 0)
            {
                panel = pool.Dequeue();
                panel.transform.gameObject.SetActive(true);
            }
            else
            {
                panel = _container.Instantiate(prefab).GetComponent<IPanel>();
            }

            panel.SetParent(parent);
            return panel;
        }

        public void Return(IPanel panel)
        {
            if (panel == null) return;

            var prefabId = panel.transform.gameObject.name.Replace("(Clone)", "").Trim();

            if (!_pools.TryGetValue(prefabId, out var pool))
            {
                pool = new Queue<IPanel>();
                _pools[prefabId] = pool;
            }

            panel.transform.gameObject.SetActive(false);
            pool.Enqueue(panel);
        }

        public void PrewarmPool(GameObject prefab, int count)
        {
            var prefabId = prefab.name;
            if (!_pools.TryGetValue(prefabId, out var pool))
            {
                pool = new Queue<IPanel>();
                _pools[prefabId] = pool;
                _prefabs[prefabId] = prefab;
            }

            for (int i = 0; i < count; i++)
            {
                var panel = _container.Instantiate(prefab).GetComponent<IPanel>();
                panel.transform.gameObject.SetActive(false);
                pool.Enqueue(panel);
            }
        }
    }
}