namespace WPFMemDumpAnalyzer.Core
{
    public class MemoryAddress
    {
        private ulong m_address;

        public MemoryAddress(ulong address)
        {
            m_address = address;
        }

        public override string ToString()
        {
            return m_address.ToString("X");
        }
    }
}