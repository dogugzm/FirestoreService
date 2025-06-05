using UnityEngine;

namespace PanelService
{
    public class HeaderData
    {
        public HeaderData(string panelName)
        {
            PanelName = panelName;
        }

        public string PanelName { get; }
    }

    public interface IHeader<THeader, TData>
        where THeader : HeaderPanelBase<TData>
        where TData : HeaderData
    {
        public Transform GetHeaderParent();

        public TData HeaderData { get; }
        public THeader HeaderView { get; set; }
    }


    public abstract class HeaderPanelBase<T> : PanelBase where T : HeaderData
    {
        public abstract void Init(T data);
    }
}