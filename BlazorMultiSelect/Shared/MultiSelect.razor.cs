using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorMultiSelect.Shared
{
	public partial class MultiSelect<T>
	{
		[Inject] IJSRuntime JS { get; set; }
		[Parameter] public Action<IList<T>> SelectionChanged { get;set; }
		[Parameter] public string LabelText { get; set; } = "Choose";
		/// <summary>
		/// Outer container class (default:multi-select)
		/// </summary>
		[Parameter] public string MainClass { get; set; } = "multi-select";
		/// <summary>
		/// Selected Tags container class (default:multi-select__tags)
		/// </summary>
		[Parameter] public string TagsClass { get; set; } = "multi-select-tags";
		/// <summary>
		/// The DOM id of the input control (default:random)
		/// </summary>
		[Parameter] public string InputId { get; set; } = Guid.NewGuid().ToString("N");
		/// <summary>
		/// The DOM id of the input control's list (default:random)
		/// </summary>
		[Parameter] public string ListId { get; set; } = Guid.NewGuid().ToString("N");
		[Parameter] public Dictionary<string, T> StaticDictionary { get; set; }
		/// <summary>
		/// A static list of things to select
		/// </summary>
		[Parameter] public List<T> StaticList { get; set; }
		IList<Tag<T>> things { get; set; }

		List<Tag<T>> selected = new List<Tag<T>> { };

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await JS.InvokeVoidAsync("MS.setFocus", InputId);
		}
		protected override void OnParametersSet()
		{
			base.OnParametersSet();
			if (StaticList is object)
			{
				things = StaticList
					.Select(s=>new Tag<T>(s,RemoveItem))
					.ToList();
			}
			else if (StaticDictionary is object)
			{
				things = StaticDictionary
					.Select(kvp=>new Tag<T>(kvp.Value,RemoveItem) { Label=kvp.Key })
					.ToList();
			}
		}
		const string _choice = "";
		string Choice
		{
			get => _choice;
			set
			{
				if (value != _choice)
				{
					InvokeAsync(async () => await Choose(value));
				}
			}
		}
		Task Choose(string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				if (!selected.Any(tag => tag.Value == value || tag.Label == value))
				{
					var tag = things.FirstOrDefault(thing => thing.Value == value || thing.Label == value);
					if (tag is object)
					{
						selected.Add(tag);
					}
					else
					{
						selected.Add(new Tag<T>(value, RemoveItem));
					}
					RaiseSelectionChanged();
				}
			}
			return Task.CompletedTask;
		}

		private void RaiseSelectionChanged()
		{
			SelectionChanged?.Invoke(selected.Select(s => s.data).ToList());
		}

		void RemoveItem(Tag<T> tag)
		{
			if (!things.Any(item => item.Value.Equals(tag.Value)))
			{
				things.Add(tag);
			}
			selected.Remove(tag);
			RaiseSelectionChanged();
		}
	}
	public struct Tag<T>
	{
		public string Label { get; set; }
		public T data;
		public Action<Tag<T>> Remove;
		public Tag(T value, Action<Tag<T>> removeAction)
		{
			Label = value.ToString();
			data = value ?? throw new ArgumentNullException(nameof(value));
			Remove = removeAction ?? throw new ArgumentNullException(nameof(removeAction));
		}
		public Tag(string value, Action<Tag<T>> removeAction)
		{
			Label = value ?? throw new ArgumentNullException(nameof(value));
			if (value is T tValue)
			{
				data = tValue;
			}
			else
			{
				data = default;
			}
			Remove = removeAction ?? throw new ArgumentNullException(nameof(removeAction));
		}
		public string Value => data.ToString();
		public void RemoveMe()
		{
			Remove?.Invoke(this);
		}
	}


}