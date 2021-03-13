using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LogsReader.Properties;

namespace LogsReader.Reader
{
	public sealed class CustomTabPage : TabPage
	{
		public CustomTabPage(TraceItemView traceItemView)
		{
			View = traceItemView;
			Controls.Add(traceItemView);
		}

		public bool CanClose { get; set; }

		public TraceItemView View { get; }
	}

	public class CustomTabControl : TabControl
	{
		public CustomTabControl()
		{
			MouseClick += CustomTabControl_MouseClick;
			MouseDoubleClick += CustomTabControl_MouseDoubleClick;
		}

		private ContextMenuStrip Strip
		{
			get
			{
				var strip = new ContextMenuStrip();

				var item = new ToolStripMenuItem(Resources.Txt_Close, Resources.notepad_closeItem);
				item.Click += (s, e) => CloseTab(SelectedTab as CustomTabPage);
				strip.Items.Add(item);

				item = new ToolStripMenuItem(Resources.Txt_CloseAllDocuments, Resources.notepad_closeAllItems);
				item.Click += (s, e) => CloseAll();
				strip.Items.Add(item);

				item = new ToolStripMenuItem(Resources.Txt_CloseAllButThis, Resources.notepad_closeAllButThis);
				item.Click += (s, e) => CloseAllButThis(SelectedTab as CustomTabPage);
				strip.Items.Add(item);

				return strip;
			}
		}

		public void CloseTab(object tab)
		{
			if (tab is CustomTabPage tabPage && tabPage.CanClose)
			{
				tabPage.View.Clear();
				TabPages.Remove(tabPage);
			}
		}

		public void CloseAllButThis(object tab)
		{
			if (!(tab is CustomTabPage tabPage))
				return;

			var tabs = new List<CustomTabPage>(TabPages.Cast<CustomTabPage>());
			foreach (var customTab in tabs.Where(x => x != tabPage && x.CanClose))
				CloseTab(customTab);
		}

		public void CloseAll()
		{
			var tabs = new List<CustomTabPage>(TabPages.Cast<CustomTabPage>());
			foreach (var tab in tabs.Where(x => x.CanClose))
				CloseTab(tab);
		}

		private void CustomTabControl_MouseClick(object sender, MouseEventArgs e)
		{
			try
			{
				if (e.Button == MouseButtons.Right)
				{
					for (var i = 0; i < TabPages.Count; ++i)
					{
						if (GetTabRect(i).Contains(e.Location))
						{
							SelectTab(TabPages[i]);
							Strip.Show(this, e.Location);
						}
					}
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void CustomTabControl_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (GetTabRect(SelectedIndex).Contains(e.Location))
				CloseTab(SelectedTab);
		}
	}
}