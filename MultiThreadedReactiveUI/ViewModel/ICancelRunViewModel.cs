using ReactiveUI;

namespace MultiThreadedReactiveUI.ViewModel
{
    public interface ICancelRunViewModel
    {
        string DisplayText { get; set; }
        ReactiveCommand<AsyncVoid> ReactiveCommandToExecute { get; set; }
    }
}