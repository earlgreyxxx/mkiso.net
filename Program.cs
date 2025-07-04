﻿using System.Runtime.Versioning;

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

>> mkiso drv sample.iso q

  ISOファイルパス(sample.iso)に指定したドライブレター(コロン不要)のドライブのディスクをダンプします。

  指定できるドライブにはデータ用のCD-ROMかDVDメディアが挿入されている必要があります。
  (音楽CD(CD Audio)や市販のDVD/Blulay Videoディスクなどのメディアはエラーになります。)

>> mkiso dir sample.iso iso\source\dir VOLNAME

   iso\source\dirパスをルートとしたISOファイルをsample.isoとして出力します。"
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
          string p = ((decimal)count / total * 100).ToString("F2");
          Console.Write($"\rコピー中: {count}/{total}({p}%)");
        };
        mi.copyDone += () =>
        {
          Console.WriteLine("\nコピー完了");
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

      string destPath = args[0],srcPath = args[1],volumeName = args[2];

      try
      {
        CheckFileExistsAndPrompt(destPath);

        var mi = new MakeISO(destPath);

        Console.WriteLine("ISOファイルを生成しています...");

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
