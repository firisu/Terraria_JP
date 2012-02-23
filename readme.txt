========== ========== ==========
  Terraria_JP  ver 0.1.0.0
========== ========== ==========

【 ソフト名 】Terraria_JP
【 開発環境 】Visual Studio C# 2010 Express
【バージョン】0.1.0.0
【最終更新日】2012/02/23
【ライセンス】MITライセンス（xnb ファイルを除く）
【配布サイト】https://github.com/firisu/Terraria_JP

---------- ---------- ----------


◇ 概要 ◇
　Terraria_JP は Terraria の実行ファイルを日本語化するソフトです。
　実行すると Terraria.exe の中身を書き換えます。
　なお元のファイルは Terraria_old.exe にバックアップされます。
　その他のファイルは一切変更しません。

　既知の不具合があるので、必ず release.txt に目を通してください。


◇ 動作条件 ◇
　Terraria に準じます。
　.NET Framework 4 が入っていれば動くはずです。
　動作確認は Windows 7 Home Premium 64bit でしか行っていません。


◇ インストール ◇
　bin フォルダの中身を Terraria.exe がある場所に丸ごとコピーして下さい。


◇ アンインストール ◇
(1) 以下のファイルとフォルダを削除してください。
    ・Terraria_JP.exe
    ・Terraria_JP
(2) Terraria_old.exe を Terraria.exe にリネームしてください。


◇ 免責 ◇
このソフトウェアは無保証で提供されます。
作者はこのソフトウェアに関して何ら責任を負いません。
詳しくはMITライセンスの文面を参照して下さい。


◇ ライセンス ◇
MITライセンスです。

ただし同梱しているスプライトフォント（*.xnb）についてのみ、
M+ フォントのライセンスに準拠するとします。

※フォントファイル自体は同梱されていませんが、派生物に関する条項が面倒くさいので念のため。


◇ 使用ライブラリ・ソフトについて ◇
Mono.Cecil
	Author:  Jb Evain
	License: MIT/X11(http://www.opensource.org/licenses/mit-license.php)

ILRepack
	Author:  Francois Valdy
	License: Apache License 2.0(http://www.apache.org/licenses/LICENSE-2.0)

M+フォント
	Author:  Itou Hiroki
	License: IPAフォントライセンス等


◇ Tips ◇
　Terraria_JP/Settings.xml の NoSteam を true に変えると、Steam API を使わず起動します。
　Steam無しでもゲームが起動できますが、当然Steamとの連携が出来なくなります。（ゲーム時間のカウントとか）

　Terraria_JP フォルダの中に language.xml を置くと、そのデータを優先して使います。
　（置かなければ Terraria.exe に埋め込まれたデータを使います。）
　テスト時などにどうぞ。
