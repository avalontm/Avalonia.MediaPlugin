using Avalonia.MediaPlugin.Abstractions;
using System.Collections.ObjectModel;
using System;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.MediaPlugin;

namespace MediaTest.ViewModels;

public class MainViewModel : ViewModelBase
{
    ObservableCollection<MediaFile> files = new ObservableCollection<MediaFile>();
   
    public MainViewModel()
    {
        files.CollectionChanged += Files_CollectionChanged;
    }

    public async void onTakePhoto()
    {
        await CrossMedia.Current.Initialize();
        files.Clear();
        if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
        {
           // await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
            return;
        }

        var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
        {
            PhotoSize = PhotoSize.Medium,
            Directory = "Sample",
            Name = "test.jpg"
        });

        if (file == null)
            return;

       // await DisplayAlert("File Location", file.Path, "OK");

        files.Add(file);
    }

    public async void onPickPhoto()
    {
        await CrossMedia.Current.Initialize();
        files.Clear();
        if (!CrossMedia.Current.IsPickPhotoSupported)
        {
            //await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
            return;
        }
        var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
        {
            PhotoSize = PhotoSize.Full,
            SaveMetaData = true
        });


        if (file == null)
            return;

        files.Add(file);
    }

    public async void onPickPhotos()
    {
        await CrossMedia.Current.Initialize();
        files.Clear();
        if (!CrossMedia.Current.IsPickPhotoSupported)
        {
           // await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
            return;
        }
        var picked = await CrossMedia.Current.PickPhotosAsync();


        if (picked == null)
            return;
        foreach (var file in picked)
            files.Add(file);
    }


    public async void onTakeVideo()
    {
        await CrossMedia.Current.Initialize();
        files.Clear();
        if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakeVideoSupported)
        {
            //await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
            return;
        }

        var file = await CrossMedia.Current.TakeVideoAsync(new StoreVideoOptions
        {
            Name = "video.mp4",
            Directory = "DefaultVideos"
        });

        if (file == null)
            return;

        //await DisplayAlert("Video Recorded", "Location: " + file.Path, "OK");

        file.Dispose();
    }

    public async void onPickVideos()
    {
        await CrossMedia.Current.Initialize();
        files.Clear();
        if (!CrossMedia.Current.IsPickVideoSupported)
        {
            //await DisplayAlert("Videos Not Supported", ":( Permission not granted to videos.", "OK");
            return;
        }
        var file = await CrossMedia.Current.PickVideoAsync();

        if (file == null)
            return;

       // await DisplayAlert("Video Selected", "Location: " + file.Path, "OK");
        file.Dispose();
    }

    private void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (files.Count == 0)
        {
            //ImageList.Children.Clear();
            return;
        }
        if (e.NewItems.Count == 0)
            return;

        var file = e.NewItems[0] as MediaFile;
        var image = new Image { Width = 300, Height = 300, Stretch = Avalonia.Media.Stretch.Uniform };
        image.Source = new Bitmap(file.Path);
        /*image.Source = ImageSource.FromStream(() =>
        {
            var stream = file.GetStream();
            return stream;
        });*/
      //  ImageList.Children.Add(image);

    }

}

