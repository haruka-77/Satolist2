﻿using Satolist2.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Satolist2.Control
{
	/// <summary>
	/// SurfacePalette.xaml の相互作用ロジック
	/// </summary>
	public partial class SurfacePalette : UserControl
	{
		internal new SurfacePaletteViewModel DataContext
		{
			get => (SurfacePaletteViewModel)base.DataContext;
			set => base.DataContext = value;
		}

		public SurfacePalette()
		{
			InitializeComponent();
		}

		private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if(sender is ListViewItem listViewItem)
			{
				DataContext.InsertSurfaceToActiveEditor((SurfacePaletteItemViewModel)listViewItem.DataContext);
			}
			e.Handled = true;
		}
	}

	internal class SurfacePaletteViewModel : NotificationObject, IDockingWindowContent
	{
		public const string ContentId = "SurfacePalette";
		private ShellImageCache cache;
		private bool isPreviewDataEnable;
		private ObservableCollection<SurfacePaletteItemViewModel> items;
		public ReadOnlyObservableCollection<SurfacePaletteItemViewModel> Items
		{
			get => new ReadOnlyObservableCollection<SurfacePaletteItemViewModel>(items);
		}
		public MainViewModel Main { get; }
		public ActionCommand GenerateSurfacePreviewCommand { get; }
		public bool IsPreviewDataEnable
		{
			get => isPreviewDataEnable;
			set
			{
				isPreviewDataEnable = value;
				NotifyChanged();
			}
		}

		public SurfacePaletteViewModel(MainViewModel main)
		{
			Main = main;
			items = new ObservableCollection<SurfacePaletteItemViewModel>();


			GenerateSurfacePreviewCommand = new ActionCommand(
				o =>
				{

				}
				);

			if(main.Ghost != null)
			{
				UpdateSurfacePreviewData();
			}
		}

		public void UpdateSurfacePreviewData()
		{
			if(Main.SurfacePreviewData != null)
			{
				items.Clear();
				IsPreviewDataEnable = true;
				cache = new ShellImageCache(DictionaryUtility.ConbinePath(Main.Ghost.FullDictionaryPath, MainViewModel.SurfacePreviewPath));

				foreach (var surface in Main.SurfacePreviewData.Items)
				{
					if (!surface.IsEnableSurfacePalette)
						continue;

					var item = new SurfacePaletteItemViewModel(this, surface, cache)
					{
						OffsetX = surface.OffsetX,
						OffsetY = surface.OffsetY
					};
					items.Add(item);
				}
			}
			else
			{
				IsPreviewDataEnable = false;
				items.Clear();
			}
		}

		public void InsertSurfaceToActiveEditor(SurfacePaletteItemViewModel item)
		{
			var insertStr = string.Format("（{0}）", DictionaryUtility.NumberZen2Han(item.Id.ToString()));
			Main.InsertToActiveEditor(insertStr); ;
		}

		public void InsertSurfaceToActiveEditorSakuraScript(SurfacePaletteItemViewModel item)
		{
			var insertStr = string.Format(@"\s[{0}]", item.Id.ToString());
			Main.InsertToActiveEditor(insertStr);
		}

		public void SendRuntimeChangeSurface(SurfacePaletteItemViewModel item)
		{
			var insertStr = string.Format(@"\p[{0}]\s[{1}]", item.Scope, item.Id.ToString());
			Satorite.SendSSTP(Main.Ghost, insertStr, false, false);
		}


		public string DockingTitle => "サーフェス パレット";

		public string DockingContentId => ContentId;
	}

	internal class SurfacePaletteItemViewModel
	{
		public Bitmap Image
		{
			get => imageCache.LoadImage(FileName).Image;
		}
		private ShellImageCache imageCache;
		private SurfacePaletteViewModel parent;
		public string Label { get; set; }
		public string FileName { get; set; }
		public int OffsetX { get; set; }
		public int OffsetY { get; set; }
		public int SizeX { get; set; }
		public int SizeY { get; set; }
		public long Id { get; set; }
		public int Scope { get; set; }

		public ActionCommand InsertSurfaceCommand { get; }
		public ActionCommand InsertSurfaceCommandSakuraScript { get; }
		public ActionCommand RuntimeChangeSurfaceCommand { get; }

		public SurfacePaletteItemViewModel(SurfacePaletteViewModel parent, Core.SurfacePreviewMetaDataRecord surface, ShellImageCache cache)
		{
			this.parent = parent;
			Id = surface.Id;
			imageCache = cache;
			Label = surface.Label;
			FileName = surface.FileName;
			SizeX = 100;
			SizeY = 100;
			Scope = surface.Scope;

			InsertSurfaceCommand = new ActionCommand(
				o =>
				{
					parent.InsertSurfaceToActiveEditor(this);
				}
				);

			InsertSurfaceCommandSakuraScript = new ActionCommand(
				o =>
				{
					parent.InsertSurfaceToActiveEditorSakuraScript(this);
				}
				);

			RuntimeChangeSurfaceCommand = new ActionCommand(
				o =>
				{
					parent.SendRuntimeChangeSurface(this);
				}
				);
		}
	}

	//SurfacePaletteViewModelの示す情報で画像をクリップ
	internal class SurfacePaletteViewModelToImageSourceConverter : IValueConverter
	{
		private static BitmapImageSourceConverter innerConverter = new BitmapImageSourceConverter();
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is SurfacePaletteItemViewModel vm)
			{
				if (vm.Image != null)
				{
					System.Drawing.Rectangle r = new System.Drawing.Rectangle(vm.OffsetX, vm.OffsetY, vm.SizeX, vm.SizeY);
					var cloneBitmap = vm.Image.Clone(r, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					return innerConverter.Convert(cloneBitmap, targetType, parameter, culture);
				}
				return null;
			}
			throw new NotImplementedException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
