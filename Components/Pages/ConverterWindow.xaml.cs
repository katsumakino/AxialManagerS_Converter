using AxialManagerS_Converter.Common;
using System.Windows;

namespace AxialManagerS_Converter.Components.Pages {
  /// <summary>
  /// ConverterWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class ConverterWindow : System.Windows.Controls.UserControl {
    public ConverterWindow() {
      InitializeComponent();
    }

    private void SelectButton_Click(object sender, RoutedEventArgs e) {
      FIleUtilities utilities = new();
      string path = string.Empty;
      string folderName = string.Empty;
      string msg = "Select Folder";

      try {
        if (!utilities.SelectFolder(ref folderName, msg, path, "\\")) {
          return;
        }

        if (!string.IsNullOrEmpty(folderName)) {
          System.Windows.MessageBox.Show($"{folderName}", "folder Path", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        if (DataContext is ViewModel.ConverterWindowVM vm) {
          vm.SrcFolder = folderName;
        }

      } catch {
      } finally { }
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e) {
      FIleUtilities utilities = new();
      string path = string.Empty;
      string folderName = string.Empty;
      string msg = "Select Copy Folder";

      try {

        if (!utilities.SelectFolder(ref folderName, msg, path, "\\")) {
          return;
        }

        // todo: 設定ファイル、コメントファイル、データベースを指定のフォルダにコピーする
        if (!utilities.CopyFolder(ConverterGlobal.Axm1DBFolderPath, folderName, false)) {
          return;
        }

        if (!utilities.CopyFolder(ConverterGlobal.Axm1CommentFolderPath, folderName, false)) {
          return;
        }

        if (!utilities.CopyFolder(ConverterGlobal.Axm1SettingFolderPath, folderName, false)) {
          return;
        }

        CopyCheckLabel.Visibility = Visibility.Visible;

        System.Windows.MessageBox.Show("Complete", "", MessageBoxButton.OK, MessageBoxImage.Information);

      } catch {
      } finally { }

    }

    private void SettingConvertButton_Click(object sender, RoutedEventArgs e) {

      if (DataContext is ViewModel.ConverterWindowVM vm) {
        vm.DoConvert();
      }
    }

    public void DeleteFolder() {

      if (!string.IsNullOrEmpty(vm.SrcFolder)) {
        FIleUtilities utilities = new();
        var result = System.Windows.MessageBox.Show("変換元のコピーデータを削除しますか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes) {
          if (DataContext is ViewModel.ConverterWindowVM vm) {
            // 変換元のコピーデータを削除
            utilities.ForceRemoveDir(vm.SrcFolder);
          }
        }
      }
    }
  }
}
