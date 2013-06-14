using System.Collections.ObjectModel;
using WPFMemDumpAnalyzer.Core;

namespace WPFMemDumpAnalyzer.UI
{
    public class MainWindowViewModel
    {
        private string m_dumpFile;

        public ObservableCollection<DependencyObjectMetadata> DependencyObjects { get; private set; }

        public MainWindowViewModel()
        {
            DependencyObjects = new ObservableCollection<DependencyObjectMetadata>();
        }

        public string DumpFile
        {
            get { return m_dumpFile; }
            set
            {
                m_dumpFile = value;
                LoadDumpFile();
            }
        }

        private void LoadDumpFile()
        {
            DependencyObjects.Clear();

            // TODO: Dispose the MemoryAnalyzer instance
            var analyzer = MemoryAnalyzer.AnalyzeMemoryDump(m_dumpFile);
            foreach (var dependencyObject in analyzer.GetDependencyObjects())
                DependencyObjects.Add(dependencyObject);
        }
    }
}