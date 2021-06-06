using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NeuralNetwork.UI.ViewModels
{
    class DataPickerDialogViewModel : BindableBase, IDialogAware
    {
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public List<Variable> Variables { get; set; } = new();

        private string _title = "Notification";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public event Action<IDialogResult> RequestClose;

        protected virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "false")
                result = ButtonResult.Cancel;

            string[] names = Variables.Select(v => v.Name).ToArray();
            bool[] outputs = Variables.Select(v => v.Output).ToArray();

            RaiseRequestClose(new DialogResult(result, new DialogParameters 
            {
                { "names",  JsonSerializer.Serialize(names)},
                { "outputs", JsonSerializer.Serialize(outputs)}
            }));
        }
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>("message");
            int count = parameters.GetValue<int>("count");
            for (int i = 0; i < count; i++)
            {
                Variables.Add(new Variable() { Name = "var" + i, Output = false });
            }
        }

        internal class Variable
        {
            public string Name { get; set; }
            public bool Output { get; set; }
        }
    }
}
