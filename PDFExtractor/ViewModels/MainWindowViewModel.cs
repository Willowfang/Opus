using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;

namespace PDFExtractor.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private DelegateCommand openLicenses;
        public DelegateCommand OpenLicenses =>
            openLicenses ?? (openLicenses = new DelegateCommand(ExecuteOpenLicenses));

        void ExecuteOpenLicenses()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "TextFiles", "Licenses.txt"))
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private DelegateCommand openManual;
        public DelegateCommand OpenManual =>
            openManual ?? (openManual = new DelegateCommand(ExecuteOpenManual));

        void ExecuteOpenManual()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@"https://github.com/CodeX-fi/ExtractPdf/wiki/K%C3%A4ytt%C3%B6ohjeet")
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private DelegateCommand openSourceCode;
        public DelegateCommand OpenSourceCode =>
            openSourceCode ?? (openSourceCode = new DelegateCommand(ExecuteOpenSourceCode));

        void ExecuteOpenSourceCode()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@"https://github.com/CodeX-fi/ExtractPdf")
            {
                UseShellExecute = true
            };
            p.Start();
        }
    }
}
