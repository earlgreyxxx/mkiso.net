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
mkiso drv <out iso path> <drive letter>
```
出力ファイルおよびドライブレター(コロン不要)を指定します。

※データ用メディアのみ対応しています（音楽CDや市販のDVD/Blulay Videoなどプロテクトがあるメディアはエラーになります。)

#### dir
任意のディレクトリからISOファイルを作成します。 (実際にメディアに焼けるかどうかは試してみません)

```
mkiso dir <dest path> <src directory> <volume name>
```
ディレクトリパスをルートとしてISOファイルを出力します。
