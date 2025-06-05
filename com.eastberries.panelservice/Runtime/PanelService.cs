using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PanelService
{
    public interface IPanelService
    {
        Task<T> ShowPanelAsync<T>() where T : IPanel;
        Task<T> ShowPanelAsync<T, TD>(TD panelParameter) where T : IPanel;
        Task HidePanelAsync<T>() where T : IPanel;
        Task HidePanelAsync(IPanel panel);
        Task HideAllAsync();
        bool TryGetPanel<T>(out T panel) where T : IPanel;
        Action<PanelBase> OnPanelShow { get; set; }
    }

    public interface IPanelParameter<T>
    {
        public T Parameter { get; set; }
    }

    public interface IIgnoreJoinStack
    {
        public bool IgnoreJoinStack { get; }
    }

    public class PanelService : IPanelService
    {
        private readonly Dictionary<string, PanelConfig> _panelConfigs = new();
        private readonly List<IPanel> _activePanels = new();
        private readonly IPanelFactory _panelFactory;
        private readonly CanvasHandler _canvasHandler;
        private readonly IPanelPool _panelPool;
        private IPanel _currentPanel;

        public Action<PanelBase> OnPanelShow { get; set; }


        private Stack<IPanel> _panelStack = new();

        public PanelService(
            IPanelFactory panelFactory,
            PanelSettings panelSettings,
            CanvasHandler canvasHandler,
            IPanelPool panelPool)
        {
            _panelFactory = panelFactory;
            _canvasHandler = canvasHandler;
            _panelPool = panelPool;
            foreach (var config in panelSettings.panelConfigs)
            {
                _panelConfigs[config.prefab.GetType().Name] = config;
                if (config.prewarmCount > 0)
                {
                    _panelPool.PrewarmPool(config.prefab.gameObject, config.prewarmCount);
                }
            }
        }

        public async Task HandleBackButton()
        {
            if (_panelStack.Count > 0)
            {
                var panel = _panelStack.Pop();
                if (panel is IPanelParameter<object> parameterHolder)
                {
                    parameterHolder.Parameter = null;
                }

                await HidePanelAsync(panel);
            }
        }

        private async Task<T> ShowPanelLocal<T, TData>(TData panelParameter) where T : IPanel
        {
            var panelId = typeof(T).Name;

            if (!_panelConfigs.TryGetValue(panelId, out var panelConfig))
            {
                throw new ArgumentException($"Panel config for panelId {panelId} not found!");
            }

            if (TryGetPanel(out T existingPanel))
            {
                if (existingPanel is IIgnoreJoinStack ignoreJoinStack && ignoreJoinStack.IgnoreJoinStack)
                {
                    // Do not add to stack
                }
                else
                {
                    _panelStack.Push(existingPanel as PanelBase);
                }

                if (existingPanel is IPanelParameter<TData> parameterHolder && panelParameter is not null)
                {
                    parameterHolder.Parameter = panelParameter;
                }

                _currentPanel = existingPanel;
                await existingPanel.ShowAsync();
                OnPanelShow?.Invoke(existingPanel as PanelBase);
                return existingPanel;
            }

            var canvasTransform = _canvasHandler.GetCanvasTransform(panelConfig.canvasId);
            var panel = _panelPool.Get(panelConfig.prefab.gameObject, canvasTransform);

            if (panel is T typedPanel)
            {
                panel.SetPanelData(new PanelData()
                {
                    DestroyOnHide = panelConfig.destroyOnHide,
                    CanvasId = panelConfig.canvasId,
                    PanelId = panelId,
                    ShouldFitToCanvas = panelConfig.shouldFitToCanvas
                });

                if (panel is IPanelParameter<TData> parameterHolder && panelParameter is not null)
                {
                    parameterHolder.Parameter = panelParameter;
                }

                panel.Reset();
                panel.Initialize();

                if (panelConfig.shouldFitToCanvas)
                {
                    panel.FitToCanvas();
                }

                if (panel is IIgnoreJoinStack ignoreJoinStack && ignoreJoinStack.IgnoreJoinStack)
                {
                    // Do not add to stack
                }
                else
                {
                    _panelStack.Push(panel as PanelBase);
                }

                _activePanels.Add(panel);
                _currentPanel = panel;
                await panel.ShowAsync();
                OnPanelShow?.Invoke(panel as PanelBase);

                return typedPanel;
            }


            throw new InvalidOperationException($"Created panel is not of type {typeof(T)}!");
        }

        public async Task<T> ShowPanelAsync<T>() where T : IPanel
        {
            return await ShowPanelLocal<T, object>(null);
        }

        public async Task<T> ShowPanelAsync<T, TD>(TD panelParameter) where T : IPanel
        {
            return await ShowPanelLocal<T, TD>(panelParameter);
        }

        public async Task HidePanelAsync<T>() where T : IPanel
        {
            if (TryGetPanel(out T panel))
            {
                await HidePanelAsync(panel);
            }
        }

        public async Task HidePanelAsync(IPanel panel)
        {
            if (panel == null || !_activePanels.Contains(panel)) return;
            await panel.HideAsync();
            _activePanels.Remove(panel);

            if (!panel.PanelData.DestroyOnHide)
            {
                _panelPool.Return(panel);
            }
        }

        public async Task HideAllAsync()
        {
            var panels = _activePanels.ToList();
            var tasks = panels.Select(HidePanelAsync);
            await Task.WhenAll(tasks);
        }

        public bool TryGetPanel<T>(out T panel) where T : IPanel
        {
            panel = _activePanels.OfType<T>().FirstOrDefault();
            return panel != null;
        }
    }
}