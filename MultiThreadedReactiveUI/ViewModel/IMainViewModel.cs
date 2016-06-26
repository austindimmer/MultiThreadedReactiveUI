using System.Collections.Generic;
using System.Threading.Tasks;
using MultiThreadedReactiveUI.Model;
using ReactiveUI;

namespace MultiThreadedReactiveUI.ViewModel
{
    public interface IMainViewModel
    {

        Task<IEnumerable<Function>> CategoryFilterSelectedAsync();

        ReactiveCommand<List<ComputationTaskViewModel>> AddFunctionToFunctionsToExecute { get; }
        ReactiveCommand<AsyncVoid> CancelRunningFunctionsToExecute { get; }
        CancelRunViewModel CancelRunViewModelCancel { get; set; }
        CancelRunViewModel CancelRunViewModelRun { get; set; }
        ReactiveCommand<IEnumerable<Function>> CategoryFilterSelected { get; }
        CancelRunViewModel CurrentCancelRunViewModel { get; set; }
        ComputationTaskViewModel CurrentComputationTaskViewModel { get; set; }
        ComputationTaskViewModel CurrentComputationTaskViewModelIndex { get; set; }
        string ExecutionLabel { get; set; }
        List<string> FunctionCategories { get; set; }
        ReactiveList<Function> Functions { get; set; }
        int ProgressForAllTasks { get; set; }
        ReactiveCommand<List<ComputationTaskViewModel>> RemoveFunctionFromFunctionsToExecute { get; }
        ReactiveCommand<AsyncVoid> RunFunctionsToExecute { get; }
        string SelectedCategory { get; set; }
        ReactiveList<Function> SelectedFunctions { get; set; }
        ComputationTaskViewModel SelectedTask { get; set; }
        ReactiveCommand<AsyncVoid> StartRunningTasks { get; }
        ReactiveList<ComputationTaskViewModel> TasksToExecute { get; set; }
        Dictionary<string, ReactiveCommand<AsyncVoid>> ToggleExecutionDictionary { get; set; }
        int TotalIterationsForAllTasks { get; set; }
    }
}