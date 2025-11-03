namespace BlazorCrudDemo.Web.Services;

public class ModalService
{
    public event Action<string, string, ModalType>? OnShow;
    public event Action? OnClose;

    public void Show(string title, string message, ModalType type = ModalType.Info)
    {
        OnShow?.Invoke(title, message, type);
    }

    public void Close()
    {
        OnClose?.Invoke();
    }
}

public enum ModalType
{
    Info,
    Success,
    Warning,
    Error
}
