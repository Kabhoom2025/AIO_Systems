using System.Runtime.InteropServices;

namespace FoodOrder.PrintAgent;

/// <summary>Sends raw bytes (ESC/POS, ZPL, etc.) directly to a Windows printer via the spooler, bypassing GDI.</summary>
internal static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DOCINFOW
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string pDocName;
        [MarshalAs(UnmanagedType.LPWStr)] public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPWStr)] public string pDataType;
    }

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool OpenPrinterW(string printerName, out IntPtr hPrinter, IntPtr defaults);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool StartDocPrinterW(IntPtr hPrinter, int level, ref DOCINFOW docInfo);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr data, int bufferSize, out int written);

    public static void SendBytes(string printerName, byte[] bytes, string jobName = "FoodOrder Print Agent")
    {
        if (!OpenPrinterW(printerName, out var hPrinter, IntPtr.Zero))
            throw new InvalidOperationException($"Could not open printer '{printerName}' (Win32 error {Marshal.GetLastWin32Error()}).");

        try
        {
            var docInfo = new DOCINFOW
            {
                pDocName = jobName,
                pOutputFile = null,
                pDataType = "RAW",
            };

            if (!StartDocPrinterW(hPrinter, 1, ref docInfo))
                throw new InvalidOperationException($"StartDocPrinter failed (Win32 error {Marshal.GetLastWin32Error()}).");

            try
            {
                if (!StartPagePrinter(hPrinter))
                    throw new InvalidOperationException($"StartPagePrinter failed (Win32 error {Marshal.GetLastWin32Error()}).");

                var unmanagedBytes = Marshal.AllocHGlobal(bytes.Length);
                try
                {
                    Marshal.Copy(bytes, 0, unmanagedBytes, bytes.Length);
                    if (!WritePrinter(hPrinter, unmanagedBytes, bytes.Length, out var written) || written != bytes.Length)
                        throw new InvalidOperationException($"WritePrinter failed (Win32 error {Marshal.GetLastWin32Error()}).");
                }
                finally
                {
                    Marshal.FreeHGlobal(unmanagedBytes);
                }

                EndPagePrinter(hPrinter);
            }
            finally
            {
                EndDocPrinter(hPrinter);
            }
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }
}
