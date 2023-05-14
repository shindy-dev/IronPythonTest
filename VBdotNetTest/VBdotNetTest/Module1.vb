Imports System.IO
Imports System.Reflection
Imports IronPython.Hosting
Imports Microsoft.Scripting.Hosting

Module Module1
    ''' <summary>
    ''' IronPythonで生成したdllを操作するクラス
    ''' </summary>
    MustInherit Class OperateIronPython
        Protected ReadOnly pyEngine As ScriptEngine
        Protected ReadOnly pyScope As ScriptScope

        Sub New(ByVal assemblyPath As String, ByVal moduleName As String)
            pyEngine = Python.CreateEngine()
            ' dllアセンブリのロード
            pyEngine.Runtime.LoadAssembly(Assembly.LoadFile(Path.GetFullPath(assemblyPath)))
            ' モジュール読み込み(pythonファイル名)
            pyScope = pyEngine.Runtime.ImportModule(moduleName)
        End Sub
        ''' <summary>
        '''  クラスのインスタンス化やグローバル関数の実行
        ''' </summary>
        ''' <param name="memberName">メンバ名</param>
        ''' <param name="paramaters">関数へ渡す引数</param>
        ''' <returns></returns>
        Protected Function Invoke(ByVal memberName As String, ParamArray ByVal paramaters As Object()) As Object
            Return pyEngine.Operations.Invoke(pyScope.GetVariable(memberName), paramaters)
        End Function
        ''' <summary>
        ''' メンバ関数の実行
        ''' </summary>
        ''' <param name="obj">オブジェクト</param>
        ''' <param name="memberName">メンバ名</param>
        ''' <param name="paramaters">関数へ渡す引数</param>
        ''' <returns></returns>
        Protected Function InvokeMember(ByVal obj As Object, ByVal memberName As String, ParamArray ByVal paramaters As Object()) As Object
            Return pyEngine.Operations.InvokeMember(obj, memberName, paramaters)
        End Function
        ''' <summary>
        ''' メンバ変数の取得
        ''' </summary>
        ''' <param name="obj">オブジェクト</param>
        ''' <param name="memberName">メンバ名</param>
        ''' <returns></returns>
        Protected Overloads Function GetMember(ByVal obj As Object, ByVal memberName As String) As Object
            Return pyEngine.Operations.GetMember(obj, memberName)
        End Function
        ''' <summary>
        ''' メンバ変数の取得
        ''' </summary>
        ''' <param name="obj">オブジェクト</param>
        ''' <param name="memberName">メンバ名</param>
        ''' <returns></returns>
        Protected Overloads Function GetMember(Of T)(ByVal obj As Object, ByVal memberName As String) As T
            Return pyEngine.Operations.GetMember(Of T)(obj, memberName)
        End Function
    End Class

    ''' <summary>
    ''' Pythonから生成したutils.lsモジュールのラッパークラス
    ''' </summary>
    Class lsWrapper
        Inherits OperateIronPython

        Sub New(ByVal assemblyPath As String)
            MyBase.New(assemblyPath, "ls")
        End Sub

        Public Function ls(Optional ByVal path As String = ".", Optional ByVal get_hidden As Boolean = False) As List(Of String)
            Return DirectCast(Invoke("ls", path, get_hidden), List(Of String))
        End Function
        Public Function ls_all(Optional ByVal path As String = ".") As List(Of String)
            Return DirectCast(Invoke("ls_all", path), List(Of String))
        End Function

        Public Function ls_dir(Optional ByVal path As String = ".", Optional ByVal get_hidden As Boolean = False) As List(Of String)
            Return DirectCast(Invoke("ls_dir", path, get_hidden), List(Of String))
        End Function
        Public Function ls_file(Optional ByVal path As String = ".", Optional ByVal get_hidden As Boolean = False) As List(Of String)
            Return DirectCast(Invoke("ls_file", path, get_hidden), List(Of String))
        End Function

        Public Shared Sub Sample(ByVal assemblyPath As String)
            Dim ls As New lsWrapper(assemblyPath)

            ' 指定したパス内のフォルダ、ファイルをすべて表示
            Console.WriteLine("***********ls_all***********")
            Print(ls.ls_all())

            ' 指定したパス内のフォルダ、ファイルを表示
            ' 引数[get_hidden: bool(=False)] をTrueにすることで隠しファイル/フォルダを取得
            Console.WriteLine("***********ls***********")
            Print(ls.ls())

            ' 指定したパス内のフォルダを表示
            ' 引数[get_hidden: bool(=False)] をTrueにすることで隠しフォルダを取得
            Console.WriteLine("***********ls_dir***********")
            Print(ls.ls_dir())

            ' 指定したパス内のファイルを表示
            ' 引数[get_hidden: bool(=False)] をTrueにすることで隠しファイルを取得
            Console.WriteLine("***********ls_file***********")
            Print(ls.ls_file())
        End Sub
        Private Shared Sub Print(Of T)(ByVal rows As List(Of T))
            For Each s As T In rows
                Console.WriteLine(s.ToString())
            Next
        End Sub

    End Class

    ''' <summary>
    ''' Pythonから生成したlogmanagerモジュールのラッパークラス
    ''' </summary>
    Class LogManagerWrapper
        Inherits OperateIronPython

        ''' <summary>
        ''' LogManagerオブジェクト
        ''' </summary>
        Private ReadOnly objLogManager As Object

        Public ReadOnly Property DBName As String
            Get
                Return GetMember(Of String)(objLogManager, "dbname")
            End Get
        End Property

        Sub New(ByVal assemblyPath As String, Optional ByVal dbPath As String = Nothing)
            MyBase.New(assemblyPath, "logmanager")

            ' LogManagerのコンストラクタ引数
            Dim paramater As Object() = If(String.IsNullOrEmpty(dbPath), New Object() {}, New Object() {dbPath})

            ' LogManagerのインスタンス化
            objLogManager = Invoke("LogManager", paramater)
        End Sub

        ''' <summary>
        ''' ログ書き込み処理
        ''' </summary>
        ''' <param name="msg">ログメッセージ</param>
        Public Sub write_log(ByVal msg As String)
            InvokeMember(objLogManager, "write_log", msg)
        End Sub

        ''' <summary>
        ''' ログデータcsv出力処理
        ''' </summary>
        ''' <param name="path">出力パス</param>
        ''' <param name="query">ログ取得クエリ(default:log全件取得sql)</param>
        ''' <param name="encoding">エンコーディング (default:shift_jis)</param>
        ''' <param name="append">追記モード(default: False)</param>
        Public Sub export_csv(ByVal path As String, Optional ByVal query As String = "select * from log", Optional ByVal encoding As String = "shift_jis", Optional ByVal append As Boolean = False)
            InvokeMember(objLogManager, "export_csv", path, query, encoding, append)
        End Sub

        Public Shared Sub Sample(ByVal assemblyPath As String, workdir As String)
            ' trueの場合、メモリ上にDBを作成し、DB切断自にDBを削除する
            Dim tempdb As Boolean = True
            Dim logManager As LogManagerWrapper = If(Not tempdb, New LogManagerWrapper(assemblyPath, Path.Combine(workdir, "sample_log.db")), New LogManagerWrapper(assemblyPath))

            ' DBへ適当に100件メッセージを書き込む
            For i As Integer = 0 To 99
                logManager.write_log($"message{i}")
            Next

            ' DBに書き込んだログを出力
            logManager.export_csv(Path.Combine(workdir, "sample_log.csv"))

            Console.WriteLine(logManager.DBName)

        End Sub
    End Class

    Sub Main()
        ' dll化したutils.ls.pyをvb.netから操作
        lsWrapper.Sample("..\..\..\..\sample\utils.dll")

        ' dll化したlogmanager.pyをvb.netから操作
        LogManagerWrapper.Sample("..\..\..\..\sample\LogManager.dll", "..\..\..\..\sample")
    End Sub

End Module
