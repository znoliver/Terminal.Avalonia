using System;
using System.Diagnostics;
using Avalonia.Threading;
using Terminal.Core.VirtualTerminal;
using Terminal.Core.VirtualTerminal.Encodings;
using Terminal.Core.VirtualTerminal.Enums;
using Terminal.Core.VirtualTerminal.Model;
using Terminal.Core.XTermParser;

namespace Terminal.Avalonia.Controls;

public partial class Terminal : IVirtualTerminalController
{
    private ECharacterSet CharacterSet
    {
        get
        {
            switch (CursorState.CharacterSetMode)
            {
                case ECharacterSetMode.IsoG1:
                    return CursorState.G1;
                case ECharacterSetMode.IsoG2:
                    return CursorState.G2;
                case ECharacterSetMode.IsoG3:
                    return CursorState.G3;
                case ECharacterSetMode.Vt300G1:
                    return CursorState.Vt300G1;
                case ECharacterSetMode.Vt300G2:
                    return CursorState.Vt300G2;
                case ECharacterSetMode.Vt300G3:
                    return CursorState.Vt300G3;
                default:
                    return CursorState.G0;
            }
        }
    }

    private ECharacterSet RightCharacterSet
    {
        get
        {
            switch (CursorState.CharacterSetModeR)
            {
                case ECharacterSetMode.IsoG1:
                    return CursorState.G1;
                case ECharacterSetMode.IsoG2:
                    return CursorState.G2;
                case ECharacterSetMode.IsoG3:
                    return CursorState.G3;
                default:
                    return CursorState.G0;
            }
        }
    }

    private DataConsumer _dataConsumer;

    private int _changeCount;
    
    /// <summary>
    /// The current buffer
    /// </summary>
    internal TerminalLines Buffer { get; set; }
    
    /// <summary>
    /// The logical top row of the view port. This translates relative to the buffer
    /// </summary>
    internal int TopRow { get; set; } = 0;
    
    /// <summary>
    /// The number of logical columns for text formatting
    /// </summary>
    internal int Columns { get; set; } = 80;

    /// <summary>
    /// The number of logical rows for text formatting
    /// </summary>
    internal int Rows { get; set; } = 24;

    /// <summary>
    /// The number of visible columns configured by the hosting application
    /// </summary>
    public int VisibleColumns { get; set; } = 0;

    /// <summary>
    /// The number of visible rows configured by the hosting application
    /// </summary>
    public int VisibleRows { get; set; } = 0;

    /// <summary>
    /// The current state of all cursor and attribute properties
    /// </summary>
    public TerminalCursorState CursorState { get; set; } = new();

    /// <summary>
    /// Encapsulates pasted text so that receiving applications know it was explicitly pasted.
    /// </summary>
    public bool BracketedPasteMode { get; set; }
    
    /// <summary>
    /// Defines the attributes which should be assigned to null character values
    /// </summary>
    /// <remarks>
    /// When drawing the background of the terminal, this attribute should be used to calculate
    /// the value of the color. The colors set here are the colors which were applied during the
    /// last screen erase.
    /// </remarks>
    public TerminalAttribute NullAttribute = new TerminalAttribute();
    
    /// <summary>
    /// Guarded text area range
    /// </summary>
    public TextRange GuardedArea { get; set; }
    
    /// <summary>
    /// Holds a reference to the last character set.
    /// </summary>
    /// <remarks>
    /// This is used for repeating characters as per (Repeat the preceding graphic character Ps times (REP).)
    /// however I'm not convinced it will always used wholesomely. 
    /// </remarks>
    private TerminalCharacter LastCharacter { get; set; }
    
    private TerminalLine GetCurrentLine()
    {
        return GetLine(TopRow + CursorState.CurrentRow);
    }
    
    private int CurrentLineColumns
    {
        get
        {
            var line = GetCurrentLine();
            if (line == null)
                return Columns;

            return (line.DoubleWidth | line.DoubleHeightTop | line.DoubleHeightBottom) ? (Columns >> 1) : Columns;
        }
    }

    public Terminal()
    {
        _dataConsumer = new DataConsumer(this);
    }

    public void ClearChanges()
    {
        _changeCount = 0;
    }

    public bool IsUtf8() => false;

    public bool IsVt52Mode()
    {
        throw new System.NotImplementedException();
    }

    public void Backspace()
    {
        throw new System.NotImplementedException();
    }

    public void Bell()
    {
        throw new System.NotImplementedException();
    }

    public void CarriageReturn()
    {
        _changeCount++;
    }

    public void ClearScrollingRegion()
    {
        throw new System.NotImplementedException();
    }

    public void ClearTab()
    {
        throw new System.NotImplementedException();
    }

    public void ClearTabs()
    {
        throw new System.NotImplementedException();
    }

    public void DeleteCharacter(int count)
    {
        throw new System.NotImplementedException();
    }

    public void DeleteColumn(int count)
    {
        throw new System.NotImplementedException();
    }

    public void DeleteLines(int count)
    {
        throw new System.NotImplementedException();
    }

    public void DeviceStatusReport()
    {
        throw new System.NotImplementedException();
    }

    public void Enable80132Mode(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void Enable132ColumnMode(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableAlternateBuffer()
    {
        throw new System.NotImplementedException();
    }

    public void EnableApplicationCursorKeys(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableAutoRepeatKeys(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableBlinkingCursor(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableLeftAndRightMarginMode(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableNationalReplacementCharacterSets(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableNormalBuffer()
    {
        throw new System.NotImplementedException();
    }

    public void EnableOriginMode(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableReverseVideoMode(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableReverseWrapAroundMode(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableSmoothScrollMode(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableSgrMouseMode(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EnableUrxvtMouseMode(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void EnableWrapAroundMode(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void EraseAbove(bool ignoreProtected)
    {
        throw new System.NotImplementedException();
    }

    public void EraseAll(bool ignoreProtected)
    {
        throw new System.NotImplementedException();
    }

    public void EraseBelow(bool ignoreProtected)
    {
        throw new System.NotImplementedException();
    }

    public void EraseCharacter(int count)
    {
        throw new System.NotImplementedException();
    }

    public void EraseLine(bool ignoreProtected)
    {
        throw new System.NotImplementedException();
    }

    public void EraseToEndOfLine(bool ignoreProtected)
    {
        throw new System.NotImplementedException();
    }

    public void EraseToStartOfLine(bool ignoreProtected)
    {
        throw new System.NotImplementedException();
    }

    public void FormFeed()
    {
        throw new System.NotImplementedException();
    }

    public void FullReset()
    {
        throw new System.NotImplementedException();
    }

    public void InsertBlanks(int count)
    {
        throw new System.NotImplementedException();
    }

    public void InsertColumn(int count)
    {
        throw new System.NotImplementedException();
    }

    public void InsertLines(int count)
    {
        throw new System.NotImplementedException();
    }

    public void InvokeCharacterSetMode(ECharacterSetMode mode)
    {
        throw new System.NotImplementedException();
    }

    public void InvokeCharacterSetModeR(ECharacterSetMode mode)
    {
        throw new System.NotImplementedException();
    }

    public void MoveCursorRelative(int x, int y)
    {
        throw new System.NotImplementedException();
    }

    public void NewLine()
    {
        HandleTextInput(Environment.NewLine);
    }

    public void PopXTermWindowIcon()
    {
        throw new System.NotImplementedException();
    }

    public void PopXTermWindowTitle()
    {
        throw new System.NotImplementedException();
    }

    public void ProtectCharacter(int protect)
    {
        throw new System.NotImplementedException();
    }

    public void PushXTermWindowIcon()
    {
        throw new System.NotImplementedException();
    }

    public void PushXTermWindowTitle()
    {
        throw new System.NotImplementedException();
    }

    public void PutChar(char character)
    {
        if (!CursorState.Utf8 && IsRGrCharacter(character))
        {
            character = Iso2022Encoding.DecodeChar((char)(character - (char)0x80), RightCharacterSet,
                CursorState.NationalCharacterReplacementMode);
        }
        else
        {
            character = Iso2022Encoding.DecodeChar(character, CharacterSet,
                CursorState.NationalCharacterReplacementMode);
        }

        if (CursorState.SingleShiftSelectCharacterMode != ECharacterSetMode.Unset)
        {
            CursorState.CharacterSetMode = CursorState.SingleShiftSelectCharacterMode;
            CursorState.SingleShiftSelectCharacterMode = ECharacterSetMode.Unset;
        }
        if (IsCombiningCharacter(character) && CursorState.CurrentColumn > 0)
        {
            // TODO : Find a better solution to ensure that combining marks work
            var changedCharacter = SetCombiningCharacter(CursorState.CurrentColumn - 1, CursorState.CurrentRow, character);
            if(changedCharacter != null)
                LastCharacter = changedCharacter.Clone();

            return;
        }

        if (CursorState.InsertMode == EInsertReplaceMode.Insert)
        {
            while (Buffer.Count <= (TopRow + CursorState.CurrentRow))
                Buffer.Add(new TerminalLine());

            var line = Buffer[TopRow + CursorState.CurrentRow];
            while (line.Count < CursorState.CurrentColumn)
                line.Add(new TerminalCharacter());

            line.Insert(CursorState.CurrentColumn, new TerminalCharacter());
        }

        if (CursorState.CurrentColumn >= CurrentLineColumns && CursorState.WordWrap)
        {
            CursorState.CurrentColumn = 0;
            NewLine();
        }

            
        LastCharacter = SetCharacter(CursorState.CurrentColumn, CursorState.CurrentRow, character, CursorState.Attributes).Clone();
        
        Dispatcher.UIThread.Post(() =>
        {
            var text = GetValue(TextProperty) + character;
            Console.WriteLine(LastCharacter.CombiningCharacters);
            HandleTextInput(LastCharacter.CombiningCharacters);
        },DispatcherPriority.Normal);
    }

    public void PutG2Char(char character)
    {
        throw new System.NotImplementedException();
    }

    public void PutG3Char(char character)
    {
        throw new System.NotImplementedException();
    }

    public void RepeatLastCharacter(int count)
    {
        throw new System.NotImplementedException();
    }

    public void RequestDecPrivateMode(int mode)
    {
        throw new System.NotImplementedException();
    }

    public void RequestStatusStringSetConformanceLevel()
    {
        throw new System.NotImplementedException();
    }

    public void RequestStatusStringSetProtectionAttribute()
    {
        throw new System.NotImplementedException();
    }

    public void ReportCursorPosition()
    {
        throw new System.NotImplementedException();
    }

    public void ReportExtendedCursorPosition()
    {
        throw new System.NotImplementedException();
    }

    public void ReportRgbBackgroundColor()
    {
        throw new System.NotImplementedException();
    }

    public void ReportRgbForegroundColor()
    {
        throw new System.NotImplementedException();
    }

    public void RestoreCursor()
    {
        throw new System.NotImplementedException();
    }

    public void RestoreEnableNormalBuffer()
    {
        throw new System.NotImplementedException();
    }

    public void RestoreEnableSgrMouseMode()
    {
        throw new System.NotImplementedException();
    }

    public void RestoreUseCellMotionMouseTracking()
    {
        throw new System.NotImplementedException();
    }

    public void RestoreUseHighlightMouseTracking()
    {
        throw new System.NotImplementedException();
    }

    public void RestoreBracketedPasteMode()
    {
        throw new System.NotImplementedException();
    }

    public void RestoreCursorKeys()
    {
        throw new System.NotImplementedException();
    }

    public void ReverseIndex()
    {
        throw new System.NotImplementedException();
    }

    public void ReverseTab()
    {
        throw new System.NotImplementedException();
    }

    public void SetAutomaticNewLine(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void SaveBracketedPasteMode()
    {
        throw new System.NotImplementedException();
    }

    public void SaveCursor()
    {
        throw new System.NotImplementedException();
    }

    public void SaveCursorKeys()
    {
        throw new System.NotImplementedException();
    }

    public void SaveEnableNormalBuffer()
    {
        throw new System.NotImplementedException();
    }

    public void SaveEnableSgrMouseMode()
    {
        throw new System.NotImplementedException();
    }

    public void SaveUseCellMotionMouseTracking()
    {
        throw new System.NotImplementedException();
    }

    public void SaveUseHighlightMouseTracking()
    {
        throw new System.NotImplementedException();
    }

    public void Scroll(int rows)
    {
        throw new System.NotImplementedException();
    }

    public void ScrollAcross(int columns)
    {
        throw new System.NotImplementedException();
    }

    public void SendDeviceAttributes()
    {
        throw new System.NotImplementedException();
    }

    public void SendDeviceAttributesSecondary()
    {
        throw new System.NotImplementedException();
    }

    public void SendDeviceAttributesTertiary()
    {
        throw new System.NotImplementedException();
    }

    public void SetAbsoluteRow(int line)
    {
        throw new System.NotImplementedException();
    }

    public void SetBracketedPasteMode(bool enable)
    {
        BracketedPasteMode = enable;
    }

    public void SetCharacterAttribute(int parameter)
    {
        throw new System.NotImplementedException();
    }

    public void SetCharacterSet(ECharacterSet characterSet, ECharacterSetMode mode)
    {
        throw new System.NotImplementedException();
    }

    public void SetCharacterSize(ECharacterSize size)
    {
        throw new System.NotImplementedException();
    }

    public void SetConformanceLevel(int level, bool eightBit)
    {
        throw new System.NotImplementedException();
    }

    public void SetCursorPosition(int column, int row)
    {
        throw new System.NotImplementedException();
    }

    public void SetCursorStyle(ECursorShape shape, bool blink)
    {
        throw new System.NotImplementedException();
    }

    public void SetEndOfGuardedArea()
    {
        throw new System.NotImplementedException();
    }

    public void SetErasureMode(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetGuardedAreaTransferMode(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetInsertReplaceMode(EInsertReplaceMode mode)
    {
        throw new System.NotImplementedException();
    }

    public void SetIso8613PaletteBackground(int paletteEntry)
    {
        throw new System.NotImplementedException();
    }

    public void SetIso8613PaletteForeground(int paletteEntry)
    {
        throw new System.NotImplementedException();
    }

    public void SetLatin1()
    {
        throw new System.NotImplementedException();
    }

    public void SetLeftAndRightMargins(int left, int right)
    {
        throw new System.NotImplementedException();
    }

    public void SetKeypadType(EKeypadType type)
    {
        throw new System.NotImplementedException();
    }

    public void SetRgbBackgroundColor(int red, int green, int blue)
    {
        throw new System.NotImplementedException();
    }

    public void SetRgbBackgroundColor(string xParseColor)
    {
        throw new System.NotImplementedException();
    }

    public void SetRgbForegroundColor(int red, int green, int blue)
    {
        throw new System.NotImplementedException();
    }

    public void SetRgbForegroundColor(string xParseColor)
    {
        throw new System.NotImplementedException();
    }

    public void SetScrollingRegion(int top, int bottom)
    {
        throw new System.NotImplementedException();
    }

    public void SetSendFocusInAndFocusOutEvents(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetStartOfGuardedArea()
    {
        throw new System.NotImplementedException();
    }

    public void SetUseAllMouseTracking(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetUTF8()
    {
        throw new System.NotImplementedException();
    }

    public void SetUtf8MouseMode(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetVt52AlternateKeypadMode(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetVt52GraphicsMode(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetVt52Mode(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetWindowTitle(string title)
    {
        // todo: set window title
        Debug.WriteLine($"SetWindowTitle: {title}");
    }

    public void SetX10SendMouseXYOnButton(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetX11SendMouseXYOnButton(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void ShiftIn()
    {
        throw new System.NotImplementedException();
    }

    public void ShiftOut()
    {
        throw new System.NotImplementedException();
    }

    public void SingleShiftSelectG2()
    {
        throw new System.NotImplementedException();
    }

    public void SingleShiftSelectG3()
    {
        throw new System.NotImplementedException();
    }

    public void ShowCursor(bool show)
    {
        throw new System.NotImplementedException();
    }

    public void Tab()
    {
        throw new System.NotImplementedException();
    }

    public void TabSet()
    {
        throw new System.NotImplementedException();
    }

    public void UseCellMotionMouseTracking(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void UseHighlightMouseTracking(bool enable)
    {
        throw new System.NotImplementedException();
    }

    public void VerticalTab()
    {
        throw new System.NotImplementedException();
    }

    public void Vt52EnterAnsiMode()
    {
        throw new System.NotImplementedException();
    }

    public void Vt52Identify()
    {
        throw new System.NotImplementedException();
    }

    public void XTermDeiconifyWindow()
    {
        throw new System.NotImplementedException();
    }

    public void XTermFullScreenEnter()
    {
        throw new System.NotImplementedException();
    }

    public void XTermFullScreenExit()
    {
        throw new System.NotImplementedException();
    }

    public void XTermFullScreenToggle()
    {
        throw new System.NotImplementedException();
    }

    public void XTermIconifyWindow()
    {
        throw new System.NotImplementedException();
    }

    public void XTermLowerToBottom()
    {
        throw new System.NotImplementedException();
    }

    public void XTermMaximizeWindow(bool horizontally, bool vertically)
    {
        throw new System.NotImplementedException();
    }

    public void XTermMoveWindow(int x, int y)
    {
        throw new System.NotImplementedException();
    }

    public void XTermRaiseToFront()
    {
        throw new System.NotImplementedException();
    }

    public void XTermRefreshWindow()
    {
        throw new System.NotImplementedException();
    }

    public void XTermReport(XTermReportType reportType)
    {
        throw new System.NotImplementedException();
    }

    public void XTermResizeTextArea(int columns, int rows)
    {
        throw new System.NotImplementedException();
    }

    public void XTermResizeWindow(int width, int height)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Returns whether the character is considered to be "Right Graphics"
    /// </summary>
    /// <todo>
    /// I believe this is good enough for now.
    /// </todo>
    /// <param name="character">The character to test</param>
    /// <returns>True if in the high character region</returns>
    private bool IsRGrCharacter(char character) => character is >= (char)0xA0 and <= (char)0xFF;
    
    private TerminalCharacter SetCombiningCharacter(int column, int row, char combiningCharacter)
    {
        var line = GetVisualLine(row);

        if (line != null && column < line.Count)
        {
            line[column].CombiningCharacters += combiningCharacter;
            return line[column];
        }

        return null;
    }
    
    private TerminalLine GetVisualLine(int y)
    {
        return GetLine(y);
    }
    
    private TerminalLine GetLine(int lineNumber)
    {
        if (lineNumber >= Buffer.Count)
            return null;

        return Buffer[lineNumber];
    }
    
    private static bool IsCombiningCharacter(char ch)
    {
        return 
            (ch >= '\u0300' && ch <= '\u036F') ||   // Combining diacritical marks
            (ch >= '\u1AB0' && ch <= '\u1ABE') ||   // Combining diacritical marks extended
            (ch >= '\u1DC0' && ch <= '\u1DFF') ||   // Combining diacritical marks supplement
            (ch >= '\u20D0' && ch <= '\u20F1') ||   // Combining diacritical marks for symbols
            (ch >= '\uFE20' && ch <= '\uFE2F')      // Combining half marks
            ;
    }
    
    private TerminalCharacter SetCharacter(int currentColumn, int currentRow, char ch, TerminalAttribute attribute, bool overwriteProtected=true)
    {
        while (Buffer.Count < (currentRow + TopRow + 1))
            Buffer.Add(new TerminalLine());

        var line = Buffer[currentRow + TopRow];
        while (line.Count < (currentColumn + 1))
            line.Add(new TerminalCharacter { Char = ' ', Attributes = NullAttribute.Clone() });

        var character = line[currentColumn];
        if ((GuardedArea == null && overwriteProtected) || (!overwriteProtected && character.Attributes.Protected != 1) || (GuardedArea != null && !GuardedArea.Contains(currentColumn, currentRow)))
        {
            character.Char = ch;
            character.Attributes = CursorState.Attributes.Clone();
            character.CombiningCharacters = "";
        }

        return character;
    }
}