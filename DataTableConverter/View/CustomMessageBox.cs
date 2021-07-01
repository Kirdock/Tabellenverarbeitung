using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class CustomMessageBox : Form
    {

        public enum SHSTOCKICONID : uint
        {
            //...
            SIID_INFO = 79,
            SIID_WARNING = 78,
            //...
        }

        [Flags]
        public enum SHGSI : uint
        {
            SHGSI_ICONLOCATION = 0,
            SHGSI_ICON = 0x000000100,
            SHGSI_SYSICONINDEX = 0x000004000,
            SHGSI_LINKOVERLAY = 0x000008000,
            SHGSI_SELECTED = 0x000010000,
            SHGSI_LARGEICON = 0x000000000,
            SHGSI_SMALLICON = 0x000000001,
            SHGSI_SHELLICONSIZE = 0x000000004
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public uint cbSize;
            public IntPtr hIcon;
            public int iSysIconIndex;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260/*MAX_PATH*/)]
            public string szPath;
        }

        [DllImport("Shell32.dll", SetLastError = false)]
        public static extern Int32 SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        internal CustomMessageBox(MessageBoxIcon icon, string text, string button1, string button2, string button3)
        {
            InitializeComponent();
            Text = icon == MessageBoxIcon.Warning ? "Warnung!" : "Fehler";
            SHSTOCKICONINFO sii = new SHSTOCKICONINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO))
            };

            Marshal.ThrowExceptionForHR(SHGetStockIconInfo(SHSTOCKICONID.SIID_WARNING,
                    SHGSI.SHGSI_ICON,
                    ref sii));
            pictureBox1.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
            //pictureBox1.Image = icon == MessageBoxIcon.Warning ? SystemIcons.Warning.ToBitmap() : SystemIcons.Error.ToBitmap();
            label1.Text = text;
            this.button1.Text = button1;
            this.button2.Text = button2;
            this.button3.Text = button3;

            FormClosing += (sender, e) => OnFormClosing(sii);
        }

        private void OnFormClosing(SHSTOCKICONINFO sii)
        {
            DestroyIcon(sii.hIcon);
        }
    }
}
