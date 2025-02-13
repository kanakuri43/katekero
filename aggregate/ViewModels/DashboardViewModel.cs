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

namespace aggregate.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private DateTime _selectedDate;

        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set { SetProperty(ref _selectedDate, value); }
        }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SelectedDate = DateTime.Now;

            ExecuteCommand = new DelegateCommand(Execute);

        }

        public DelegateCommand ExecuteCommand { get; }

        private async void Execute()
        {
            // カスタムダイアログを表示
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            if (metroWindow == null)
            {
                return;
            }

            // メッセージで確認
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

            using (var context = new AppDbContext())
            {
                // パラメータを定義
                var parameter1 = new SqlParameter("@ParameterName1", "2025/01/31");
                //var parameter2 = new SqlParameter("@ParameterName2", 99);

                try
                {
                    // ストアドプロシージャを実行し、パラメータを渡す
                    //context.Database.ExecuteSqlRaw("EXEC usp_aggregate_invoice_balance @ParameterName1, @ParameterName2", parameter1, parameter2);
                    context.Database.ExecuteSqlRaw("EXEC usp_aggregate_invoice_balance @ParameterName1", parameter1);
                }
                catch (Exception ex)
                {
                    // エラーハンドリング
                    Console.WriteLine("エラーが発生しました: " + ex.Message);
                }
            }

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
