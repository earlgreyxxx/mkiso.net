using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using IMAPI2FS;
using System.Runtime.Versioning;

namespace mkiso
{
  using IStream = System.Runtime.InteropServices.ComTypes.IStream;

  [SupportedOSPlatform("windows")]
  public class MakeISO
  {
    const string ERROR_MESSAGE_INVALID_TYPE = "指定ドライブはCD-ROMもしくはDVDのみです。";
    const string ERROR_MESSAGE_INVALID_BUFFER_SIZE = "バッファサイズが不正です。";
    const string ERROR_MESSAGE_PROCESS_STOP = "";
    const string ERROR_MESSAGE_DIRECTORY_NOT_EXIST = "指定したディレクトリが存在しません。";
    const string ERROR_FORMAT_OPEN = "ドライブをオープンできません。エラーコード：{0}";
    const string ERROR_NOPATH = "出力先のファイルパスが指定されていません。";

    public event Action<string>? copyStart;
    public event Action<int,int>? readDone;
    public event Action? copyDone;

    private string strOutputPath { set; get; }

    public MakeISO(string strOutputFilePath)
    {
      if(string.IsNullOrEmpty(strOutputFilePath))
        throw new ArgumentException(ERROR_NOPATH);

      strOutputPath = strOutputFilePath;
    }

    // implements copy drive to file
    // ------------------------------------------------------------------------
    public async Task Copy(string letter,int bufSize = 81920)
    {
      if(bufSize <= 0)
        throw new Exception(ERROR_MESSAGE_INVALID_BUFFER_SIZE);

      var driveInfo = new DriveInfo(letter);

      var buf = new byte[bufSize];
      using SafeFileHandle hDrive = OpenDrive(letter);

      if(hDrive.IsInvalid)
        throw new Exception(string.Format(ERROR_FORMAT_OPEN,Marshal.GetLastWin32Error()));

      using FileStream dest = File.Open(strOutputPath, FileMode.Create);
      using FileStream src = new(hDrive, FileAccess.Read);

      copyStart?.Invoke(strOutputPath);
      int numRead = 0;
      int count = 0;
      int totalCount = (int)((driveInfo.TotalSize - driveInfo.TotalFreeSpace) / (long)bufSize);
      if ((driveInfo.TotalSize - driveInfo.TotalFreeSpace) % (long)bufSize > 0)
        totalCount++;

      while (0 < (numRead = await src.ReadAsync(buf, 0, bufSize)))
      {
        await dest.WriteAsync(buf, 0, numRead);
        readDone?.Invoke(++count, totalCount);
      }

      copyDone?.Invoke();
    }

    // open cd/dvd drive and return SafeFileHandle
    // ------------------------------------------------------------------------
    private SafeFileHandle OpenDrive(string driveLetter)
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

    // ------------------------------------------------------------------------

    public bool Make(string srcPath,string volumeName)
    {
      if(!Directory.Exists(srcPath))
        throw new Exception(ERROR_MESSAGE_DIRECTORY_NOT_EXIST);

      IFileSystemImage image = (IFileSystemImage)new MsftFileSystemImage();
      image.ChooseImageDefaultsForMediaType(IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK);
      image.FileSystemsToCreate = FsiFileSystems.FsiFileSystemJoliet | FsiFileSystems.FsiFileSystemISO9660;
      image.VolumeName = volumeName;
      image.Root.AddTree(srcPath, false);

      IFileSystemImageResult resultImage = image.CreateResultImage();
      IStream srcStream = (IStream)resultImage.ImageStream;
      Windows.SHCreateStreamOnFileEx(
        strOutputPath,
        Windows.STGM_WRITE | Windows.STGM_CREATE,
        Windows.FILE_ATTRIBUTE_NORMAL,
        false,
        null,
        out IStream destStream
      );

      srcStream.CopyTo(destStream, (long)resultImage.TotalBlocks * resultImage.BlockSize, IntPtr.Zero, IntPtr.Zero);
      destStream.Commit(0);
      Marshal.ReleaseComObject(srcStream);
      Marshal.ReleaseComObject(destStream);

      return true;
    }
  }

  // Import Windows platform API
  internal partial class Windows
  {
    public const uint FILE_ATTRIBUTE_SYSTEM = 0x4;
    public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    public const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x8;

    public const uint STGM_WRITE = 0x1;
    public const uint STGM_CREATE = 0x1000;

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern SafeFileHandle CreateFile(
      string lpFileName,
      [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
      [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
      IntPtr lpSecurityAttributes,
      [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
      uint dwFlagsAndAttributes,
      IntPtr hTemplateFile
    );

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern void SHCreateStreamOnFileEx(
      string pszFile,
      uint grfMode,
      uint dwAttributes,
      bool fCreate,
      IStream? pstmTemplate,
      out IStream ppstm
    );
  }
}
