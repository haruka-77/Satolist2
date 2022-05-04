﻿using Satolist2.Model;
using Satolist2.Utility;
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

namespace Satolist2.Dialog
{
	/// <summary>
	/// GeneralSettingsDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class GeneralSettingsDialog : Window
	{
		internal new GeneralSettingsDialogViewModel DataContext
		{
			get => (GeneralSettingsDialogViewModel)base.DataContext;
			set => base.DataContext = value;
		}

		internal GeneralSettingsDialog(MainViewModel main)
		{
			InitializeComponent();
			Owner = main.MainWindow;
			DataContext = new GeneralSettingsDialogViewModel(main, this);
		}

		
	}

	internal class GeneralSettingsDialogViewModel : NotificationObject
	{
		public GeneralSettings Model { get; }
		public GeneralSettingsDialog Dialog { get; }
		public ActionCommand OkCommand { get; }
		public ActionCommand CancelCommand { get; }

		public bool IsChanged
		{
			get => !Model.IsEqlals(MainViewModel.EditorSettings.GeneralSettings);
		}

		public GeneralSettingsDialogViewModel(MainViewModel main, GeneralSettingsDialog dialog)
		{
			Dialog = dialog;
			Model = MainViewModel.EditorSettings.GeneralSettings.Clone();
			dialog.Closing += Dialog_Closing;

			OkCommand = new ActionCommand(
				o =>
				{
					if (IsChanged)
					{
						dialog.DialogResult = true;
					}
					dialog.Close();
				}
				);

			CancelCommand = new ActionCommand(
				o =>
				{
					
					dialog.Close();
				}
				);
		}

		private void Dialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (Dialog.DialogResult.HasValue)
				return;

			if (IsChanged)
			{
				var result = MessageBox.Show("変更を保存せずに閉じてもよろしいですか？", "基本設定", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (result != MessageBoxResult.Yes)
				{
					e.Cancel = true;
					return;
				}
			}
			Dialog.DialogResult = false;
		}
	}
}
