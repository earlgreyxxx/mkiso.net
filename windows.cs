using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;

namespace mkiso
{
  using IStream = System.Runtime.InteropServices.ComTypes.IStream;

  // Import Windows platform API
  [SupportedOSPlatform("windows")]
  internal partial class Windows
  {
    const string ERROR_MESSAGE_INVALID_TYPE = "指定ドライブはCD-ROMもしくはDVDのみです。";

    public const uint FILE_ATTRIBUTE_SYSTEM = 0x4;
    public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    public const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x8;

    public const uint STGM_WRITE = 0x1;
    public const uint STGM_CREATE = 0x1000;

    [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern SafeFileHandle CreateFile(
      string lpFileName,
      [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
      [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
      IntPtr lpSecurityAttributes,
      [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
      uint dwFlagsAndAttributes,
      IntPtr hTemplateFile
    );

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    internal static extern void SHCreateStreamOnFileEx(
      string pszFile,
      uint grfMode,
      uint dwAttributes,
      bool fCreate,
      IStream? pstmTemplate,
      out IStream ppstm
    );

    // open cd/dvd drive and return SafeFileHandle
    // ------------------------------------------------------------------------
    internal static SafeFileHandle OpenDrive(string driveLetter)
    {
      var driveInfo = new DriveInfo(driveLetter);
      if(driveInfo.DriveType != DriveType.CDRom)
        throw new Exception(ERROR_MESSAGE_INVALID_TYPE);

      string physicalDriveName = string.Format(@"\\.\{0}:",driveLetter);
      SafeFileHandle handle = Windows.CreateFile(
        physicalDriveName,
        FileAccess.Read,
        FileShare.Write | FileShare.Read | FileShare.Delete,
        IntPtr.Zero,
        FileMode.Open,
        Windows.FILE_ATTRIBUTE_SYSTEM | Windows.FILE_FLAG_SEQUENTIAL_SCAN,
        IntPtr.Zero
      );

      return handle;
    }
  }
}
