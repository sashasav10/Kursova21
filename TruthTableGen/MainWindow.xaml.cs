using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace TruthTableGen
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Button_Click(object sender, EventArgs e)
		{
			try
			{
				//відображаємо кнопки, браузер та текстові поля
				Pcnf.Visibility=Visibility.Visible;
				Pdnf.Visibility=Visibility.Visible;
				TruthTable.Visibility=Visibility.Visible;
				WB_Pcnf.Visibility = Visibility.Visible;
				WB_Pdnf.Visibility = Visibility.Visible;
				WB_TruthTable.Visibility = Visibility.Visible;
				TTOpenInBrowser_Button.Visibility = Visibility.Visible;
				PcnfOpenInBrowser_Button.Visibility = Visibility.Visible;
				PdnfOpenInBrowser_Button.Visibility = Visibility.Visible;
				TTSave_Button.Visibility = Visibility.Visible;
				PcnfSave_Button.Visibility = Visibility.Visible;
				PdnfSave_Button.Visibility = Visibility.Visible;

				//Відформатування вхідного тексту, щоб змінити оператори
				Query.Text = FormatInput(Query.Text);

				//Створення екземпляра класу
				Evaluator evaluatorTe = new EvaluatorText(Query.Text);

				//Відображення заголовка нa вкладці
				PdnfLabel.Content = PcnfLabel.Content = TableLabel.Content = "Вираз: " + evaluatorTe.Query;

				//Оновлення таблиці істиності, ДДНФ та ДКНФ
				TruthTable.ItemsSource = evaluatorTe.EvaluateQuery();
                Pcnf.Text = evaluatorTe.FindPCNF();
                Pdnf.Text = evaluatorTe.FindPDNF();

				//Створення екземпляра класу для html
				Evaluator evaluatorHt = new EvaluatorHtml(Query.Text);
				//таблиця
				StreamWriter stream = new StreamWriter("wwwroot\\index1.html");
				stream.WriteLine("<!DOCTYPE html>" + Environment.NewLine +
						"<html><head><meta charset=\"UTF-8\"><link rel=\"stylesheet\" href=\"style.css\"></head><body>" + Environment.NewLine
						+ evaluatorHt.EvaluateQuery() + "</body></html>");
				stream.Close();
				////відкриваємо сторінку всередині програми
				WB_TruthTable.Navigate(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\index1.html");

				//ДКНФ html
				StreamWriter stream2 = new StreamWriter("wwwroot\\index2.html");
			stream2.WriteLine("<!DOCTYPE html>" + Environment.NewLine +
					"<html><head><meta charset=\"UTF-8\"><link rel=\"stylesheet\" href=\"style.css\"></head><body>" + Environment.NewLine
					+ evaluatorHt.FindPCNF() + "</body></html>");
			stream2.Close();
			//відкриваємо сторінку всередині програми
			WB_Pcnf.Navigate(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\index2.html");

				//ДДНФ  html
				StreamWriter stream3 = new StreamWriter("wwwroot\\index3.html");
			stream3.WriteLine("<!DOCTYPE html>" + Environment.NewLine +
					"<html><head><meta charset=\"UTF-8\"><link rel=\"stylesheet\" href=\"style.css\"></head><body>" + Environment.NewLine
					+ evaluatorHt.FindPDNF() + "</body></html>");
			stream3.Close();
			////відкриваємо сторінку всередині програми
			WB_Pdnf.Navigate(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\index3.html");

				
			}

			catch
			{
				//Якщо взагалі щось піде не так
				//Єдиний можливий випадок, коли символи не збалансовані
				//Або в текстовому полі немає введення
				if (Query.Text.Length == 0) { MessageBox.Show("Введіть вираз", "Немає виразу"); }
				else { MessageBox.Show("Попередження: знайдені незбалансовані символи", "Помилка"); }
			}
		}

		// Буде викликано для оновлення елементів форми при зміні розміру вікна
		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
			//Виправляє параметри TabContainer
			TabContainer.Width = e.NewSize.Width - 40;
            TabContainer.Height = e.NewSize.Height - 150;

			//Виправляє параметри Go Button
			var goMargin = Go.Margin;
            goMargin.Left = TabContainer.Width + TabContainer.Margin.Left - Go.Width;
            Go.Margin = goMargin;

			//Виправляє параметри Query TextBox
			var queryMargin = Query.Margin;
            queryMargin.Left = TabContainer.Margin.Left;
            Query.Width = goMargin.Left - 10 - queryMargin.Left;
            Query.Margin = queryMargin;

        }

       
        private string FormatInput(string inputText)
        {
			// Заміна всіх схожих операторів на їх фактичні символи
			inputText = inputText.Replace('~', '¬');
			inputText = inputText.Replace('!', '¬');
			inputText = inputText.Replace('|', '∨');
			inputText = inputText.Replace('+', '∨');
			inputText = inputText.Replace('&', '∧');
			inputText = inputText.Replace('*', '∧');
			inputText = inputText.Replace('-', '↔');
            inputText = inputText.Replace('>', '→');

            for(int i=0; i<inputText.Length; i++)
            {
                if (Char.IsLetter(inputText[i]) == true  ||
					Evaluator.prec.Contains(inputText[i]) == true || inputText[i] == ' ') { }
                else { inputText = inputText.Remove(i); }
            }
            return inputText;
        }

        private void Query_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox sendText = sender as TextBox;
            sendText.Text = FormatInput(sendText.Text);
            sendText.Select(sendText.Text.Length, 0);
        }

        private void Query_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) 
            {
				if ((sender as TextBox).Name == "Query")
				{
					Button_Click(sender, new EventArgs());
				}
            }
        }

		private void CreatePdf_Button_Click(object sender, RoutedEventArgs e)
		{
			try { 
			Query.Text = FormatInput(Query.Text);
			
			Evaluator evaluatorLa = new EvaluatorLatex(Query.Text);
			StreamWriter stream4 = new StreamWriter("wwwroot\\temp.tex");
			stream4.WriteLine("\\documentclass{article}" + Environment.NewLine +
			"\\usepackage{amsmath,amsthm,amssymb}" + Environment.NewLine +
			"\\usepackage{mathtext}"
			+ Environment.NewLine +
			"\\usepackage[T1, T2A]{fontenc}"
			+ Environment.NewLine +
			"\\usepackage[utf8]{inputenc}"
			+ Environment.NewLine +
			"\\usepackage[english, ukrainian, russian]{babel}"
			+ Environment.NewLine +
			"\\title{Course work}"
			+ Environment.NewLine +
			"\\author{Oleksandr Saveliev}"
			+ Environment.NewLine + "\\begin{document}"
			+ Environment.NewLine +
			"\\maketitle " + Environment.NewLine +
			"\\section{Таблиця істинності}"
			+ Environment.NewLine + evaluatorLa.EvaluateQuery() + Environment.NewLine +
			"\\section{ДКНФ}"
			+ Environment.NewLine + evaluatorLa.FindPCNF() + Environment.NewLine +
			"\\section{ДДНФ}"
			+ Environment.NewLine + evaluatorLa.FindPDNF() + Environment.NewLine +
			"\\end{document}");
			stream4.Close();

				string filename = "wwwroot\\temp.tex";
				Process p1 = new Process();
				p1.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory+"wwwroot\\pdflatex.exe";
				p1.StartInfo.Arguments = filename;
				p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				p1.StartInfo.RedirectStandardOutput = true;
				p1.StartInfo.UseShellExecute = false;

				p1.Start();
				var output = p1.StandardOutput.ReadToEnd();
				p1.WaitForExit();
				p1.Close();
				p1 = null;
				pdf_WB.Visibility = Visibility.Visible;
				pdf_WB.Navigate(AppDomain.CurrentDomain.BaseDirectory + "temp.pdf");
				OpenOutsideProgram_Button.Visibility = Visibility.Visible;
				texSave_Button.Visibility = Visibility.Visible;
				pdfSave_Button.Visibility = Visibility.Visible;

			}

			catch
			{
				//Якщо взагалі щось піде не так
				//Єдиний можливий випадок, коли символи не збалансовані
				//Або в текстовому полі немає введення
				if (Query.Text.Length == 0) { MessageBox.Show("Введіть вираз", "Немає виразу"); }
				else { MessageBox.Show("Попередження: знайдені незбалансовані символи", "Помилка"); }
			}
		}

		private void OpenOutsideProgram_Button_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "temp.pdf");
		}

		private void TTOpenInBrowser_Button_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\index1.html");
		}

		private void PcnfOpenInBrowser_Button_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\index2.html");
		}

		private void PdnfOpenInBrowser_Button_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\index3.html");
		}

		private void TTSave_Button_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfdTT = new SaveFileDialog();
			sfdTT.Filter = "html files (*.html)|*.html|All files (*.*)|*.*";
			sfdTT.FilterIndex = 1;
			sfdTT.RestoreDirectory = true;
			if (sfdTT.ShowDialog() == true)
			{
				File.Copy(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\index1.html", sfdTT.FileName);
			}
		}

		private void PcnfSave_Button_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfdPcnf = new SaveFileDialog();
			sfdPcnf.Filter = "html files (*.html)|*.html|All files (*.*)|*.*";
			sfdPcnf.FilterIndex = 1;
			sfdPcnf.RestoreDirectory = true;
			if (sfdPcnf.ShowDialog() == true)
			{
				File.Copy(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\index2.html", sfdPcnf.FileName);
			}
		}

		private void PdnfSave_Button_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfdPdnf = new SaveFileDialog();
			sfdPdnf.Filter = "html files (*.html)|*.html|All files (*.*)|*.*";
			sfdPdnf.FilterIndex = 1;
			sfdPdnf.RestoreDirectory = true;
			if (sfdPdnf.ShowDialog() == true)
			{
				File.Copy(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\index3.html", sfdPdnf.FileName);
			}
		}

		private void texSave_Button_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfdTex = new SaveFileDialog();
			sfdTex.Filter = "tex files (*.tex)|*.tex|All files (*.*)|*.*";
			sfdTex.FilterIndex = 1;
			sfdTex.RestoreDirectory = true;
			if (sfdTex.ShowDialog() == true)
			{
				File.Copy(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\temp.tex", sfdTex.FileName);
			}
		}

		private void pdfSave_Button_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfdPdf = new SaveFileDialog();
			sfdPdf.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
			sfdPdf.FilterIndex = 1;
			sfdPdf.RestoreDirectory = true;
			if (sfdPdf.ShowDialog() == true)
			{
				File.Copy(AppDomain.CurrentDomain.BaseDirectory + "temp.pdf", sfdPdf.FileName);
			}
		}
	}
}