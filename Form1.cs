using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form, IView
    {
        private readonly Presenter _presenter;
        public string SourceDir = @"C:\SourceDirectory";
        public string DestDir = @"D:\TargetDirectory";

        public Form1()
        {
            InitializeComponent();
            _presenter = new Presenter(this, new Model());
        }

        private void syncButton_Click(object sender, EventArgs e)
        {
            ClearLog();
            ClearNames();
            SyncDir(sender, e);
        }

        public void AddLog(string logMessage)
        {
            listBox3.Items.Add($"{DateTime.Now}: {logMessage}\n");
        }
        private void DestTextBoxChanged_TextChanged(object sender, EventArgs e)
        {
            DestDir = destPath.Text;
        }
        private void SourceTextBoxChanged_TextChanged(object sender, EventArgs e)
        {
            SourceDir = SourcePath.Text;
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

        public event EventHandler<EventArgs> SyncDir;

        public string GetSourceDir()
        {
            return SourceDir;
        }
        public string GetDestDir()
        {
            return DestDir;
        }
        }
    public interface IView
    {
        string GetDestDir();
        string GetSourceDir();
        void AddLog(string logMessage);
        void ClearLog();
        void ShowSourceNames(string fileName);
        void ShowDestNames(string fileName);
        void ClearNames();
        event EventHandler<EventArgs> SyncDir;
    }

    public class Model
    {
        public event Action<string> LogUpdated;

        public List<string> SyncDirectories(string sourceDir, string destDir)
        {
            List<string> logResult = new List<string>();
            string[] sourceFiles = Directory.GetFiles(sourceDir);
            string[] destFiles = Directory.GetFiles(destDir);

            foreach (string file in sourceFiles)
            {
                string fileName = Path.GetFileName(file);
                string destFilePath = Path.Combine(destDir, fileName);

                if (!File.Exists(destFilePath))
                {
                    File.Copy(file, destFilePath);
                    logResult.Add($"Файл \"{fileName}\" создан");
                }
                else
                {
                    DateTime sourceFileLastWriteTime = File.GetLastWriteTime(file);
                    DateTime destFileLastWriteTime = File.GetLastWriteTime(destFilePath);

                    if (sourceFileLastWriteTime > destFileLastWriteTime)
                    {
                        File.Copy(file, destFilePath, true);
                        logResult.Add($"Файл \"{fileName}\" изменен");
                    }
                    else if (sourceFileLastWriteTime == destFileLastWriteTime)
                    {
                        logResult.Add($"Директории идентичны");
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
                    logResult.Add($"Файл \"{fileName}\" удален");
                }
            }
            LogUpdated?.Invoke("Директории успешно синхронизированы.");

            return logResult;
        }
        public List<string> GetFileNames(string dirName)
        {
            List<string> fileNames = new List<string>();
            foreach (string file in Directory.GetFiles(dirName))
            {
                fileNames.Add(Path.GetFileName(file));
            }
            return fileNames;
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
            _view.SyncDir += new EventHandler<EventArgs>(StartSync);

            _model.LogUpdated += OnLogUpdated;
        }

        public void StartSync(object sender, EventArgs e)
        {
            string sourceDir = _view.GetSourceDir();
            string targetDir = _view.GetDestDir();

            List<string> logs = new List<string>();
            List<string> fileNames = new List<string>();

            logs = _model.SyncDirectories(sourceDir, targetDir);
            foreach(string logMessage in logs)
            {
                _view.AddLog(logMessage);
            }
            fileNames = _model.GetFileNames(sourceDir);
            foreach(string file in fileNames)
            {
                _view.ShowDestNames(file);
                _view.ShowSourceNames(file);
            }
        }

        private void OnLogUpdated(string logMessage)
        {
            _view.AddLog(logMessage);
        }
    }

}

