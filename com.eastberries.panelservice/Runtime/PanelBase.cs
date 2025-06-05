using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PanelService
{
    public interface IPanel
    {
        RectTransform RectTransform { get; }
        Transform transform { get; }
        IPanelData PanelData { get; }
        void Initialize();
        Task ShowAsync();
        Task HideAsync();
        void SetParent(Transform parent);
        void SetPanelData(IPanelData data);
        void FitToCanvas();
        void Reset();
        public event Action OnPanelHide;
    }

    public abstract class InnerPanelBase<TPanel> : PanelBase where TPanel : PanelBase
    {
        [Inject] private IPanelService _panelService;

        public override async Task ShowAsync()
        {
            if (_panelService.TryGetPanel<TPanel>(out var basePanel))
            {
                basePanel.OnPanelHide += () => { HideAsync(); };
            }

            await base.ShowAsync();
        }

        public override async Task HideAsync()
        {
            if (_panelService.TryGetPanel<TPanel>(out var basePanel))
            {
                basePanel.OnPanelHide -= () => { HideAsync(); };
            }

            await base.HideAsync();
        }
    }


    [RequireComponent(typeof(CanvasGroup), typeof(Canvas), typeof(GraphicRaycaster))]
    public abstract class PanelBase : MonoBehaviour, IPanel
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;

        public RectTransform RectTransform => rectTransform;
        public IPanelData PanelData { get; private set; }

        public event Action OnPanelHide;

        protected virtual string PanelName => GetType().Name;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Initialize()
        {
            // Override in derived classes for specific initialization
        }

        public virtual async Task ShowAsync()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            await canvasGroup.DOFade(1, 0.25f).AsyncWaitForCompletion();

            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual async Task HideAsync()
        {
            await canvasGroup.DOFade(0, 0.25f).AsyncWaitForCompletion();
            OnPanelHide?.Invoke();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);

            if (PanelData.DestroyOnHide)
            {
                Destroy(gameObject);
            }
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent, false);
        }

        public void FitToCanvas()
        {
            RectTransform.anchorMin = Vector2.zero;
            RectTransform.anchorMax = Vector2.one;
            RectTransform.offsetMax = Vector2.zero;
            RectTransform.offsetMin = Vector2.zero;
        }

        public void SetPanelData(IPanelData data)
        {
            PanelData = data;
        }

        public virtual void Reset()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}