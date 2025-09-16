using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel.Dialogs
{
    public class DialogViewModelBase<T> : ObservableObject
    {
        public string Title { get; set; }

        public T DialogResult { get; set; }

        public DialogViewModelBase(string title)
        {
            Title = title;
        }

        public void CloseDialogWithResult(IDialogWindow window, T result)
        {
            DialogResult = result;
            if (window != null)
                window.DialogResult = true;
        }
    }
}
