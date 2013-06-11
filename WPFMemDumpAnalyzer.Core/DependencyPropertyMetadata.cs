using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace WPFMemDumpAnalyzer.Core
{
    public class DependencyPropertyMetadata
    {
        internal ClrType ClrType { get; private set; }
        public ulong Address { get; private set; }
        public DependencyObjectMetadata DependencyObject { get; private set; }

        internal DependencyPropertyMetadata(DependencyObjectMetadata dependencyObjectMetadata, ClrType clrType, ulong address)
        {
            DependencyObject = dependencyObjectMetadata;
            ClrType = clrType;
            Address = address;
        }

        public string Name
        {
            get { return (string)ClrType.GetFieldValue(Address, new List<string> { "_name" }); }
        }

        public int PropertyId
        {
            get
            {
                var packedDataValue = (int)ClrType.GetFieldValue(Address, new List<string> { "_packedData" });
                return ExtractDependencyPropertyUniqueIdFromPackedData(packedDataValue);
            }
        }

        public object DefaultValue
        {
            get
            {
                var valueAddress = (ulong)ClrType.GetFieldValue(Address, new List<string> { "_defaultMetadata", "_defaultValue" });
                if (valueAddress == 0)
                    return "NULL";

                ClrType valueType = ClrType.Heap.GetObjectType(valueAddress);
                return ValueHelper.GetValue(valueType, valueAddress);
            }
        }

        public string Type
        {
            get
            {
                var valueAddress = (ulong)ClrType.GetFieldValue(Address, new List<string> { "_defaultMetadata", "_defaultValue" });
                if (valueAddress == 0)
                    return "<No Type>";

                ClrType valueType = ClrType.Heap.GetObjectType(valueAddress);
                return valueType.Name;
            }
        }

        private int ExtractDependencyPropertyUniqueIdFromPackedData(int packedData)
        {
            return (packedData & 0xFFFF);
        }
    }
}