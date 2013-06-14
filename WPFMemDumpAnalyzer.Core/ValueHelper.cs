using System;
using Microsoft.Diagnostics.Runtime;

namespace WPFMemDumpAnalyzer.Core
{
    public static class ValueHelper
    {
        public static object GetValue(ClrType type, ulong address)
        {
            if (type.IsPrimitive && type.HasSimpleValue)
                return type.GetValue(address);

            if (type.IsObjectReference)
            {
                if (type.IsString)
                    return ReadString(type, address);

                return new MemoryAddress(address);
            }

            // TODO: Handle structs
            return "<Struct>";
        }

        private static string ReadString(ClrType stringType, ulong stringAddress)
        {
            ClrInstanceField lengthField = stringType.GetFieldByName("m_stringLength");
            if (lengthField == null)
                return String.Empty;

            var stringLength = (int)lengthField.GetFieldValue(stringAddress);
            if (stringLength <= 0)
                return String.Empty;

            var content = new byte[stringLength * 2];
            int bytesRead;
            stringType.Heap.GetRuntime().ReadVirtual(stringAddress + 12, content, stringLength * 2, out bytesRead);
            return System.Text.Encoding.Unicode.GetString(content);
        }
    }
}