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
			//Debug.WriteLine("painting...");
			Bitmap image = new Bitmap(640, 480);
			Graphics g = Graphics.FromImage(image);
			for (int i = 0; i < _drawOrder.Length; i++)
			{
				if (_drawOrder[i] == -1 || _images[_drawOrder[i]].ProcessedImage == null) continue;
				g.DrawImageUnscaled(_images[_drawOrder[i]].ProcessedImage, _images[_drawOrder[i]].X, _images[_drawOrder[i]].Y);
				//Debug.WriteLine("image drawn: " + lstBlocks.Items[_drawOrder[i]]);
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
			updateView();
			PaintFilm();
		}
		/// <summary>Paint a box around the extents of the image</summary>
		/// <param name="index">Block index</param>
		public void BoxImage(int index)
		{
			// TODO: fix BoxImage so it'll work on any visible frame
			string str = lstBlocks.Items[index].ToString();
			str = str.Replace("*", "");
			Resource img = _lfd.Resources[str];
			if (img == null)
				if (lstBlocks.Items[index].ToString() == _stars.ToString() + "*") img = _stars;
				else return;
			if (_film.Blocks[index].Chunks[0].Code == Film.Chunk.OpCode.End || _film.Blocks[index].Chunks[0].Vars[0] != 0) return; // skip stuff not on first frame
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
		void playWav(int blockIndex)
		{
			playWav(lstBlocks.Items[blockIndex].ToString());
		}
		void playWav(Film.Block block)
		{
			playWav(block.ToString());
		}
		void playWav(string id)
		{
			var plr = new System.Windows.Media.MediaPlayer();
			plr.MediaEnded += mediaPlayer_MediaEnded;
			try
			{
				plr.Open(new Uri(MainForm._tempDir + id + ".wav"));
				// plr.Volume is 0-1, default is .5
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
									//simple sounds, OnOff/Vol/FadeVar?/FadeVar
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
										// modify existing event
									}
								}
								else if (c.Code == Film.Chunk.OpCode.Stereo)
								{
									//adv sound, OnOff/Vol///Balance/FadeVar/FadeVar
									if (c.Vars[0] == 1)
									{
										// new event
										Event stereo = new Event
										{
											Time = t,
											BlockIndex = b,
											Volume = c.Vars[1],
											Balance = c.Vars[4]
										};
										_events.Add(stereo);
									}
									else if (c.Vars[0] == 0)
									{
										// modify existing event
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
						for (int chunk = 0; chunk < _film.Blocks[b].NumberOfChunks; chunk++)
						{
							Film.Chunk c = _film.Blocks[b].Chunks[chunk];
							if (c.Code == Film.Chunk.OpCode.Time) time = c.Vars[0];
							if (time < t) continue;
							if ((c.Code == Film.Chunk.OpCode.End) || (time > t)) break;

							if (c.Code == Film.Chunk.OpCode.Move)
							{
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
								// TODO: this can't be right. c.V[0] can be -1, 0 or 1. c.V[1] has a range, but default is 0
								image.Animate = (c.Vars[0] != 0);
								image.Framerate = c.Vars[1];
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
							else if (c.Code == Film.Chunk.OpCode.Display) image.Display = (c.Vars[0] == 1);
						}
						_events.Add(image);
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

		void performEvents()
		{
			//TODO: event processing

			short unused = -2527;  // there are negative layers in a few cases, need to make room for them
			bool needToSort = false;
			for (int ev = 0; ev < _events.Count; ev++)
			{
				if (_events[ev].Time > _time) break;
				if (_events[ev].Time < _time) continue;

				Event e = _events[ev];

				if (e.LoadPalette) loadPltt(_film.Blocks[e.BlockIndex].ToString());

				if (_film.Blocks[e.BlockIndex].TypeNum == 3)
				{
					needToSort = true;
					if (!e.Display)
					{
						_images[e.BlockIndex].Layer = unused;
						// TODO: there are cases where commands are also assigned during an OFF code
					}
					else
					{
						_images[e.BlockIndex].Layer = e.Layer;
						if (e.FlipX && e.FlipY) _images[e.BlockIndex].FlipType = RotateFlipType.RotateNoneFlipXY;
						else if (e.FlipX) _images[e.BlockIndex].FlipType = RotateFlipType.RotateNoneFlipX;
						else if (e.FlipY) _images[e.BlockIndex].FlipType = RotateFlipType.RotateNoneFlipY;
						_images[e.BlockIndex].XRate = e.XRate;
						_images[e.BlockIndex].YRate = e.YRate;
						if (e.Left != 0 || e.Right != 0) _images[e.BlockIndex].Window = new Rectangle(e.Left, e.Top, e.Right - e.Left + 1, e.Bottom - e.Top + 1);

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

										_images[e.BlockIndex].Frame = e.Frame;
										_images[e.BlockIndex].FrameRate = e.Framerate;
										_images[e.BlockIndex].ProcessedImage = (Bitmap)a.Frames[e.Frame].Image.Clone();
										_images[e.BlockIndex].X = (short)(a.Left + e.X);
										_images[e.BlockIndex].Y = (short)(a.Top + e.Y);
										// Anim flips will be handled later
									}
									// currently no CUST processing, but I think it's the same as DELT
									break;
								}
							}
						}
						_images[e.BlockIndex].ProcessedImage.MakeTransparent(Color.Black);
					}
				}
			}
			if (needToSort) sortLayers();

			// update images: animation frames, Moves, Windows
			// View transitions
			// update Volume/balance, audio looping
		}

		/// <summary>Takes the raw block layer info, sorts it into _drawOrder</summary>
		void sortLayers()
		{
			short[] layers = new short[_film.NumberOfBlocks];
			for (int b = 0; b < _film.NumberOfBlocks; b++)
			{
				_drawOrder[b] = (short)(_images[b].Layer != -2527 ? b : -1);
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
		
		void updateView()
		{
			for (int b = 0; b < _film.NumberOfBlocks; b++) _drawOrder[b] = -1;
			for (short b = 0; b < _film.NumberOfBlocks; b++)
			{
				string str = lstBlocks.Items[b].ToString();
				if (str.StartsWith("PLTT") && str.IndexOf("*") == -1)
					foreach (Film.Chunk c in _film.Blocks[b].Chunks)
						if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] == _time) loadPltt(_film.Blocks[b].ToString());

				_images[b].Layer = -2527;   // there are negative layers in a few cases, need to make room for them
				if (_film.Blocks[b].TypeNum == 3)
					foreach (Film.Chunk c in _film.Blocks[b].Chunks)
					{
						if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] > _time) break;
						if (c.Code == Film.Chunk.OpCode.Display && c.Vars[0] == 0) { _images[b].Layer = -2527; break; } // hidden
						if (c.Code == Film.Chunk.OpCode.Display && c.Vars[0] == 1 && _images[b].Layer == -2527) { _images[b].Layer = 0; }    // default (top)
						if (c.Code == Film.Chunk.OpCode.Layer) { _images[b].Layer = c.Vars[0]; }
					}
			}
			sortLayers();

			#region get images
			for (int i = 0; i < _drawOrder.Length; i++)
			{
				int block = _drawOrder[i];
				if (block == -1 || _film.Blocks[block].Type == Film.Block.BlockType.Cust) continue;
				if (_film.Blocks[block].ToString() == _stars.ToString())
				{
					_images[block].ProcessedImage = _stars.Image;
					_images[block].ProcessedImage.MakeTransparent(Color.Black);
					_images[block].X = _stars.Left;
					_images[block].Y = _stars.Top;
				}
				for (int j = 0; j < _lfd.Resources.Count; j++)
					if (_lfd.Resources[j].ToString() == _film.Blocks[block].ToString())
					{
						if (_lfd.Resources[j].Type == Resource.ResourceType.Delt)
						{
							Delt d = (Delt)_lfd.Resources[j];
							d.Palette = _palette;
							_images[block].ProcessedImage = (Bitmap)d.Image.Clone();
							_images[block].ProcessedImage.MakeTransparent(Color.Black);
							_images[block].X = d.Left;
							_images[block].Y = d.Top;
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
								else if (c.Code == Film.Chunk.OpCode.Frame) _images[block].Frame = c.Vars[0];
								else if (c.Code == Film.Chunk.OpCode.Animation)
								{
									_images[block].FrameRate = c.Vars[0];
									//_animation[block, 2] = c.Vars[1];
								}
								else if (c.Code == Film.Chunk.OpCode.Time && c.Vars[0] > _time) break;
							}
							_images[block].ProcessedImage = (Bitmap)a.Frames[_images[block].Frame].Image.Clone();
							_images[block].ProcessedImage.MakeTransparent(Color.Black);
							_images[block].X = a.Left;
							_images[block].Y = a.Top;
						}
						// currently no CUST processing, but I think it's the same as DELT
						break;
					}
				foreach (Film.Chunk c in _film.Blocks[block].Chunks)
				{
					if (c.Code == Film.Chunk.OpCode.Move)
					{
						_images[block].X += c.Vars[0];
						_images[block].Y += c.Vars[1];
					}
					else if (c.Code == Film.Chunk.OpCode.Window)
					{
						Debug.WriteLine("Window code detected: " + _film.Blocks[block].ToString());
					}
					else if (c.Code == Film.Chunk.OpCode.Shift)
					{
						Debug.WriteLine("Shift code detected: " + _film.Blocks[block].ToString());
					}
					else if (c.Code == Film.Chunk.OpCode.Orientation && _images[block].ProcessedImage != null)
					{
						if (c.Vars[0] == 1) _images[block].ProcessedImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
						if (c.Vars[1] == 1) _images[block].ProcessedImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
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
						else if ((c.Code == Film.Chunk.OpCode.Sound || c.Code == Film.Chunk.OpCode.Stereo) && c.Vars[0] == 1 && currentTime == _time && _isPlaying) playWav(b);
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
			if (_film.Blocks[lstBlocks.SelectedIndex].Type == Film.Block.BlockType.Voic) playWav(lstBlocks.SelectedIndex);
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
				updateView();
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
			lblTime.Text = _time.ToString("x4");
			if (_loading) return;
			updateView();
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
			//public Film.Chunk.OpCode OpCode;
			//public short[] Vars;

			// View
			public short ViewTransition;
			public short ViewParameter;

			// Images
			public bool Display;
			public short Layer;
			public short X;
			public short Y;
			public short XRate;
			public short YRate;
			public bool Animate;
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
