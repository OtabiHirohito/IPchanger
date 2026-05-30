# IPアドレス切替器

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

**IPアドレス切替器** は、Windowsのネットワーク設定（IPアドレス、サブネットマスク、ゲートウェイ、DNS）を素早く、簡単に切り替えるためのオープンソースソフトウェアです。

社内ネットワーク、客先ネットワーク、開発環境など、ネットワーク設定を頻繁に切り替えるエンジニアやIT管理者に最適です。

![スクリーンショット](./screenshot1.png)

## 🚀 主な機能

* **複数の設定パターン保存**: 最大4つまでのネットワーク設定を保存し、ボタン一つで切り替え可能。
* **DHCP切り替え**: ワンクリックでIPアドレスとDNSを自動取得（DHCP）にリセット。
* **詳細設定モード**: ゲートウェイやDNSの設定を必要に応じて表示・非表示に切り替え可能。
* **アダプター選択**: PCに搭載されている有効なネットワークアダプターを自動検出し、対象を選択可能。
* **メモ機能**: 設定内容や用途を記録しておける便利なサイドメモウィンドウを搭載。
* **設定の自動保存**: 入力した内容は自動的に保存され、次回の起動時にも保持されます。
* **モダンなUI**: 直感的で使いやすいWPFベースのデザイン。

## 📋 動作要件

* **OS**: Windows 10 / 11 (64bit推奨)
* **ランタイム**: [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
* **権限**: ネットワーク設定を変更するため、**管理者権限**が必要です。

## 🛠 使い方

1. `IPchanger.exe` を実行します（ネットワーク設定変更のため、管理者として実行されます）。
2. 上部のドロップダウンから、設定を変更したい「ネットワークアダプター」を選択します。
3. 切り替えたいIPアドレス、サブネットマスク等を入力します（「詳細設定」ボタンでゲートウェイとDNSの入力欄が表示されます）。
4. 「適用」ボタンをクリックすると、設定が反映されます。
5. 「DHCP (自動取得) にする」ボタンを押すと、すべての設定が自動取得に戻ります。

## 📦 インストール / 開発

 ### インストール方法

1. 以下のリンクから `IPchanger.zip` をダウンロードします。

   [IPchanger.zip をダウンロード](https://github.com/OtabiHirohito/IPchanger/releases)

2. ダウンロードしたZIPファイルを任意の場所に展開します。
3. 展開したフォルダー内の `IPchanger.exe` を実行します。
4. ネットワーク設定を変更するため、管理者権限で実行してください。

 ### ビルド方法

1. [Visual Studio 2022](https://visualstudio.microsoft.com/ja/vs/) または [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) をインストールします。
2. リポジトリをクローンします。

   ```bash
   git clone https://github.com/OtabiHirohito/IPchanger.git
   ```

3. ソリューションファイル `IPchanger.sln` を開いてビルドするか、コマンドラインで以下を実行します。

   ```bash
   dotnet build
   ```

## 🤝 寄付について

   このソフトを気に入っていただけた場合は、よろしければ以下の寄付先への支援をご検討ください。
<sub>本ソフトおよび制作者はリンク先の組織とは一切関係がございません。</sub>

* [寄付先1](https://donate.jrc.or.jp/ "日本赤十字社")
* [寄付先2](https://www.doubutukikin.or.jp/legal/business/ "どうぶつ基金")

## 📄 ライセンス

  このプロジェクトは **MITライセンス** のもとで公開されています。詳細は [LICENSE.txt](LICENSE.txt) をご覧ください。

  ---

  Created by 大度寛仁

