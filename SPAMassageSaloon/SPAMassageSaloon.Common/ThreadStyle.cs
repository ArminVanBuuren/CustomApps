using System;
using System.Drawing;

namespace SPAMassageSaloon.Common
{
	public class ControlStyleColor
	{
		public Color Back { get; set; }

		public Color Fore { get; set; }
	}

	public class ControlStyle
	{
		public ControlStyleColor EnabledBody { get; set; }
		public ControlStyleColor DisabledBody { get; set; }

		public ControlStyleColor EnabledContent { get; set; }
		public ControlStyleColor DisabledContent { get; set; }

		public ControlStyleColor EnabledButton { get; set; }
		public ControlStyleColor DisabledButton { get; set; }

		public ControlStyleColor EnabledTab { get; set; }
		public ControlStyleColor DisabledTab { get; set; }

		public ControlStyleColor EnabledFooter { get; set; }
		public ControlStyleColor DisabledFooter { get; set; }

		public ControlStyleColor EnabledBorder { get; set; }
		public ControlStyleColor DisabledBorder { get; set; }
	}

	public enum ControlStyles
	{
		Default = 0,
		Dark = 1
	}

	public static class ThreadStyle
	{
		internal static event EventHandler StyleChanged;

		internal static ControlStyle Style { get; }

		static ThreadStyle() => Style = new ControlStyle();

		public static void ChangeStyle(ControlStyles style)
		{
			try
			{
				switch (style)
				{
					case ControlStyles.Default:
						break;
					case ControlStyles.Dark:
						break;
				}
			}
			finally
			{
				StyleChanged?.Invoke(null, EventArgs.Empty);
			}
		}
	}
}