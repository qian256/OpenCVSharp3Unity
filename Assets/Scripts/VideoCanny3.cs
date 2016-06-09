// Copyright (c) 2016, Long Qian
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, this list
//      of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this
//      list of conditions and the following disclaimer in the documentation and/or other
//      materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
// SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
// THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;

// Parallel computation support
using Uk.Org.Adcock.Parallel;
using System;
using System.Runtime.InteropServices;


// class for video display and processed video display
// current process is canny edge detection
public class VideoCanny3 : MonoBehaviour
{

    // Video parameters
    public MeshRenderer WebCamTextureRenderer;
    public MeshRenderer ProcessedTextureRenderer;
    public int deviceNumber;
    private WebCamTexture _webcamTexture;

    // Video size
    private const int imWidth = 1280;
    private const int imHeight = 720;
    private int imFrameRate;

    // OpenCVSharp parameters
    private Mat videoSourceImage;
    private Mat cannyImage;
    private Texture2D processedTexture;
    private Vec3b[] videoSourceImageData;
    private byte[] cannyImageData;

    // Frame rate parameter
    private int updateFrameCount = 0;
    private int textureCount = 0;
    private int displayCount = 0;

    void Start() {

        // create a list of webcam devices that is available
        WebCamDevice[] devices = WebCamTexture.devices;
        
        if (devices.Length > 0) { 

            // initialized the webcam texture by the specific device number
            _webcamTexture = new WebCamTexture(devices[deviceNumber].name, imWidth, imHeight);
            // assign webcam texture to the meshrenderer for display
            WebCamTextureRenderer.material.mainTexture = _webcamTexture;

            // Play the video source
            _webcamTexture.Play();

            // initialize video / image with given size
            videoSourceImage = new Mat(imHeight, imWidth, MatType.CV_8UC3);
            videoSourceImageData = new Vec3b[imHeight * imWidth];
            cannyImage = new Mat(imHeight, imWidth, MatType.CV_8UC1);
            cannyImageData = new byte[imHeight * imWidth];

            // create processed video texture as Texture2D object
            processedTexture = new Texture2D(imWidth, imHeight, TextureFormat.RGBA32, true, true);

            // assign the processedTexture to the meshrenderer for display
            ProcessedTextureRenderer.material.mainTexture = processedTexture;


        }

        // create opencv window to display the original video
        Cv2.NamedWindow("Copy video");
        

    }


    
    void Update() {

        updateFrameCount++;

        if (_webcamTexture.isPlaying) {

            if (_webcamTexture.didUpdateThisFrame) {

                textureCount++;

                // convert texture of original video to OpenCVSharp Mat object
                TextureToMat();
                // update the opencv window of source video
                UpdateWindow(videoSourceImage);
                // create the canny edge image out of source image
                ProcessImage(videoSourceImage);
                // convert the OpenCVSharp Mat of canny image to Texture2D
                // the texture will be displayed automatically
                MatToTexture();

            }

        }
        else {
            Debug.Log("Can't find camera!");
        }


        // output frame rate information
        if (updateFrameCount % 30 == 0) {
            Debug.Log("Frame count: " + updateFrameCount + ", Texture count: " + textureCount + ", Display count: " + displayCount);
        }


    }


    // Convert Unity Texture2D object to OpenCVSharp Mat object
    void TextureToMat() {
        // Color32 array : r, g, b, a
        Color32[] c = _webcamTexture.GetPixels32();

        // Parallel for loop
        // convert Color32 object to Vec3b object
        // Vec3b is the representation of pixel for Mat
        Parallel.For(0, imHeight, i => {
            for (var j = 0; j < imWidth; j++) {
                var col = c[j + i * imWidth];
                var vec3 = new Vec3b {
                    Item0 = col.b,
                    Item1 = col.g,
                    Item2 = col.r
                };
                // set pixel to an array
                videoSourceImageData[j + i * imWidth] = vec3;
            }
        });
        // assign the Vec3b array to Mat
        videoSourceImage.SetArray(0, 0, videoSourceImageData);
    }



    // Convert OpenCVSharp Mat object to Unity Texture2D object
    void MatToTexture() {
        // cannyImageData is byte array, because canny image is grayscale
        cannyImage.GetArray(0, 0, cannyImageData);
        // create Color32 array that can be assigned to Texture2D directly
        Color32[] c = new Color32[imHeight * imWidth];

        // parallel for loop
        Parallel.For(0, imHeight, i => {
            for (var j = 0; j < imWidth; j++) {
                byte vec = cannyImageData[j + i * imWidth];
                var color32 = new Color32 {
                    r = vec,
                    g = vec,
                    b = vec,
                    a = 0
                };
                c[j + i * imWidth] = color32;
            }
        });

        processedTexture.SetPixels32(c);
        // to update the texture, OpenGL manner
        processedTexture.Apply();
    }



    // Simple example of canny edge detect
    void ProcessImage(Mat _image) {
        Cv2.Flip(_image, _image, FlipMode.X);
        Cv2.Canny(_image, cannyImage, 100, 100);
    }


    // Display the original video in a opencv window
    void UpdateWindow(Mat _image) {
        Cv2.Flip(_image, _image, FlipMode.X);
        Cv2.ImShow("Copy video", _image);
        displayCount++;
    }
    
    // close the opencv window
    public void OnDestroy() {
        Cv2.DestroyAllWindows();

    }

    
}