//D:\Проги\VIsualStudioProjects\WindowsFormsApp3\Dir1
//D:\Проги\VIsualStudioProjects\WindowsFormsApp3\Dir2


using System;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form, IView
    {
        string sourceDir = @"C:\SourceDir";
        string destDir = @"C:\DestDir";
        private readonly Presenter _presenter;

        public Form1()
        {
            InitializeComponent();
            _presenter = new Presenter(this);
        }

        public void AddLog(string logMessage)
        {
            LogBox.Text = $"{DateTime.Now}: {logMessage}";
        }

        public void ClearLog()
        {
            listBox1.Items.Clear();
        }

        public void ShowFiles(string sourceDir, string destDir)
        {
            string[] sourceFiles = Directory.GetFiles(sourceDir);
            string[] destFiles = Directory.GetFiles(destDir);
            foreach (string file in sourceFiles)
            {
                listBox1.Items.Add(Path.GetFileName(file));
            }
            foreach (string file in destFiles)
            {
                listBox2.Items.Add(Path.GetFileName(file));
            }
        }

        private void DestTextBoxChanged_TextChanged(object sender, EventArgs e)
        {
            destDir = destPath.Text;
        }
        private void SourceTextBoxChanged_TextChanged(object sender, EventArgs e)
        {
           sourceDir = SourcePath.Text;
        }

       


        private void syncButton_Click(object sender, EventArgs e)
        {
            ClearLog();

            if (Directory.Exists(sourceDir) && Directory.Exists(destDir))
            {
                _presenter.SyncDirectories(sourceDir, destDir);
                ShowFiles(sourceDir, destDir);
            }
            else
            {
                AddLog("Ошибка: Одна из директорий не существует");
            }
        }

    }
    public interface IView
    {
        void AddLog(string logMessage);
        void ClearLog();
    }

    public class Presenter
    {
        private readonly IView _view;

        public Presenter(IView view)
        {
            _view = view;
        }

        public void SyncDirectories(string sourceDir, string destDir)
        {
            SyncDirectory(sourceDir, destDir);
            SyncDirectory(destDir, sourceDir);
        }

        private void SyncDirectory(string sourceDir, string destDir)
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
                    _view.AddLog($"Файл \"{fileName}\" создан");
                }
                else
                {
                    DateTime sourceFileLastWriteTime = File.GetLastWriteTime(file);
                    DateTime destFileLastWriteTime = File.GetLastWriteTime(destFilePath);

                    if (sourceFileLastWriteTime > destFileLastWriteTime)
                    {
                        File.Copy(file, destFilePath, true);
                        _view.AddLog($"Файл \"{fileName}\" изменен");
                    }
                    else if (sourceFileLastWriteTime == destFileLastWriteTime)
                    {
                        _view.AddLog($"Директории идентичны");
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
            
        }
    }

}
