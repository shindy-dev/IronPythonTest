import csv
import contextlib
import enum
import datetime
import os
import re
import sqlite3

"""
ipyc logmanager.py
"""

class LogManager:
    """ログ管理クラス"""
    class __table_flag(enum.Enum):
        """テーブル管理クラス"""
        # logテーブル
        log=0
    
    # クエリチェック処理
    __query_check = lambda query, complied_pattern: complied_pattern.search(query.lower())
    # selectクエリチェック用コンパイル済パターン取得処理
    __query_check4select = lambda table_flag: re.compile(r"^( )*select( )+.*( )+from( )+{}(( )+.*|$)".format(table_flag.name).lower())
    # selectクエリチェック用コンパイル済パターン（logテーブル用）
    __query_check4select_log = __query_check4select(__table_flag.log)
    
    def __init__(self, dbpath=":memory:"):
        """
        [magic]コンストラクタ
        :param dbpath: 接続するDBパス:str (DBファイルが存在しなければ自動作成し、パスの指定がない場合はインメモリDB(切断した場合に自動削除されるDB)を作成する)
        """
        # DBパス
        self.dbpath=dbpath
        # DBファイル名
        self.dbname=os.path.splitext(os.path.basename(dbpath))[0]
        # DB接続
        self.__conn = sqlite3.connect(self.dbpath, 
                                      detect_types=sqlite3.PARSE_DECLTYPES|sqlite3.PARSE_COLNAMES) # 適合関数・変換関数を有効可
        # テーブル作成
        self.__create_table(LogManager.__table_flag.log)
    
    def __del__(self):
        """[magic]デストラクタ"""
        self.__destroy()

    def __enter__(self):
        """[magic]"""
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        """[magic]"""
        self.__destroy()

    def __destroy(self):
        """[private]インスタンス破棄処理"""
        self.__conn.close()

    def __create_table(self, table_flag):
        """[private]テーブル作成処理"""
        with self.__conn: # auto-commits
            with contextlib.closing(self.__conn.cursor()) as cur: # auto-closes
                if table_flag == LogManager.__table_flag.log:
                    cur.execute("CREATE TABLE IF NOT EXISTS log(id INTEGER PRIMARY KEY AUTOINCREMENT, msg TEXT, created datetime)")

    def __fetch_log(self, query:str):
        """
        [private]ログデータ取得
        :param query: ログ取得クエリ:str
        :return: ログデータ行:tuple
        """
        # logテーブルをselectするクエリかチェック
        assert LogManager.__query_check(query, LogManager.__query_check4select_log), "not found \"select ... from {}\" in query".format(self.__table_flag.log.name)

        with contextlib.closing(self.__conn.cursor()) as cur: # auto-closes
            cur.execute(query)
            return cur.fetchall()
        
    def write_log(self, msg):
        """
        ログ書き込み処理
        :param msg: ログメッセージ:str
        """
        with self.__conn: # auto-commits
            with contextlib.closing(self.__conn.cursor()) as cur: # auto-closes
                cur.execute("insert into log(msg, created) values (?,?)",(msg, datetime.datetime.now()))

    def export_csv(self, path:str, query:str="select * from log", encoding="shift_jis", append:bool=False):
        """
        ログデータcsv出力処理
        :param path: 出力パス:str
        :param query: ログ取得クエリ:str (default:log全件取得sql)
        :param encoding: エンコーディング:str (default:shift_jis)
        :param append: 追記モード:bool (default: False)
        """
        # データ取得
        rows = self.__fetch_log(query)

        # データ出力
        with open(path, "a" if append else "w", encoding=encoding, newline='') as f:
            csv.writer(f).writerows(rows)

if __name__ == "__main__":
    # trueの場合、メモリ上にDBを作成し、DB切断自にDBを削除する
    tempdb = True

    with LogManager("test.db") if not tempdb else LogManager() as lm:
        # DBへ適当に100件メッセージを書き込む
        [lm.write_log("message{}".format(n)) for n in range(0, 100)]
        # DBに書き込んだログを出力
        lm.export_csv(query="select * from log " ,path="log.csv")