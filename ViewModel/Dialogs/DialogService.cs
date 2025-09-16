using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ViewModel.Dialogs
{
    internal class DialogService : IDialogService
    {
        public T OpenDialog<T>(DialogViewModelBase<T> viewModel)
        {
            DialogWindow window = new DialogWindow();
            window.DataContext = viewModel;
            window.ShowDialog();
            return viewModel.DialogResult;
        }

        public async Task<T> OpenDialogAsync<T>(DialogViewModelBase<T> viewModel)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() => {
                return OpenDialog(viewModel);
            });
        }
    }
}
