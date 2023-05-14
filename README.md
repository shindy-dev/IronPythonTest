# IronPythonTest
IronPythonのテスト用リポジトリ（Windows環境）

# [IronPython](https://ironpython.net/)とは
.Net Core上で動作するPythonのこと

## 実行環境インストール
* 以下から.msiをダウンロードし、インストール  
  https://github.com/IronLanguages/ironpython3/releases/tag/v3.4.0
* システム環境変数「Path」へipy.exeが格納されているディレクトリを追加  
  例）C:\Program Files\IronPython 3.4

## Pythonコードからdll/exe生成
以下はos内に含まれるpyスクリプトをdllへ変換するコマンド例
~~~
cd sample
ipyc logmanager.py
rem logmanager.dllが作成される

ipyc /out:utils /main:utils/__init__.py utils/ls.py utils/wc.py /target:dll
rem ls.pyとwc.pyの内容を含んだutils.dllが作成される
~~~
[オプション一覧](https://github.com/IronLanguages/ironpython3/tree/master/Src/IronPythonCompiler)


# 開発環境準備
生成したdllを.Net系の言語で読み込むための準備
* Visual StudioでVB.NetやC#のプロジェクトを作成後にNugetで下記のパッケージをインストール
  * IronPython
  * IronPython.StdLib ... IronPythonで使用可能なPythonの標準ライブラリ  

* プロジェクト設定コンパイルのビルドイベント内のビルド前イベントに以下コマンドを登録  
※IronPythonやPythonがインストールされていない環境でも実施したい場合に設定すること
~~~
rd /s /q "$(TargetDir)lib"
XCOPY "$(ProjectDir)lib" "$(TargetDir)lib" /E /H /C /I /Y
~~~

# dllをVB.Netで読み込む
以下のdllを参照に加える
具体的なコーディング方法については、[Module1.vb](VBdotNetTest/VBdotNetTest/Module1.vb)を参照のこと

# 参考文献
* IronPythonで利用可能な（ipy -m pip install でインストールできる）外部ライブラリ  
https://pypi.org/search/?q=ironpython&page=1

* Pythonコードから生成したdllを.Net系言語で読み込み処理する  
http://elicon.blog57.fc2.com/blog-entry-293.html  
http://elicon.blog57.fc2.com/blog-entry-230.html