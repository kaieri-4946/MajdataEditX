using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class UnityHost
{
    private readonly string _exePath;
    private Process _unityProcess;
    private IntPtr _unityHWND;
    private bool _isStarted;
    private Panel _unityPanel;

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern long GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern long SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const int GWL_STYLE = -16;
    private const int WS_CHILD = 0x40000000;
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_THICKFRAME = 0x00040000;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int WS_MAXIMIZEBOX = 0x00010000;
    private const int WS_SYSMENU = 0x00080000;
    private const long WS_POPUP = 0x80000000;

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;

    public UnityHost(string exePath, Panel UnityPanel)
    {
        _exePath = exePath;
        _unityPanel = UnityPanel;
    }

    public void StartUnityApp()
    {
        if (_isStarted) { return; }

        IntPtr panelHandle = _unityPanel.Handle;

        _unityProcess = new Process();
        _unityProcess.StartInfo.FileName = _exePath;
        _unityProcess.StartInfo.Arguments = $"-parentHWND {panelHandle.ToInt32()} {Environment.CommandLine}";

        _unityProcess.StartInfo.UseShellExecute = true;
        _unityProcess.StartInfo.CreateNoWindow = false;

        _unityProcess.Start();
        _unityProcess.WaitForInputIdle();

        EnumChildWindows(panelHandle, (IntPtr hWnd, IntPtr lParam) =>
        {
            _unityHWND = hWnd;
            return false; // Stop iterating once found
        }, IntPtr.Zero);

        _unityPanel.SizeChanged += FormsHost_SizeChanged;

        _isStarted = true;
    }

    public void DestroyUnityApp()
    {
        _unityPanel.SizeChanged -= FormsHost_SizeChanged;

        if (_unityProcess != null && !_unityProcess.HasExited)
        {
            _unityProcess.Kill();
            _unityProcess.Dispose();
        }
    }

    public bool IsProcessKilled()
    {
        return _unityProcess is null || _unityProcess.HasExited;
    }

    public void UndockUnityApp()
    {
        _unityPanel.SizeChanged -= FormsHost_SizeChanged;

        SetParent(_unityHWND, IntPtr.Zero);

        long style = GetWindowLong(_unityHWND, GWL_STYLE);

        style &= ~WS_CHILD;
        style |= WS_POPUP | WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU;

        SetWindowLong(_unityHWND, GWL_STYLE, style);

        // Force redraw or app popup without a way to move
        SetWindowPos(_unityHWND, IntPtr.Zero, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
    }

    public void RedockUnityApp()
    {
        _unityPanel.SizeChanged += FormsHost_SizeChanged;

        SetParent(_unityHWND, _unityPanel.Handle);

        long style = GetWindowLong(_unityHWND, GWL_STYLE);

        style &= ~(WS_POPUP | WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
        style |= WS_CHILD;

        SetWindowLong(_unityHWND, GWL_STYLE, style);

        MoveWindow(_unityHWND, 0, 0, _unityPanel.Width, _unityPanel.Height, true);
    }

    private void FormsHost_SizeChanged(object? sender, EventArgs e)
    {
        if (_unityHWND != IntPtr.Zero)
        {
            MoveWindow(_unityHWND, 0, 0, _unityPanel.Width, _unityPanel.Height, true);
        }
    }

    public void FixUnityWindowForRecording()
    {
        if (_unityHWND != IntPtr.Zero)
        {
            // Forcing even width and height
            MoveWindow(_unityHWND, 0, 0, _unityPanel.Width / 2 * 2, _unityPanel.Height / 2 * 2, true);
        }
    }
}