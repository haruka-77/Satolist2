﻿using Satolist2.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satolist2.Core
{
	internal class LogMessage : NotificationObject
	{
		public static readonly LogMessage Instance = new LogMessage();
		private ObservableCollection<LogMessageItemViewModel> logItems;
		public ReadOnlyObservableCollection<LogMessageItemViewModel> LogItems
		{
			get => new ReadOnlyObservableCollection<LogMessageItemViewModel>(logItems);
		}
		public LogMessageItemViewModel NewestLogMessage
		{
			get
			{
				return logItems.LastOrDefault();
			}
		}

		private LogMessage()
		{
			logItems = new ObservableCollection<LogMessageItemViewModel>();
			SetVersionInfo();
		}

		public static void AddLog(LogMessageItemViewModel log)
		{
			Instance.logItems.Add(log);
			Instance.NotifyChanged(nameof(NewestLogMessage));
		}

		public static void AddLog(string message, LogMessageType type = LogMessageType.Notice)
		{
			AddLog(new LogMessageItemViewModel(message, type));
		}

		private void AddLogInternal(string message, LogMessageType type = LogMessageType.Notice)
		{
			logItems.Add(new LogMessageItemViewModel(message, type));
			NotifyChanged(nameof(NewestLogMessage));
		}

		public static void ClearLog()
		{
			Instance.logItems.Clear();
			Instance.SetVersionInfo();
		}

		private void SetVersionInfo()
		{
			try
			{
				if(System.IO.File.Exists("version.txt"))
					AddLogInternal(System.IO.File.ReadAllText("version.txt"));
				else
					AddLogInternal("Satolist2 (LocalBuild)");
			}
			catch { }
		}
	}

	internal enum LogMessageType
	{
		Notice,
		Error
	}

	internal class LogMessageItemViewModel : NotificationObject
	{
		private string message;
		private LogMessageType type;

		public string Message
		{
			get => message;
			set
			{
				message = value;
				NotifyChanged();
			}
		}

		public LogMessageType Type
		{
			get => type;
			set
			{
				type = value;
				NotifyChanged();
			}
		}

		public LogMessageItemViewModel(string message, LogMessageType type = LogMessageType.Notice)
		{
			Message = message;
			Type = type;
		}
	}
}
