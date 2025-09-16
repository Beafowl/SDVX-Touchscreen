using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel.Dialogs
{
    internal interface IDialogService
    {
        T OpenDialog<T>(DialogViewModelBase<T> viewModel);

        Task<T> OpenDialogAsync<T>(DialogViewModelBase<T> viewModel);
    }
}
