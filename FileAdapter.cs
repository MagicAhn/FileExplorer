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
    class FileAdapter : ArrayAdapter<FileSystemItem>
    {
        private ListView listView;

        public FileAdapter(Context context, ListView listView)
            : base(context, 0)
        {
            this.listView = listView;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view;
            if (convertView == null)
            {
                view = LayoutInflater.From(Context).Inflate(Resource.Layout.layoutFileSystem, null);

                var chkSelected = view.FindViewById<CheckBox>(Resource.Id.chkSelected);
                chkSelected.CheckedChange += ChkSelectedOnCheckedChange;
            }
            else
            {
                view = convertView;
            }
            // view = convertView ?? LayoutInflater.From(Context).Inflate(Resource.Layout.layoutFileSystem, null);

            FileSystemItem item = GetItem(position);

            var imgIcon = view.FindViewById<ImageView>(Resource.Id.imgIcon);
            var txtName = view.FindViewById<TextView>(Resource.Id.txtName);

            if (item.IsDir)
            {
                imgIcon.SetImageResource(Resource.Drawable.folder);
            }
            else
            {
                imgIcon.SetImageDrawable(null);
            }
            txtName.Text = item.Name;

            var cb = view.FindViewById<CheckBox>(Resource.Id.chkSelected);
            cb.Tag = position;
            //View 复用的问题
            cb.Checked = listView.IsItemChecked(position);
            //这样会造成 内存泄露，应该只注册一次
            //cb.CheckedChange += ChkSelectedOnCheckedChange;

            return view;
        }

        private void ChkSelectedOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs checkedChangeEventArgs)
        {

        }
    }
}