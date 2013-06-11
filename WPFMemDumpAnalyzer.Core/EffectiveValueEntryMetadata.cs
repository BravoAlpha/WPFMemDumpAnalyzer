using Microsoft.Diagnostics.Runtime;

namespace WPFMemDumpAnalyzer.Core
{
    public class EffectiveValueEntryMetadata
    {
        internal ClrType ClrType { get; private set; }
        public ulong Address { get; private set; }
        public int PropertyId { get; private set; }

        public string Type
        {
            get { return ClrType.Name; }
        }

        public object Value
        {
            get { return ValueHelper.GetValue(ClrType, Address); }
        }

        internal EffectiveValueEntryMetadata(ClrType type, ulong address, int propertyId)
        {
            ClrType = type;
            Address = address;
            PropertyId = propertyId;
        }
    }
}