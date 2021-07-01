using DataTableConverter.Classes;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace DataTableConverter.View.CustomControls
{
    class CountListbox : CheckedListBox
    {
        internal IEnumerable<ExportCustomItem> Dict { get; set; }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index > -1 && Items != null && e.Index + 1 <= Items.Count)
            {
                string text = Items[e.Index].ToString();

                CheckBoxState state = GetItemChecked(e.Index) ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                Size glyphSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, state);

                int checkPad = (e.Bounds.Height - glyphSize.Height) / 2;


                if (Items[e.Index] is CountListboxItem item)
                {
                    bool contains = ListContainsCustomExportItem(text);
                    //e.Graphics.FillRectangle(new SolidBrush(contains ? Color.Gray : Color.Transparent), e.Bounds);

                    CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.Bounds.X + checkPad, e.Bounds.Y + checkPad), state);
                    using (StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center })
                    {
                        using (Brush brush = new SolidBrush(contains ? Properties.Settings.Default.SeparateColor : Color.Black))
                        {
                            e.Graphics.DrawString(text + (item.Count > 0 ? $" (Anzahl: {item.Count})" : string.Empty), Font, brush, new Rectangle(e.Bounds.Height, e.Bounds.Top, e.Bounds.Width - e.Bounds.Height, e.Bounds.Height), sf);
                        }
                    }
                }
            }
        }

        private bool ListContainsCustomExportItem(string value)
        {
            return Dict?.SelectMany(item => item.SelectedValues).Contains(value) ?? false;
        }
    }
}
