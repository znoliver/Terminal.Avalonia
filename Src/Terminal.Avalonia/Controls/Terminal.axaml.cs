using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Metadata;
using Avalonia.Threading;
using Terminal.Avalonia.Utilities;

namespace Terminal.Avalonia.Controls;

[TemplatePart(TextPresenterName, typeof(TerminalTextPresenter))]
[TemplatePart(ScrollViewerName, typeof(ScrollViewer))]
public class Terminal : TemplatedControl
{
    private const string TextPresenterName = "PART_TextPresenter";
    private const string ScrollViewerName = "PART_ScrollViewer";
    private static readonly string[] InvalidCharacters = ["\u007f"];

    #region Avalonia Properties

    /// <summary>
    /// Defines the <see cref="CaretIndex"/> property
    /// </summary>
    public static readonly StyledProperty<int> CaretIndexProperty =
        AvaloniaProperty.Register<Terminal, int>(nameof(CaretIndex), coerce: CoerceCaretIndex);

    /// <summary>
    /// Defines the <see cref="MediaTypeNames.Text"/> property
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<Terminal>(new(defaultBindingMode: BindingMode.TwoWay));

    /// <summary>
    /// Defines see <see cref="TextPresenter.LineHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> LineHeightProperty =
        TextBlock.LineHeightProperty.AddOwner<Terminal>(new(defaultValue: double.NaN));

    /// <summary>
    /// Defines see <see cref="TextBlock.LetterSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> LetterSpacingProperty =
        TextBlock.LetterSpacingProperty.AddOwner<Terminal>();

    /// <summary>
    /// Defines the <see cref="SelectionStart"/> property
    /// </summary>
    public static readonly StyledProperty<int> SelectionStartProperty =
        AvaloniaProperty.Register<Terminal, int>(nameof(SelectionStart), coerce: CoerceCaretIndex);

    /// <summary>
    /// Defines the <see cref="SelectionEnd"/> property
    /// </summary>
    public static readonly StyledProperty<int> SelectionEndProperty =
        AvaloniaProperty.Register<Terminal, int>(nameof(SelectionEnd), coerce: CoerceCaretIndex);

    /// <summary>
    /// Defines the <see cref="MaxLength"/> property
    /// </summary>
    public static readonly StyledProperty<int> MaxLengthProperty =
        AvaloniaProperty.Register<Terminal, int>(nameof(MaxLength));

    /// <summary>
    /// Defines the <see cref="MaxLines"/> property
    /// </summary>
    public static readonly StyledProperty<int> MaxLinesProperty =
        AvaloniaProperty.Register<Terminal, int>(nameof(MaxLines));

    /// <summary>
    /// Defines the <see cref="TextChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent<TextChangedEventArgs> TextChangedEvent =
        RoutedEvent.Register<TextBox, TextChangedEventArgs>(nameof(TextChanged), RoutingStrategies.Bubble);

    /// <summary>
    /// Defines the <see cref="TextChanging"/> event.
    /// </summary>
    public static readonly RoutedEvent<TextChangingEventArgs> TextChangingEvent =
        RoutedEvent.Register<TextBox, TextChangingEventArgs>(nameof(TextChanging), RoutingStrategies.Bubble);

    #endregion

    #region Coreces

    internal static int CoerceCaretIndex(AvaloniaObject sender, int value)
    {
        var text = sender.GetValue(TextProperty);

        if (text == null)
        {
            return 0;
        }

        var length = text.Length;

        if (value < 0)
        {
            return 0;
        }

        if (value > length)
        {
            return length;
        }

        if (value > 0 && text[value - 1] == '\r' && value < length && text[value] == '\n')
        {
            return value + 1;
        }

        return value;
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs asynchronously after text changes and the new text is rendered.
    /// </summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged
    {
        add => AddHandler(TextChangedEvent, value);
        remove => RemoveHandler(TextChangedEvent, value);
    }

    /// <summary>
    /// Occurs synchronously when text starts to change but before it is rendered.
    /// </summary>
    /// <remarks>
    /// This event occurs just after the <see cref="Text"/> property value has been updated.
    /// </remarks>
    public event EventHandler<TextChangingEventArgs>? TextChanging
    {
        add => AddHandler(TextChangingEvent, value);
        remove => RemoveHandler(TextChangingEvent, value);
    }

    #endregion

    private TerminalTextPresenter? _presenter;
    private ScrollViewer? _scrollViewer;
    private readonly TerminalInputMethodClient _imClient = new();

    static Terminal()
    {
        FocusableProperty.OverrideDefaultValue<Terminal>(true);
        BackgroundProperty.OverrideDefaultValue<Terminal>(new SolidColorBrush(Colors.Black));

        TextInputMethodClientRequestedEvent.AddClassHandler<Terminal>((terminal, e) => e.Client = terminal._imClient);
    }

    #region Properties

    /// <summary>
    /// Gets or sets the index of the text caret
    /// </summary>
    public int CaretIndex
    {
        get => GetValue(CaretIndexProperty);
        set => SetValue(CaretIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets the Text content of the Terminal
    /// </summary>
    [Content]
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the line height.
    /// </summary>
    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between characters
    /// </summary>
    public double LetterSpacing
    {
        get => GetValue(LetterSpacingProperty);
        set => SetValue(LetterSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the starting position of the text selected in the Terminal
    /// </summary>
    public int SelectionStart
    {
        get => GetValue(SelectionStartProperty);
        set => SetValue(SelectionStartProperty, value);
    }

    /// <summary>
    /// Gets or sets the end position of the text selected in the Terminal
    /// </summary>
    /// <remarks>
    /// When the SelectionEnd is equal to <see cref="SelectionStart"/>, there is no 
    /// selected text and it marks the caret position
    /// </remarks>
    public int SelectionEnd
    {
        get => GetValue(SelectionEndProperty);
        set => SetValue(SelectionEndProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of visible lines.
    /// </summary>
    public int MaxLength
    {
        get => GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of lines the Terminal can contain
    /// </summary>
    public int MaxLines
    {
        get => GetValue(MaxLinesProperty);
        set => SetValue(MaxLinesProperty, value);
    }

    #endregion

    #region Overrides

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _presenter = e.NameScope.Get<TerminalTextPresenter>(TextPresenterName);
        _scrollViewer = e.NameScope.Find<ScrollViewer>(ScrollViewerName);

        _imClient.SetPresenter(_presenter, this);
        if (IsFocused)
        {
            _presenter?.ShowCaret();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (_presenter == null) return;
        if (IsFocused)
        {
            _presenter.ShowCaret();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _presenter?.HideCaret();
        _imClient.SetPresenter(null, null);
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);

        _imClient.SetPresenter(_presenter, this);
        _presenter?.ShowCaret();
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        _presenter?.HideCaret();
        _imClient.SetPresenter(null, null);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty)
        {
            CoerceValue(CaretIndexProperty);
            CoerceValue(SelectionStartProperty);
            CoerceValue(SelectionEndProperty);

            RaiseTextChangeEvents();

            // UpdatePseudoclasses();
            // UpdateCommandStates();
        }
        else if (change.Property == CaretIndexProperty)
        {
            OnCaretIndexChanged(change);
        }
        else if (change.Property == SelectionStartProperty)
        {
            OnSelectionStartChanged(change);
        }
        else if (change.Property == SelectionEndProperty)
        {
            _presenter?.MoveCaretToTextPosition(CaretIndex);

            OnSelectionEndChanged(change);
        }
        else if (change.Property == MaxLinesProperty)
        {
            InvalidateMeasure();
        }
        // else if (change.Property == UndoLimitProperty)
        // {
        //     OnUndoLimitChanged(change.GetNewValue<int>());
        // }
        // else if (change.Property == IsUndoEnabledProperty && change.GetNewValue<bool>() == false)
        // {
        //     // from docs at
        //     // https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.primitives.textboxbase.isundoenabled:
        //     // "Setting this property to false clears the undo stack.
        //     // Therefore, if you disable undo and then re-enable it, undo commands still do not work
        //     // because the undo stack was emptied when you disabled undo."
        //     _undoRedoHelper.Clear();
        //     _selectedTextChangesMadeSinceLastUndoSnapshot = 0;
        //     _hasDoneSnapshotOnce = false;
        // }
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (e.Handled) return;

        HandleTextInput(e.Text);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (_presenter == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(_presenter.PreeditText))
        {
            return;
        }

        var text = Text ?? string.Empty;
        var caretIndex = CaretIndex;
        var movement = false;
        var selection = false;
        var handled = false;
        var modifiers = e.KeyModifiers;

        var keymap = Application.Current!.PlatformSettings!.HotkeyConfiguration;

        bool Match(List<KeyGesture> gestures) => gestures.Any(g => g.Matches(e));
        bool DetectSelection() => e.KeyModifiers.HasAllFlags(keymap.SelectionModifiers);

        if (Match(keymap.SelectAll))
        {
            SelectAll();
            handled = true;
        }
        else if (Match(keymap.Copy))
        {
            // Copy();
            handled = true;
        }
        else if (Match(keymap.Cut))
        {
            // Cut();

            handled = true;
        }
        else if (Match(keymap.Paste))
        {
            // Paste();
            handled = true;
        }
        else if (Match(keymap.Undo))
        {
            // Undo();

            handled = true;
        }
        else if (Match(keymap.Redo))
        {
            // Redo();

            handled = true;
        }
        else if (Match(keymap.MoveCursorToTheStartOfDocument))
        {
            MoveHome(true);
            movement = true;
            selection = false;
            handled = true;
            SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
        }
        else if (Match(keymap.MoveCursorToTheEndOfDocument))
        {
            MoveEnd(true);
            movement = true;
            selection = false;
            handled = true;
            SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
        }
        else if (Match(keymap.MoveCursorToTheStartOfLine))
        {
            MoveHome(false);
            movement = true;
            selection = false;
            handled = true;
            SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
        }
        else if (Match(keymap.MoveCursorToTheEndOfLine))
        {
            MoveEnd(false);
            movement = true;
            selection = false;
            handled = true;
            SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
        }
        else if (Match(keymap.MoveCursorToTheStartOfDocumentWithSelection))
        {
            SetCurrentValue(SelectionStartProperty, caretIndex);
            MoveHome(true);
            SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
            movement = true;
            selection = true;
            handled = true;
        }
        else if (Match(keymap.MoveCursorToTheEndOfDocumentWithSelection))
        {
            SetCurrentValue(SelectionStartProperty, caretIndex);
            MoveEnd(true);
            SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
            movement = true;
            selection = true;
            handled = true;
        }
        else if (Match(keymap.MoveCursorToTheStartOfLineWithSelection))
        {
            SetCurrentValue(SelectionStartProperty, caretIndex);
            MoveHome(false);
            SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
            movement = true;
            selection = true;
            handled = true;
        }
        else if (Match(keymap.MoveCursorToTheEndOfLineWithSelection))
        {
            SetCurrentValue(SelectionStartProperty, caretIndex);
            MoveEnd(false);
            SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
            movement = true;
            selection = true;
            handled = true;
        }
        else if (Match(keymap.PageLeft))
        {
            MovePageLeft();
            movement = true;
            selection = false;
            handled = true;
        }
        else if (Match(keymap.PageRight))
        {
            MovePageRight();
            movement = true;
            selection = false;
            handled = true;
        }
        else if (Match(keymap.PageUp))
        {
            MovePageUp();
            movement = true;
            selection = false;
            handled = true;
        }
        else if (Match(keymap.PageDown))
        {
            MovePageDown();
            movement = true;
            selection = false;
            handled = true;
        }
        else
        {
            bool hasWholeWordModifiers = modifiers.HasAllFlags(keymap.WholeWordTextActionModifiers);
            switch (e.Key)
            {
                case Key.Left:
                    selection = DetectSelection();
                    MoveHorizontal(-1, hasWholeWordModifiers, selection, true);
                    movement = true;
                    break;

                case Key.Right:
                    selection = DetectSelection();
                    MoveHorizontal(1, hasWholeWordModifiers, selection, true);
                    movement = true;
                    break;

                case Key.Up:
                {
                    selection = DetectSelection();

                    _presenter.MoveCaretVertical(LogicalDirection.Backward);

                    if (caretIndex != _presenter.CaretIndex)
                    {
                        movement = true;
                    }

                    if (selection)
                    {
                        SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
                    }
                    else
                    {
                        SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
                    }

                    break;
                }
                case Key.Down:
                {
                    selection = DetectSelection();

                    _presenter.MoveCaretVertical();

                    if (caretIndex != _presenter.CaretIndex)
                    {
                        movement = true;
                    }

                    if (selection)
                    {
                        SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
                    }
                    else
                    {
                        SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
                    }

                    break;
                }
                case Key.Back:
                {
                    //TODO: SnapshotUndoRedo();

                    if (hasWholeWordModifiers && SelectionStart == SelectionEnd)
                    {
                        SetSelectionForControlBackspace();
                    }

                    if (!DeleteSelection())
                    {
                        var characterHit = _presenter.GetNextCharacterHit(LogicalDirection.Backward);

                        var backspacePosition = characterHit.FirstCharacterIndex + characterHit.TrailingLength;

                        if (caretIndex != backspacePosition)
                        {
                            var start = Math.Min(backspacePosition, caretIndex);
                            var end = Math.Max(backspacePosition, caretIndex);

                            var length = end - start;

                            var sb = StringBuilderCache.Acquire(text.Length);
                            sb.Append(text);
                            sb.Remove(start, end - start);

                            SetCurrentValue(TextProperty, StringBuilderCache.GetStringAndRelease(sb));

                            SetCurrentValue(CaretIndexProperty, start);
                        }
                    }

                    // SnapshotUndoRedo();

                    handled = true;
                    break;
                }
                case Key.Delete:
                    // SnapshotUndoRedo();

                    if (hasWholeWordModifiers && SelectionStart == SelectionEnd)
                    {
                        SetSelectionForControlDelete();
                    }

                    if (!DeleteSelection())
                    {
                        var characterHit = _presenter.GetNextCharacterHit();

                        var nextPosition = characterHit.FirstCharacterIndex + characterHit.TrailingLength;

                        if (nextPosition != caretIndex)
                        {
                            var start = Math.Min(nextPosition, caretIndex);
                            var end = Math.Max(nextPosition, caretIndex);

                            var sb = StringBuilderCache.Acquire(text.Length);
                            sb.Append(text);
                            sb.Remove(start, end - start);

                            SetCurrentValue(TextProperty, StringBuilderCache.GetStringAndRelease(sb));
                        }
                    }

                    // SnapshotUndoRedo();

                    handled = true;
                    break;

                case Key.Enter:
                    // SnapshotUndoRedo();
                    HandleTextInput(Environment.NewLine);
                    handled = true;
                    break;

                case Key.Tab:
                    HandleTextInput("\t");
                    handled = true;
                    break;

                case Key.Space:
                    // SnapshotUndoRedo(); // always snapshot in between words
                    break;

                default:
                    handled = false;
                    break;
            }
        }

        if (movement && !selection)
        {
            ClearSelection();
        }

        if (handled || movement)
        {
            e.Handled = true;
        }
    }

    #endregion

    #region Property Changed Handlers

    private void OnCaretIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var newValue = e.GetNewValue<int>();
        SetCurrentValue(SelectionStartProperty, newValue);
        SetCurrentValue(SelectionEndProperty, newValue);
    }

    private void OnSelectionStartChanged(AvaloniaPropertyChangedEventArgs e)
    {
        //TODO: UpdateCommandStates();

        var value = e.GetNewValue<int>();
        if (SelectionEnd == value && CaretIndex != value)
        {
            SetCurrentValue(CaretIndexProperty, value);
        }
    }

    private void OnSelectionEndChanged(AvaloniaPropertyChangedEventArgs e)
    {
        //TODO: UpdateCommandStates();

        var value = e.GetNewValue<int>();
        if (SelectionStart == value && CaretIndex != value)
        {
            SetCurrentValue(CaretIndexProperty, value);
        }
    }

    #endregion

    private void ClearSelection()
    {
        SetCurrentValue(CaretIndexProperty, SelectionStart);
        SetCurrentValue(SelectionEndProperty, SelectionStart);
    }

    private void SelectAll()
    {
        SetCurrentValue(SelectionStartProperty, 0);
        SetCurrentValue(SelectionEndProperty, Text?.Length ?? 0);
    }

    /// <summary>
    /// Raises both the <see cref="TextChanging"/> and <see cref="TextChanged"/> events.
    /// </summary>
    /// <remarks>
    /// This must be called after the <see cref="Text"/> property is set.
    /// </remarks>
    private void RaiseTextChangeEvents()
    {
        // Note the following sequence of these events (following WinUI)
        // 1. TextChanging occurs synchronously when text starts to change but before it is rendered.
        //    This occurs after the Text property is set.
        // 2. TextChanged occurs asynchronously after text changes and the new text is rendered.

        var textChangingEventArgs = new TextChangingEventArgs(TextChangingEvent);
        RaiseEvent(textChangingEventArgs);

        Dispatcher.UIThread.Post(() =>
        {
            var textChangedEventArgs = new TextChangedEventArgs(TextChangedEvent);
            RaiseEvent(textChangedEventArgs);
        }, DispatcherPriority.Normal);
    }

    private void HandleTextInput(string? input)
    {
        input = SanitizeInputText(input);

        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        // _selectedTextChangesMadeSinceLastUndoSnapshot++;
        //TODO: SnapshotUndoRedo(ignoreChangeCount: false);

        var currentText = Text ?? string.Empty;
        var selectionLength = Math.Abs(SelectionStart - SelectionEnd);
        var newLength = input.Length + currentText.Length - selectionLength;

        // if (MaxLength > 0 && newLength > MaxLength)
        // {
        //     input = input.Remove(Math.Max(0, input.Length - (newLength - MaxLength)));
        //     newLength = MaxLength;
        // }

        if (!string.IsNullOrEmpty(input))
        {
            var textBuilder = StringBuilderCache.Acquire(Math.Max(currentText.Length, newLength));
            textBuilder.Append(currentText);

            var caretIndex = CaretIndex;

            if (selectionLength != 0)
            {
                var (start, _) = GetSelectionRange();

                textBuilder.Remove(start, selectionLength);

                caretIndex = start;
            }

            textBuilder.Insert(caretIndex, input);

            var text = StringBuilderCache.GetStringAndRelease(textBuilder);

            SetCurrentValue(TextProperty, text);

            ClearSelection();

            SetCurrentValue(CaretIndexProperty, caretIndex + input.Length);
        }
    }

    private string? SanitizeInputText(string? text)
    {
        if (text is null)
            return null;

        for (var i = 0; i < InvalidCharacters.Length; i++)
        {
            text = text.Replace(InvalidCharacters[i], string.Empty);
        }

        return text;
    }

    private (int start, int end) GetSelectionRange()
    {
        var selectionStart = SelectionStart;
        var selectionEnd = SelectionEnd;

        return (Math.Min(selectionStart, selectionEnd), Math.Max(selectionStart, selectionEnd));
    }

    private bool DeleteSelection()
    {
        var (start, end) = GetSelectionRange();

        if (start != end)
        {
            var text = Text!;
            var textBuilder = StringBuilderCache.Acquire(text.Length);

            textBuilder.Append(text);
            textBuilder.Remove(start, end - start);

            SetCurrentValue(TextProperty, textBuilder.ToString());

            _presenter?.MoveCaretToTextPosition(start);

            SetCurrentValue(SelectionStartProperty, start);

            ClearSelection();

            return true;
        }

        SetCurrentValue(CaretIndexProperty, SelectionStart);

        return false;
    }

    private string GetSelection()
    {
        var text = Text;

        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        var selectionStart = SelectionStart;
        var selectionEnd = SelectionEnd;
        var start = Math.Min(selectionStart, selectionEnd);
        var end = Math.Max(selectionStart, selectionEnd);

        if (start == end || (Text?.Length ?? 0) < end)
        {
            return "";
        }

        return text.Substring(start, end - start);
    }

    private void MoveHorizontal(int direction, bool wholeWord, bool isSelecting, bool moveCaretPosition)
    {
        if (_presenter == null)
        {
            return;
        }

        var text = Text ?? string.Empty;
        var selectionStart = SelectionStart;
        var selectionEnd = SelectionEnd;

        if (!wholeWord)
        {
            if (isSelecting)
            {
                _presenter.MoveCaretToTextPosition(selectionEnd);

                _presenter.MoveCaretHorizontal(direction > 0 ? LogicalDirection.Forward : LogicalDirection.Backward);

                SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
            }
            else
            {
                if (selectionStart != selectionEnd)
                {
                    _presenter.MoveCaretToTextPosition(direction > 0
                        ? Math.Max(selectionStart, selectionEnd)
                        : Math.Min(selectionStart, selectionEnd));
                }
                else
                {
                    _presenter.MoveCaretHorizontal(direction > 0
                        ? LogicalDirection.Forward
                        : LogicalDirection.Backward);
                }

                SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
            }
        }
        else
        {
            int offset;

            if (direction > 0)
            {
                offset = StringUtils.NextWord(text, selectionEnd) - selectionEnd;
            }
            else
            {
                offset = StringUtils.PreviousWord(text, selectionEnd) - selectionEnd;
            }

            SetCurrentValue(SelectionEndProperty, SelectionEnd + offset);

            if (moveCaretPosition)
            {
                _presenter.MoveCaretToTextPosition(SelectionEnd);
            }

            if (!isSelecting && moveCaretPosition)
            {
                SetCurrentValue(CaretIndexProperty, SelectionEnd);
            }
            else
            {
                SetCurrentValue(SelectionStartProperty, selectionStart);
            }
        }
    }

    private void MoveHome(bool document)
    {
        if (_presenter is null)
        {
            return;
        }

        var caretIndex = CaretIndex;

        if (document)
        {
            _presenter.MoveCaretToTextPosition(0);
        }
        else
        {
            var textLines = _presenter.TextLayout.TextLines;
            var lineIndex = _presenter.TextLayout.GetLineIndexFromCharacterIndex(caretIndex, false);
            var textLine = textLines[lineIndex];

            _presenter.MoveCaretToTextPosition(textLine.FirstTextSourceIndex);
        }
    }

    private void MoveEnd(bool document)
    {
        if (_presenter is null)
        {
            return;
        }

        var text = Text ?? string.Empty;
        var caretIndex = CaretIndex;

        if (document)
        {
            _presenter.MoveCaretToTextPosition(text.Length, true);
        }
        else
        {
            var textLines = _presenter.TextLayout.TextLines;
            var lineIndex = _presenter.TextLayout.GetLineIndexFromCharacterIndex(caretIndex, false);
            var textLine = textLines[lineIndex];

            var textPosition = textLine.FirstTextSourceIndex + textLine.Length - textLine.NewLineLength;

            _presenter.MoveCaretToTextPosition(textPosition, true);
        }
    }

    private void MovePageRight()
    {
        _scrollViewer?.PageRight();
    }

    private void MovePageLeft()
    {
        _scrollViewer?.PageLeft();
    }

    private void MovePageUp()
    {
        _scrollViewer?.PageUp();
    }

    private void MovePageDown()
    {
        _scrollViewer?.PageDown();
    }

    private void SetSelectionForControlBackspace()
    {
        var text = Text ?? string.Empty;
        var selectionStart = CaretIndex;

        MoveHorizontal(-1, true, false, false);

        if (SelectionEnd > 0 &&
            selectionStart < text.Length && text[selectionStart] == ' ')
        {
            SetCurrentValue(SelectionEndProperty, SelectionEnd - 1);
        }

        SetCurrentValue(SelectionStartProperty, selectionStart);
    }

    private void SetSelectionForControlDelete()
    {
        var textLength = Text?.Length ?? 0;
        if (_presenter == null || textLength == 0)
        {
            return;
        }

        SetCurrentValue(SelectionStartProperty, CaretIndex);

        MoveHorizontal(1, true, true, false);

        if (SelectionEnd < textLength && Text![SelectionEnd] == ' ')
        {
            SetCurrentValue(SelectionEndProperty, SelectionEnd + 1);
        }
    }
}