﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Beeline.BeeZap.Infrastructure;
using Beeline.BeeZap.Model;
using Beeline.BeeZap.Properties;

namespace Beeline.BeeZap
{
	public partial class MainView : Form, IMainView
	{
		private readonly MainPresenter _presenter;

		public MainView(MainPresenter presenter)
		{
			InitializeComponent();

			_presenter = presenter;
			_presenter.Register(this);
		}

		private void MainFormLoad(Object sender, EventArgs ea)
		{
			_presenter.Loading();

#if DEBUG
			pathTextBox.Text = "C:\\TEMP\\html";
			searchTextBox.Text = " | <a href=\"http://www.trade-schools.net/advertise.asp\" target=\"_blank\" rel=\"nofollow\">Advertise</a>";
#endif
		}

		public Parameters ReadParameters()
		{
			Int32 excMaxLine;
			Int32.TryParse(maxLineExcludeTextBox.Text, out excMaxLine);
			Int32 excMinLine;
			Int32.TryParse(minLineExcludeTextBox.Text, out excMinLine);
			Int32 incMaxLine;
			Int32.TryParse(maxLineIncludeTextBox.Text, out incMaxLine);
			Int32 incMinLine;
			Int32.TryParse(minLineIncludeTextBox.Text, out incMinLine);

			return new Parameters()
			{
				ExplicitCapture = explicitCaptureCheckBox.Checked,
				FileContentExcludeMaxLine = excMaxLine,
				FileContentExcludeMinLine = excMinLine,
				FileContentExcludePattern = fileContentExcludeTextBox.Text,
				FileContentIncludeMaxLine = incMaxLine,
				FileContentIncludeMinLine = incMinLine,
				FileContentIncludePattern = fileContentIncludeTextBox.Text,
				FindPattern = searchTextBox.Text,
				FullNameExcludePattern = fullNameExcludeTextBox.Text,
				FullNameIncludePattern = fullNameIncludeTextBox.Text,
				IgnoreCase = ignoreCaseCheckBox.Checked,
				IncludeSubDirectories = subdirectoriesRadioButton.Checked,
				IsRegex = regularExpressionCheckBox.Checked,
				LiteralReplacement = literalReplacementCheckBox.Checked,
				Multiline = multilineCheckBox.Checked,
				Path = pathTextBox.Text,
				Pattern = patternTextBox.Text,
				ReplacePattern = replaceTextBox.Text,
				Singleline = singlelineCheckBox.Checked
			};
		}

		public void UpdateControlStates()
		{
			explicitCaptureCheckBox.Enabled = regularExpressionCheckBox.Checked;
			ignoreCaseCheckBox.Enabled = regularExpressionCheckBox.Checked;
			singlelineCheckBox.Enabled = regularExpressionCheckBox.Checked;
			multilineCheckBox.Enabled = regularExpressionCheckBox.Checked;
			literalReplacementCheckBox.Enabled = regularExpressionCheckBox.Checked;
			if (!literalReplacementCheckBox.Enabled)
				literalReplacementCheckBox.Checked = true;
		}

		public void AppendToLog(String message)
		{
			logTextBox.AppendText(message);
		}

		public void ClearLog()
		{
			logTextBox.Clear();
		}

		public void BeginCancelableOperation()
		{
			this.UiThread(BeginCancelableOperationImp);
		}

		private void BeginCancelableOperationImp()
		{
			foreach (ToolStripButton button in toolStripButtons.Items)
			{
				button.Enabled = false;
			}
			stopButton.Enabled = true;
		}

		public void EndCancelableOperation()
		{
			this.UiThread(EndCancelableOperationImp);
		}

		private void EndCancelableOperationImp()
		{
			foreach (ToolStripButton button in toolStripButtons.Items)
			{
				button.Enabled = true;
			}
			stopButton.Enabled = false;
		}

		private void BrowseButtonClick(object sender, EventArgs e)
		{
			var path = _presenter.ChoosePath();
			if (!String.IsNullOrWhiteSpace(path))
				pathTextBox.Text = path;
		}

		private void RegularExpressionCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			UpdateControlStates();
		}

		private void ViewFilesButtonClick(object sender, EventArgs e)
		{
			_presenter.ViewFiles();
		}

		private void ViewMatchesButtonClick(object sender, EventArgs e)
		{
			_presenter.ViewMatches();
		}

		private void ExecuteButtonClick(object sender, EventArgs e)
		{
			_presenter.Replace();
		}

		private void UndoButtonClick(object sender, EventArgs e)
		{
			var result = MessageBox.Show(this, Resources.UndoChangesConfirmationText, Resources.UndoChangesConfirmationCaption, MessageBoxButtons.YesNo);
			if (result == DialogResult.Yes)
			{
				_presenter.Undo();
			}
		}

		private void DeleteBackupsButtonClick(object sender, EventArgs e)
		{
			_presenter.DeleteBackups();
		}

		private void ClearLogButtonClick(object sender, EventArgs e)
		{
			_presenter.ClearLog();
		}

		private void StopButtonClick(object sender, EventArgs e)
		{
			_presenter.Stop();
		}

		private void MainView_FormClosing(object sender, FormClosingEventArgs e)
		{
			_presenter.Quit();
		}

		private void LogTextBoxDoubleClick(object sender, EventArgs e)
		{
			TextBox tb = sender as TextBox;
			MouseEventArgs mea = e as MouseEventArgs;

			if (tb == null || mea == null)
				return;

			Int32 index = tb.GetLineFromCharIndex(tb.SelectionStart);

			if (tb.Lines[index].Length == 0)
				return;

			while (tb.Lines[index][0] == '\t' && index >= 0) {
				index--;
			}

			Int32 selStart = tb.GetFirstCharIndexFromLine(index);

			List<String> lines = new List<String> {
				tb.Lines[index++]
			};

			while (tb.Lines[index].Length != 0 && tb.Lines[index][0] == '\t' && index <= tb.Lines.Length) {
				lines.Add(tb.Lines[index++]);
			}

			Int32 selEnd = tb.GetFirstCharIndexFromLine(--index) + tb.Lines[index].Length;

			tb.Select(selStart, selEnd - selStart);

			_presenter.LogEntrySelected(lines);
		}
	}
}