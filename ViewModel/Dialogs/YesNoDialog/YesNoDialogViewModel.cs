

using CommunityToolkit.Mvvm.Input;

namespace ViewModel.Dialogs.YesNoDialog
{
    public class YesNoDialogViewModel : DialogViewModelBase<bool>
    {
        public string Message { get; set; }

        public RelayCommand<IDialogWindow> YesCommand { get; private set; }

        public RelayCommand<IDialogWindow> NoCommand { get; private set; }

        public YesNoDialogViewModel(string title, string message) : base(title)
        {
            Message = message;
            YesCommand = new RelayCommand<IDialogWindow>(OnYes);
            NoCommand = new RelayCommand<IDialogWindow>(OnNo);
        }

        public void OnYes(IDialogWindow window)
        {
            CloseDialogWithResult(window, true);
        }

        public void OnNo(IDialogWindow window)
        {
            CloseDialogWithResult(window, false);
        }
    }
}
