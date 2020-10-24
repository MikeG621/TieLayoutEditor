using System;
using System.IO;
using System.Windows.Forms;
using Idmr.LfdReader;

namespace Idmr.TieLayoutEditor
{
	public partial class MainForm : Form
	{
		string _filePath = "";
		ViewForm _fView = null;
		LfdFile _lfd;

		public MainForm()
		{
			InitializeComponent();
			Height = 376;
			Left = 10;
			Top = 100;
		}

		private void FileOpen()
		{
			FileStream stream = null;
			try
			{
				try { _fView.Close(); }
				catch { /* do nothing */ }
				lstFILM.Items.Clear();
				grpFILM.Enabled = false;
				stream = File.OpenRead(txtFilename.Text);
				_filePath = txtFilename.Text;
				if (Resource.GetType(stream, 0) != Resource.ResourceType.Rmap) { stream.Close(); return; }	// no RMAP found, FILMs never solo
				_lfd = new LfdFile(stream);
				for (int i = 0; i < _lfd.Resources.Count; i++)
					if (_lfd.Resources[i].Type == Resource.ResourceType.Film)
						_lfd.Resources[i].Tag = Resource.ResourceType.Film + lstFILM.Items.Add(_lfd.Resources[i].Name).ToString();
				if (lstFILM.Items.Count == 1)
				{
					lstFILM.SelectedIndex = 0;
					cmdLoad_Click("Load", new EventArgs());
				}
			}
			catch (Exception x)
			{
				MessageBox.Show(x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				stream.Close();
				return;
			}
		}

		private void cmdFileOpen_Click(object sender, EventArgs e)
		{
			opnFile.ShowDialog();
		}
		private void opnFile_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			txtFilename.Text = opnFile.FileName;
			if (_fView != null && _fView.Created) _fView.Close();
			FileOpen();
		}
		private void txtFilename_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (Convert.ToInt32(e.KeyChar) == 13) FileOpen();
		}
		private void cmdLoad_Click(object sender, EventArgs e)
		{
			if (lstFILM.SelectedIndex == -1) return;
			grpFILM.Enabled = true;
			Film film = (Film)_lfd.Resources.GetResourceByTag(Resource.ResourceType.Film + lstFILM.SelectedIndex.ToString());
			txtFILM.Text = film.Name;
			numBlocks.Value = film.NumberOfBlocks;
			numFrames.Value = film.NumberOfFrames;
			if (_fView == null || !_fView.Created)
			{
				_fView = new ViewForm(ref _lfd, film.Tag)
				{
					Left = Left + Width + 5,
					Top = Top
				};
				_fView.Show();
			}
			else _fView.LoadFilm(ref _lfd, film.Tag);
		}
		private void cmdSave_Click(object sender, EventArgs e)
		{
			// TODO: save FILM
		}
	}
}
