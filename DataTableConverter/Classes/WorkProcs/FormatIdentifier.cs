using System;

namespace DataTableConverter.Classes.WorkProcs
{
    class FormatIdentifier : IEquatable<FormatIdentifier>
    {
        internal string Header;
        internal int Index;

        public bool Equals(FormatIdentifier other)
        {
            return Header == other.Header && Index == other.Index;
        }

        public override int GetHashCode()
        {
            return Header.GetHashCode() ^ Index.GetHashCode();
        }
    }
}
