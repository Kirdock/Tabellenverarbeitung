using System;
using System.Reflection;
using System.Windows.Forms;

namespace DataTableConverter.Extensions
{
    internal static class TextBoxExtension
    {
        private static readonly FieldInfo Field;
        private static readonly PropertyInfo Prop;

        static TextBoxExtension()
        {
            Type type = typeof(Control);
            Field = type.GetField("text", BindingFlags.Instance | BindingFlags.NonPublic);
            Prop = type.GetProperty("WindowText", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        internal static void SetText(this TextBox box, string text)
        {
            Field.SetValue(box, text);
            Prop.SetValue(box, text, null);
        }
    }
}
