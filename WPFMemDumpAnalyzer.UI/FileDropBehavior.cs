using System;
using System.IO;
using System.Windows;
using System.Windows.Interactivity;

namespace WPFMemDumpAnalyzer.UI
{
    public class FileDropBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty DroppedFileProperty = 
            DependencyProperty.Register("DroppedFile", typeof (string), typeof(FileDropBehavior));

        public string DroppedFile
        {
            get { return (string)GetValue(DroppedFileProperty); }
            set { SetValue(DroppedFileProperty, value); }
        }

        public static readonly DependencyProperty FileExtensionProperty =
            DependencyProperty.Register("FileExtension", typeof (string), typeof (FileDropBehavior), new PropertyMetadata(String.Empty));

        public string FileExtension
        {
            get { return (string) GetValue(FileExtensionProperty); }
            set { SetValue(FileExtensionProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AllowDrop = true;
            AssociatedObject.PreviewDragEnter += OnAssociatedObjectPreviewDragEnter;
            AssociatedObject.PreviewDragOver += OnAssociatedObjectPreviewDragOver;
            AssociatedObject.PreviewDrop += OnAssociatedObjectPreviewDrop;
        }

        private void OnAssociatedObjectPreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = HasMatchingFile(e) ? DragDropEffects.All : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnAssociatedObjectPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = HasMatchingFile(e) ? DragDropEffects.All : DragDropEffects.None;
            e.Handled = true;
        }

        private bool HasMatchingFile(DragEventArgs args)
        {
            if (!args.Data.GetDataPresent(DataFormats.FileDrop, true))
                return false;

            var filenames = (string[])args.Data.GetData(DataFormats.FileDrop, true);
            string firstFile = filenames[0];
            if (!File.Exists(firstFile))
                return false;

            var info = new FileInfo(firstFile);
            return info.Extension.ToLower() == FileExtension.ToLower();
        }

        private void OnAssociatedObjectPreviewDrop(object sender, DragEventArgs e)
        {
            var filenames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            DroppedFile = filenames[0];
            e.Handled = true; 
        }
    }
}