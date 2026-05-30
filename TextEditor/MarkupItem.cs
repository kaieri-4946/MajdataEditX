using MajdataEdit.TextEditor.Enums;
using System.Windows.Media;

namespace MajdataEdit.TextEditor
{
    public class MarkupItem
    {
        public MarkupStyle MarkupStyle { get; set; }
        public Brush Brush { get; set; }

        public string Name { get; set; }

        public Position Position { get; set; }
        public bool MarkupEntireToken { get; set; }
    }
}
