using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace WPFMemDumpAnalyzer.Core
{
    public class DependencyObjectMetadata
    {
        private readonly IList<DependencyPropertyMetadata> m_dependencyProperties = new List<DependencyPropertyMetadata>();
        private readonly IList<EffectiveValueEntryMetadata> m_effectiveValues = new List<EffectiveValueEntryMetadata>();

        internal ClrType ClrType { get; private set; }
        public ulong Address { get; private set; }

        public IEnumerable<DependencyPropertyMetadata> DependencyProperties
        {
            get { return m_dependencyProperties; }
        }

        public IEnumerable<EffectiveValueEntryMetadata> EffectiveValues
        {
            get { return m_effectiveValues; }
        }

        internal DependencyObjectMetadata(ClrType clrType, ulong address)
        {
            ClrType = clrType;
            Address = address;

            PopulateDependencyPropertiesFromObjectHierarchy(ClrType);
            GetEffectiveValues();
        }

        public string Type
        {
            get { return ClrType.Name; }
        }

        private void PopulateDependencyPropertiesFromObjectHierarchy(ClrType clrType)
        {
            if (clrType == null)
                return;

            foreach (ClrStaticField clrStaticField in clrType.StaticFields)
            {
                if (!IsDependencyProperty(clrStaticField.Type)) 
                    continue;

                var dependencyPropertyAddress = (ulong)clrStaticField.GetFieldValue(ClrType.Heap.GetRuntime().AppDomains[0]);
                if (dependencyPropertyAddress != 0)
                {
                    var dependencyPropertyMetadata = new DependencyPropertyMetadata(this, clrStaticField.Type, dependencyPropertyAddress);
                    m_dependencyProperties.Add(dependencyPropertyMetadata);
                }
            }

            PopulateDependencyPropertiesFromObjectHierarchy(clrType.BaseType);
        }

        private bool IsDependencyProperty(ClrType clrType)
        {
            const string dependencyPropertyName = "System.Windows.DependencyProperty";

            if (clrType == null)
                return false;

            return clrType.Name == dependencyPropertyName;
        }

        private void GetEffectiveValues()
        {
            var effectiveValuesFieldAddress = (ulong)ClrType.GetFieldValue(Address, new List<string> { "_effectiveValues" });
            if (effectiveValuesFieldAddress == 0)
                return;

            ClrType effectiveValuesFieldType = ClrType.Heap.GetObjectType(effectiveValuesFieldAddress);
            ClrType effectiveValueEntryType = effectiveValuesFieldType.ArrayComponentType;
            ClrInstanceField valueField = effectiveValueEntryType.GetFieldByName("_value");
            ClrInstanceField propertyIndexField = effectiveValueEntryType.GetFieldByName("_propertyIndex");

            int numberOfElements = effectiveValuesFieldType.GetArrayLength(effectiveValuesFieldAddress);
            for (int elementIndex = 0; elementIndex < numberOfElements; ++elementIndex)
            {
                ulong effectiveValueEntryAddress = effectiveValuesFieldType.GetArrayElementAddress(effectiveValuesFieldAddress, elementIndex);
                var propertyIndexFieldValue = (short)propertyIndexField.GetFieldValue(effectiveValueEntryAddress, true);

                var valueAddress = (ulong)valueField.GetFieldValue(effectiveValueEntryAddress, true);
                if (valueAddress != 0)
                {
                    ClrType valueType = ClrType.Heap.GetObjectType(valueAddress);
                    m_effectiveValues.Add(new EffectiveValueEntryMetadata(valueType, valueAddress, propertyIndexFieldValue));
                }
            }
        }
    }
}