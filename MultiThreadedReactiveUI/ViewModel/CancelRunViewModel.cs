using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadedReactiveUI.ViewModel
{
    public class CancelRunViewModel : ReactiveObject, ICancelRunViewModel
    {
        [Reactive]
        public string DisplayText { get; set; }
        public ReactiveCommand<AsyncVoid> ReactiveCommandToExecute { get; set; }
    }
}
