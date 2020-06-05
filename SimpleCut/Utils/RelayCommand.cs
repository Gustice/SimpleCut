using System;
using System.Windows.Input;

namespace SimpleCut.Utils
{
    /// <summary>
    /// Relay Command consumer for commands sent from an arbitrary GUI
    /// </summary>
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        /// <summary>
        /// Constructor for action only
        /// </summary>
        /// <param name="execute"></param>
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        /// <summary>
        /// Constructor with Predicator
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Notification consumer for events when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Implements the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Implements the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
    }
}

