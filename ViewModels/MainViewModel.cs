using OrderViewApp.Models;
using OrderViewApp.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OrderViewApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // UIがデータの変更を自動で検知できるように ObservableCollection を使う
        public ObservableCollection<Order> Orders { get; set; }

        // 全データを保持（フィルタリング前）
        private List<Order> _allOrders = new List<Order>();

        private readonly OrderService? _orderService;

        // 期限の優先順位マッピング（数値が小さいほど優先度が高い）
        private static readonly Dictionary<string, int> ExpiryDatePriority = new Dictionary<string, int>
        {
            { "至急", 1 },
            { "でき次第", 2 },
            { "朝一", 3 },
            { "朝２", 4 },
            { "６時", 5 },
            { "７時", 6 },
            { "８時", 7 },
            { "９時", 8 },
            { "１０時", 9 },
            { "１１時", 10 },
            { "１２時", 11 },
            { "午前中", 12 },
            { "昼一", 13 },
            { "午後１時", 14 },
            { "昼２", 15 },
            { "午後２時", 16 },
            { "午後３時", 17 },
            { "午後４時", 18 },
            { "午後５時", 19 },
            { "午後", 20 },
            { "今日中", 21 },
            { "午後６時", 22 },
            { "午後７時", 23 },
            { "午後８時", 24 },
            { "連絡待ち", 25 },
            { "その他", 26 },
            { "未入力", 27 }
        };

        // 期限文字列を正規化するメソッド（空白除去のみ）
        // 期限と期限2は分離されているため、装飾文字（「頃」など）は含まれない
        private static string NormalizeExpiryDate(string? expiryDate)
        {
            if (string.IsNullOrEmpty(expiryDate))
            {
                return string.Empty;
            }

            // 前後の空白を除去
            return expiryDate.Trim();
        }

        // 期限の優先順位を取得するメソッド（完全一致のみ）
        private static int GetExpiryDatePriority(string? expiryDate)
        {
            if (string.IsNullOrEmpty(expiryDate))
            {
                return ExpiryDatePriority["未入力"];
            }

            // 空白を除去して正規化
            var normalized = NormalizeExpiryDate(expiryDate);

            // 完全一致を確認
            if (ExpiryDatePriority.ContainsKey(normalized))
            {
                return ExpiryDatePriority[normalized];
            }

            // 完全一致しない場合は最後に配置
            return int.MaxValue;
        }

        // 期間フィルタリング用のプロパティ
        private PeriodFilterType _selectedPeriodFilter = PeriodFilterType.Today;
        public PeriodFilterType SelectedPeriodFilter
        {
            get => _selectedPeriodFilter;
            set
            {
                if (_selectedPeriodFilter != value)
                {
                    _selectedPeriodFilter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCustomPeriodVisible));
                    FilterOrders();
                }
            }
        }

        private DateTime _customStartDate = DateTime.Today;
        public DateTime CustomStartDate
        {
            get => _customStartDate;
            set
            {
                if (_customStartDate != value)
                {
                    _customStartDate = value;
                    OnPropertyChanged();
                    if (SelectedPeriodFilter == PeriodFilterType.Custom)
                    {
                        FilterOrders();
                    }
                }
            }
        }

        private DateTime _customEndDate = DateTime.Today;
        public DateTime CustomEndDate
        {
            get => _customEndDate;
            set
            {
                if (_customEndDate != value)
                {
                    _customEndDate = value;
                    OnPropertyChanged();
                    if (SelectedPeriodFilter == PeriodFilterType.Custom)
                    {
                        FilterOrders();
                    }
                }
            }
        }

        public bool IsCustomPeriodVisible => SelectedPeriodFilter == PeriodFilterType.Custom;

        // 表示モードフィルタリング用のプロパティ
        private DisplayModeType _selectedDisplayMode = DisplayModeType.Incomplete;
        public DisplayModeType SelectedDisplayMode
        {
            get => _selectedDisplayMode;
            set
            {
                if (_selectedDisplayMode != value)
                {
                    _selectedDisplayMode = value;
                    OnPropertyChanged();
                    FilterOrders();
                }
            }
        }

        // 背景色表示切り替え用のプロパティ
        private bool _isBackgroundColorEnabled = true;
        public bool IsBackgroundColorEnabled
        {
            get => _isBackgroundColorEnabled;
            set
            {
                if (_isBackgroundColorEnabled != value)
                {
                    _isBackgroundColorEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainViewModel()
        {
            Orders = new ObservableCollection<Order>();
            
            try
            {
                // 接続文字列を取得してOrderServiceを初期化
                var connectionString = App.GetConnectionString();
                _orderService = new OrderService(connectionString);
                
                // データ読み込みは画面表示後に開始するため、ここでは開始しない
            }
            catch (Exception ex)
            {
                // 初期化時のエラー（接続文字列の取得失敗など）を処理
                System.Diagnostics.Debug.WriteLine($"初期化エラー: {ex}");
                // UIスレッドでエラーメッセージを表示
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show(
                            $"初期化中にエラーが発生しました:\n{ex.Message}",
                            "エラー",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }));
                }
            }
        }

        public async Task LoadOrdersAsync()
        {
            if (_orderService == null)
            {
                return;
            }

            try
            {
                var orders = await _orderService.GetOrdersAsync();
                
                // UIスレッドでObservableCollectionを更新
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher != null)
                {
                    // 既にUIスレッドの場合はInvoke、そうでない場合はBeginInvokeを使用
                    if (dispatcher.CheckAccess())
                    {
                        UpdateOrders(orders);
                    }
                    else
                    {
                        _ = dispatcher.BeginInvoke(new Action(() => UpdateOrders(orders)));
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // SQL Server固有のエラーを処理
                System.Diagnostics.Debug.WriteLine($"データベースエラー: {sqlEx}");
                ShowError($"データベース接続エラーが発生しました:\n{sqlEx.Message}\n\n接続文字列を確認してください。");
            }
            catch (Exception ex)
            {
                // その他のエラーを処理
                System.Diagnostics.Debug.WriteLine($"予期しないエラー: {ex}");
                ShowError($"データベースからのデータ取得中にエラーが発生しました:\n{ex.Message}");
            }
        }

        private void UpdateOrders(List<Order> orders)
        {
            // 全データを保存
            _allOrders = orders;
            
            // フィルタリングを実行
            FilterOrders();
        }

        private void ShowError(string message)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null)
            {
                if (dispatcher.CheckAccess())
                {
                    MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                }
            }
        }

        // リロードコマンド
        private ICommand? _reloadCommand;
        public ICommand ReloadCommand
        {
            get
            {
                if (_reloadCommand == null)
                {
                    _reloadCommand = new RelayCommand(_ => _ = LoadOrdersAsync());
                }
                return _reloadCommand;
            }
        }

        // シンプルなRelayCommand実装
        private class RelayCommand : ICommand
        {
            private readonly Action<object?> _execute;
            private readonly Func<object?, bool>? _canExecute;

            public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler? CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object? parameter)
            {
                return _canExecute == null || _canExecute(parameter);
            }

            public void Execute(object? parameter)
            {
                _execute(parameter);
            }
        }

        private void FilterOrders()
        {
            if (_allOrders == null || _allOrders.Count == 0)
            {
                Orders.Clear();
                return;
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // 期間フィルターを適用
            IEnumerable<Order> filteredOrders = _selectedPeriodFilter switch
            {
                PeriodFilterType.Today => _allOrders.Where(o => o.DueDate.Date == today),
                PeriodFilterType.Tomorrow => _allOrders.Where(o => o.DueDate.Date == tomorrow),
                PeriodFilterType.TodayAndTomorrow => _allOrders.Where(o => o.DueDate.Date == today || o.DueDate.Date == tomorrow),
                PeriodFilterType.Custom => _allOrders.Where(o => o.DueDate.Date >= CustomStartDate.Date && o.DueDate.Date <= CustomEndDate.Date),
                _ => _allOrders
            };

            // 表示モードフィルターを適用
            filteredOrders = _selectedDisplayMode switch
            {
                DisplayModeType.Incomplete => filteredOrders.Where(o => (o.Preparer ?? 0) == 0),
                DisplayModeType.Completed => filteredOrders.Where(o => (o.Preparer ?? 0) != 0),
                DisplayModeType.All => filteredOrders,
                _ => filteredOrders
            };

            // 用件フィルターを適用（「<配達>」、「来店(納品)」、「交換」のみ表示）
            filteredOrders = filteredOrders.Where(o => 
                o.Requirements == "<配達>" || 
                o.Requirements == "来店(納品)" || 
                o.Requirements == "交換"
            );

            // 配達用件の出発フィルターを適用（用件が「配達」の場合、出発が0のデータのみ表示）
            filteredOrders = filteredOrders.Where(o => 
                o.Requirements != "<配達>" || 
                (o.Departure == false || o.Departure == null)
            );

            // 並び替え処理：受付番号でグループ化してから、期日と期限で並び替え
            // 同じ受付番号のOrderは期日と期限が同じため、グループの代表値でソート
            // ソートには期限基本（ExpiryDate）を使用（ExpiryDateは期限基本のみ、ExpiryDate2は期限2）
            var groupedOrders = filteredOrders
                .GroupBy(o => o.ReceptionNumber)
                .OrderBy(g => g.First().DueDate)
                .ThenBy(g => GetExpiryDatePriority(g.First().ExpiryDate))
                .ToList(); // グループを保持（SelectManyではなくToList）

            // グループごとに色インデックスを割り当て（グループ内のすべてのOrderに同じ色を設定）
            int colorIndex = 0; // 最初のグループは0（水色）から始める
            foreach (var group in groupedOrders)
            {
                // グループ内のすべてのOrderオブジェクトに同じ色インデックスを設定
                foreach (var order in group)
                {
                    order.GroupColorIndex = colorIndex;
                }
                // 次のグループは交互の色にする（0→1→0→1...）
                colorIndex = (colorIndex + 1) % 2;
            }

            // グループを展開してOrdersに追加（パフォーマンス最適化：一度にクリアしてから一括追加）
            Orders.Clear();
            // 全アイテムを一度に追加するため、先にリストに集約
            var itemsToAdd = new List<Order>();
            foreach (var group in groupedOrders)
            {
                itemsToAdd.AddRange(group);
            }
            // 一括追加（AddRangeはObservableCollectionにないため、個別に追加するが事前にリスト化することで最適化）
            foreach (var order in itemsToAdd)
            {
                Orders.Add(order);
            }
        }
    }
}
