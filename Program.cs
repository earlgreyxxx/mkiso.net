using System.Runtime.Versioning;

namespace mkiso
{
  [SupportedOSPlatform("windows")]
  internal class Program
  {
    const int BUFFER_SIZE = 1024 * 1024;

    static async Task<int> Main(string[] args)
    {
      if (args.Length <= 1)
      {
        var list = args.ToList();
        list.Add("help");
        args = [.. list];
      }

      string command = args[0];
      var parameters = new string[args.Length - 1];

      Array.Copy(args, 1, parameters, 0, args.Length - 1);

      switch (command)
      {
        case "drv":
          await drv2iso(parameters);
          break;

        case "dir":
          await dir2iso(parameters);
          break;

        case "help":
          help();
          break;

        default:
          Console.Error.WriteLine("helpコマンドを指定してヘルプを参照してください。");
          return -1;
      }

      return 0;
    }

    private static void help()
    {
      Console.WriteLine(
        @"[usage] mkiso サブコマンド コマンドパラメーター
サブコマンド:
   drv: CD/DVD/BDドライブに挿入されているメディアからISOファイルをダンプします。
   dir: 任意のディレクトリからISOファイルを作成します。
  help: このヘルプを表示します。

>> mkiso drv <drive letter> [out directory path]

  ドライブレター(コロンは不要:エラーチェック無し)を指定し、
  指定したディレクトリにダンプ出力します。
  (ファイル名にボリュームラベルを使用します)

  指定できるドライブはCD-ROMかDVDメディアなど。
  (市販のDVD Videoなどプロテクトがあるメディアは当然ですがエラーになります。)

>> mkiso dir <dest path> <volume name> <src directory>
   ディレクトリパスをルートとしてISOファイルを出力します。"
      );
    }

    // command drv2iso
    // ------------------------------------------------------------------------
    private static async Task drv2iso(string[] args)
    {
      if (args.Length < 2)
      {
        Console.WriteLine("Usage: drv2iso <output directory path> <drive letter>");
        return;
      }

      string destPath = args[0];
      string letter = args[1];

      try {
        CheckFileExistsAndPrompt(destPath);

        var mi = new MakeISO(destPath);

        mi.copyStart += path =>
        {
          Console.WriteLine($"出力先: {path}");
        };
        mi.readDone += (count, total) =>
        {
          Console.WriteLine($"コピー中: {count}/{total}");
        };
        mi.copyDone += () =>
        {
          Console.WriteLine("コピー完了");
        };

        await mi.Copy(letter, BUFFER_SIZE);

      } catch (Exception e) {
        Console.WriteLine(e.Message);
      }
    }

    // command dir2iso
    // ------------------------------------------------------------------------
    private static async Task dir2iso(string[] args)
    {
      if (args.Length < 3)
      {
        Console.WriteLine("Usage: dir2iso <dest iso path> <src directory path> <volume name>");
        return;
      }

      string destPath = args[0];
      string srcPath = args[1];
      string volumeName = args[2];

      try
      {
        CheckFileExistsAndPrompt(destPath);

        var mi = new MakeISO(destPath);

        if (await Task.Run(() => mi.Make(srcPath,volumeName)))
          Console.WriteLine($"{srcPath}:出力しました。");
        else
          Console.Error.WriteLine($"{srcPath}:出力に失敗しました。");
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }

    // check existing output path if exists, throw exception
    // ------------------------------------------------------------------------
    private static void CheckFileExistsAndPrompt(string path)
    {
      if (File.Exists(path))
      {
        ConsoleKeyInfo cki;
        Console.WriteLine($"{path}が既に存在しています。続行すると上書きされ元に戻せません。");
        Console.Write("続行しますか？：はい[y] ／ いいえ[N]");

        do
        {
          cki = Console.ReadKey(true);
          if (cki.Key == ConsoleKey.N || cki.Key == ConsoleKey.Enter)
          {
            Console.WriteLine("");
            throw new Exception("処理を中断しました。");
          }

        } while (cki.Key != ConsoleKey.Y);

        Console.WriteLine("");
      }
    }

  }
}
