using System;
using System.Windows.Input;

namespace Sqor.Utils.Bind
{
    public class Command : ICommand
    {
        private readonly Action handler;
        private readonly Action<object> handlerWithParam;
        private bool isEnabled;

        public Command(Action handler, bool isEnabled = true)
        {
            this.handler = handler;
            this.isEnabled = isEnabled;
        }

        public Command(Action<object> handler, bool isEnabled = true)
        {
            handlerWithParam = handler;
            this.isEnabled = isEnabled;
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value != isEnabled)
                {
                    isEnabled = value;
                    if (CanExecuteChanged != null)
                    {
                        CanExecuteChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (handlerWithParam != null) handlerWithParam(parameter); else handler();
        }
    }
}
