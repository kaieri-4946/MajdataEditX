using MajdataEdit.TextEditor.Enums;
using MajdataEdit.TextEditor.Interfaces;
using MajSimai.Extensions.Checker;
using System.Windows.Media;

namespace MajdataEdit.TextEditor.MarkupComponents
{
    public class ErrorMarkupComponent : IMarkupComponent
    {
        public List<MarkupItem> Execute(string text)
        {
            var result = new List<MarkupItem>();

            // Not using text here as SyntaxCheck already populating to Errors
            foreach (var error in MainWindow.Errors)
            {
                var item = new MarkupItem()
                {
                    MarkupStyle = MarkupStyle.Underline,
                    Brush = error.Severity switch
                    {
                        Severity.Error => Brushes.Red,
                        Severity.Warning => Brushes.Yellow,
                        Severity.Info => Brushes.Cyan,
                        _ => throw new Exception("Unknown severity type")
                    },
                    Position = new Position(error.Position.x - 1, error.Position.y - 1),
                    MarkupEntireToken = error.IsFullNoteDiag,
                    Name = $"Error{Guid.NewGuid().ToString("N")}"
                };

                result.Add(item);
            }

            return result;
        }

        public string GetMarkupNamingPrefix()
        {
            return "Error";
        }
    }
}
