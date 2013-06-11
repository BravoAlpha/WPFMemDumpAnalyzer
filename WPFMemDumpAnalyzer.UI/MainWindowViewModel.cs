using System.Collections.ObjectModel;
using WPFMemDumpAnalyzer.Core;

namespace WPFMemDumpAnalyzer.UI
{
    public class MainWindowViewModel
    {
        public ObservableCollection<DependencyObjectMetadata> DependencyObjects { get; private set; }

        public MainWindowViewModel()
        {
            DependencyObjects = new ObservableCollection<DependencyObjectMetadata>();

            var analyzer =
                MemoryAnalyzer.AnalyzeMemoryDump(
                    @"E:\Moshe\Projects\SandBox\LiveDebugging\TestWPFApplication\bin\Debug\TestWPFApplication.DMP");

            foreach (var dependencyObject in analyzer.GetDependencyObjects())
            {
                DependencyObjects.Add(dependencyObject);
            }
        }
    }
}