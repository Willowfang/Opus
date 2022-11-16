using Prism.Ioc;
using WF.PdfLib.iText7;
using WF.PdfLib.PDFTools;
using WF.PdfLib.Services;
using WF.ZipLib.Framework;
using WF.ZipLib;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RExternalServices
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering external services...", LogLevel.Debug, callerName: "App");

            registry.Register<IAnnotationService, AnnotationService>();
            registry.Register<IPdfAConvertService, PdfAConverter>();
            registry.Register<IBookmarkService, BookmarkService>();
            registry.Register<IExtractionService, ExtractionService>();
            registry.Register<ISigningService, SigningService>();
            registry.Register<IMergingService, MergingService>();
            registry.Register<IWordConvertService, WordConvertService>();
            registry.Register<IZipService, ZipService>();

            logbook.Write($"External services registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
