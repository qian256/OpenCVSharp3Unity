OpenCVSharp 3 in Unity Example
===
Author: Long Qian

Date: 2016-06-07

## OpenCVSharp 3
Wrapper for OpenCV on .NET framework

See [OpenCVSharp3](https://github.com/shimat/opencvsharp)

OpenCVSharp version should be identical to OpenCV version.

## Unity3D Canny Edge Display
* Conversion between OpenCVSharp image format (_Mat_) to Unity3D image holder (*Texture2D*). The conversion is paralleled, inspired by another [example](https://sourceforge.net/projects/unityopencvsharpcamshift/)
* Currently supports windows x86, x64 standalone and Unity editor.

## Capture

![capture](capture.png "Capture")

## Fix
For my PC, several reference assemblies need to be copied to Plugins folder:

* PresentationCore.dll
* UIAutomationProvider.dll
* UIAutomationTypes.dll
* WindowsBase.dll
