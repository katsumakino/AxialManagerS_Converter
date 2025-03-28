using AxialManagerS_Converter.Common;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
      string fileName = string.Empty;
      string filter = "JSONファイル(*.json)|*.json|すべてのファイル(*.*)|*.*";

      try {
        if (!utilities.SelectFile(ref fileName, path, "", filter, "")) {
          return;
        }

        if (!string.IsNullOrEmpty(fileName)) {
          System.Windows.MessageBox.Show($"{fileName}", "fileName", MessageBoxButton.OK, MessageBoxImage.Information);
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

        if(!utilities.SelectFolder(ref folderName, msg, path, "\\")) {
          return;
        }

        // todo: 設定ファイル、コメントファイル、データベースを指定のフォルダにコピーする
        if(!utilities.CopyFolder(ConverterGlobal.Axm1DBFolderPath, folderName, false)) {
          return;
        }

        if (!utilities.CopyFolder(ConverterGlobal.Axm1CommentFolderPath, folderName, false)) {
          return;
        }

        if (!utilities.CopyFolder(ConverterGlobal.Axm1SettingFolderPath, folderName, false)) {
          return;
        }

        if (DataContext is ViewModel.ConverterWindowVM vm) {
          vm.SrcFolder = folderName;
        }

        System.Windows.MessageBox.Show("Complete", "", MessageBoxButton.OK, MessageBoxImage.Information);

      } catch {
      } finally { }

    }

    private void SettingConvertButton_Click(object sender, RoutedEventArgs e) {

      if(DataContext is ViewModel.ConverterWindowVM vm) {
        vm.DoConvert();
      }
    }
  }
}
