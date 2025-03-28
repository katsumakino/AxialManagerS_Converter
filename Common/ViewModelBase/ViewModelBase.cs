using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace AxialManagerS_Converter.Common.ViewModelBase {
    internal class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo {
        #region ■内部クラス

        // ********************************************************************
        /// <summary>
        /// プロパティ値変更イベント・パラメータ
        /// </summary>
        protected class _propertyValueChangeEventArg {
            #region ■ コンストラクタ

            // ================================================================
            /// <summary>
            /// (+) コンストラクタ
            /// </summary>
            /// <param name="old_value">object : 変更前プロパティ値</param>
            /// <param name="new_value">object : 変更後プロパティ値</param>
            public _propertyValueChangeEventArg(object old_value, object new_value) {
                OldValue = old_value;
                NewValue = new_value;
                Cancel = false;
            }

            #endregion

            #region ■ プロパティ

            // ================================================================
            /// <summary>
            /// (+) キャンセルフラグ
            /// 
            /// true にセットするとプロパティ値の変更をキャンセルするx
            /// </summary>
            public bool Cancel { get; set; }

            // ================================================================
            /// <summary>
            /// (+-) 変更前プロパティ値
            /// </summary>
            public object OldValue { get; private set; }

            // ================================================================
            /// <summary>
            /// (+-) 変更後プロパティ値
            /// </summary>
            public object NewValue { get; private set; }

            #endregion
        }

        // ********************************************************************
        /// <summary>
        /// アラート表示通知イベント・パラメータ
        /// </summary>
        public class AlertEventArg {
            #region ■ コンストラクタ

            // ================================================================
            /// <summary>
            /// (+) コンストラクタ
            /// </summary>
            /// <param name="msg"></param>
            public AlertEventArg(string msg) {
                Message = msg;
            }

            #endregion

            #region ■ プロパティ

            // ================================================================
            /// <summary>
            /// (+:RO) メッセージ
            /// </summary>
            public string Message { get; private set; }

            #endregion
        }

        // ********************************************************************
        /// <summary>
        /// メッセージ表示通知イベント・パラメータ
        /// </summary>
        public class ShowMessageEventArg {
            #region ■ コンストラクタ

            // ================================================================
            /// <summary>
            /// (+) コンストラクタ
            /// </summary>
            /// <param name="msg">string : メッセージ</param>
            /// <param name="caption">string : キャプション</param>
            /// <param name="btns">MessageBoxButton : ボタン</param>
            public ShowMessageEventArg(string msg, string caption, MessageBoxButton btns) {
                Message = msg;
                Caption = caption;
                Buttons = btns;
                Result = MessageBoxResult.None;
            }

            #endregion

            #region ■ プロパティ

            // ================================================================
            /// <summary>
            /// (+:RO) メッセージ
            /// </summary>
            public string Message { get; private set; }

            // ================================================================
            /// <summary>
            /// (+:RO) キャプション
            /// </summary>
            public string Caption { get; private set; }

            // ================================================================
            /// <summary>
            /// (+:RO) ボタン
            /// </summary>
            public MessageBoxButton Buttons { get; private set; }

            // ================================================================
            /// <summary>
            /// (+) 結果
            /// </summary>
            public MessageBoxResult Result { get; set; }

            #endregion
        }

        // ********************************************************************
        /// <summary>
        /// View クローズ要請通知イベント・パラメータ
        /// </summary>
        public class CloseViewEventArg {
            #region ■ コンストラクタ

            // ================================================================
            /// <summary>
            /// (+) コンストラクタ
            /// </summary>
            public CloseViewEventArg() {
                Result = true;
            }

            #endregion

            #region ■ プロパティ

            // ================================================================
            /// <summary>
            /// (+) 戻り値(Viewが閉じられた場合は True を返す)
            /// </summary>
            public bool Result { get; set; }

            #endregion
        }

        // ********************************************************************
        /// <summary>
        /// バリデーションチェック関数用のリザルトクラス
        /// </summary>
        public class ValidationCheckResult {
            #region ■ コンストラクタ

            // ================================================================
            /// <summary>
            /// (+) コンストラクタ
            /// </summary>
            /// <param name="result">bool : リザルトコード</param>
            /// <param name="message">string : メッセージ</param>
            public ValidationCheckResult(bool result, string message) {
                Result = result;
                Message = message;
            }

            #endregion

            #region ■ プロパティ

            // ================================================================
            /// <summary>
            /// (+) リザルトコード
            /// </summary>
            public bool Result { get; set; }

            // ================================================================
            /// <summary>
            /// (+) メッセージ
            /// </summary>
            public string Message { get; set; }

            #endregion
        }
        // ********************************************************************
        /// <summary>
        /// プロパティ基底クラス
        /// </summary>
        protected abstract class _propetryBase {
            #region ■ メンバ変数

            // ================================================================
            /// <summary>
            /// (-) 内部変数
            /// </summary>
            protected ViewModelBase _vm;            // ビューモデルへの参照

            #endregion

            #region ■ イベント・デレゲート

            // ================================================================
            /// <summary>
            /// (+) 値変更前に発生するイベント
            /// </summary>
            public event EventHandler<_propertyValueChangeEventArg> OnValueBeforeChange;

            // ================================================================
            /// <summary>
            /// (+) 値変更後に発生するイベント
            /// </summary>
            public event EventHandler OnValueChanged;

            #endregion

            #region ■ コンストラクタ

            // ================================================================
            /// <summary>
            /// (+) コンストラクタ
            /// </summary>
            public _propetryBase(string property_name, ViewModelBase vm) {
                PropertyName = property_name;
                _vm = vm;

                ClearError();
                IsDirty = false;
            }

            #endregion

            #region ■ プロパティ

            // ================================================================
            /// <summary>
            /// (+-) プロパティ名称
            /// </summary>
            public string PropertyName { get; protected set; }

            // ================================================================
            /// <summary>
            /// (+-) 値が変更されている
            /// </summary>
            public bool IsDirty { get; protected set; }

            #endregion

            #region ■ メソッド

            // ================================================================
            /// <summary>
            /// (+:A) 値の型を取得する
            /// </summary>
            /// <returns></returns>
            public abstract Type GetValueType();


            // ================================================================
            /// <summary>
            /// (+:A) 値を初期化する
            /// </summary>
            public abstract void Init();

            // ================================================================
            /// <summary>
            /// (+:A) 値を取得する
            /// </summary>
            /// <returns>object</returns>
            public abstract object GetValue();

            // ================================================================
            /// <summary>
            /// (+:A) 値を設定する
            /// </summary>
            /// <param name="value">object : 設定する値</param>
            public abstract void SetValue(object value);

            // ================================================================
            /// <summary>
            /// (-) OnValueChange イベントを発生させる
            /// 
            /// 派生クラスからプロパティ値が変更されるときに呼び出される。
            /// </summary>
            /// <param name="sender">_propetryBase : プロパティオブジェクト</param>
            /// <param name="old_value">object : 変更前プロパティ値</param>
            /// <param name="new_value">object : 変更後プロパティ値</param>
            /// <returns></returns>
            protected bool ValueBeforeChange(object old_value, object new_value) {
                if (OnValueBeforeChange != null) {
                    _propertyValueChangeEventArg e = new _propertyValueChangeEventArg(old_value, new_value);
                    OnValueBeforeChange(this, e);
                    return !e.Cancel;
                } else
                    return true;
            }

            // ================================================================
            /// <summary>
            /// (-) OnValueChanged イベントを発生させる
            /// 
            /// 派生クラスからプロパティ値が変更された後に呼び出される。
            /// </summary>
            protected void ValueChanged() {
                if (OnValueChanged != null)
                    OnValueChanged(this, EventArgs.Empty);
            }

            // ================================================================
            /// <summary>
            /// (-) バリデーションチェック
            /// </summary>
            /// <returns>bool</returns>
            protected abstract bool ValidationCheck();

            // ================================================================
            /// <summary>
            /// (+) エラーをセットする
            /// 
            /// エラーメッセージにnullか空文字列が指定された場合はエラーを削除
            /// </summary>
            /// <param name="err_message">string : エラーメッセージ</param>
            public void SetError(string err_message) {
                if (string.IsNullOrEmpty(err_message)) {
                    if (_vm.Errors.ContainsKey(PropertyName))
                        _vm.Errors.Remove(PropertyName);
                } else
                    _vm.Errors[PropertyName] = err_message;
            }

            // =================================================================
            /// <summary>
            /// (+) エラーを解除する
            /// </summary>
            public void ClearError() {
                SetError(null);
            }

            // =================================================================
            /// <summary>
            /// (+) エラーかどうかを返す
            /// </summary>
            public bool HasError {
                get { return _vm.Errors.ContainsKey(PropertyName); }
            }

            // ================================================================
            /// <summary>
            /// (+) エラーメッセージを取得する
            /// </summary>
            public string ErrorMessage {
                get { return HasError ? _vm.Errors[PropertyName] : ""; }
                protected set { SetError(value); }
            }

            #endregion

        }

        // ********************************************************************
        /// <summary>
        /// 汎用プロパティクラス
        /// </summary>
        /// <typeparam name="T">T : プロパティの型</typeparam>
        protected class _property<T> : _propetryBase {
            #region ■ メンバ変数

            // ================================================================
            /// <summary>
            /// (-) 内部変数定義
            /// </summary>
            private T _initial_value;   // 初期値
            private T _value;           // 値を保持する内部変数

            #endregion

            #region ■ イベント・デレゲート

            // ================================================================
            /// <summary>
            /// (+) 値取得のデレゲート
            /// </summary>
            public Func<T> GetValueFunc = null;

            // ================================================================
            /// <summary>
            /// (+) 値設定のデレゲート
            /// </summary>
            public Action<T> SetValueFunc = null;

            // ================================================================
            /// <summary>
            /// (+) バリデーションチェック関数のデレゲート
            /// </summary>
            public Func<T, ValidationCheckResult> ValidationCheckFunc = null;

            #endregion

            #region ■ コンストラクタ

            // ================================================================
            /// <summary>
            /// (+) コンストラクタ
            /// </summary>
            /// <param name="name">string : プロパティ名称</param>
            /// <param name="vm">ViewModelBase : ビューモデル</param>
            public _property(string name, ViewModelBase vm)
                : base(name, vm) {
                // ビューモデルのプロパティ値辞書に登録する
                vm.Properties.Add(PropertyName, this);
            }

            // ================================================================
            /// <summary>
            /// (+) コンストラクタ
            /// </summary>
            /// <param name="name">string : プロパティ名称</param>
            /// <param name="v">T : 初期値</param>
            /// <param name="vm">ViewModelBase : ビューモデル</param>
            public _property(string name, T v, ViewModelBase vm)
                : this(name, vm) {
                // 初期値を保存しておく
                _initial_value = v;
                Init(v);
            }

            #endregion

            #region ■ プロパティ

            // ================================================================
            /// <summary>
            /// (+) 値プロパティ
            /// </summary>
            public T Value {
                get { return _GetValue(); }
                set { _SetValue(value); }
            }

            #endregion

            #region ■ メソッド

            // ================================================================
            /// <summary>
            /// (+:OW) 値の型を取得する
            /// </summary>
            /// <returns>Type</returns>
            public override Type GetValueType() {
                return typeof(T);
            }

            // ================================================================
            /// <summary>
            /// (-) 値を取得する
            /// </summary>
            /// <returns>T : 値</returns>
            private T _GetValue() {
                if (GetValueFunc != null)
                    return GetValueFunc();
                else
                    return _value;
            }

            // ================================================================
            /// <summary>
            /// (+:OW) 値を取得する
            /// </summary>
            /// <returns>object</returns>
            public override object GetValue() {
                return _GetValue();
            }

            // ================================================================
            /// <summary>
            /// (+) 値を初期化する
            /// </summary>
            /// <param name="v">T : 初期値</param>
            public void Init(T v) {
                if (SetValueFunc != null) {
                    try { SetValueFunc(v); } catch (Exception exp) { SetError(exp.Message); }
                } else
                    _value = v;
                IsDirty = false;
                _vm.RaisePropertyChanged(PropertyName);
            }

            // ================================================================
            /// <summary>
            /// (+:OW) 値を初期化する
            /// </summary>
            public override void Init() {
                Init(_initial_value);
            }

            // ================================================================
            /// <summary>
            /// (-) 与えられた引数と値が等しい
            /// 
            /// 値がIComparableを実装していない場合、「値」ではなく参照先の等価判定となる
            /// </summary>
            /// <param name="v">T : 比較する値</param>
            /// <returns>bool</returns>
            private bool Equals(T v) {
                T value = _GetValue();
                if (value is IComparable)
                    return (value as IComparable).CompareTo(v) == 0;
                else {
                    return (object)value == (object)v;
                }
            }

            // ================================================================
            /// <summary>
            /// (+) 値を設定する
            /// </summary>
            /// <param name="v">T : 設定する値</param>
            private void _SetValue(T v) {
                if (!Equals(v)) {
                    if (ValueBeforeChange(_GetValue(), v)) {
                        ClearError();
                        if (SetValueFunc != null) {
                            try { SetValueFunc(v); } catch (Exception exp) { SetError(exp.Message); }
                        } else
                            _value = v;
                        IsDirty = true;
                        ValidationCheck();
                        ValueChanged();
                        _vm.RaisePropertyChanged(PropertyName);
                    }
                }
            }

            // ================================================================
            /// <summary>
            /// (+:OW) 値を設定する
            /// 
            /// ※ 値をTにキャスト不可の場合は例外が発生する
            /// </summary>
            /// <param name="value">object : 設定する値</param>
            public override void SetValue(object value) {
                _SetValue((T)value);
            }

            // ================================================================
            /// <summary>
            /// (+) 値アクセス用のデレゲートを設定する
            /// </summary>
            /// <param name="set_v">Action : 値設定デレゲート</param>
            /// <param name="get_v">Func : 値参照デレゲート</param>
            public _property<T> SetDelegate(Action<T> set_v, Func<T> get_v) {
                SetValueFunc = set_v;
                GetValueFunc = get_v;
                return this;
            }

            // ================================================================
            /// <summary>
            /// (-) バリデーションチェック
            /// </summary>
            /// <returns>bool</returns>
            protected override bool ValidationCheck() {
                if (ValidationCheckFunc != null) {
                    ValidationCheckResult result = ValidationCheckFunc(_GetValue());
                    if (!result.Result)
                        SetError(result.Message);
                    return result.Result;
                } else
                    return true;
            }

            // ================================================================
            /// <summary>
            /// (+) 値が妥当かチェックする
            /// </summary>
            /// <param name="value">T : 検証する値</param>
            /// <returns>bool</returns>
            public bool IsValid(T value) {
                if (ValidationCheckFunc != null)
                    return ValidationCheckFunc(value).Result;
                else
                    return true;
            }

            #endregion
        }

        // ********************************************************************
        /// <summary>
        /// 汎用コマンドクラス
        /// 
        /// ICommand を実装するクラス
        /// </summary>
        public class _delegateCommand : ICommand {
            #region ■ メンバ変数

            // =====================================================================
            /// <summary>
            /// 内部利用
            /// </summary>
            private Action _execute;                        // 引数なしのコマンド本体
            private Action<object> _execute_with_param;     // 引数ありのコマンド本体
            private Func<bool> _can_execute;                // 実行可否

            public Action<object> Value { get; }

            #endregion

            #region ■ コンストラクタ

            // ================================================================
            /// <summary>
            /// (+) コンストラクタ
            /// </summary>
            /// <param name="execute">Executeメソッドで実行する処理</param>
            /// <param name="can_execute">CanExecuteメソッドで実行する処理</param>
            public _delegateCommand(Action execute, Func<bool> can_execute) {
                if (execute == null || can_execute == null)
                    throw new ArgumentNullException();

                _execute = execute;
                _execute_with_param = null;
                _can_execute = can_execute;
            }

            // ================================================================
            /// <summary>
            /// (+) コンストラクタ
            /// </summary>
            /// <param name="execute">Executeメソッドで実行する処理（引数つき)</param>
            /// <param name="can_execute">CanExecuteメソッドで実行する処理</param>
            public _delegateCommand(Action<object> execute, Func<bool> can_execute) {
                if (execute == null || can_execute == null)
                    throw new ArgumentNullException();

                _execute = null;
                _execute_with_param = execute;
                _can_execute = can_execute;
            }

            public _delegateCommand(Action<object> value) {
                Value = value;
            }

            #endregion

            #region ■ メソッド

            // ================================================================
            /// <summary>
            /// ICommand.Executeの実装。
            /// 
            /// コマンドを実行します。
            /// <param name="parameter">object : パラメータ</param>
            /// </summary>
            public void Execute(object parameter = null) {
                if (_execute_with_param != null)
                    _execute_with_param(parameter);
                else
                    _execute();
            }

            // ================================================================
            /// <summary>
            /// ICommand.CanExecuteの実装。
            /// 
            /// コマンドが実行可能か返す
            /// </summary>
            /// <param name="parameter">object : パラメータ</param>
            /// <returns>bool</returns>
            public bool CanExecute(object parameter) {
                return _can_execute();
            }

            // ================================================================
            /// <summary>
            /// ICommand.CanExecuteChanged の実装。
            /// 
            /// CanExecuteの結果に変更があったことを通知するイベント。
            /// </summary>
            public event EventHandler CanExecuteChanged {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            #endregion
        }


        #endregion

        #region ■ コンストラクタ

        // ====================================================================
        /// <summary>
        /// (+) コンストラクタ
        /// </summary>
        public ViewModelBase() {
            // プロパティ値保持用の辞書を生成
            Properties = new Dictionary<string, _propetryBase>();

            // エラー保持用の辞書を生成
            Errors = new Dictionary<string, string>();
        }

        #endregion

        #region ■ イベント

        // ====================================================================
        /// <summary>
        /// (+) プロパティ変更通知イベント
        /// 
        /// INotifyPropertyChanged 実装用
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        // ====================================================================
        /// <summary>
        /// (+) アラート表示要請通知イベント
        /// </summary>
        public event EventHandler<AlertEventArg> OnShowAlert;

        // ====================================================================
        /// <summary>
        /// (+) メッセージ表示要請通知イベント
        /// </summary>
        public event EventHandler<ShowMessageEventArg> OnShowMessage;

        // ====================================================================
        /// <summary>
        /// (+) Viewクローズ要請通知イベント
        /// </summary>
        public event EventHandler<CloseViewEventArg> OnCloseView;

        #endregion

        #region ■ プロパティ

        // ====================================================================
        /// <summary>
        /// (-) プロパティ値を保持する辞書
        /// </summary>
        protected Dictionary<string, _propetryBase> Properties { get; private set; }

        // ====================================================================
        /// <summary>
        /// (-) プロパティ毎のエラーメッセージを保持する辞書
        /// </summary>
        protected Dictionary<string, string> Errors { get; set; }

        // ====================================================================
        /// <summary>
        /// (-) エラーが存在する？
        /// </summary>
        protected bool HasError {
            get { return Errors.Count > 0; }
        }

        // =====================================================================
        /// <summary>
        /// (+) IDataErrorInfo.Error の実装
        /// </summary>
        public string Error {
            get {
                if (Errors.Count > 0)
                    return "Has Error";
                else
                    return null;
            }
        }

        // =====================================================================
        /// <summary>
        /// (+) IDataErrorInfo.item の実装
        /// </summary>
        /// <param name="columnName">string : カラム名（プロパティ名称）</param>
        public string this[string columnName] {
            get {
                if (Errors.ContainsKey(columnName))
                    return Errors[columnName];
                else
                    return null;
            }
        }

        #endregion

        #region ■ メソッド

        // ====================================================================
        /// <summary>
        /// (-) PropertyChanged イベントを発生させる
        /// </summary>
        /// <param name="propertyName">string : プロパティ名称</param>
        protected virtual void RaisePropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // ====================================================================
        /// <summary>
        /// (-) プロパティの生成 
        /// </summary>
        /// <param name="property_name">string : プロパティ名称</param>
        /// <returns>_property</returns>
        protected _property<T> CreateProperty<T>(string property_name) {
            return new _property<T>(property_name, this);
        }

        // ====================================================================
        /// <summary>
        /// (-) プロパティの生成 
        /// </summary>
        /// <param name="property_name">string : プロパティ名称</param>
        /// <param name="initial_value">int : 初期値</param>
        /// <returns>_property</returns>
        protected _property<T> CreateProperty<T>(string property_name, T initial_value) {
            return new _property<T>(property_name, initial_value, this);
        }

        // ====================================================================
        /// <summary>
        /// (-) コマンドの生成
        /// </summary>
        /// <param name="execute">Action : コマンド本体のデレゲート</param>
        /// <param name="can_execute">Func : コマンドの実行可否を返すデレゲート</param>
        /// <returns>ICommand</returns>
        protected ICommand CreateCommand(Action execute, Func<bool> can_execute) {
            return new _delegateCommand(execute, can_execute) as ICommand;
        }

        // ====================================================================
        /// <summary>
        /// (-) コマンドの生成
        /// </summary>
        /// <param name="execute">Action : コマンド本体のデレゲート</param>
        /// <param name="can_execute">Func : コマンドの実行可否を返すデレゲート</param>
        /// <returns>ICommand</returns>
        protected ICommand CreateCommand(Action<object> execute, Func<bool> can_execute) {
            return new _delegateCommand(execute, can_execute) as ICommand;
        }

        // ====================================================================
        /// <summary>
        /// (-) コマンドの生成
        /// </summary>
        /// <param name="execute">Action : コマンド本体のデレゲート</param>
        /// <returns>ICommand</returns>
        protected ICommand CreateCommand(Action execute) {
            return new _delegateCommand(execute, () => { return true; }) as ICommand;
        }

        // ====================================================================
        /// <summary>
        /// (-) コマンドの生成
        /// </summary>
        /// <param name="execute">Action : コマンド本体のデレゲート</param>
        /// <returns>ICommand</returns>
        protected ICommand CreateCommand(Action<object> execute) {
            return new _delegateCommand(execute, () => { return true; }) as ICommand;
        }

        // ====================================================================
        /// <summary>
        /// (-) アラートを表示する
        /// </summary>
        /// <param name="msg"></param>
        protected void ShowAlert(string msg) {
            if (OnShowAlert != null)
                OnShowAlert(this, new AlertEventArg(msg));
        }

        // ====================================================================
        /// <summary>
        /// (-) メッセージを表示する
        /// </summary>
        /// <param name="msg">string : メッセージ</param>
        /// <param name="caption">string : キャプション</param>
        /// <param name="btns">MessageBoxButton : ボタン</param>
        /// <returns>MessageBoxResult</returns>
        protected MessageBoxResult ShowMessage(string msg, string caption = "", MessageBoxButton btns = MessageBoxButton.YesNo) {
            if (OnShowMessage != null) {
                ShowMessageEventArg e = new ShowMessageEventArg(msg, caption, btns);
                OnShowMessage(this, e);
                return e.Result;
            } else
                return MessageBoxResult.None;
        }

        // ====================================================================
        /// <summary>
        /// (-) メッセージを表示しユーザーに確認を促す
        /// </summary>
        /// <param name="msg">string : メッセージ</param>
        /// <param name="caption">string : キャプション</param>
        /// <param name="btns">MessageBoxButton : ボタン</param>
        /// <returns>bool</returns>
        protected bool Confirm(string msg, string caption = "", MessageBoxButton btns = MessageBoxButton.YesNo) {
            if (OnShowMessage != null) {
                ShowMessageEventArg e = new ShowMessageEventArg(msg, caption, btns);
                OnShowMessage(this, e);
                return e.Result == MessageBoxResult.OK || e.Result == MessageBoxResult.Yes;
            } else
                return false;
        }

        // ====================================================================
        /// <summary>
        /// (-) View を閉じる
        /// </summary>
        /// <returns></returns>
        protected bool CloseView() {
            if (OnCloseView != null) {
                CloseViewEventArg e = new CloseViewEventArg();
                OnCloseView(this, e);
                return e.Result;
            } else
                return false;
        }

        // ====================================================================
        /// <summary>
        /// (-) プロパティ値を初期化する
        /// </summary>
        protected void InitProperties() {
            foreach (_propetryBase prop in Properties.Values)
                prop.Init();
        }

        // ====================================================================
        /// <summary>
        /// (-) すべてのエラーを解除する
        /// </summary>
        protected void ClearError() {
            Errors.Clear();
            foreach (string prop_name in Properties.Keys)
                RaisePropertyChanged(prop_name);
        }

        #endregion
    }
}
