using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PanelService
{
    public interface IPanelFactory
    {
        IPanel CreatePanel(GameObject prefab);
    }

    public class PanelFactory : IPanelFactory
    {
        private readonly IObjectResolver _container;

        public PanelFactory(IObjectResolver container)
        {
            _container = container;
        }

        public IPanel CreatePanel(GameObject prefab)
        {
            return _container.Instantiate(prefab).GetComponent<IPanel>();
        }
    }
}