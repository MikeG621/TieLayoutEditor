﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Idmr.LfdReader;

namespace Idmr.TieLayoutEditor
{
	public partial class ViewForm : Form
	{
		Film _film;
		readonly LfdFile _lfd;
		static LfdFile _tiesfx = null;
		static LfdFile _tiesfx2 = null;
		static LfdFile _tiespch = null;
		static LfdFile _tiespch2 = null;
		ColorPalette _palette;
		static ColorPalette _empirePalette = null;
		Bitmap[] _images;
		int[] _drawOrder;	// determines order of painting bm[]
		int[,] _imageLocation;  //until I get a better way of doing this, [index,x/y]
		short[,] _animation;	// keeps track of which ANIM frames are currently displayed [block, frame/direction/rate]
		static Delt _stars = null;    // universal background from EMPIRE.LFD
		int _time;
		bool _loading;
		bool _isPlaying;

		// this gets us the required function to play .WAV
		[DllImport("winmm.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		static extern bool PlaySound(byte[] b_ary, IntPtr ptr, SoundFlags sf);      // from memory
		[Flags]
		public enum SoundFlags : int
		{
			//SND_SYNC = 0x0000,  // play synchronously (default) 
			SND_ASYNC = 0x0001,  // play asynchronously 
			//SND_NODEFAULT = 0x0002,  // silence (!default) if sound not found 
			SND_MEMORY = 0x0004,  // pszSound points to a memory file
			//SND_LOOP = 0x0008,  // loop the sound until next sndPlaySound 
			//SND_NOSTOP = 0x0010,  // don't stop any currently playing sound 
			//SND_NOWAIT = 0x00002000, // don't wait if the driver is busy 
			//SND_ALIAS = 0x00010000, // name is a registry alias 
			//SND_ALIAS_ID = 0x00110000, // alias is a predefined ID
			SND_FILENAME = 0x00020000, // name is file name 
			//SND_RESOURCE = 0x00040004  // name is resource name or atom 
		}

		public ViewForm(ref LfdFile lfd, object tag)
		{
			Debug.WriteLine("frmView created");
			InitializeComponent();
			Height = 600;
			_lfd = lfd;
			loadEmpire();
			_palette = _empirePalette;
			LoadFilm(ref lfd, tag);
		}

		public void PaintFilm()
		{
			Debug.WriteLine("painting...");
			Bitmap image = new Bitmap(640, 480);
			Graphics g = Graphics.FromImage(image);
			for (int i = 0; i < _drawOrder.Length; i++)
			{
				if (_drawOrder[i] == -1 || _images[_drawOrder[i]] == null) continue;
				g.DrawImageUnscaled(_images[_drawOrder[i]], _imageLocation[_drawOrder[i], 0], _imageLocation[_drawOrder[i], 1]);
				Debug.WriteLine("image drawn: " + lstBlocks.Items[_drawOrder[i]]);
			}
			pctView.BackColor = Color.Black;
			pctView.Image = image;
			g.Dispose();
		}
		public void LoadFilm(ref LfdFile lfd, object tag)
		{
			Debug.WriteLine("View loading...");
			_film = (Film)lfd.Resources.GetResourceByTag(tag);
			lstBlocks.Items.Clear();
			_palette = _empirePalette;
			_images = new Bitmap[_film.NumberOfBlocks];
			_drawOrder = new int[_film.NumberOfBlocks];
			_imageLocation = new int[_film.NumberOfBlocks, 2];
			_animation = new short[_film.NumberOfBlocks, 3];
			#region populate lstBlocks
			foreach (Film.Block b in _film.Blocks)
			{
				string str = b.ToString() + "*";
				Debug.WriteLine(str);
				for (int i = 0; i < _lfd.Resources.Count; i++)
					if (str == (_lfd.Resources[i].ToString() + "*"))
					{
						str = str.Replace("*", "");	// leaves anything not in the LFD marked, typ VOIC
						break;
					}
				for (int i = 0; i < _tiesfx.Resources.Count; i++)
					if (str == (_tiesfx.Resources[i].ToString() + "*"))
					{
						str = str.Replace("*", ""); // start finding VOIC
						break;
					}
				for (int i = 0; i < _tiesfx2.Resources.Count; i++)
					if (str == (_tiesfx2.Resources[i].ToString() + "*"))
					{
						str = str.Replace("*", "");
						break;
					}
				for (int i = 0; i < _tiespch.Resources.Count; i++)
					if (str == (_tiespch.Resources[i].ToString() + "*"))
					{
						str = str.Replace("*", "");
						break;
					}
				for (int i = 0; i < _tiespch2.Resources.Count; i++)
					if (str == (_tiespch2.Resources[i].ToString() + "*"))
					{
						str = str.Replace("*", "");
						break;
					}
				if (b.Type == Film.Block.BlockType.View) str = str.Replace("*", "");
				lstBlocks.Items.Add(str);
			}
			#endregion

			_loading = true;	// this prevents double-paint
			hsbTime.Value = 0;
			hsbTime.Maximum = _film.NumberOfFrames;
			_loading = false;
			updateView();
			PaintFilm();
		}
		/// <summary>Paint a box around the extents of the image</summary>
		/// <param name="index">Block index</param>
		public void BoxImage(int index)
		{
			string str = lstBlocks.Items[index].ToString();
			str = str.Replace("*", "");
			Resource img = _lfd.Resources[str];
			if (img == null)
				if (lstBlocks.Items[index].ToString() == _stars.ToString() + "*") img = _stars;
				else return;
			if (_film.Blocks[index].Chunks[0].Vars[0] != 0) return; // skip stuff not on first frame
			Debug.WriteLine("boxing...");
			Graphics g = pctView.CreateGraphics();
			Pen pnFrame = new Pen(Color.Red);
			if (img.Type == Resource.ResourceType.Delt)
			{
				Delt d = (Delt)img;
				g.DrawRectangle(pnFrame, _imageLocation[index, 0], _imageLocation[index, 1], d.Width - 1, d.Height - 1);
				Debug.WriteLine("L:" + _imageLocation[index, 0] + " T:" + _imageLocation[index, 1] + " W:" + d.Width + " H:" + d.Height);
			}
			else if (img.Type == Resource.ResourceType.Anim)
			{
				Anim a = (Anim)img;
				g.DrawRectangle(pnFrame, _imageLocation[index, 0], _imageLocation[index, 1], a.Width - 1, a.Height - 1);
				Debug.WriteLine("L:" + _imageLocation[index, 0] + " T:" + _imageLocation[index, 1] + " W:" + a.Width + " H:" + a.Height);
			}
			// again, skip CUST for now
			g.Dispose();
		}
		/// <summary>Play the audio resource</summary>
		/// <param name="index">Block index</param>
		public void PlayWav(int index)
		{
			string id = lstBlocks.Items[index].ToString();
			Resource res = _tiesfx.Resources[id];
			if (res == null)
				res = _tiesfx2.Resources[id];
			if (res == null)
				res = _tiespch.Resources[id];
			if (res == null)
				res = _tiespch2.Resources[id];
			if (res == null)
			{
				MessageBox.Show("Error: " + id + " not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			byte[] wav = ((Blas)res).GetWavBytes();
			PlaySound(wav, IntPtr.Zero, SoundFlags.SND_MEMORY | SoundFlags.SND_ASYNC);
		}

		void loadPltt(string name)
		{
			Pltt p = (Pltt)_lfd.Resources["PLTT" + name];
			if (p != null)
				for (int i = p.StartIndex; i <= p.EndIndex; i++)
					_palette.Entries[i] = p.Entries[i];
		}
		void loadEmpire()
		{
			if (_stars != null) return;	// already loaded in, can skip over since it's static
			string dir = Path.GetDirectoryName(_lfd.FilePath) + "\\";
			LfdFile empire = new LfdFile(dir + "EMPIRE.LFD");
			Pltt p = (Pltt)empire.Resources["PLTTstandard"];
			_stars = (Delt)empire.Resources["DELTstars"];
			_empirePalette = p.Palette;
			_stars.Palette = _empirePalette;
			_tiesfx = new LfdFile(dir + "TIESFX.LFD");
			_tiesfx2 = new LfdFile(dir + "TIESFX2.LFD");
			_tiespch = new LfdFile(dir + "TIESPCH.LFD");
			_tiespch2 = new LfdFile(dir + "TIESPCH2.LFD");
		}

		//TODO: really need to work out an Events structure and drive from there, there's too many moving parts to parse everything individually
		void updateView()
		{
			int[] layer = new int[_film.NumberOfBlocks];
			for (int b = 0; b < _film.NumberOfBlocks; b++) _drawOrder[b] = -1;
			for (int b = 0; b < _film.NumberOfBlocks; b++)
			{
				string str = lstBlocks.Items[b].ToString();
				if (str.StartsWith("PLTT") && str.IndexOf("*") == -1)
					foreach (Film.Chunk c in _film.Blocks[b].Chunks)
						if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] <= _time) loadPltt(_film.Blocks[b].Name);

				layer[b] = -2527;   // there are negative layers in a few cases, need to make room for them
				if (_film.Blocks[b].TypeNum == 3)
					foreach (Film.Chunk c in _film.Blocks[b].Chunks)
					{
						if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] > _time) break;
						if (c.Code == Film.Chunk.OpCode.Display && c.Vars[0] == 0) { layer[b] = -2527; _drawOrder[b] = -1; break; } // hidden
						if (c.Code == Film.Chunk.OpCode.Display && c.Vars[0] == 1 && layer[b] == -2527) { layer[b] = 0; _drawOrder[b] = b; }    // default (top), intialize order
						if (c.Code == Film.Chunk.OpCode.Layer) { layer[b] = c.Vars[0]; _drawOrder[b] = b; }
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

			#region get images
			for (int i = 0; i < _drawOrder.Length; i++)
			{
				int block = _drawOrder[i];
				if (block == -1 || _film.Blocks[block].Type == Film.Block.BlockType.Cust) continue;
				if (_film.Blocks[block].ToString() == _stars.ToString())
				{
					_images[block] = _stars.Image;
					_images[block].MakeTransparent(Color.Black);
					_imageLocation[block, 0] = _stars.Left;
					_imageLocation[block, 1] = _stars.Top;
				}
				for (int j = 0; j < _lfd.Resources.Count; j++)
					if (_lfd.Resources[j].ToString() == _film.Blocks[block].ToString())
					{
						if (_lfd.Resources[j].Type == Resource.ResourceType.Delt)
						{
							Delt d = (Delt)_lfd.Resources[j];
							d.Palette = _palette;
							_images[block] = (Bitmap)d.Image.Clone();
							_images[block].MakeTransparent(Color.Black);
							_imageLocation[block, 0] = d.Left;
							_imageLocation[block, 1] = d.Top;
						}
						else if (_lfd.Resources[j].Type == Resource.ResourceType.Anim)
						{
							Anim a = (Anim)_lfd.Resources[j];
							a.SetPalette(_palette);
							a.RelativePosition = true;
							int currentTime = -1;
							foreach (Film.Chunk c in _film.Blocks[block].Chunks)
							{
								if (currentTime != _time) currentTime = -1;

								if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] == _time) currentTime = _time;
								else if (c.Code == Film.Chunk.OpCode.Frame) _animation[block, 0] = c.Vars[0];
								else if (c.Code == Film.Chunk.OpCode.Animation)
								{
									_animation[block, 1] = c.Vars[0];
									_animation[block, 2] = c.Vars[1];
								}
								else if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] > _time) break;
							}
							_images[block] = (Bitmap)a.Frames[_animation[block, 0]].Image.Clone();
							_images[block].MakeTransparent(Color.Black);
							_imageLocation[block, 0] = a.Left;
							_imageLocation[block, 1] = a.Top;
						}
						// currently no CUST processing, but I think it's the same as DELT
						break;
					}
				foreach (Film.Chunk c in _film.Blocks[block].Chunks)
				{
					if (c.Code == Film.Chunk.OpCode.Move)
					{
						_imageLocation[block, 0] += c.Vars[0];
						_imageLocation[block, 1] += c.Vars[1];
					}
					else if (c.Code == Film.Chunk.OpCode.Window)
					{
						Debug.WriteLine("Window code detected: " + _film.Blocks[block].ToString());
					}
					else if (c.Code == Film.Chunk.OpCode.Shift)
					{
						Debug.WriteLine("Shift code detected: " + _film.Blocks[block].ToString());
					}
					else if (c.Code == Film.Chunk.OpCode.Orientation && _images[block] != null)
					{
						if (c.Vars[0] == 1) _images[block].RotateFlip(RotateFlipType.RotateNoneFlipX);
						if (c.Vars[1] == 1) _images[block].RotateFlip(RotateFlipType.RotateNoneFlipY);
					}
					else if (c.Code == Film.Chunk.OpCode.Animation)
					{
						Debug.WriteLine("Animation code detected: " + _film.Blocks[block].ToString());
					}
					else if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] > _time) break;
				}

			}
			#endregion
			for (int b = 0; b < _film.NumberOfBlocks; b++)
			{
				if (_film.Blocks[b].Type == Film.Block.BlockType.Voic)
				{
					int currentTime = -1;
					foreach (Film.Chunk c in _film.Blocks[b].Chunks)
					{
						if (currentTime != _time) currentTime = -1;
						if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] == _time) currentTime = _time;
						else if ((c.Code == Film.Chunk.OpCode.Sound || c.Code == Film.Chunk.OpCode.Stereo) && currentTime == _time && _isPlaying) PlayWav(b);
						else if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] > _time) break;
						else if (c.Code == Film.Chunk.OpCode.Loop) Debug.WriteLine("Loop code detected: " + _film.Blocks[b].ToString());
					}
				}
			}
		}

		// TODO: should figure out mouse clicks/drags for pctView
		private void lstBlocks_SelectedIndexChanged(object sender, EventArgs e)
		{
			pctView.Refresh();
			if (_film.Blocks[lstBlocks.SelectedIndex].TypeNum == 3) BoxImage(lstBlocks.SelectedIndex);
			if (_film.Blocks[lstBlocks.SelectedIndex].Type == Film.Block.BlockType.Voic) PlayWav(lstBlocks.SelectedIndex);
			// TODO: import to frmTLE_Events
		}

		private void cmdForward_Click(object sender, EventArgs e)
		{
			if (_time == _film.NumberOfFrames - 1)
			{
				_isPlaying = false;
				tmrPlayback.Stop();
				cmdPlayPause.Text = ">";
				return;
			}
			hsbTime.Value++;
		}

		private void tmrPlayback_Tick(object sender, EventArgs e)
		{
			cmdForward_Click("tmrPlayback_Tick", new EventArgs());
		}

		private void cmdPlayPause_Click(object sender, EventArgs e)
		{
			if (cmdPlayPause.Text == ">")
			{
				cmdPlayPause.Text = "| |";
				_isPlaying = true;
				tmrPlayback.Start();
				updateView();
				PaintFilm();
			}
			else
			{
				_isPlaying = false;
				tmrPlayback.Stop();
				cmdPlayPause.Text = ">";
			}
		}

		private void hsbTime_ValueChanged(object sender, EventArgs e)
		{
			_time = hsbTime.Value;
			if (_loading) return;
			updateView();
			PaintFilm();
		}

		private void cmdStart_Click(object sender, EventArgs e)
		{
			hsbTime.Value = 0;
		}
	}
}
