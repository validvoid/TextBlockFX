![TextBlockFX_Logo_Large](https://user-images.githubusercontent.com/8193074/147546728-6149baa5-a4b1-4c4a-bcc3-01946f741e5b.png)

# TextBlockFX

A TextBlock control which animates the text with customizable effects.

TextBlockFx generates difference results for attached effect to animate the text when its content changes by using its built-in diffing algorithm.


https://user-images.githubusercontent.com/8193074/147348037-efe70068-d188-4a26-a23a-c94e2b03ede9.mp4


## Get Started

### Install

![](https://img.shields.io/nuget/v/TextBlockFX.Win2D.UWP?style=flat-square)

In Solution Explorer panel, right click on your project name and select Manage NuGet Packages. Search for `TextBlockFX.Win2D.UWP` then click **install** to install the package.

Or enter the following command in Package Manager Console to install it:

```powershell
Install-Package TextBlockFX.Win2D.UWP -Version 1.0.3
```

### Usage

In your XAML page, add a reference at the top of your page:

```xaml
xmlns:tbfx="using:TextBlockFX.Win2D.UWP"
xmlns:effects="using:TextBlockFX.Win2D.UWP.Effects"
```

Then add TextBlockFX to your page:

```xaml
<tbfx:TextBlockFX Text="Your text here">
    <tbfx:TextBlockFX.TextEffect>
        <effects:Default/>
    </tbfx:TextBlockFX.TextEffect>
</tbfx:TextBlockFX>
```

### Built-in Effects

* Default
* Motion Blur
* Blur

### Supported Features

| Feature              	| UWP(Win2D) 	| WinUI3(Win2D) 	|
|----------------------	|------------	|---------------	|
|     FontFamily       	| ✅          	| 🚧             	|
|     FontSize         	| ✅          	| 🚧             	|
|     FontStretch      	| ✅          	| 🚧             	|
|     FontStyle        	| ✅          	| 🚧             	|
|     FontWeight       	| ✅          	| 🚧             	|
|     TextAlignment    	| ✅          	| 🚧             	|
|     TextDirection    	| ✅          	| 🚧             	|
|     TextTrimming     	| ✅          	| 🚧             	|
|     TextWrapping     	| ✅          	| 🚧             	|

✅: Supported

🚧: WIP

TextBlockFX only support UWP projects for now. WinUI3 support and Skiasharp based implementation for other platform targets are WIP.

### Write Your Own Effect

* Define a new effect class and implement the `ITextEffect` interface.
* Draw the text by using Win2D API.

