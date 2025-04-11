using AxialManagerS_Converter.Common;
using AxialManagerS_Converter.Components.Windows;
using System.Diagnostics;
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

        vm.SrcFolder = folderName;

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

        // 設定ファイル、コメントファイル、データベースを指定のフォルダにコピーする
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
      vm.GetConvertPatientCount();

      // 非同期で、変換処理とプログレスバー表示を実行
      ProcessingStart();
      DbConvertStart();
    }

    /// <summary>
    /// 変換中のプログレスバーを表示
    /// </summary>
    /// <param name="processingWindow"></param>
    private async void ProcessingStart() {
      var processingWindow = new ProcessingWindow();
      processingWindow.Show();

      Stopwatch stopwatch = new Stopwatch();

      // ストップウォッチを開始
      stopwatch.Start();

      IsEnabled = false;

      var progress = new Progress<int>(value => processingWindow.UpdateProgress(value));
      await Task.Run(() => DoWork(progress));

      // ストップウォッチを停止
      stopwatch.Stop();

      System.Windows.MessageBox.Show($"処理時間: {stopwatch.ElapsedMilliseconds}ミリ秒", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);

      // 処理が完了したら、ウィンドウを閉じる
      if (processingWindow.IsVisible) {
        processingWindow.Close();
        IsEnabled = true;
      }
    }

    private async void DbConvertStart() {
      await Task.Run(() => {
        vm.DoConvert();
      });
    }

    private void DoWork(IProgress<int> progress) {
      int progressValue;
      while (true) {
        Task.Delay(50).Wait();

        progressValue = vm.GetConvertProgress();
        progress.Report(progressValue);

        if(progressValue >= 100) {
          break;
        }
      }
    }

    public void DeleteFolder() {

      if (!string.IsNullOrEmpty(vm.SrcFolder)) {
        FIleUtilities utilities = new();
        var result = System.Windows.MessageBox.Show("変換元のコピーデータを削除しますか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes) {
          // 変換元のコピーデータを削除
          utilities.ForceRemoveDir(vm.SrcFolder);
        }
      }
    }
  }
}
