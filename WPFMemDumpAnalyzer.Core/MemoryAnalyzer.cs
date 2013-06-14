using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace WPFMemDumpAnalyzer.Core
{
    public class MemoryAnalyzer : IDisposable
    {
        private readonly DataTarget m_dataTarget;
        private readonly ClrRuntime m_runtime;
        private readonly ClrHeap m_heap;

        private MemoryAnalyzer (int processId)
            : this(DataTarget.AttachToProcess(processId, 2000, AttachFlag.NonInvasive))
        {
            
        }

        private MemoryAnalyzer (string memoryDumpPath)
            : this(DataTarget.LoadCrashDump(memoryDumpPath))
        {
        }

        private MemoryAnalyzer(DataTarget dataTarget)
        {
            // TODO: Exit gracefully for memory dumps from different platforms

            m_dataTarget = dataTarget;
            string dacLocation = m_dataTarget.ClrVersions[0].TryGetDacLocation();
            if (String.IsNullOrEmpty(dacLocation))
                throw new ArgumentException("Cannot find DAC location for process");

            m_runtime = m_dataTarget.CreateRuntime(dacLocation);
            m_heap = m_runtime.GetHeap();
        }

        public IEnumerable<DependencyObjectMetadata> GetDependencyObjects()
        {
            return (from objectAddress in m_heap.EnumerateObjects()
                    let objectType = m_heap.GetObjectType(objectAddress)
                    where IsDependencyObject(objectType)
                    select new DependencyObjectMetadata(objectType, objectAddress))
                    .ToList();
        }

        private bool IsDependencyObject(ClrType clrType)
        {
            const string dependencyObjectName = "System.Windows.DependencyObject";

            if (clrType.Name == dependencyObjectName)
                return true;

            ClrType baseType = clrType.BaseType;
            if (baseType == null)
                return false;

            return IsDependencyObject(baseType);
        }

        public void Dispose()
        {
            m_dataTarget.Dispose();
        }


        #region Static Factory Methods
        public static MemoryAnalyzer AttachToProcessNamed(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
                throw new ArgumentException(String.Format("Cannot find process named {0}", processName));

            return AttachToProcessWithId(processes[0].Id);
        }

        public static MemoryAnalyzer AttachToProcessWithId(int processId)
        {
            return new MemoryAnalyzer(processId);
        }

        public static MemoryAnalyzer AnalyzeMemoryDump(string memoryDumpPath)
        {
            return new MemoryAnalyzer(memoryDumpPath);
        }
        #endregion
    }
}