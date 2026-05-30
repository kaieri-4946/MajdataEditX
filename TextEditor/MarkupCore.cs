using MajdataEdit.TextEditor.Enums;
using MajdataEdit.TextEditor.Interfaces;
using MajdataEdit.TextEditor.MarkupComponents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MajdataEdit.TextEditor;
public partial class CustomTextEditor
{
    private IMarkupComponent _errorMarkupComponent = new ErrorMarkupComponent();

    private List<IMarkupComponent> _markupComponents = new List<IMarkupComponent>();

    private bool _isMarkupEnabled;

    public void DisableMarkup()
    {
        _isMarkupEnabled = false;
        Reset();
    }

    public void EnableMarkup()
    {
        _isMarkupEnabled = true;
    }

    public void RerenderMarkup<T>() where T : IMarkupComponent
    {
        var matchedComponent = _markupComponents.Where(x => x is T);
        foreach (var markupComponent in matchedComponent)
        {
            var prefix = markupComponent.GetMarkupNamingPrefix();
            var existingMarkups = _canvas.Children.OfType<Rectangle>().Where(x => x.Name.StartsWith(prefix)).ToList();
            foreach (var existingMarkup in existingMarkups)
            {
                _canvas.Children.Remove(existingMarkup);
            }

            var markup = markupComponent.Execute(Text);
            AddMarkups(markup);
        }
    }

    private void StartMarkupProcessAsync()
    {
        _markupComponents = new List<IMarkupComponent> { _errorMarkupComponent };

        foreach (var markupComponent in _markupComponents)
        {
            var markup = markupComponent.Execute(Text);
            AddMarkups(markup);
        }
    }
    private void UpdateMarkup()
    {
        if (!_isMarkupEnabled || _isUpdating || !_internalTextBox.IsLoaded)
        {
            return;
        }

        _isUpdating = true;
        Reset();
        StartMarkupProcessAsync();
        _isUpdating = false;
    }

    private void Reset()
    {
        if (_canvas != null && Children.Contains(_canvas))
        {
            Children.Remove(_canvas);
        }
        _canvas = GetNewCanvas();
        Children.Add(_canvas);
    }

    private Canvas GetNewCanvas()
    {
        var x = _internalTextBox.Padding.Left;
        var y = _internalTextBox.Padding.Top;
        var width = _internalTextBox.Width - _internalTextBox.Padding.Left - _internalTextBox.Padding.Right;
        var height = _internalTextBox.Height - _internalTextBox.Padding.Top - _internalTextBox.Padding.Bottom;
        var highlightCanvasBounds = new Rect(x, y, width, height);
        return new Canvas { Clip = new RectangleGeometry(highlightCanvasBounds) };
    }

    private void AddMarkups(List<MarkupItem> markupItems)
    {
        foreach (var markupItem in markupItems)
        {
            var positionRect = GetPosition(markupItem);
            if (positionRect is null) continue;
            AddMarkup(positionRect.Value, markupItem);
        }
    }

    private void AddMarkup(Rect rect, MarkupItem markupItem)
    {
        switch (markupItem.MarkupStyle)
        {
            case MarkupStyle.Underline: AddUnderlineMarkup(rect, markupItem); break;
            default: throw new InvalidOperationException("Invalid markup");
        };
    }

    private void AddUnderlineMarkup(Rect rect, MarkupItem markupItem)
    {
        // Overriding character rect for underline style
        var markup = new Rectangle { Width = rect.Width, Height = Math.Min(3, rect.Height / 10), Fill = markupItem.Brush, Name = markupItem.Name };
        _canvas.Children.Add(markup);
        SetZIndex(markup, 128);
        SetTop(markup, rect.Bottom + 2);
        SetLeft(markup, rect.Left);
    }

    private Rect? GetPosition(MarkupItem markupItem)
    {
        if (!markupItem.MarkupEntireToken)
            try
            {
                return GetSingleCharacterPosition(markupItem);
            }
            catch
            {
                return null;
            }
        else
            return GetFullNotePosition(markupItem);
    }

    private Rect GetSingleCharacterPosition(MarkupItem markupItem)
    {
        var leading = GetCharacterRectFromLineAndPosition(markupItem.Position.y, markupItem.Position.x, false);
        var trailing = GetCharacterRectFromLineAndPosition(markupItem.Position.y, markupItem.Position.x, true);
        return new Rect(location: leading.Location, size: new Size(trailing.X - leading.X, leading.Height));
    }

    private Rect GetFullNotePosition(MarkupItem markupItem)
    {
        int itemIndex = GetCharacterIndexFromLineIndex(_internalTextBox, markupItem.Position.y);
        itemIndex += markupItem.Position.x;
        int textLength = _internalTextBox.Text.Length;
        if (itemIndex > textLength)
            throw new ArgumentOutOfRangeException("Position is beyond the text length.");

        // Note should never span multiple lines, we'll assume it's cut off right there
        int startPos = itemIndex;
        int endPos = itemIndex;
        while (startPos > 0)
        {
            startPos--;
            if (_internalTextBox.Text[startPos] is '/' or ',' or '\n')
            {
                startPos++;
                break;
            }
        }
        while (endPos < textLength - 1)
        {
            endPos++;
            if (_internalTextBox.Text[endPos] is '/' or ',' or '\n')
            {
                endPos--;
                break;
            }
        }

        var leading = _internalTextBox.GetRectFromCharacterIndex(startPos, false);
        var trailing = _internalTextBox.GetRectFromCharacterIndex(endPos, true);
        return new Rect(location: leading.Location, size: new Size(trailing.X - leading.X, leading.Height));
    }

    public Rect GetCharacterRectFromLineAndPosition(int lineIndex, int charIndexInLine, bool trailingEdge)
    {
        int startIndex = GetCharacterIndexFromLineIndex(_internalTextBox, lineIndex);
        int totalCharIndex = startIndex + charIndexInLine;
        if (totalCharIndex > _internalTextBox.Text.Length)
            throw new ArgumentOutOfRangeException("Position is beyond the text length.");
        return _internalTextBox.GetRectFromCharacterIndex(totalCharIndex, trailingEdge);
    }

    // The built-in function is returning from the wrong line, no idea why :D
    private int GetCharacterIndexFromLineIndex(TextBox textBox, int lineIndex)
    {
        if (lineIndex == 0) return 0;

        var text = _internalTextBox.Text;
        int currentLine = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                currentLine++;
                if (currentLine == lineIndex)
                {
                    return i + 1;
                }
            }
        }

        throw new ArgumentOutOfRangeException(nameof(lineIndex), "End of text reached");
    }
}
