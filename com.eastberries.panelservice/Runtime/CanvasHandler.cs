using System.Collections.Generic;
using UnityEngine;

namespace PanelService
{
    public enum CanvasId
    {
        MainCanvas = 0,
        SettingsCanvas = 1,
        GameplayCanvas = 2,
        NavbarCanvas = 3,
        // Add more canvas IDs as needed
    }

    public class CanvasHandler
    {
        private readonly Dictionary<CanvasId, Transform> _canvases = new();

        public Transform GetCanvasTransform(CanvasId canvasId)
        {
            if (_canvases.TryGetValue(canvasId, out var canvasTransform))
            {
                return canvasTransform;
            }

            throw new KeyNotFoundException($"Canvas with ID {canvasId} not found.");
        }

        public void RegisterCanvas(CanvasId canvasId, Transform canvasTransform)
        {
            _canvases.TryAdd(canvasId, canvasTransform);
        }
    }
}