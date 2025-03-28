using System.Windows;

namespace AxialManagerS_Converter {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();

      // ボタンイベントハンドラー定義
      Converter.FinishButton.Click += FinishButton_Click;
    }

    private void FinishButton_Click(object sender, RoutedEventArgs e) {
      Close();
    }
  }
}