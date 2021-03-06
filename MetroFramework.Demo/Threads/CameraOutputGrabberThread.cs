﻿using System;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Diagnostics;
using Emgu.CV.UI;
using Nkujukira.Demo.Singletons;
using Nkujukira;
using System.Threading;
using System.Drawing;
using Nkujukira.Demo.Entitities;

namespace Nkujukira.Demo.Threads
{
    public class CameraOutputGrabberThread : AbstractThread
    {
        //HANDLE TO THE WEB CAM
        private Capture camera_capture;

        //THE FRAME CURRENTLY BEING WORKED ON
        private Image<Bgr, byte> current_frame;

        //SIGNALS TO OTHER THREADS THAT THIS THREAD HAS FINIHSED WORK
        public static bool WORK_DONE = false;

       
        //CONSTRUCTOR
        public CameraOutputGrabberThread(Camera camera)
            : base()
        {
            Debug.WriteLine("Cam output thread starting");
            if (camera != null)
            {
                this.camera_capture = camera.camera_capture;
            }
            WORK_DONE      = false;

        }


        //WHILE RUNNING THIS THREAD WILL GET THE NEXT FRAME FROM THE CAMERA
        //IT WILL THEN ADD IT TO CONCURRENT QUEUES FOR EASY ACCESS BY OTHER THREADS
        public override void DoWork(object sender, System.ComponentModel.DoWorkEventArgs ex)
        {
            try
            {
                Debug.WriteLine("Cam output thread running");

                while (running)
                {
                    if (!paused)
                    {
                        
                        AddNextFrameToQueuesForProcessing();
                        Thread.Sleep(100);
                    }
                }

                //THREAD IS TERMINATED
                CleanUp();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void CleanUp()
        {
           
            camera_capture = null;
            current_frame  = null;
        }

        //ADDS A CAPTURED FRAME TO THREAD SAFE QUEUES 
        //FOR EASY ACESS WHEN THE FRAME IS PROCESSED BY MULTIPLE THREADS LATER
        public bool AddNextFrameToQueuesForProcessing()
        {
            //get next frame from camera
            current_frame            = FramesManager.GetNextFrame(camera_capture);

            if (current_frame != null)
            {
                int new_width        = Singleton.MAIN_WINDOW.GetControl(MainWindow.MainWindowControls.live_stream_image_box1).Width;
                int new_height       = Singleton.MAIN_WINDOW.GetControl(MainWindow.MainWindowControls.live_stream_image_box1).Height;
                Size new_size        = new Size(new_width,new_height);

                //add frame to queue for display
                Singleton.LIVE_FRAMES_TO_BE_DISPLAYED.Enqueue(FramesManager.ResizeColoredImage(current_frame.Clone(), new_size));

                //add frame to queue for storage
                Singleton.FRAMES_TO_BE_STORED.Enqueue(current_frame.Clone());

                //resize frame to save on memory and improve performance
                int width            = Singleton.MAIN_WINDOW.GetControl(MainWindow.MainWindowControls.review_image_box).Width;
                int height           = Singleton.MAIN_WINDOW.GetControl(MainWindow.MainWindowControls.review_image_box).Height;

                Size size            = new Size(width,height);

                current_frame        = FramesManager.ResizeColoredImage(current_frame,size);

                //add frame to queue for face detection and recognition
                Singleton.LIVE_FRAMES_TO_BE_PROCESSED.Enqueue(current_frame.Clone());

                //return
                return true;
            }

            //FRAME IS NULL 
            //MEANING END OF FILE IS REACHED
            else
            {
                //ADD BLACK FRAME TO DATASTORE AND TERMINATE THREAD
                //ALSO SIGNAL TO OTHERS THAT THIS THREAD IS DONE
                WORK_DONE = true;
                running   = false;

                Debug.WriteLine("Terminating camera output");
                return false;
            }
            

        }
    }
}
