namespace MajdataEdit.TextEditor.Interfaces
{
    public interface IMarkupComponent
    {
        public List<MarkupItem> Execute(string text);
        public string GetMarkupNamingPrefix();
    }
}
