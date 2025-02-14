using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using aggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.IO;
using FastMember;

namespace aggregate.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private DateTime _selectedDate;
        private bool _isProgressRingActive;

        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set { SetProperty(ref _selectedDate, value); }
        }
        public bool IsProgressRingActive
        {
            get { return _isProgressRingActive; }
            set { SetProperty(ref _isProgressRingActive, value); }
        }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SelectedDate = DateTime.Now;

            ExecuteCommand = new DelegateCommand(Execute);
            ExportInvoiceBalanceCommand = new DelegateCommand(ExportInvoiceBalance);

            IsProgressRingActive = false;

        }

        public DelegateCommand ExecuteCommand { get; }
        public DelegateCommand ExportInvoiceBalanceCommand { get; }

        private async void Execute()
        {
            // カスタムダイアログ
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            if (metroWindow == null)
            {
                return;
            }
            var dialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "はい",
                NegativeButtonText = "いいえ",
                AnimateShow = false,
                AnimateHide = false
            };
            var result = await metroWindow.ShowMessageAsync("請求処理を開始します", "よろしいか？", MessageDialogStyle.AffirmativeAndNegative, dialogSettings);
            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }

            IsProgressRingActive = true;

            using (var context = new AppDbContext())
            {
                // パラメータを定義
                var parameter1 = new SqlParameter("@ParameterName1", SelectedDate);

                try
                {
                    context.Database.ExecuteSqlRaw("EXEC usp_aggregate_invoice_balance @ParameterName1", parameter1);
                    // 複数パラメータの例
                    //context.Database.ExecuteSqlRaw("EXEC usp_aggregate_invoice_balance @ParameterName1, @ParameterName2", parameter1, parameter2);
                }
                catch (Exception ex)
                {
                }
            }

            IsProgressRingActive = false;

        }

        private async void ExportInvoiceBalance()
        {
            IsProgressRingActive = true;

            // SQL Server からデータ取得
            var dt = FetchDataFromDatabase("usp_invoice_details", this.SelectedDate);

            // データが0件だったら終了
            if ((dt == null) || (dt.Rows.Count == 0))
            {
                return;
            }

            // CSVファイルを保存
            SaveAsCsvFile(dt, "rakuraku.csv");

            IsProgressRingActive = false;
        }

        private static DataTable FetchDataFromDatabase(string ProcedureName, DateTime ClosedAt)
        {
            using (var context = new AppDbContext())
            {
                var dt = new DataTable();

                // SQLコマンドを準備
                var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = $"EXEC {ProcedureName} @ClosedAt";
                command.CommandType = CommandType.Text;

                // パラメータを追加
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@ClosedAt";
                parameter.Value = ClosedAt;
                command.Parameters.Add(parameter);

                // 接続を開く
                context.Database.OpenConnection();

                // データを読み込む
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                }

                return dt;
            }
        }

        private static void SaveAsCsvFile(DataTable dt, string csvFileName)
        {
            StringBuilder csvContent = new StringBuilder();

            // ヘッダー行を追加
            string[] columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
            csvContent.AppendLine(string.Join(",", columnNames));

            // 行を追加
            foreach (DataRow row in dt.Rows)
            {
                string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                csvContent.AppendLine(string.Join(",", fields));
            }

            // Shift-JISエンコードでCSVファイルに書き出し
            File.WriteAllText(csvFileName, csvContent.ToString(), Encoding.GetEncoding("Shift-JIS"));

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SelectedDate = DateTime.Now;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

    }
}
