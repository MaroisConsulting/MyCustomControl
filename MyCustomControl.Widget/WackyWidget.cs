﻿namespace MyCustomControl.Widget
{
  using System;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using MyCustomControl.ThemeController;

  public class WackyWidget : Control, INotifyPropertyChanged
  {    
    #region Resource keys
    public static ComponentResourceKey CaptionStyleKey =
      new ComponentResourceKey(typeof(WackyWidget), "CaptionStyle");

    public static ComponentResourceKey SearchButtonStyleKey =
      new ComponentResourceKey(typeof(WackyWidget), "SearchButtonStyle");

    public static ComponentResourceKey ClearButtonStyleKey =
      new ComponentResourceKey(typeof(WackyWidget), "ClearButtonStyle");

    public static ComponentResourceKey SearchBoxStyleKey =
      new ComponentResourceKey(typeof(WackyWidget), "SearchBoxStyle");

    public static ComponentResourceKey ListBoxStyleKey =
      new ComponentResourceKey(typeof(WackyWidget), "ListBoxStyle");

    public static ComponentResourceKey ListBoxItemStyleKey =
      new ComponentResourceKey(typeof(WackyWidget), "ListBoxItemStyle");
    #endregion Resource keys

    #region Event Declarations
    public event PropertyChangedEventHandler? PropertyChanged;
    #endregion

    #region Properties
    private ObservableCollection<string>? _Results;
    public ObservableCollection<string> Results
    {
      get { return _Results; }
      set
      {
        if (_Results != value)
        {
          _Results = value;
          RaisePropertyChanged(nameof(Results));
        }
      }
    }

    private string? _ResultsText = "0 items found";
    public string ResultsText
    {
      get { return _ResultsText; }
      set
      {
        if (_ResultsText != value)
        {
          _ResultsText = value;
          RaisePropertyChanged(nameof(ResultsText));
        }
      }
    }

    private string? _SearchText;
    public string SearchText
    {
      get { return _SearchText; }
      set
      {
        if (_SearchText != value)
        {
          _SearchText = value;
          RaisePropertyChanged(nameof(SearchText));
        }
      }
    }
    #endregion

    #region DP's

    #region DP Caption
    public static readonly DependencyProperty CaptionProperty =
                    DependencyProperty.Register("Caption",
                    typeof(string),
                    typeof(WackyWidget),
                    new PropertyMetadata("", new PropertyChangedCallback(OnCaptionChanged)));

    public string Caption
    {
      get { return (string)GetValue(CaptionProperty); }
      set { SetValue(CaptionProperty, value); }
    }

    private static void OnCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      //var control = (WackyWidget)d;
    }

    #endregion

    #endregion

    #region Commands
    private ICommand? _ClearCommand;
    public ICommand ClearCommand
    {
      get
      {
        // Avoid this kind of initialization.
        // Use the constructor instead. Consistency is key.
        // Don't spread instance confifuration accross the type.
        // As a bonus, initilaizing the read-only property from the constructor
        // eliminates the backing field. See AnotherWackyWidget class for the difference.
        if (_ClearCommand == null)
          _ClearCommand = new RelayCommand(ClearExecuted, ClearCanExecute);
        return _ClearCommand;
      }
    }

    private ICommand? _SearchCommand;
    public ICommand SearchCommand
    {
      get
      {
        // Avoid this kind of initialization.
        // Use the constructor instead. Consistency is key.
        // Don't spread instance confifuration accross the type.
        if (_SearchCommand == null)
          _SearchCommand = new RelayCommand(SearchExecuted, SearchCanExecute);
        return _SearchCommand;
      }
    }
    #endregion

    #region CTOR
    static WackyWidget()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(WackyWidget),
          new FrameworkPropertyMetadata(typeof(WackyWidget)));
    }

    public WackyWidget()
    {
      Results = new ObservableCollection<string>();
      ResultsText = string.Empty;
      SearchText = string.Empty;
    }

    #endregion

    #region Private Methods

    private bool ClearCanExecute()
    {
      return !string.IsNullOrEmpty(SearchText);
    }

    private void ClearExecuted()
    {
      Results.Clear();
      ResultsText = "0 items found";
      SearchText = string.Empty;
    }

    private void RaisePropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SearchCanExecute()
    {
      return !string.IsNullOrEmpty(SearchText);
    }

    private void SearchExecuted()
    {
      for (int i = 0; i < 10; i++)
      {
        Results.Add($"{i}: {SearchText}");
      }
      ResultsText = "10 items found";

    }
    #endregion
  }
}
