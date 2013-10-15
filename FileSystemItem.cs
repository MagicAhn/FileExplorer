using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FileExplorer
{
    class FileSystemItem : Java.Lang.Object
    {
        public String Path { get; set; }//文件全路径 /sdcard/1.txt
        public String Name { get; set; }//1.txt
        public Boolean IsDir { get; set; }//是 文件 还是 文件夹
    }
}