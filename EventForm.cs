using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Idmr.LfdReader;

namespace TIE_Layout_Editor
{
	public partial class EventForm : Form
	{
		Film.Block _block;

		public EventForm(ref Film.Block block)
		{
			InitializeComponent();

			LoadBlock(ref block);
		}

		public void LoadBlock(ref Film.Block block)
		{
			_block = block;
			lstEvents.Items.Clear();

			for (int i = 0; i < _block.NumberOfChunks; i++) lstEvents.Items.Add(_block.Chunks[i].ToString());
		}
	}
}
