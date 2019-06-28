using System;
using System.Windows.Forms;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class WaitCursor : IDisposable
    {
        private Cursor m_oldCursor = null;

        public WaitCursor()
        {
            m_oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Position = Cursor.Position;
        }

        public void Dispose()
        {
            Cursor.Current = m_oldCursor;
            Cursor.Position = Cursor.Position;
        }
    }
}
