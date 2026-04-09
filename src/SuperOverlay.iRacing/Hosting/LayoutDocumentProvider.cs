using SuperOverlay.LayoutBuilder.Layout;
using SuperOverlay.LayoutBuilder.Persistence;
using System.IO;
namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutDocumentProvider
{
    private readonly LayoutFileStore _fileStore = new();

    public LayoutDocument GetOrCreateDefault(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (File.Exists(path))
        {
            return _fileStore.Load(path);
        }

        var document = DefaultLayoutFactory.Create();
        _fileStore.Save(path, document);
        return document;
    }
}
