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
            //new Java.Lang.Object  ��������أ���ʽ�����ƶ�
            var cursor = WritableDatabase.RawQuery("select * from T_Favorite where Path = ? and IsDir = ?", new String[] { path, isDir ? "1" : "0" });
            if (cursor.Count <= 0)
            {
                //�������ݿ�
                WritableDatabase.ExecSQL("insert into T_Favorite(Path,IsDir) values(?,?)", new Java.Lang.Object[]
                {
                    path,isDir
                });
                Toast.MakeText(_context, Resource.String.AddToFavoriteSuccess, ToastLength.Short).Show();
            }
            else
            {
                //��ʾ�Ѵ���
                Toast.MakeText(_context, Resource.String.AddToFavoriteExist, ToastLength.Short).Show();
            }
        }

        //��������ղؼ�  ������Ҫ��List<FileSystemItem> ��Ȼ��Add��Clear��Remove�ȷ������������ڴ�С����ȷ�����;���ʹ�� �򵥽ӿ�
        public FileSystemItem[] GetItems()
        {
            using (var cursor = WritableDatabase.RawQuery("select * from T_Favorite", null))
            {
                FileSystemItem[] items = new FileSystemItem[cursor.Count];
                Int32 flag = 0;//���λ

                while (cursor.MoveToNext())
                {
                    FileSystemItem item = new FileSystemItem();
                    String path = cursor.GetString(cursor.GetColumnIndex("Path"));
                    item.Path = path;
                    //���ݿ��� 0Ϊfalse�� 1Ϊtrue
                    item.IsDir = (cursor.GetInt(cursor.GetColumnIndex("IsDir")) == 1);
                    item.Name = System.IO.Path.GetFileName(path);

                    items[flag++] = item;
                }
                return items;
            }
        }
    }
}