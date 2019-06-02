using DataTableConverter.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace DataTableConverter.View.CustomControls
{
    class CountListbox : CheckedListBox
    {
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index > -1 && Items != null && e.Index + 1 <= Items.Count)
            {
                string text = Items[e.Index].ToString();
                if ((Items[e.Index] is CountListboxItem))
                {
                    text += $" (Anzahl: {(Items[e.Index] as CountListboxItem).Count})";
                }
                CheckBoxState state = GetItemChecked(e.Index) ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                Size glyphSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, state);
                var b = e.Bounds;
                int checkPad = (b.Height - glyphSize.Height) / 2;
                CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(b.X + checkPad, b.Y + checkPad),
                    new Rectangle(
                        new Point(b.X + b.Height, b.Y),
                        new Size(b.Width - b.Height, b.Height)),
                    text, this.Font, TextFormatFlags.Left, false, state);
            }
        }
    }
}
