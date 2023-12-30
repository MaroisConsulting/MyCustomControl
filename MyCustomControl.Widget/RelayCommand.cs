using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyCustomControl.Widget
{
  public class RelayCommand : ICommand
  {
    public bool IsManualCanExecuteChangedEventEnabled { get; set; }

    private event EventHandler ManualCanExecuteChanged;
    private Action ExecuteDelegate { get; }
    private Func<bool> CanExecuteDelegate { get; }

    #region Constructors 
    public RelayCommand(Action execute) : this(() => execute?.Invoke(), null) { }
    public RelayCommand(Action execute, Func<bool> canExecute)
    {
      this.ExecuteDelegate = execute ?? throw new ArgumentNullException(nameof(execute));
      this.CanExecuteDelegate = canExecute ?? (() => true);
    }

    #endregion Constructors 

    #region ICommand Members 

    public event EventHandler CanExecuteChanged
    {
      add
      {
        if (this.ManualCanExecuteChanged is null)
        {
          CommandManager.RequerySuggested += OnCommandManagerRequerySuggested; 
        }

        this.ManualCanExecuteChanged += value;
      }
      remove
      {
        this.ManualCanExecuteChanged -= value;
        if (this.ManualCanExecuteChanged is null)
        {
          CommandManager.RequerySuggested -= OnCommandManagerRequerySuggested;
        }
      }
    }

    [DebuggerStepThrough]
    public bool CanExecute() => this.CanExecuteDelegate.Invoke();
    public void Execute() => this.ExecuteDelegate.Invoke();

    #endregion ICommand Members 

    // Explicit ICommand implementation.
    // Thess methods are only visible when you explicitly cast RelayCommand to ICommand.
    bool ICommand.CanExecute(object? parameter) => CanExecute();
    void ICommand.Execute(object? parameter) => Execute();

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
