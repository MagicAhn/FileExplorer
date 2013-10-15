using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using Java.IO;

namespace FileExplorer
{
    [Activity(Label = "FileExplorer", MainLauncher = true, Icon = "@drawable/icon")]
    public class ActivityMain : Activity
    {
        private EditText _editDir;
        private ListView _listViewFile;
        private DateTime lastBackPressTime;

        private const Int32 MKDIR = 0;
        private const Int32 REFRESH = 1;
        private const Int32 BATCHREMOVE = 2;
        private const Int32 REMOVE = 3;
        private const Int32 RENAME = 4;
        private const Int32 FAVORITE = 5;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _editDir = FindViewById<EditText>(Resource.Id.editDir);
            _listViewFile = FindViewById<ListView>(Resource.Id.listViewFile);
            _listViewFile.ChoiceMode = ChoiceMode.Multiple;

            RegisterForContextMenu(_listViewFile);

            //判断 sdcard  是否存在,并设置根目录
            #region 重构前的写法
            //String root;
            //if (Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted)
            //{
            //    //获取 sdcard 的路径
            //    root = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            //}
            //else
            //{
            //    root = "/";
            //} 
            #endregion

            String root = Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted
                ? Android.OS.Environment.ExternalStorageDirectory.AbsolutePath
                : "/";

            LoadDir(root);


            #region ListView 每一项的 ItemClick
            //ListView 每一项的 ItemClick
            _listViewFile.ItemClick += (sender, args) =>
                {
                    var fileAdapter = (FileAdapter)_listViewFile.Adapter;
                    var fileSystemItem = fileAdapter.GetItem(args.Position);
                    if (fileSystemItem.IsDir)
                    {
                        var file = new Java.IO.File(fileSystemItem.Path);
                        //判断是否有权限
                        if (!file.CanRead())
                        {
                            Toast.MakeText(this, Resource.String.DirCanNotRead, ToastLength.Short).Show();
                            return; //有 return 就不需要 else 了
                        }
                        //加载点击的文件夹
                        LoadDir(fileSystemItem.Path);
                    }
                    else
                    {
                        // 获取文件的 mimeType
                        //String mimeType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(
                        //    MimeTypeMap.GetFileExtensionFromUrl(fileSystemItem.Path));
                        String mimeType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(MimeTypeMap.GetFileExtensionFromUrl(GetExtension(fileSystemItem.Path)));
                        // 如果匹配不到，则返回 null
                        if (mimeType == null)
                        {
                            mimeType = "*/*";
                        }
                        var intent = new Intent();
                        //intent.SetAction(Intent.ActionView);
                        //intent.SetDataAndType(Android.Net.Uri.Parse("file://" + fileSystemItem.Path), mimeType);

                        // 不能单独设置 SetData和SetType，否则会报错 ActivityNotFoundException
                        // 因为 SetData 是设置Data把Type清零，SetType 是设置Type把Data清零，SetDataAndType才是同时设置
                        intent.SetAction(Intent.ActionView)
                            .SetDataAndType(Android.Net.Uri.Parse("file://" + fileSystemItem.Path), mimeType);
                        try
                        {
                            StartActivity(intent);
                        }
                        catch (ActivityNotFoundException e)
                        {
                            Toast.MakeText(this, Resource.String.ActivityNotFound, ToastLength.Short).Show();
                        }
                    }
                };
            #endregion
        }

        private void LoadDir(string dirToLoad)
        {
            var adapter = new FileAdapter(this, _listViewFile);

            foreach (var dir in Directory.GetDirectories(dirToLoad))
            {
                //Path.GetFileName(dir) 返回指定路径字符串的文件名和扩展名(没有全路径)。
                var item = new FileSystemItem { Path = dir, Name = Path.GetFileName(dir), IsDir = true };
                adapter.Add(item);
            }
            foreach (var filename in Directory.GetFiles(dirToLoad))
            {
                var item = new FileSystemItem() { Path = filename, Name = Path.GetFileName(filename), IsDir = false };
                adapter.Add(item);
            }
            _listViewFile.Adapter = adapter;
            _editDir.Text = dirToLoad;
        }

        #region + OnBackPressed override后退键按钮的触发后调用的方法
        public override void OnBackPressed()
        {
            // 返回上一级
            String currentDir = _editDir.Text;

            // 当前不是 根目录
            if (!Directory.GetDirectoryRoot(currentDir).Equals(currentDir))
            {
                LoadDir(Directory.GetParent(currentDir).FullName);
            }
            else
            {
                // 如果上次点击后退键的时间 距离现在 小于2秒，则退出 Activity
                // 否则 提示用户：再点击一次后退键 退出程序
                if (DateTime.Now - lastBackPressTime > TimeSpan.FromSeconds(2))
                {
                    Toast.MakeText(this, Resource.String.BackPressAgainToExit, ToastLength.Short).Show();
                    lastBackPressTime = DateTime.Now;
                    return;
                }
                Finish();
            }
        }
        #endregion

        #region + OnCreateOptionsMenu 创建菜单项
        public override Boolean OnCreateOptionsMenu(IMenu menu)
        {
            menu.Add(0, MKDIR, 0, Resource.String.MKDIR);
            menu.Add(0, REFRESH, 0, Resource.String.REFRESH);
            menu.Add(0, BATCHREMOVE, 0, Resource.String.BATCHREMOVE);

            return true;
        }
        #endregion

        #region + OnOptionsItemSelected 菜单项选择事件
        public override Boolean OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == MKDIR)
            {
                var mkrBuilder = new AlertDialog.Builder(this);
                var editText = new EditText(this);
                //var edittext = new EditText(this) {Id = 1};
                mkrBuilder.SetTitle(Resource.String.MKDIR)
                    .SetView(editText)
                    .SetPositiveButton(Android.Resource.String.Ok,
                        (sender, args) =>
                        {
                            ////尽量避免声明 太多的成员变量
                            //AlertDialog alertDialog = (AlertDialog)sender;
                            //var edittext = alertDialog.FindViewById(1);//todo
                            String folder = editText.Text;
                            //尽量 Path.Combine 拼接路径
                            Directory.CreateDirectory(Path.Combine(_editDir.Text, folder));

                            //刷新一下
                            LoadDir(_editDir.Text);
                        }).SetNegativeButton(Android.Resource.String.Cancel, (IDialogInterfaceOnClickListener)null).Show();
            }
            else if (item.ItemId == REFRESH)
            {
                LoadDir(_editDir.Text);
            }
            else if (item.ItemId == BATCHREMOVE)
            {
                RemoveItem();
            }

            return true;
        }

        private void RemoveItem()
        {
            var batchRemoveBuilder = new AlertDialog.Builder(this);
            var adapter = (FileAdapter)_listViewFile.Adapter;

            var positions = _listViewFile.CheckedItemPositions;
            //避免不必要的 Bug
            if (positions.Size() <= 0)
            {
                return;
            }
            batchRemoveBuilder.SetTitle(Resource.String.Warn)
                .SetMessage(Resource.String.ConfirmBatchRemove)
                .SetPositiveButton(Android.Resource.String.Ok,
                    (sender, args) =>
                    {
                        for (int i = 0; i < positions.Size(); i++)
                        {
                            // 得到 一项的具体位置
                            //Int32 position = positions.KeyAt(i);
                            var fileSystemItem = adapter.GetItem(positions.KeyAt(i));
                            if (fileSystemItem.IsDir)
                            {
                                Directory.Delete(fileSystemItem.Path);
                            }
                            else
                            {
                                System.IO.File.Delete(fileSystemItem.Path);
                            }
                        }
                        LoadDir(_editDir.Text);
                    }).SetNegativeButton(Android.Resource.String.Cancel, (IDialogInterfaceOnClickListener)null).Show();
        }

        #endregion

        #region + OnCreateContextMenu 创建上下文菜单
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            menu.Add(0, REMOVE, 0, Resource.String.Remove);
            menu.Add(0, RENAME, 0, Resource.String.Rename);
            menu.Add(0, FAVORITE, 0, Resource.String.Favorite);
        }
        #endregion

        public override Boolean OnContextItemSelected(IMenuItem item)
        {
            if (item.ItemId == REMOVE)
            {
                RemoveItem();
            }
            else if (item.ItemId == RENAME)
            {
                // 获得 具体弹出上下文菜单的 某一项
                var menuinfo = (Android.Widget.AdapterView.AdapterContextMenuInfo)item.MenuInfo;
                FileAdapter adapter = (FileAdapter)_listViewFile.Adapter;

                FileSystemItem fileitem = adapter.GetItem(menuinfo.Position);



                var renameBuilder = new AlertDialog.Builder(this);
                //var editNameText = new EditText(this) { Id = 1 };
                var editNameText = new EditText(this);
                editNameText.Text = fileitem.Name;
                ////  把当前上下文菜单所指向的FileSystemItem设置到Tag上，在 点击事件的毁掉方法中 再取，可以避免使用成员变量
                //editNameText.Tag = fileitem;
                renameBuilder.SetTitle(Resource.String.Rename)
                    .SetView(editNameText)
                    .SetPositiveButton(Android.Resource.String.Ok, (sender, args) =>
                    {
                        #region 一种写法: 传统的写法：通过设置 Tag
                        //var renameDialog = (AlertDialog)sender;
                        //var editText = renameDialog.FindViewById<EditText>(1);
                        //var fileItem = editText.Tag;

                        //String oldPath = fileitem.Path;
                        //String newName = editText.Text;
                        //String newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName); 
                        #endregion

                        String oldPath = fileitem.Path;
                        String newName = editNameText.Text;
                        String newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);

                        if (fileitem.IsDir)
                        {
                            Directory.Move(oldPath, newPath);
                        }
                        else
                        {
                            System.IO.File.Move(oldPath, newPath);
                        }

                        LoadDir(_editDir.Text);
                    }).SetNegativeButton(Android.Resource.String.Cancel, (IDialogInterfaceOnClickListener)null)
                    .Show();
            }
            return true;
        }

        #region 去除 路径中的空格
        //private String TrimSpace(String str)
        //{
        //    Char[] temp = new Char[str.Length];
        //    Int32 flag = 0;
        //    for (var i = 0; i < str.Length; i++)
        //    {
        //        if (!str[i].Equals('ͼ'))
        //        {
        //            temp[flag++] = str[i];
        //        }
        //        else
        //        {
        //            i++;
        //        }
        //    }
        //    String strtemp = new String(temp);
        //    return strtemp.Trim();
        //} 
        #endregion

        #region 去除 路径中的空格
        private String GetExtension(String str)
        {
            String[] temp = str.Split(new string[] { "ͼƬ" }, StringSplitOptions.RemoveEmptyEntries);
            return temp[temp.Length - 1];
        }
        #endregion
    }
}

