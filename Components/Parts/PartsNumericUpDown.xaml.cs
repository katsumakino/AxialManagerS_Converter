using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// ↓をカスタマイズして作成
// https://github.com/gogowaten/userControl/releases/tag/1.2.2

namespace AxialManagerS_Converter.Components.Parts {
    /// <summary>
    /// PartsNumericUpDown.xaml の相互作用ロジック
    /// 使用時は、下記パラメータを設定すること
    /// MyValue : 設定値
    /// MyStep : ボタン押下・マウスホイール処理の変化量
    /// MyStringFormat : MyValueの文字列変換値(設定不要)
    /// MyKetaRear : 小数点以下表示桁数
    /// MyMin : MyValueの下限値
    /// MyMax : MyValueの上限値
    /// </summary>
    public partial class PartsNumericUpDown : System.Windows.Controls.UserControl {
        public PartsNumericUpDown() {
            InitializeComponent();

            //MyValueとTextBoxのTextとのBinding
            var mb = new MultiBinding();
            mb.Converter = new MyStringConverter();

            System.Windows.Data.Binding b;
            //Value用のBinding
            b = new System.Windows.Data.Binding();
            b.Source = this;
            b.Path = new PropertyPath(MyValueProperty);
            b.Mode = BindingMode.TwoWay;
            mb.Bindings.Add(b);

            //StringFormat用のBinding
            b = new System.Windows.Data.Binding();
            b.Source = this;
            b.Path = new PropertyPath(MyStringFormatProperty);
            //【重要】TextBoxの値からはStringFormatを変換しないので渡さない
            b.Mode = BindingMode.OneWay;
            mb.Bindings.Add(b);

            MyTextBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, mb);
        }

        public decimal MyValue {
            get { return (decimal)GetValue(MyValueProperty); }
            set { SetValue(MyValueProperty, value); }
        }

        public static readonly DependencyProperty MyValueProperty =
            DependencyProperty.Register(nameof(MyValue), typeof(decimal), typeof(PartsNumericUpDown),
                new FrameworkPropertyMetadata(0m, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                    , OnMyValueChanged, CoerceMyValue));

        private static void OnMyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {           
        }

        private static object CoerceMyValue(DependencyObject d, object baseValue) {
            //最小値と最大値を越えないように値を矯正
            var value = (decimal)baseValue;
            var nud = (PartsNumericUpDown)d;
            if (value < nud.MyMin)
                value = nud.MyMin;
            if (value > nud.MyMax)
                value = nud.MyMax;
            return value;
        }

        private decimal ChangeValue(decimal value) {
            if (value < MyMin) value = MyMin;
            if (value > MyMax) value = MyMax;
            return value;
        }

        private static string ProofreadingInputText(string text) {
            if (text.StartsWith('.') || text.EndsWith('.')) {
                text = text.Replace(".", "");
            }

            // -. も変なのでピリオドだけ削除
            text = text.Replace("-.", "-");

            //数値がないのにハイフンやピリオドがあった場合は削除
            if (text == "-" || text == ".")
                text = "";

            return text;
        }

        public decimal MyStep {
            get => (decimal)GetValue(MyStepProperty);
            set => SetValue(MyStepProperty, value);
        }
        public static readonly DependencyProperty MyStepProperty =
            DependencyProperty.Register(nameof(MyStep), typeof(decimal), typeof(PartsNumericUpDown)
                , new PropertyMetadata(1m));

        #region stringformat
        public string MyStringFormat {
            get => (string)GetValue(MyStringFormatProperty);
            set => SetValue(MyStringFormatProperty, value);
        }
        public static readonly DependencyProperty MyStringFormatProperty =
            DependencyProperty.Register(nameof(MyStringFormat), typeof(string), typeof(PartsNumericUpDown)
                , new PropertyMetadata(new string('0', 1)));

        //小数桁数
        public int MyKetaRear {
            get => (int)GetValue(MyKetaRearProperty);
            set => SetValue(MyKetaRearProperty, value);
        }
        public static readonly DependencyProperty MyKetaRearProperty =
            DependencyProperty.Register(nameof(MyKetaRear), typeof(int), typeof(PartsNumericUpDown)
                , new PropertyMetadata(0, OnMyKetaRearChanged));

        private static void OnMyKetaRearChanged(DependencyObject obj
            , DependencyPropertyChangedEventArgs e) {
            var uc = (PartsNumericUpDown)obj;
            if (uc != null) {
                int keta = (int)e.NewValue;
                if (keta < 0)
                    keta = 0;

                if (keta == 0) {
                    uc.MyStringFormat = new string('0', 1);
                } else {
                    uc.MyStringFormat = new string('0', 1) + '.' + new string('0', keta);
                }
            }
        }
        #endregion stringformat

        #region MinMax
        public decimal MyMin {
            get { return (decimal)GetValue(MyMinProperty); }
            set { SetValue(MyMinProperty, value); }
        }

        public static readonly DependencyProperty MyMinProperty =
            DependencyProperty.Register("MyMin", typeof(decimal), typeof(PartsNumericUpDown),
                new PropertyMetadata(decimal.MinValue, OnMyMinChanged));
        private static void OnMyMinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            //今の値と設定された最小値に矛盾があれば、今の値を最小値と同じ値に変更
            var uc = (PartsNumericUpDown)d;
            var value = (decimal)e.NewValue;
            if (uc.MyValue < value)
                uc.MyValue = value;
        }

        public decimal MyMax {
            get { return (decimal)GetValue(MyMaxProperty); }
            set { SetValue(MyMaxProperty, value); }
        }

        public static readonly DependencyProperty MyMaxProperty =
            DependencyProperty.Register("MyMax", typeof(decimal), typeof(PartsNumericUpDown),
                new PropertyMetadata(decimal.MaxValue, OnMyMaxChanged));
        private static void OnMyMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            //今の値と設定された最大値に矛盾があれば、今の値を最大値と同じ値に変更
            var uc = (PartsNumericUpDown)d;
            var value = (decimal)e.NewValue;
            if (uc.MyValue > value)
                uc.MyValue = value;
        }
        
        #endregion MinMax

        #region クリックやキー入力での数値加減

        private void RepeatButtonUp_Click(object sender, RoutedEventArgs e) {
            MyValue = ChangeValue(MyValue + MyStep);
        }

        private void RepeatButtonDown_Click(object sender, RoutedEventArgs e) {
            MyValue = ChangeValue(MyValue - MyStep);
        }

        private void RepeatButton_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta < 0) {
                MyValue = ChangeValue(MyValue - MyStep);
            } else {
                MyValue = ChangeValue(MyValue + MyStep);
            }

            e.Handled = true;
        }

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta < 0) {
                MyValue = ChangeValue(MyValue - MyStep);
            } else {
                MyValue = ChangeValue(MyValue + MyStep);
            }

            e.Handled = true;
        }

        //方向キーでの数値加減
        private void MyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            //スペースキーが押されたときは無効にする
            if (e.Key == Key.Space) {
                e.Handled = true;
                return;
            }

            //up、downでSmallChange
            else if (e.Key == Key.Up)
                MyValue = ChangeValue(MyValue + MyStep);
            else if (e.Key == Key.Down)
                MyValue = ChangeValue(MyValue - MyStep);
        }

        #endregion クリックやキー入力での数値加減

        #region テキストボックスの入力制限
        private void MyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            var textbox = (System.Windows.Controls.TextBox)sender;
            string str = textbox.Text;//文字列
            var inputStr = e.Text;//入力された文字

            if(inputStr == "\r") {
                //入力文字がEnterの場合、テキストの確定処理を実行
                decimal value;
                if(decimal.TryParse(ProofreadingInputText(str), out value)) {
                    value = ChangeValue(value);
                    textbox.Text = value.ToString();
                    MyValue = value;
                }
                e.Handled = true;
                return;
            }

            //正規表現で入力文字の判定、数字とピリオド、ハイフンならtrue
            bool neko = new System.Text.RegularExpressions.Regex("[0-9.-]").IsMatch(inputStr);

            //入力文字が数値とピリオド、ハイフン以外だったら無効
            if (neko == false) {
                e.Handled = true;//無効
                return;//終了
            }

            //カーソル位置が先頭(0)じゃないときの、ハイフン入力は無効
            if (textbox.CaretIndex != 0 && inputStr == "-") { e.Handled = true; return; }

            //2つ目のハイフン入力は無効(全選択時なら許可)
            if (textbox.SelectedText != str) {
                if (str.Contains("-") && inputStr == "-") { e.Handled = true; return; }
            }

            //2つ目のピリオド入力は無効
            if (str.Contains(".") && inputStr == ".") { e.Handled = true; return; }
        }

        private void MyTextBox_LostFocus(object sender, RoutedEventArgs e) {
            //数値表現の確認。入力途中のピリオド、ハイフンを削除
            var tb = (System.Windows.Controls.TextBox)sender;
            string text = tb.Text;
            decimal value;
            if (decimal.TryParse(ProofreadingInputText(text), out value)) {
                value = ChangeValue(value);
                tb.Text = value.ToString();
                MyValue = value;
            }
            //e.Handled = true;//無効
        }

        private void MyTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e) {
            //貼り付け無効
            if (e.Command == ApplicationCommands.Paste) {
                e.Handled = true;
            }
        }

        #endregion テキストボックスの入力制限

        //クリックしたときにテキストを全選択
        private void MyTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb.IsFocused == false) {
                tb.Focus();
                e.Handled = true;
            }
        }

        //focusしたときにテキストを全選択
        private void MyTextBox_GotFocus(object sender, RoutedEventArgs e) {
            var tb = sender as System.Windows.Controls.TextBox;
            tb.SelectAll();
        }

        //UserControlの使用先でTextChangerイベントが使用できるように、イベントハンドラを定義する
        public event TextChangedEventHandler TextChanged {
            add {
                MyTextBox.TextChanged += value;
            } remove {
                MyTextBox.TextChanged -= value;
            }
        }
    }

    //テキストボックスの文字列と数値の変換用
    public class MyStringConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            decimal d = (decimal)values[0];//数値
            string s = (string)values[1];//StringFormat
            return d.ToString(s);
        }

        public object[] ConvertBack(object value, Type[] targetTypes
            , object parameter, CultureInfo culture) {
            string f = (string)value;

            //数字とハイフンとピリオドだけ抜き出す
            System.Text.RegularExpressions.MatchCollection ss =
                new System.Text.RegularExpressions.Regex("[0-9.-]").Matches(f);
            string str = "";
            for (int i = 0; i < ss.Count; i++) {
                str += ss[i];
            }
            
            if (str == "")
                str = "0";
            
            if (decimal.TryParse(str, out decimal m) == false) {
                return null;
            }
            return new object[] { m };
        }
    }

}
