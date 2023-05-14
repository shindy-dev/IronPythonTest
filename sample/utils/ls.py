"""
author: shindy-dev
created: 2020/10/03
github: https://github.com/shindy-dev
"""

__all__ = (
    "ls",
    "ls_all",
    "ls_dir",
    "ls_file",
    "ls_tree",
)
import os

import clr
clr.AddReference("System.Collections")
from System.Collections.Generic import List, Dictionary
from System import String, Object

def ls(path: str = ".", get_hidden: bool = False):
    """
    Get files and folder list from path.\n
    get_hidden: If you want to get hidden files and folders, select True.
    """
    return List[String](
        ls_all(path)
        if get_hidden
        else [f for f in os.listdir(path) if not f.startswith(".")]
    )


def ls_all(path: str = "."):
    """
    Get all file and folder list from path.
    """
    return List[String]([f for f in os.listdir(path)])


def ls_dir(path: str = ".", get_hidden: bool = False):
    """
    Get folder list from path.\n
    get_hidden: If you want to get hidden folders, select True.
    """
    return List[String]([f for f in ls(path, get_hidden) if os.path.isdir(os.path.join(path, f))])


def ls_file(path: str = ".", get_hidden: bool = False):
    """
    Get file list from path.\n
    get_hidden: If you want to get hidden files, select True.
    """
    return List[String]([f for f in ls(path, get_hidden) if os.path.isfile(os.path.join(path, f))])


def ls_tree(path: str = ".", get_hidden: bool = False):
    """
    Get Directory Tree from path.\n
    get_hidden: If you want to get hidden files and folders, select True.
    """
    return {
        f: (
            ls_tree(os.path.join(path, f))
            if os.path.isdir(os.path.join(path, f))
            else None
        )
        for f in ls(path, get_hidden)
    }


if __name__ == "__main__":
    import ls

    # 指定したパス内のフォルダ、ファイルをすべて表示
    print(ls.ls_all())

    # 指定したパス内のフォルダ、ファイルを表示
    # 引数[get_hidden: bool(=False)] をTrueにすることで隠しファイル/フォルダを取得
    print(ls.ls())

    # 指定したパス内のフォルダを表示
    # 引数[get_hidden: bool(=False)] をTrueにすることで隠しフォルダを取得
    print(ls.ls_dir())

    # 指定したパス内のファイルを表示
    # 引数[get_hidden: bool(=False)] をTrueにすることで隠しファイルを取得
    print(ls.ls_file())

    # 指定したパス内のディレクトリツリーを表示
    # 引数[get_hidden: bool(=False)] をTrueにすることで隠しファイル/フォルダを取得
    print(ls.ls_tree(get_hidden=True))
