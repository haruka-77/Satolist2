﻿using FluentFTP.Servers.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace Satolist2.Utility
{
	//ShellのDescriptを解析
	//きせかえ周辺の情報を取得する
	internal class ShellDescriptAnalyzer
	{
		private static readonly Regex BindGroupNamePattern = new Regex(@"(sakura|kero|char[0-9]+)\.bindgroup([0-9]+)\.name,(.+),(.+),?(.*)");
		private static readonly Regex BindGroupDefaultPattern = new Regex(@"(sakura|kero|char[0-9]+)\.bindgroup([0-9]+)\.default,([0-9]+)");
		private static readonly Regex BindOptionPattern = new Regex(@"(sakura|kero|char[0-9]+)\.bindoption([0-9]+)\.group,(.+),(.+)");

		//データ本体
		public ShellDescriptModel Model { get; private set; }

		//きせかえアイテム一覧
		public BindPartModel[] GetBindParts(int scopeId)
		{
			var scope = Model.GetScope(scopeId);
			if (scope == null)
				return Array.Empty<BindPartModel>();

			var items = new List<BindPartModel>();

			foreach(var c in scope.Categories)
			{
				items.AddRange(c.Value.Items.Select(o => o.Value));
			}
			return items.ToArray();
		}

		public void Load(string path)
		{
			var lines = System.IO.File.ReadAllLines(path, Constants.EncodingShiftJis);
			Model = new ShellDescriptModel();

			//各行を解析する
			foreach(var line in lines)
			{
				var bindGroupNameMatch = BindGroupNamePattern.Match(line);
				if(bindGroupNameMatch.Success)
				{
					var scopeName = bindGroupNameMatch.Groups[1].Value;
					var bindIdName = bindGroupNameMatch.Groups[2].Value;
					var categoryName = bindGroupNameMatch.Groups[3].Value;
					var partName = bindGroupNameMatch.Groups[4].Value;
					var scopeId = GetScopeIndex(scopeName);
					var bindId = int.Parse(bindIdName);
					Model.AddBindName(scopeId, bindId, categoryName, partName);
					continue;
				}

				var bindGroupDefaultMatch = BindGroupDefaultPattern.Match(line);
				if(bindGroupDefaultMatch.Success)
				{
					var scopeName = bindGroupDefaultMatch.Groups[1].Value;
					var bindIdName = bindGroupDefaultMatch.Groups[2].Value;
					var isDefaultName = bindGroupDefaultMatch.Groups[3].Value;

					var scopeId = GetScopeIndex(scopeName);
					var bindId = int.Parse(bindIdName);
					var isDefault = int.Parse(isDefaultName);
					Model.AddBindDefault(scopeId, bindId, isDefault != 0);
					continue;
				}

				var bindOptionMatch = BindOptionPattern.Match(line);
				if(bindOptionMatch.Success)
				{
					var scopeName = bindOptionMatch.Groups[1].Value;
					var categoryName = bindOptionMatch.Groups[3].Value;
					var isMustSelect = bindOptionMatch.Groups[4].Value.Contains("mustselect");
					var isMultiple = bindOptionMatch.Groups[4].Value.Contains("multiple");
					var scopeId = GetScopeIndex(scopeName);
					Model.AddBindOption(scopeId, categoryName, isMustSelect, isMultiple);
					continue;
				}
			}
		}

		public static int GetScopeIndex(string id)
		{
			switch(id)
			{
				case "sakura":
					return 0;
				case "kero":
					return 1;
				default:
					return int.Parse(id.Substring("char".Length));
			}
		}
	}

	public class ShellDescriptModel
	{
		public Dictionary<int, BindScopeModel> Bind { get; }

		public ShellDescriptModel()
		{
			Bind = new Dictionary<int, BindScopeModel>();
		}

		public BindScopeModel AddScope(int id)
		{
			BindScopeModel scope;
			if(!Bind.TryGetValue(id, out scope))
			{
				scope = new BindScopeModel();
				Bind.Add(id, scope);
			}
			return scope;
		}

		public BindScopeModel GetScope(int id)
		{
			BindScopeModel scope;
			Bind.TryGetValue(id, out scope);
			return scope;
		}

		public void AddBindName(int scopeId, int bindId, string categoryName, string partName)
		{
			var scope = AddScope(scopeId);
			var category = scope.AddCategory(categoryName);
			category.AddPart(partName, bindId);
		}

		public void AddBindDefault(int scopeId, int bindId, bool isDefault)
		{
			var scope = AddScope(scopeId);
			scope.SetDefault(bindId, isDefault);
		}

		public void AddBindOption(int scopeId, string categoryName, bool isMustSelect, bool isMultiple)
		{
			var scope = AddScope(scopeId);
			var category = scope.AddCategory(categoryName);
			category.IsMustSelect = isMustSelect;
			category.IsMultiple = isMultiple;
		}
	}

	public class BindScopeModel
	{
		public Dictionary<string, BindCategoryModel> Categories { get; }

		public BindScopeModel()
		{
			Categories = new Dictionary<string, BindCategoryModel>();
		}

		public BindCategoryModel AddCategory(string categoryName)
		{
			if(!Categories.ContainsKey(categoryName))
			{
				Categories.Add(categoryName, new BindCategoryModel()
				{
					Name = categoryName
				});
			}
			return Categories[categoryName];
		}

		public void SetDefault(int bindId, bool isDefault)
		{
			foreach(var c in Categories)
			{
				BindPartModel part;
				if(c.Value.Items.TryGetValue(bindId, out part))
				{
					part.IsDefault = isDefault;
					return;	//打ち切り
				}
			}
		}
	}

	public class BindCategoryModel
	{
		public string Name { get; set; }
		public bool IsMultiple { get; set; }
		public bool IsMustSelect { get; set; }
		public Dictionary<int, BindPartModel> Items { get; }

		public BindCategoryModel()
		{
			Items = new Dictionary<int, BindPartModel>();
		}

		public BindPartModel AddPart(string partName, int bindId)
		{
			BindPartModel part;
			if(!Items.TryGetValue(bindId, out part))
			{
				part = new BindPartModel(this)
				{
					Name = partName,
					BindId = bindId
				};
				Items.Add(bindId, part);
			}
			return part;
		}
	}

	public class BindPartModel
	{
		public BindCategoryModel Category { get; }
		public string Name { get; set; }
		public int BindId { get; set; }
		public bool IsDefault { get; set; }

		public BindPartModel(BindCategoryModel category)
		{
			Category = category;
		}
	}
}
