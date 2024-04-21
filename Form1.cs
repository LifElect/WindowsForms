using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form, IView
    {
        private readonly Presenter _presenter;
        public string sourceDir = @"C:\SourceDirectory";
        public string destDir = @"D:\TargetDirectory";

        public Form1()
        {
            InitializeComponent();
            _presenter = new Presenter(this, new Model());
        }

        private void syncButton_Click(object sender, EventArgs e)
        {
            ClearLog();
            ClearNames();
            _presenter.StartSync(sourceDir, destDir);
        }

        public void AddLog(string logMessage)
        {
            listBox3.Items.Add($"{DateTime.Now}: {logMessage}\n");
        }
        private void DestTextBoxChanged_TextChanged(object sender, EventArgs e)
        {
            destDir = destPath.Text;
        }
        private void SourceTextBoxChanged_TextChanged(object sender, EventArgs e)
        {
            sourceDir = SourcePath.Text;
        }
        public void ClearLog()
        {
            listBox3.Items.Clear();
        }
        public void ClearNames()
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
        }
        public void ShowSourceNames(string fileName)
        {
            listBox1.Items.Add(fileName);
        }
        public void ShowDestNames(string fileName)
        {
            listBox2.Items.Add(fileName);
        }
    }
    public interface IView
    {
        void AddLog(string logMessage);
        void ClearLog();
        void ShowSourceNames(string fileName);
        void ShowDestNames(string fileName);
        void ClearNames();
    }

    public class Model
    {
        public event Action<string> LogUpdated;

        public void SyncDirectories(string sourceDir, string destDir, IView _view)
        {
            
            string[] sourceFiles = Directory.GetFiles(sourceDir);
            string[] destFiles = Directory.GetFiles(destDir);

            foreach (string file in sourceFiles)
            {
                string fileName = Path.GetFileName(file);
                string destFilePath = Path.Combine(destDir, fileName);

                if (!File.Exists(destFilePath))
                {
                    File.Copy(file, destFilePath);
                    _view.AddLog($"Файл \"{fileName}\" создан\n");
                }
                else
                {
                    DateTime sourceFileLastWriteTime = File.GetLastWriteTime(file);
                    DateTime destFileLastWriteTime = File.GetLastWriteTime(destFilePath);

                    if (sourceFileLastWriteTime > destFileLastWriteTime)
                    {
                        File.Copy(file, destFilePath, true);
                        _view.AddLog($"Файл \"{fileName}\" изменен\n");
                    }
                    else if (sourceFileLastWriteTime == destFileLastWriteTime)
                    {
                        _view.AddLog($"Директории идентичны\n");
                    }
                }
            }

            foreach (string file in destFiles)
            {
                string fileName = Path.GetFileName(file);
                string sourceFilePath = Path.Combine(sourceDir, fileName);

                if (!File.Exists(sourceFilePath))
                {
                    File.Delete(file);
                    _view.AddLog($"Файл \"{fileName}\" удален");
                }
            }
            LogUpdated?.Invoke("Директории успешно синхронизированы.");

            foreach(string file in destFiles)
            {
                _view.ShowDestNames(Path.GetFileName(file));
            }

            foreach (string file in sourceFiles)
            {
                _view.ShowSourceNames(Path.GetFileName(file));
            }
            
        }
    }

    public class Presenter
    {
        private readonly IView _view;
        private readonly Model _model;

        public Presenter(IView view, Model model)
        {
            _view = view;
            _model = model;

            _model.LogUpdated += OnLogUpdated;
        }

        public void StartSync(string sourceDir, string targetDir)
        {
            _model.SyncDirectories(sourceDir, targetDir, _view);
        }

        private void OnLogUpdated(string logMessage)
        {
            _view.AddLog(logMessage);
        }
    }

}

