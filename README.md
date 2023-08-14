## Update Agosto 2023

## Media Plugin for Avalonia

Simple cross platform plugin to take photos and video or pick them from a gallery from shared code.

**Platform Support**

|Platform|Version|
| ------------------- | :------------------: |
|Avalonia.iOS|iOS 7+|
|Avalonia.Android|API 14+|

### API Usage

Call **CrossMedia.Current** from any project or PCL to gain access to APIs.

Before taking photos or videos you should check to see if a camera exists and if photos and videos are supported on the device. There are five properties that you can check:

#### Android 

 **UseMediaPlugin(this)**

```csharp
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseMediaPlugin(this)
            .UseReactiveUI();
    }
```

#### iOS

 **UseMediaPlugin()**
 
```csharp
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseMediaPlugin()
            .UseReactiveUI();
    }
```
Your app is required to have keys in your Info.plist for `NSCameraUsageDescription` and `NSPhotoLibraryUsageDescription` in order to access the device's camera and photo/video library. If you are using the Video capabilities of the library then you must also add `NSMicrophoneUsageDescription`.  If you want to "SaveToGallery" then you must add the `NSPhotoLibraryAddUsageDescription` key into your info.plist. The string that you provide for each of these keys will be displayed to the user when they are prompted to provide permission to access these device features. You can read me here: [New iOS 10 Privacy Permission Settings](https://devblogs.microsoft.com/xamarin/new-ios-10-privacy-permission-settings?WT.mc_id=friends-0000-jamont)

Such as:
```xml
<key>NSCameraUsageDescription</key>
<string>This app needs access to the camera to take photos.</string>
<key>NSPhotoLibraryUsageDescription</key>
<string>This app needs access to photos.</string>
<key>NSMicrophoneUsageDescription</key>
<string>This app needs access to microphone.</string>
<key>NSPhotoLibraryAddUsageDescription</key>
<string>This app needs access to the photo gallery.</string>
```
