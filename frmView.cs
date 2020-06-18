using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Idmr.LfdReader;

namespace Idmr.TieLayoutEditor
{
	public partial class frmView : Form
	{
		Film _film;
		LfdFile _lfd;
		ColorPalette _palette;
		ColorPalette _empirePalette;
		Bitmap[] _images;
		int[] _drawOrder;	// determines order of painting bm[]
		int[,] _imageLocation;	//until I get a better way of doing this, [index,x/y]
		Delt _stars;	// universal background from EMPIRE.LFD

		public frmView(ref LfdFile lfd, object tag)
		{
			System.Diagnostics.Debug.WriteLine("frmView created");
			InitializeComponent();
			Height = 600;
			_lfd = lfd;
			_loadEmpire();
			_palette = _empirePalette;
			LoadFilm(ref lfd, tag);
		}

		public void PaintFilm()
		{
			System.Diagnostics.Debug.WriteLine("painting...");
			Bitmap image = new Bitmap(640, 480);
			Graphics g = Graphics.FromImage(image);
			for (int i=0;i<_drawOrder.Length;i++)
			{
				if (_drawOrder[i] == -1 || _images[_drawOrder[i]] == null) continue;
				g.DrawImageUnscaled(_images[_drawOrder[i]], _imageLocation[_drawOrder[i], 0], _imageLocation[_drawOrder[i], 1]);
				System.Diagnostics.Debug.WriteLine("image drawn");
			}
			pctView.BackColor = Color.Black;
			pctView.Image = image;
			g.Dispose();
		}
		public void LoadFilm(ref LfdFile lfd, object tag)
		{
			System.Diagnostics.Debug.WriteLine("View loading...");
			_film = (Film)lfd.Resources.GetResourceByTag(tag);
			lstBlocks.Items.Clear();
			_palette = _empirePalette;
			_images = new Bitmap[_film.NumberOfBlocks];
			_drawOrder = new int[_film.NumberOfBlocks];
			_imageLocation = new int[_film.NumberOfBlocks, 2];
			for (int i = 0; i < _film.NumberOfBlocks; i++) _drawOrder[i] = -1;
			#region populate lstBlocks
			foreach (Film.Block b in _film.Blocks)
			{
				string str = b.Type.ToString().ToUpper() + " " + b.Name + "*";
				System.Diagnostics.Debug.WriteLine(str);
				for (int i = 0; i < _lfd.Resources.Count; i++)
					if (str == (_lfd.Resources[i].Type.ToString().ToUpper() + " " + _lfd.Resources[i].Name + "*"))
					{
						str = str.Replace("*", "");	// leaves anything not in the LFD marked, typ VOIC
						break;
					}
				if (b.Type == Film.Block.BlockType.View) str = str.Replace("*", "");
				lstBlocks.Items.Add(str);
				if (b.Type == Film.Block.BlockType.Pltt && str.IndexOf("*") == -1)
					foreach (Film.Chunk c in b.Chunks)
						if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] == 0) _loadPltt(b.Name);
			}
			#endregion
			#region prepare layers and drawOrder
			int[] layer = new int[_film.NumberOfBlocks];
			for (int i = 0; i < _film.NumberOfBlocks; i++)
			{
				layer[i] = -2527;	// there are negative layers in a few cases, need to make room for them
				if (_film.Blocks[i].TypeNum == 3)
					foreach (Film.Chunk c in _film.Blocks[i].Chunks)
					{
						if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] != 0) break;	// only want at start for now
						if (c.Code == Film.Chunk.OpCode.Display && c.Vars[0] == 0) { layer[i] = -2527; break; }	// hidden
						if (c.Code == Film.Chunk.OpCode.Display && c.Vars[0] == 1 && layer[i] == -2527) { layer[i] = 0; _drawOrder[i] = i; }	// default (top), intialize order
						if (c.Code == Film.Chunk.OpCode.Layer) { layer[i] = c.Vars[0]; _drawOrder[i] = i; }
					}
			}
			// takes the saved layer info, sorts them into order[], desc x64 to 0
			for (int i = 1; i < layer.Length; i++)
				for (int j = i; j > 0; j--)
					if (layer[j] > layer[j - 1])
					{
						int t = layer[j];
						layer[j] = layer[j - 1];
						layer[j - 1] = t;
						t = _drawOrder[j];
						_drawOrder[j] = _drawOrder[j - 1];
						_drawOrder[j - 1] = t;
					}
					else break;
			#endregion
			#region get images
			for (int i=0;i<_drawOrder.Length;i++)
			{
				if (_drawOrder[i] == -1) continue;
				if ((int)_film.Blocks[_drawOrder[i]].Type == (int)_stars.Type && _film.Blocks[_drawOrder[i]].Name == _stars.Name)
				{
					_images[_drawOrder[i]] = _stars.Image;
					_images[_drawOrder[i]].MakeTransparent(Color.Black);
					_imageLocation[_drawOrder[i], 0] = _stars.Left;
					_imageLocation[_drawOrder[i], 1] = _stars.Top;
				}
				for(int j = 0; j < _lfd.Resources.Count; j++)
					if ((int)_lfd.Resources[j].Type == (int)_film.Blocks[_drawOrder[i]].Type && _lfd.Resources[j].Name == _film.Blocks[_drawOrder[i]].Name)
					{
						if (_lfd.Resources[j].Type == Resource.ResourceType.Delt)
						{
							Delt d = (Delt)_lfd.Resources[j];
							d.Palette = _palette;
							_images[_drawOrder[i]] = d.Image;
							_images[_drawOrder[i]].MakeTransparent(Color.Black);
							_imageLocation[_drawOrder[i], 0] = d.Left;
							_imageLocation[_drawOrder[i], 1] = d.Top;
						}
						else if (_lfd.Resources[j].Type == Resource.ResourceType.Anim)
						{
							Anim a = (Anim)_lfd.Resources[j];
							a.SetPalette(_palette);
							_images[_drawOrder[i]] = a.Frames[0].Image;
							_images[_drawOrder[i]].MakeTransparent(Color.Black);
							_imageLocation[_drawOrder[i], 0] = a.Left;
							_imageLocation[_drawOrder[i], 1] = a.Top;
						}
						// currently no CUST processing, but I think it's the same as DELT
						break;
					}
			}
			#endregion
			PaintFilm();
		}
		public void BoxImage(int index)
		{
			string str = lstBlocks.Items[index].ToString();
			str = str.Replace(" ", "").Replace("*", "");
			Resource img = _lfd.Resources[str];
			if (img == null)
				if (lstBlocks.Items[index].ToString() == _stars.Type + " " + _stars.Name + "*") img = _stars;
				else return;
			System.Diagnostics.Debug.WriteLine("boxing...");
			Graphics g = pctView.CreateGraphics();
			Pen pnFrame = new Pen(Color.Red);
			if (img.Type == Resource.ResourceType.Delt)
			{
				Delt d = (Delt)img;
				g.DrawRectangle(pnFrame, d.Left, d.Top, d.Width - 1, d.Height - 1);
				System.Diagnostics.Debug.WriteLine("L:" + d.Left + " T:" + d.Top + " W:" + d.Width + " H:" + d.Height);
			}
			else if (img.Type == Resource.ResourceType.Anim)
			{
				Anim a = (Anim)img;
				g.DrawRectangle(pnFrame, a.Left, a.Top, a.Width - 1, a.Height - 1);
				System.Diagnostics.Debug.WriteLine("L:" + a.Left + " T:" + a.Top + " W:" + a.Width + " H:" + a.Height);
			}
			// again, skip CUST for now
			g.Dispose();
		}

		void _loadPltt(string name)
		{
			Pltt p = null;
			for (int i = 0; i < _lfd.Rmap.NumberOfHeaders; i++)
				if (_lfd.Rmap.SubHeaders[i].Type == Resource.ResourceType.Pltt && _lfd.Rmap.SubHeaders[i].Name == name)
				{
					p = (Pltt)_lfd.Resources[i];
					break;
				}
			for (int i = p.StartIndex; i < p.EndIndex; i++)
				_palette.Entries[i] = p.Entries[i];
		}
		void _loadEmpire()
		{
			string file = Path.GetDirectoryName(_lfd.FilePath) + "\\EMPIRE.LFD";
			LfdFile empire = new LfdFile(file);
			Pltt p = null;
			// the layout of EMPIRE is known, there's an RMAP also, but I'm doing it this way to prevent using magic numbers
			for (int i = 0; i < empire.Resources.Count; i++)
				if (empire.Resources[i].Type == Resource.ResourceType.Pltt) p = (Pltt)empire.Resources[i];	// only 1 PLTT in that file
				else if (empire.Resources[i].Type == Resource.ResourceType.Delt) _stars = (Delt)empire.Resources[i];	// only 1 DELT
			_empirePalette = p.Palette;
			_stars.Palette = _empirePalette;
		}

		// TODO: should figure out mouse clicks/drags for pctView
		private void lstBlocks_SelectedIndexChanged(object sender, EventArgs e)
		{
			pctView.Refresh();
			if (_film.Blocks[lstBlocks.SelectedIndex].TypeNum == 3) BoxImage(lstBlocks.SelectedIndex);
			// TODO: import to frmTLE_Events
		}
	}
}
