using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database.Sqlite;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FileExplorer
{
    class DbHelper : SQLiteOpenHelper
    {
        private Context _context;

        public DbHelper(Context context)
            : base(context, "FileExplorer_v1", null, 1)
        {
            _context = context;
        }

        public override void OnCreate(SQLiteDatabase db)
        {
            db.ExecSQL("CREATE Table If NOT EXISTS T_Favorite(Id integer primary key autoincrement,Path text,IsDir integer)");
        }

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {

        }

        public void AddToFavorite(String path, Boolean isDir)
        {
            //new Java.Lang.Object  运算符重载，隐式类型推断
            var cursor = WritableDatabase.RawQuery("select * from T_Favorite where Path = ? and IsDir = ?", new String[] { path, isDir ? "1" : "0" });
            if (cursor.Count <= 0)
            {
                //插入数据库
                WritableDatabase.ExecSQL("insert into T_Favorite(Path,IsDir) values(?,?)", new Java.Lang.Object[]
                {
                    path,isDir
                });
                Toast.MakeText(_context, Resource.String.AddToFavoriteSuccess, ToastLength.Short).Show();
            }
            else
            {
                //提示已存在
                Toast.MakeText(_context, Resource.String.AddToFavoriteExist, ToastLength.Short).Show();
            }
        }

        //获得所有收藏夹  尽量不要用List<FileSystemItem> 虽然有Add，Clear，Remove等方法，但是由于大小可以确定，就尽量使用 简单接口
        public FileSystemItem[] GetItems()
        {
            using (var cursor = WritableDatabase.RawQuery("select * from T_Favorite", null))
            {
                FileSystemItem[] items = new FileSystemItem[cursor.Count];
                Int32 flag = 0;//标记位

                while (cursor.MoveToNext())
                {
                    FileSystemItem item = new FileSystemItem();
                    String path = cursor.GetString(cursor.GetColumnIndex("Path"));
                    item.Path = path;
                    //数据库中 0为false， 1为true
                    item.IsDir = (cursor.GetInt(cursor.GetColumnIndex("IsDir")) == 1);
                    item.Name = System.IO.Path.GetFileName(path);

                    items[flag++] = item;
                }
                return items;
            }
        }
    }
}