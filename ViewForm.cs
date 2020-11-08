using Idmr.LfdReader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using TIE_Layout_Editor;

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
		short[] _drawOrder;	// determines order of painting bm[]
		Image[] _images;
		static Delt _stars = null;    // universal background from EMPIRE.LFD
		int _time;
		bool _loading;
		bool _isPlaying;
		// this prevents early garbage collection that would cut off the audio early
		// also, I'm not declaring the using since it causes class name conflicts within Drawing
		readonly List<System.Windows.Media.MediaPlayer> _activeSounds = new List<System.Windows.Media.MediaPlayer>();
		readonly List<Event> _events = new List<Event>();
		EventForm _fEvent = null;
		readonly short _unused = -2527;

		public ViewForm(ref LfdFile lfd, object tag)
		{
			Debug.WriteLine("frmView created");
			InitializeComponent();
			Height = 600;
			_lfd = lfd;
			loadEmpire();
			Bitmap t = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
			_palette = t.Palette;
			t.Dispose();
			for (int c = 0; c < 256; c++) _palette.Entries[c] = _empirePalette.Entries[c]; // this ensures _empire isn't modified
			LoadFilm(ref lfd, tag);
		}

		public void PaintFilm()
		{
			Bitmap image = new Bitmap(640, 480);
			Graphics g = Graphics.FromImage(image);
			for (int i = 0; i < _drawOrder.Length; i++)
			{
				if (_drawOrder[i] == -1 || _images[_drawOrder[i]].ProcessedImage == null || !_images[_drawOrder[i]].IsVisible) continue;
				// TODO: Windows; DrawImageUnscaledAndClipped
				g.DrawImageUnscaled(_images[_drawOrder[i]].ProcessedImage, _images[_drawOrder[i]].X, _images[_drawOrder[i]].Y);
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
			for (int c = 0; c < 256; c++) _palette.Entries[c] = _empirePalette.Entries[c];
			_drawOrder = new short[_film.NumberOfBlocks];
			_images = new Image[_film.NumberOfBlocks];
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
				Resource res = null;
				for (int i = 0; i < _tiesfx.Resources.Count; i++)
					if (str == (_tiesfx.Resources[i].ToString() + "*"))
					{
						str = str.Replace("*", ""); // start finding VOIC
						res = _tiesfx.Resources[i];
						break;
					}
				for (int i = 0; i < _tiesfx2.Resources.Count; i++)
					if (str == (_tiesfx2.Resources[i].ToString() + "*"))
					{
						str = str.Replace("*", "");
						res = _tiesfx2.Resources[i];
						break;
					}
				for (int i = 0; i < _tiespch.Resources.Count; i++)
					if (str == (_tiespch.Resources[i].ToString() + "*"))
					{
						str = str.Replace("*", "");
						res = _tiespch.Resources[i];
						break;
					}
				for (int i = 0; i < _tiespch2.Resources.Count; i++)
					if (str == (_tiespch2.Resources[i].ToString() + "*"))
					{
						str = str.Replace("*", "");
						res = _tiespch2.Resources[i];
						break;
					}
				if (b.Type == Film.Block.BlockType.View) str = str.Replace("*", "");
				lstBlocks.Items.Add(str);

				if (res != null)
				{
					if (!Directory.Exists(MainForm._tempDir)) Directory.CreateDirectory(MainForm._tempDir);
					if (!File.Exists(MainForm._tempDir + res.ToString() + ".wav"))
						using (FileStream fs = new FileStream(MainForm._tempDir + res.ToString() + ".wav", FileMode.OpenOrCreate))
						{
							using (BinaryWriter bw = new BinaryWriter(fs))
							{
								Blas blas = (Blas)res;
								if (blas.SoundBlocks[0].NumberOfRepeats > -1) bw.Write(blas.GetWavBytes(true));
								else bw.Write(blas.GetWavBytes(false));
								fs.SetLength(fs.Position);
							}
						}
				}
			}
			#endregion
			buildEvents();
			_loading = true;	// this prevents double-paint
			hsbTime.Value = 0;
			hsbTime.Maximum = _film.NumberOfFrames;
			_loading = false;
			performEvents();
			PaintFilm();
		}
		/// <summary>Paint a box around the extents of the image</summary>
		/// <param name="index">Block index</param>
		public void BoxImage(int index)
		{
			// TODO: fix BoxImage so it'll only highlight if currently visible
			string str = lstBlocks.Items[index].ToString();
			str = str.Replace("*", "");
			Resource img = _lfd.Resources[str];
			if (img == null)
				if (lstBlocks.Items[index].ToString() == _stars.ToString() + "*") img = _stars;
				else return;
			Debug.WriteLine("boxing...");
			Graphics g = pctView.CreateGraphics();
			Pen pnFrame = new Pen(Color.Red);
			if (img.Type == Resource.ResourceType.Delt)
			{
				Delt d = (Delt)img;
				g.DrawRectangle(pnFrame, _images[index].X, _images[index].Y, d.Width - 1, d.Height - 1);
				Debug.WriteLine("L:" + _images[index].X + " T:" + _images[index].Y + " W:" + d.Width + " H:" + d.Height);
			}
			else if (img.Type == Resource.ResourceType.Anim)
			{
				Anim a = (Anim)img;
				g.DrawRectangle(pnFrame, _images[index].X, _images[index].Y, a.Width - 1, a.Height - 1);
				Debug.WriteLine("L:" + _images[index].X + " T:" + _images[index].Y + " W:" + a.Width + " H:" + a.Height);
			}
			// again, skip CUST for now
			g.Dispose();
		}
		void playWav(int blockIndex, short volume, short balance)
		{
			playWav(lstBlocks.Items[blockIndex].ToString(), volume, balance);
		}
		void playWav(Film.Block block, short volume, short balance)
		{
			playWav(block.ToString(), volume, balance);
		}
		void playWav(string id, short volume, short balance)
		{
			var plr = new System.Windows.Media.MediaPlayer();
			plr.MediaEnded += mediaPlayer_MediaEnded;
			try
			{
				plr.Open(new Uri(MainForm._tempDir + id + ".wav"));
				plr.Volume = (volume == 0 ? 100 : volume) * 0.5 / 100;
				plr.Balance = (double)((balance == 0 ? 64 : balance) - 64) / 63;
				plr.Play();
				_activeSounds.Add(plr);
			}
			catch { }
		}

		private void mediaPlayer_MediaEnded(object sender, EventArgs e)
		{
			_activeSounds.Remove((System.Windows.Media.MediaPlayer)sender);
			// TODO: Maybe do a Dictionary instead? Could tie the MP to the index of the FILM, then from here back out the index to handle repeats
			// would need a new way to call Stop() during PlayPause
		}

		void buildEvents()
		{
			_events.Clear();
			for (short t = 0; t < _film.NumberOfFrames; t++)
			{
				for (short b = 0; b < _film.NumberOfBlocks; b++)
				{
					short time = 0;
					if (_film.Blocks[b].TypeNum != 3)
						for (int chunk = 0; chunk < _film.Blocks[b].NumberOfChunks; chunk++)
						{
							Film.Chunk c = _film.Blocks[b].Chunks[chunk];
							if (c.Code == Film.Chunk.OpCode.Time) time = c.Vars[0];
							if (time < t) continue;
							if ((c.Code == Film.Chunk.OpCode.End) || (time > t)) break;

							if (_film.Blocks[b].Type == Film.Block.BlockType.View && c.Code == Film.Chunk.OpCode.Transition)
							{
								Event view = new Event
								{
									Time = t,
									BlockIndex = b,
									ViewTransition = c.Vars[0],
									ViewParameter = c.Vars[1]
								};
								_events.Add(view);
							}

							else if (_film.Blocks[b].Type == Film.Block.BlockType.Pltt && c.Code == Film.Chunk.OpCode.Use)
							{
								Event pal = new Event
								{
									Time = t,
									BlockIndex = b,
									LoadPalette = true
								};
								_events.Add(pal);
							}

							else if (_film.Blocks[b].Type == Film.Block.BlockType.Voic && c.Code != Film.Chunk.OpCode.Preload)  // don't need Preload, we've already done it
							{
								if (c.Code == Film.Chunk.OpCode.Sound)
								{
									//simple sounds, OnOff/Vol//?
									if (c.Vars[0] == 1)
									{
										// new event
										Event sound = new Event
										{
											Time = t,
											BlockIndex = b,
											Start = true,
											Volume = c.Vars[1]
										};
										// TODO: looping
										_events.Add(sound);
									}
									else if (c.Vars[0] == 0)
									{
										// modify existing event, unk
									}
								}
								else if (c.Code == Film.Chunk.OpCode.Stereo)
								{
									//adv sound, OnOff/Vol///Balance/?/?
									if (c.Vars[0] == 1)
									{
										// new event
										Event stereo = new Event
										{
											Time = t,
											BlockIndex = b,
											Start = true,
											Volume = c.Vars[1],
											Balance = c.Vars[4]
										};
										_events.Add(stereo);
									}
									else if (c.Vars[0] == 0)
									{
										// modify existing event, unk
									}
								}
							}
						}
					else
					{
						// these are split out due to multiple chunks needing to be processed for a single event
						if (_film.Blocks[b].Type == Film.Block.BlockType.Cust || _film.Blocks[b].Chunks[0].Code == Film.Chunk.OpCode.End) continue;
						// there will be special cases like REGISTER where the door has no chunks, but is triggered by the EXE

						Event image = new Event { BlockIndex = b };
						image.Frame = -1;   // this'll get reset if explicitly set, otherwise use defaults'
						image.Time = -1;
						image.Framerate = _unused;

						for (int chunk = 0; chunk < _film.Blocks[b].NumberOfChunks; chunk++)
						{
							Film.Chunk c = _film.Blocks[b].Chunks[chunk];
							if (c.Code == Film.Chunk.OpCode.Time) time = c.Vars[0];
							if (time < t) continue;
							if ((c.Code == Film.Chunk.OpCode.End) || (time > t)) break;
							image.Time = time;

							if (c.Code == Film.Chunk.OpCode.Move)
							{
								image.SetPosition = true;
								image.X = c.Vars[0];
								image.Y = c.Vars[1];
							}
							else if (c.Code == Film.Chunk.OpCode.Speed)
							{
								image.XRate = c.Vars[0];
								image.YRate = c.Vars[1];
							}
							else if (c.Code == Film.Chunk.OpCode.Layer) image.Layer = c.Vars[0];
							else if (c.Code == Film.Chunk.OpCode.Frame && _film.Blocks[image.BlockIndex].Type == Film.Block.BlockType.Anim)
							{
								// for some reason there's DELTs with this assigned, which shouldn't do anything
								image.Frame = c.Vars[0];
							}
							else if (c.Code == Film.Chunk.OpCode.Animation)
							{
								// TODO: work out c.V[1]
								image.Framerate = c.Vars[0];
							}
							// skip Event and Region
							else if (c.Code == Film.Chunk.OpCode.Window)
							{
								image.Left = c.Vars[0];
								image.Top = c.Vars[1];
								image.Right = c.Vars[2];
								image.Bottom = c.Vars[3];
							}
							// skip Shift for now, need to find examples
							else if (c.Code == Film.Chunk.OpCode.Orientation)
							{
								image.FlipX = (c.Vars[0] == 1);
								image.FlipY = (c.Vars[1] == 1);
							}
							else if (c.Code == Film.Chunk.OpCode.Display)
							{
								image.ToggleDisplay = true;
								image.Display = (c.Vars[0] == 1);
							}
						}
						if (image.Time == t) _events.Add(image);
					}
				}
			}
		}

		void loadPltt(string id)
		{
			Pltt p = (Pltt)_lfd.Resources[id];
			if (p != null)
				for (int i = p.StartIndex; i <= p.EndIndex; i++)
					_palette.Entries[i] = p.Entries[i];
			_palette.Entries[0] = Color.Fuchsia;
		}
		void loadEmpire()
		{
			if (_stars != null) return;	// already loaded in, can skip over since it's static
			string dir = Path.GetDirectoryName(_lfd.FilePath) + "\\";
			LfdFile empire = new LfdFile(dir + "EMPIRE.LFD");
			_stars = (Delt)empire.Resources["DELTstars"];
			_empirePalette = ((Pltt)empire.Resources["PLTTstandard"]).Palette;
			_empirePalette.Entries[0] = Color.Fuchsia;
			_stars.Palette = _empirePalette;
			_tiesfx = new LfdFile(dir + "TIESFX.LFD");
			_tiesfx2 = new LfdFile(dir + "TIESFX2.LFD");
			_tiespch = new LfdFile(dir + "TIESPCH.LFD");
			_tiespch2 = new LfdFile(dir + "TIESPCH2.LFD");
		}

		void performEvents()
		{
			if (_time == 0)
				for (int i = 0; i < _images.Length; i++)
				{
					_images[i].StartTime = -1;
					_images[i].Layer = _unused;
					_images[i].ProcessedImage = null;
					_images[i].Frame = 0;
				}

			// Process Animate and Move updates prior to potentially reassigning values
			for (int b = 0; b < _images.Length; b++)
			{
				if (_images[b].StartTime == -1 || _images[b].StartTime > _time) continue;

				if (_images[b].StartTime < _time)
				{
					_images[b].X += _images[b].XRate;
					_images[b].Y += _images[b].YRate;
					if (_images[b].FrameRate != 0)
					{
						var res = _lfd.Resources[lstBlocks.Items[b].ToString()];
						if (res != null && res.Type == Resource.ResourceType.Anim)
						{
							Anim a = (Anim)res;
							if (_images[b].Frame + _images[b].FrameRate >= a.NumberOfFrames) _images[b].Frame = (short)(a.NumberOfFrames - 1);
							else if (_images[b].Frame + _images[b].FrameRate < 0) _images[b].Frame = 0;
							else _images[b].Frame += _images[b].FrameRate;

							_images[b].ProcessedImage = (Bitmap)a.Frames[_images[b].Frame].Image.Clone();
							_images[b].ProcessedImage.RotateFlip(_images[b].FlipType);
							_images[b].ProcessedImage.MakeTransparent(Color.Fuchsia);
						}
					}
				}
			}

			bool needToSort = false;
			for (int ev = 0; ev < _events.Count; ev++)
			{
				if (_events[ev].Time > _time) break;
				if (_events[ev].Time < _time) continue;

				Event e = _events[ev];

				if (e.LoadPalette) loadPltt(_film.Blocks[e.BlockIndex].ToString());

				if (_film.Blocks[e.BlockIndex].TypeNum == 3)
				{
					if (e.ToggleDisplay && !e.Display)
					{
						_images[e.BlockIndex].IsVisible = false;
						needToSort = true;
					}
					else if (e.ToggleDisplay && e.Display)
					{
						_images[e.BlockIndex].IsVisible = true;
						_images[e.BlockIndex].StartTime = (short)_time;
						needToSort = true;
					}

					if (e.FlipX && e.FlipY) _images[e.BlockIndex].FlipType = RotateFlipType.RotateNoneFlipXY;
					else if (e.FlipX) _images[e.BlockIndex].FlipType = RotateFlipType.RotateNoneFlipX;
					else if (e.FlipY) _images[e.BlockIndex].FlipType = RotateFlipType.RotateNoneFlipY;
					_images[e.BlockIndex].XRate = e.XRate;
					_images[e.BlockIndex].YRate = e.YRate;
					if (e.Left != e.Right) _images[e.BlockIndex].Window = new Rectangle(e.Left, e.Top, e.Right - e.Left + 1, e.Bottom - e.Top + 1);

					if (_images[e.BlockIndex].ProcessedImage == null)
					{
						// this prevents Layer from being redefined, assuming it never gets reset
						_images[e.BlockIndex].Layer = e.Layer;

						if (_film.Blocks[e.BlockIndex].ToString() == _stars.ToString())
						{
							_images[e.BlockIndex].ProcessedImage = (Bitmap)_stars.Image.Clone();
							// DELTstars is at 0,0, so we can just use these, which default to 0,0 anyway
							_images[e.BlockIndex].X = e.X;
							_images[e.BlockIndex].Y = e.Y;
							_images[e.BlockIndex].ProcessedImage.RotateFlip(_images[e.BlockIndex].FlipType);
						}
						else
						{
							for (int r = 0; r < _lfd.Resources.Count; r++)
							{
								if (_lfd.Resources[r].ToString() == _film.Blocks[e.BlockIndex].ToString())
								{
									if (_lfd.Resources[r].Type == Resource.ResourceType.Delt)
									{
										Delt d = (Delt)_lfd.Resources[r];
										d.Palette = _palette;
										_images[e.BlockIndex].ProcessedImage = (Bitmap)d.Image.Clone();
										_images[e.BlockIndex].X = (short)(d.Left + e.X);
										_images[e.BlockIndex].Y = (short)(d.Top + e.Y);
										_images[e.BlockIndex].ProcessedImage.RotateFlip(_images[e.BlockIndex].FlipType);
									}
									else if (_lfd.Resources[r].Type == Resource.ResourceType.Anim)
									{
										Anim a = (Anim)_lfd.Resources[r];
										a.SetPalette(_palette);
										a.RelativePosition = true;

										// either going to define the frame, or set a motion
										if (e.Frame != -1) _images[e.BlockIndex].Frame = e.Frame;
										_images[e.BlockIndex].FrameRate = e.Framerate;
										_images[e.BlockIndex].ProcessedImage = (Bitmap)a.Frames[_images[e.BlockIndex].Frame].Image.Clone();
										_images[e.BlockIndex].X = (short)(a.Left + e.X);
										_images[e.BlockIndex].Y = (short)(a.Top + e.Y);
										_images[e.BlockIndex].ProcessedImage.RotateFlip(_images[e.BlockIndex].FlipType);
									}
									// currently no CUST processing, but I think it's the same as DELT
									break;
								}
							}
						}
						_images[e.BlockIndex].ProcessedImage.MakeTransparent(Color.Fuchsia);
					}
					else
					{
						if (e.Layer != 0) Debug.WriteLine("Layer reassignment: " + lstBlocks.Items[e.BlockIndex].ToString() + " Time: " + e.Time);

						var res = _lfd.Resources[lstBlocks.Items[e.BlockIndex].ToString()];
						if (res != null && res.Type == Resource.ResourceType.Anim)
						{
							Anim a = (Anim)res;
							a.SetPalette(_palette);
							a.RelativePosition = true;

							if (e.Frame != -1) _images[e.BlockIndex].Frame = e.Frame;
							if (e.Framerate != _unused) _images[e.BlockIndex].FrameRate = e.Framerate;
							_images[e.BlockIndex].ProcessedImage = (Bitmap)a.Frames[_images[e.BlockIndex].Frame].Image.Clone();
							if (e.SetPosition)
							{
								_images[e.BlockIndex].X = (short)(a.Left + e.X);
								_images[e.BlockIndex].Y = (short)(a.Top + e.Y);
							}
							_images[e.BlockIndex].ProcessedImage.RotateFlip(_images[e.BlockIndex].FlipType);
							_images[e.BlockIndex].ProcessedImage.MakeTransparent(Color.Fuchsia);
						}
						else if (res != null && res.Type == Resource.ResourceType.Delt && e.SetPosition)
						{
							Delt d = (Delt)res;
							_images[e.BlockIndex].X = (short)(d.Left + e.X);
							_images[e.BlockIndex].Y = (short)(d.Top + e.Y);
						}
					}
				}

				if (e.Start && _isPlaying)
				{
					playWav(e.BlockIndex, e.Volume, e.Balance);
				}
			}
			if (needToSort) sortLayers();

			// TODO: View transitions
			// TODO: audio looping
		}

		/// <summary>Takes the raw block layer info, sorts it into _drawOrder</summary>
		void sortLayers()
		{
			short[] layers = new short[_film.NumberOfBlocks];
			for (int b = 0; b < _film.NumberOfBlocks; b++)
			{
				_drawOrder[b] = (short)(_images[b].Layer != _unused && _images[b].IsVisible ? b : -1);
				layers[b] = _images[b].Layer;
			}

			for (int i = 1; i < layers.Length; i++)
				for (int j = i; j > 0; j--)
					if (layers[j] > layers[j - 1])
					{
						short t = layers[j];
						layers[j] = layers[j - 1];
						layers[j - 1] = t;
						t = _drawOrder[j];
						_drawOrder[j] = _drawOrder[j - 1];
						_drawOrder[j - 1] = t;
					}
					else break;
		}

		// TODO: should figure out mouse clicks/drags for pctView
		private void lstBlocks_SelectedIndexChanged(object sender, EventArgs e)
		{
			pctView.Refresh();
			if (_film.Blocks[lstBlocks.SelectedIndex].TypeNum == 3) BoxImage(lstBlocks.SelectedIndex);
			if (_film.Blocks[lstBlocks.SelectedIndex].Type == Film.Block.BlockType.Voic) playWav(lstBlocks.SelectedIndex, 100, 0);
			if (_fEvent == null || !_fEvent.Created)
			{
				_fEvent = new EventForm(ref _film.Blocks[lstBlocks.SelectedIndex]);
				_fEvent.Show();
				_fEvent.Left = Left + Width + 5;
				_fEvent.Top = Top;
			}
			else _fEvent.LoadBlock(ref _film.Blocks[lstBlocks.SelectedIndex]);
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
				performEvents();
				//updateView();
				PaintFilm();
			}
			else
			{
				_isPlaying = false;
				tmrPlayback.Stop();
				cmdPlayPause.Text = ">";
				for (int i = 0; i < _activeSounds.Count; i++) _activeSounds[i].Stop();
			}
		}

		private void hsbTime_ValueChanged(object sender, EventArgs e)
		{
			_time = hsbTime.Value;
			lblTime.Text = _time.ToString();
			if (_loading) return;

			if (_time == 0)
			{
				_isPlaying = false;
				tmrPlayback.Stop();
				cmdPlayPause.Text = ">";
				for (int i = 0; i < _activeSounds.Count; i++) _activeSounds[i].Stop();
			}
			performEvents();
			PaintFilm();
		}

		private void cmdStart_Click(object sender, EventArgs e)
		{
			hsbTime.Value = 0;
		}

		private void form_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!Directory.Exists(MainForm._tempDir)) return;
			_activeSounds.Clear();
			string[] files = Directory.GetFiles(MainForm._tempDir);
			for (int i = 0; i < files.Length; i++) File.Delete(files[i]);
			if (_fEvent != null) _fEvent.Close();
		}

		public struct Event
		{
			public short Time;
			public short BlockIndex;

			// View
			public short ViewTransition;
			public short ViewParameter;

			// Images
			public bool ToggleDisplay;
			public bool Display;
			public short Layer;
			public bool SetPosition;
			public short X;
			public short Y;
			public short XRate;
			public short YRate;
			public short Frame;
			public short Framerate;
			public short Left;
			public short Top;
			public short Right;
			public short Bottom;
			public bool FlipX;
			public bool FlipY;

			// Palette
			public bool LoadPalette;	// not really necessary, since the PLTT event won't be created if false

			// Audio
			public short LoopCount;
			public short Volume;
			public short Balance;
			public bool Start;
		}

		public struct Image
		{
			public bool IsVisible;
			public short StartTime;
			public short X;
			public short Y;
			public short Layer;
			public short XRate;
			public short YRate;
			public short Frame;
			public short FrameRate;
			public Bitmap ProcessedImage;
			public Rectangle Window;
			public RotateFlipType FlipType;
		}
	}
}
