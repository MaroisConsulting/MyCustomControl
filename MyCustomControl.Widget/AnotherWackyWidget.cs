namespace MyCustomControl.Widget
{
  using System;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using MyCustomControl.ThemeController;

  public class AnotherWackyWidget : Control, INotifyPropertyChanged
  {
    #region Resource keys
    public static ComponentResourceKey CaptionStyleKey =
      new ComponentResourceKey(typeof(AnotherWackyWidget), "CaptionStyle");

    public static ComponentResourceKey SearchButtonStyleKey =
      new ComponentResourceKey(typeof(AnotherWackyWidget), "SearchButtonStyle");

    public static ComponentResourceKey ClearButtonStyleKey =
      new ComponentResourceKey(typeof(AnotherWackyWidget), "ClearButtonStyle");

    public static ComponentResourceKey SearchBoxStyleKey =
      new ComponentResourceKey(typeof(AnotherWackyWidget), "SearchBoxStyle");

    public static ComponentResourceKey ListBoxStyleKey =
      new ComponentResourceKey(typeof(AnotherWackyWidget), "ListBoxStyle");

    public static ComponentResourceKey ListBoxItemStyleKey =
      new ComponentResourceKey(typeof(AnotherWackyWidget), "ListBoxItemStyle");
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
                    typeof(AnotherWackyWidget),
                    new PropertyMetadata("", new PropertyChangedCallback(OnCaptionChanged)));

    public string Caption
    {
      get { return (string)GetValue(CaptionProperty); }
      set { SetValue(CaptionProperty, value); }
    }

    private static void OnCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      //var control = (AnotherWackyWidget)d;
    }

    #endregion

    #endregion

    #region Commands
    public ICommand ClearCommand { get; }

    public ICommand SearchCommand { get; }

    #endregion

    #region CTOR
    static AnotherWackyWidget()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(AnotherWackyWidget),
          new FrameworkPropertyMetadata(typeof(AnotherWackyWidget)));
    }

    public AnotherWackyWidget()
    {
      this.SearchCommand = new RelayCommand(ExecuteSearch, CanExecuteSearch);
      this.ClearCommand = new RelayCommand(ExecuteClear, CanExecuteClear);
      Results = new ObservableCollection<string>();
      ResultsText = string.Empty;
      SearchText = string.Empty;
    }

    #endregion

    #region Private Methods

    private bool CanExecuteClear()
    {
      return !string.IsNullOrEmpty(SearchText);
    }

    private void ExecuteClear()
    {
      Results.Clear();
      ResultsText = "0 items found";
      SearchText = string.Empty;
    }

    private void RaisePropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool CanExecuteSearch()
    {
      return !string.IsNullOrEmpty(SearchText);
    }

    private void ExecuteSearch()
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
