using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Psi.ExpectedTypes;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.Structured.Logging.Completion;

/// <summary>
/// A template property name offered inside a <c>{...}</c> hole. The item closes the hole when the
/// template does not, so that accepting a name leaves a complete hole and the caret past it.
/// Its type is also what tells the provider own items apart from everyone else, both when the
/// other providers items are dropped and when a test asks for the list this provider contributed.
/// </summary>
public sealed class TemplatePropertyLookupItem : TextLookupItem
{
    private readonly bool _insertClosingBrace;

    public TemplatePropertyLookupItem(
        [NotNull] string propertyName,
        bool insertClosingBrace,
        [NotNull] LookupItemPlacement placement)
        : base(propertyName, isDynamic: false)
    {
        _insertClosingBrace = insertClosingBrace;

        // The setter is protected, so the placement can only be given to the item from the inside
        Placement = placement;
    }

    /// <summary>
    /// The brace is written here rather than carried in the item text: the engine writes that text on a
    /// path of its own and the hole would end up with the name in it twice.
    /// </summary>
    protected override void OnAfterComplete(
        ITextControl textControl,
        ref DocumentRange nameRange,
        ref DocumentRange decorationRange,
        TailType tailType,
        ref Suffix suffix,
        ref IRangeMarker caretPositionRangeMarker)
    {
        base.OnAfterComplete(
            textControl,
            ref nameRange,
            ref decorationRange,
            tailType,
            ref suffix,
            ref caretPositionRangeMarker);

        if (!_insertClosingBrace)
        {
            return;
        }

        // The hole the name was written into has no closing brace, so it is closed here and the caret
        // moved past it, which is where the rest of the message goes
        var braceOffset = nameRange.EndOffset;
        textControl.Document.InsertText(braceOffset, "}");

        var caretOffset = braceOffset.Shift(1)
            .Offset;
        caretPositionRangeMarker = new TextRange(caretOffset, caretOffset).CreateRangeMarker(
            textControl.Document);
    }
}
