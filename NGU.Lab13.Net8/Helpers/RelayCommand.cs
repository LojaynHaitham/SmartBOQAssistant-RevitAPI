using System;
using System.Windows.Input;

namespace NGU.Lab13.Helpers
{
    /// <summary>
    /// A beginner-friendly implementation of the ICommand interface, common in MVVM patterns.
    /// It delegates command execution and validation to specified Action and Predicate delegates.
    /// This removes the need to create a separate class for every single button command.
    /// </summary>
    public class RelayCommand : ICommand
    {
        // Private fields that hold the actions to run
        private readonly Action<object?>? _executeWithParam;
        private readonly Action? _executeWithoutParam;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Constructor for commands that require an input parameter.
        /// </summary>
        /// <param name="execute">The action to execute when the command is triggered.</param>
        /// <param name="canExecute">Optional check to determine if the command can run (enables/disables the button).</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Constructor for commands that DO NOT require any input parameters.
        /// </summary>
        /// <param name="execute">The action to execute when the command is triggered.</param>
        /// <param name="canExecute">Optional check to determine if the command can run.</param>
        public RelayCommand(Action execute, Predicate<object?>? canExecute = null)
        {
            _executeWithoutParam = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Checks if the command can execute in its current state.
        /// WPF automatically calls this to enable or disable bound buttons.
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// The main logic called when the command is executed (e.g., button clicked).
        /// </summary>
        public void Execute(object? parameter)
        {
            if (_executeWithParam != null)
            {
                _executeWithParam(parameter);
            }
            else if (_executeWithoutParam != null)
            {
                _executeWithoutParam();
            }
        }

        /// <summary>
        /// Event that notifies WPF when the execution status of a command has changed.
        /// Binds to the command manager's RequerySuggested event, which automatically re-evaluates
        /// CanExecute when user interactions occur (like typing or clicking).
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
