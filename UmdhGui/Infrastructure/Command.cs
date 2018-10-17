using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace UmdhGui.Infrastructure
{
    public class Command : ICommand
    {
        private static readonly List<Command> Commands = new List<Command>();
        private readonly Func<object, bool> _canExecute;


        private bool _canExecuteFlag = true;
        private readonly Action<object> _func;

        public Command(Action<object> func)
        {
            // use the CanExecuteFlag to externally set the state!
            _func = func;
            _canExecute = null;
        }

        public Command(Action<object> func, Func<object, bool> canExecute)
        {
            _func = func;
            _canExecute = canExecute;
        }

        public bool CanExecuteFlag
        {
            get { return _canExecuteFlag; }
            set
            {
                _canExecuteFlag = value;
                Refresh();
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return CanExecuteFlag;

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _func?.Invoke(parameter);
        }

        public void Refresh()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public static Command CreateWithRegistration(Action<object> func, Func<object, bool> canExecute)
        {
            var command = new Command(func, canExecute);
            Commands.Add(command);
            return command;
        }

        internal static void RefreshAll()
        {
            foreach (var command in Commands)
                command.CanExecuteChanged?.Invoke(command, new EventArgs());
        }
    }
}