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

        //public IEnumerable<DependencyPropertyMetadata> GetRegisteredDependencyProperties()
        //{
        //    const string registeredDependencyPropertiesFieldName = "RegisteredPropertyList";
        //    IList<DependencyPropertyMetadata> registeredDependencyProperties = new List<DependencyPropertyMetadata>();

        //    ClrType dependencyObjectType = m_heap.EnumerateObjects()
        //                                         .Select(m_heap.GetObjectType)
        //                                         .Where(IsDependencyProperty)
        //                                         .FirstOrDefault();

        //    if (dependencyObjectType == null)
        //        return registeredDependencyProperties;

        //    // Get the RegisteredDependencyProperties field, its type, and address
        //    ClrStaticField registeredDependencyPropertiesField = dependencyObjectType.GetStaticFieldByName(registeredDependencyPropertiesFieldName);
        //    if (registeredDependencyPropertiesField == null)
        //        return registeredDependencyProperties;

        //    ClrType registeredDependencyPropertiesFieldType = registeredDependencyPropertiesField.Type;
        //    ulong registeredDependencyPropertiesFieldAddress = registeredDependencyPropertiesField.GetFieldAddress(m_runtime.AppDomains[0]);

        //    // Get the List field
        //    ClrInstanceField internalListField = registeredDependencyPropertiesFieldType.Fields[0];
        //    var internalListFieldAddress = (ulong)internalListField.GetFieldValue(registeredDependencyPropertiesFieldAddress, true);
        //    ClrType internalListType = m_heap.GetObjectType(internalListFieldAddress);

        //    // Get the Array field
        //    ClrInstanceField internalArrayField = internalListType.Fields[0];
        //    var internalArrayFieldAddress = (ulong)internalArrayField.GetFieldValue(internalListFieldAddress);
        //    ClrType internalArrayType = m_heap.GetObjectType(internalArrayFieldAddress);

        //    int numberOfElements = internalArrayType.GetArrayLength(internalArrayFieldAddress);
        //    for (int elementIndex = 0; elementIndex < numberOfElements; ++elementIndex)
        //    {
        //        object propertyAddress = internalArrayType.GetArrayElementValue(internalArrayFieldAddress, elementIndex);
        //        if (propertyAddress == null)
        //            continue;

        //        var castedPropertyAddress = (ulong)propertyAddress;
        //        ClrType propertyType = m_heap.GetObjectType(castedPropertyAddress);
        //        if (propertyType == null)
        //            continue;

        //        var dependencyPropertyMetadata = new DependencyPropertyMetadata(propertyType, castedPropertyAddress);
        //        registeredDependencyProperties.Add(dependencyPropertyMetadata);
        //    }

        //    return registeredDependencyProperties;
        //}

        private bool IsDependencyProperty(ClrType clrType)
        {
            const string dependencyPropertyName = "System.Windows.DependencyProperty";

            if (clrType == null)
                return false;

            return clrType.Name == dependencyPropertyName;
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