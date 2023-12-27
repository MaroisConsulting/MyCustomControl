using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyCustomControl.Widget
{
  public class RelayCommand<TCommandParameter> : ICommand
  {
    public bool IsManualCanExecuteChangedEventEnabled { get; set; }

    private event EventHandler ManualCanExecuteChanged;
    private Action<TCommandParameter?> ExecuteDelegate { get; }
    private Func<TCommandParameter?, bool> CanExecuteDelegate { get; }

    #region Constructors 
    public RelayCommand(Action<TCommandParameter> execute) : this(execute!, null) { }
    public RelayCommand(Action<TCommandParameter?> execute, Func<TCommandParameter?, bool>? canExecute)
    {
      this.ExecuteDelegate = execute ?? throw new ArgumentNullException(nameof(execute));
      this.CanExecuteDelegate = canExecute ?? (commandParameter => true);
    }

    #endregion Constructors 

    #region ICommand Members 

    public event EventHandler CanExecuteChanged
    {
      add
      {
        CommandManager.RequerySuggested += OnCommandManagerRequerySuggested;
        this.ManualCanExecuteChanged += value;
      }
      remove
      {
        CommandManager.RequerySuggested -= OnCommandManagerRequerySuggested;
        this.ManualCanExecuteChanged -= value;
      }
    }

    [DebuggerStepThrough]
    public bool CanExecute(TCommandParameter? parameter) => this.CanExecuteDelegate.Invoke(parameter);
    public void Execute(TCommandParameter? parameter) => this.ExecuteDelegate.Invoke(parameter);

    #endregion ICommand Members 

    // Explicit ICommand implementation.
    // Thess methods are only visible when you explicitly cast RelayCommand to ICommand.
    bool ICommand.CanExecute(object? parameter) => CanExecute((TCommandParameter)parameter);
    void ICommand.Execute(object? parameter) => Execute((TCommandParameter)parameter);

    public void InvalidateCommand() => OnManualCanExecuteChanged();

    private void OnCommandManagerRequerySuggested(object? sender, EventArgs e)
    {
      if (!this.IsManualCanExecuteChangedEventEnabled)
      {
        OnManualCanExecuteChanged();
      }
    }

    private void OnManualCanExecuteChanged() => this.ManualCanExecuteChanged?.Invoke(this, EventArgs.Empty);
  }
}
