# mkiso
ドライブもしくはディレクトリからISOファイルを作成します。ほぼAPIドキュメントのサンプルそのままです。

## プラットフォーム
* Windows10以降／.NET9 以降

## ビルド
* Visual studio 2022 or higher.
* Windows SDK commandline tools.

## 説明
```
mkiso サブコマンド コマンドパラメーター
```
### サブコマンド:  
#### drv
CD/DVD/BDドライブに挿入されているメディアからISOファイルをダンプします。

```
mkiso drv <drive letter> [out directory path]
```
ドライブレター(コロンは不要:エラーチェック無し)を指定し、指定したディレクトリにダンプ出力します。  
(ファイル名にボリュームラベルを使用します)

指定できるドライブはCD-ROMかDVDメディアなど。  
(市販のDVD Videoなどプロテクトがあるメディアは当然ですがエラーになります。)

#### dir
任意のディレクトリからISOファイルを作成します。 (実際にメディアに焼けるかどうかは試してみません)

```
mkiso dir <dest path> <volume name> <src directory>
```
ディレクトリパスをルートとしてISOファイルを出力します。
