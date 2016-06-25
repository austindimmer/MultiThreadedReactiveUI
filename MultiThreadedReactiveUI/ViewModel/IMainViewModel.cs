using System.Collections.Generic;
using MultiThreadedReactiveUI.Model;
using ReactiveUI;

namespace MultiThreadedReactiveUI.ViewModel
{
    public interface IMainViewModel
    {
        ReactiveCommand<List<ComputationTaskViewModel>> AddFunctionToFunctionsToExecute { get; }
        ReactiveCommand<AsyncVoid> CancelRunningFunctionsToExecute { get; }
        ReactiveCommand<AsyncVoid> CategoryFilterSelected { get; }
        string ExecutionLabel { get; set; }
        ReactiveList<Function> FilteredFunctions { get; set; }
        List<string> FunctionCategories { get; set; }
        ReactiveList<Function> Functions { get; set; }
        int Progress { get; set; }
        ReactiveCommand<List<ComputationTaskViewModel>> RemoveFunctionFromFunctionsToExecute { get; }
        ReactiveCommand<AsyncVoid> RunFunctionsToExecute { get; }
        string SelectedCategory { get; set; }
        ReactiveList<Function> SelectedFunctions { get; set; }
        ComputationTaskViewModel SelectedTask { get; set; }
        ReactiveCommand<AsyncVoid> StartAsyncCommand { get; }
        ReactiveList<ComputationTaskViewModel> TasksToExecute { get; set; }
        ReactiveCommand<AsyncVoid> ToggleRunCancelCommand { get; }
    }
}