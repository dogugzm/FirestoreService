namespace PanelService
{
    public interface IPanelData
    {
        string PanelId { get; }
        bool DestroyOnHide { get; }
        CanvasId CanvasId { get; }
        bool ShouldFitToCanvas { get; }
    }

    public class PanelData : IPanelData
    {
        public string PanelId { get; set; }
        public bool DestroyOnHide { get; set; }
        public CanvasId CanvasId { get; set; } = CanvasId.MainCanvas;
        public bool ShouldFitToCanvas { get; set; }
    }
}