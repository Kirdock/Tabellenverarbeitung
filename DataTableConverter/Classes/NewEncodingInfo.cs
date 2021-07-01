namespace DataTableConverter.Classes
{
    class NewEncodingInfo
    {
        public string DisplayName { get; set; }
        public int CodePage { get; set; }

        public NewEncodingInfo(string displayName, int codePage)
        {
            DisplayName = displayName;
            CodePage = codePage;
        }
    }
}
