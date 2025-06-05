using System.Collections.Generic;
using UnityEngine;

namespace PanelService
{
    [CreateAssetMenu(fileName = "PanelSettings", menuName = "Panel System/Panel Settings")]
    public class PanelSettings : ScriptableObject
    {
        public List<PanelConfig> panelConfigs = new();
    }
    
    
}