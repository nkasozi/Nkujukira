﻿using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using MetroFramework.Forms;
using System.Collections.Concurrent;
using Emgu.CV;
using Emgu.CV.Structure;
using Nkujukira.Entities;
using Nkujukira.Threads;
using System.Threading;
using System.Diagnostics;
using Emgu.CV.UI;
using MB.Controls;
using Nkujukira;

namespace MetroFramework.Demo
{
    public partial class MainWindow : MetroForm
    {
        private const string SELECT_VIDEO_MESSAGE = "Please Select a Video file";
        private const string LOAD_CAMERA_FOOTAGE_MESSAGE = "You Are Loading Footage From Your camera!!";
        private const string FILE_FILTER = "All files (*.*)|*.*";
        private const string CLEAN_UP_THREAD_NAME = "Cleaner";
        private const string DISPLAY_UPDATER_THREAD_NAME = "VIDEO_UPDATER";
        private const string VIDEO_FILE_GRABBER_THREAD_NAME = "FILE GRABBER THREAD";
        private const string FACE_DETECTOR_THREAD_NAME = "FACE_DETECTOR";
        private const string MESSAGE_BOX_TITLE = "Message!!";

        private const int IMAGE_BOX_HEIGHT = 315;
        private const int IMAGE_BOX_WIDTH = 390;

        public static ConcurrentQueue<Image<Bgr, byte>> FRAMES_TO_BE_PROCESSED = new ConcurrentQueue<Image<Bgr, byte>>();
        public static ConcurrentQueue<Image<Bgr, byte>> FRAMES_TO_BE_DISPLAYED = new ConcurrentQueue<Image<Bgr, byte>>();
        public static ConcurrentDictionary<int, Face> DETECTED_FACES_DATASTORE = new ConcurrentDictionary<int, Face>();

        CleanUpThread clean_upper;
        CameraOutputGrabberThread cam_output;
        VideoFromFileThread video_from_file_grabber;
        FaceDetectingThread face_detector;
        DisplayUpdaterThread video_updater;

        Thread camera_output_grabber_thread;
        Thread video_from_file_grabber_thread;
        Thread face_detecting_thread;
        Thread video_updater_thread;
        Thread clean_up_thread;
        private const string PAUSE_BUTTON_TEXT = "Pause";
        private const string PLAY_BUTTON_TEXT = "Play";
        public static int FRAME_WIDTH;
        public static int FRAME_HEIGHT;


        public MainWindow()
        {
            InitializeComponent();
            metroStyleManager.Style = MetroColorStyle.Red;
            FRAME_WIDTH = review_footage_imageBox.Width;
            FRAME_HEIGHT = review_footage_imageBox.Height;
        }



        private void metroButton1_Click(object sender, EventArgs e)
        {
            MetroTaskWindow.ShowTaskWindow(this, "SubControl in TaskWindow", new TaskWindowControl(), 10);
        }


        private void metroButton5_Click(object sender, EventArgs e)
        {
            metroContextMenu1.Show(metroButton5, new Point(0, metroButton5.Height));
        }

        private void metroButton6_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "This is a sample MetroMessagebox `OK` only button", "MetroMessagebox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void metroButton10_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "This is a sample MetroMessagebox `OK` and `Cancel` button", "MetroMessagebox", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        }

        private void metroButton7_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "This is a sample MetroMessagebox `Yes` and `No` button", "MetroMessagebox", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        private void metroButton8_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "This is a sample MetroMessagebox `Yes`, `No` and `Cancel` button", "MetroMessagebox", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        }

        private void metroButton11_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "This is a sample MetroMessagebox `Retry` and `Cancel` button.  With warning style.", "MetroMessagebox", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
        }

        private void metroButton9_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "This is a sample MetroMessagebox `Abort`, `Retry` and `Ignore` button.  With Error style.", "MetroMessagebox", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
        }

        private void metroButton12_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "This is a sample `default` MetroMessagebox ", "MetroMessagebox");
        }


        //STARTS A CONTINUOUS RUNNING THREAD TO GRAB FRAMES FROM THE CAMERA IN THE BACKGROUND
        private void StartCameraOutputGrabberThread()
        {

            cam_output = new CameraOutputGrabberThread(live_stream_imageBox);
            camera_output_grabber_thread = new Thread(cam_output.DoWork);
            camera_output_grabber_thread.Name = "Camera Thread";
            camera_output_grabber_thread.Priority = ThreadPriority.Highest;
            camera_output_grabber_thread.IsBackground = true;
            camera_output_grabber_thread.Start();
            Debug.WriteLine("Starting camera output thread");
            while (!camera_output_grabber_thread.IsAlive) ;
            Debug.WriteLine("Camera Output Thread is alive");
        }


        //STARTS A CONTINUOUS RUNNING THREAD TO GRAB FRAMES FROM THE VIDEO FILE IN THE BACKGROUND
        private void StartVideoFileGrabberThread(String file_name)
        {
            video_from_file_grabber = new VideoFromFileThread(file_name, review_footage_imageBox);
            video_from_file_grabber_thread = new Thread(video_from_file_grabber.DoWork);
            video_from_file_grabber_thread.Name = VIDEO_FILE_GRABBER_THREAD_NAME;
            video_from_file_grabber_thread.IsBackground = true;
            video_from_file_grabber_thread.Priority = ThreadPriority.Highest;
            video_from_file_grabber_thread.Start();
            Debug.WriteLine("Starting Video Output Thread for " + file_name);
            while (!video_from_file_grabber_thread.IsAlive) ;
            Debug.WriteLine("Video Output Thread is alive");
        }

        //STARTS THREAD TO DETECT FACES IN FRAME OFF THE MAIN THREAD
        public bool StartFaceDetectingThread()
        {
            face_detector = new FaceDetectingThread(review_footage_imageBox);
            face_detecting_thread = new Thread(face_detector.DoWork);
            face_detecting_thread.Name = FACE_DETECTOR_THREAD_NAME;
            face_detecting_thread.IsBackground = true;
            face_detecting_thread.Priority = ThreadPriority.Lowest;
            face_detecting_thread.Start();
            while (!face_detecting_thread.IsAlive) ;
            return true;
        }


        private void StartDisplayUpdaterThread(ImageBox an_image_box)
        {
            video_updater = new DisplayUpdaterThread(an_image_box);
            video_updater_thread = new Thread(video_updater.DoWork);
            video_updater_thread.Name = DISPLAY_UPDATER_THREAD_NAME;
            video_updater_thread.Priority = ThreadPriority.AboveNormal;
            video_updater_thread.IsBackground = true;
            video_updater_thread.Start();
            Debug.WriteLine("Starting video updater thread");
            while (!video_updater_thread.IsAlive) ;
            Debug.WriteLine("Video Updater Thread is alive");
        }


        private void StartCleanUpThread()
        {
            clean_upper = new CleanUpThread();
            clean_up_thread = new Thread(clean_upper.DoWork);
            clean_up_thread.Name = CLEAN_UP_THREAD_NAME;
            clean_up_thread.IsBackground = true;
            clean_up_thread.Priority = ThreadPriority.Normal;
            clean_up_thread.Start();
            while (!clean_up_thread.IsAlive) ;
        }

        //THIS RETURNS A THE FILEPATH TO A GIVEN VIDEO 
        //AFTER PRESENTING A USER WITH A DIALOG
        private String LoadVideoFromFile()
        {
            String file_name = null;
            try
            {

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = FILE_FILTER;
                dialog.Title = SELECT_VIDEO_MESSAGE;
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    file_name = dialog.FileName;
                    return file_name;
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        //INITIALIZES ALL DATA STORES AND NECESSARY OBJECTS
        private void InitializeStuff()
        {
            FRAMES_TO_BE_PROCESSED = new ConcurrentQueue<Image<Bgr, byte>>();
            FRAMES_TO_BE_DISPLAYED = new ConcurrentQueue<Image<Bgr, byte>>();
            DETECTED_FACES_DATASTORE = new ConcurrentDictionary<int, Face>();
        }


        //THIS EMPTIES DATASTORES
        public void ClearDataStores()
        {
            Image<Bgr, byte> image=null;
            while (FRAMES_TO_BE_PROCESSED.TryDequeue(out image)) ;
            while (FRAMES_TO_BE_DISPLAYED.TryDequeue(out image)) ;
            DETECTED_FACES_DATASTORE.Clear();
            image = null;

        }

        //STOPS ALL BACKGROUND THREADS THAT ARE RUNNING UPON REUQEST
        private void StopThreads()
        {
            try
            {

                if (video_updater_thread != null)
                {
                    //IF THE VIDEO IS PAUSED RESUME IT
                    //ResumeVideo();

                    //PROCEED TO TERMINATE THE THREADS
                    video_updater.RequestStop();
                    face_detector.RequestStop();
                    video_from_file_grabber.RequestStop();
                    //clean_upper.RequestStop();

                    //LOOP HERE UNTIL ALL THREADS ARE TERMINATED
                    while (video_updater_thread.IsAlive) ;
                    Debug.WriteLine("Video Updater Is Dead".ToUpper());
                    while (face_detecting_thread.IsAlive) ;
                    Debug.WriteLine("face detector Is Dead".ToUpper());
                    while (video_from_file_grabber_thread.IsAlive) ;
                    Debug.WriteLine("Video From File Is Dead".ToUpper());

                    
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void PauseThreads() 
        {
            video_updater.paused = true;
            face_detector.paused = true;
            video_from_file_grabber.paused = true;
        }

        public void ResumeThreads() 
        {
            video_updater.paused = false;
            face_detector.paused = false;
            video_from_file_grabber.paused = false;
        }

        //THIS DISABLES UNCESSARY BUTTON CONTROLS
        public void DisableButtons()
        {
            pause_button.Enabled = false;
            stop_button.Enabled = false;
            stop_button2.Enabled = false;

            //ENABLE THE PICK VIDEO BUTTON
            //SO THE USER HAS THE ABILITY TO PICK 
            //ANOTHER VIDEO
            pick_video_button.Enabled = true;

        }

        //THIS ENABLES THE NECESSARY BUTTON CONTROLS
        public void EnableButtons()
        {
            pause_button.Enabled = true;
            stop_button.Enabled = true;
            stop_button2.Enabled = true;

            //DISABLE THE PICK VIDEO BUTTON
            //SO THE USER CAN NOT PICK ANOTHER VIDEO
            //WHILE THE CURRENT ONE IS STILL RUNNING
            pick_video_button.Enabled = false;
        }

        //THIS DISPLAYS A DIALOG ALLOWING A USER TO LOAD A VIDEO
        //IT THEN STARTS THREADS TO LOAD AND PROCESS VIDEO FRAME BY FRAME
        private void pick_video_button_Click(object sender, EventArgs e)
        {
            try
            {
                
                String file_name = LoadVideoFromFile();
                
                if (file_name != null)
                {
                    InitializeStuff();
                    StartThreads(file_name);
                    EnableButtons();
                }
                else
                {
                    MetroMessageBox.Show(this, SELECT_VIDEO_MESSAGE, MESSAGE_BOX_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }


            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in Pick Video" + ex.Message);
            }
        }

        private void StartThreads(String file_name)
        {
            StartVideoFileGrabberThread(file_name);
            StartDisplayUpdaterThread(review_footage_imageBox);
            StartFaceDetectingThread();
            //StartCleanUpThread();
        }

        //
        public void ReleaseResources()
        {
            try
            {
                //SET THREADS TO NULL FOR GARBAGE COLLECTOR
                video_updater = null;
                face_detector = null;
                video_from_file_grabber = null;
                clean_upper = null;
                video_updater_thread = null;
                video_from_file_grabber_thread = null;
                face_detecting_thread = null;
                clean_up_thread = null;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void MakeBackgroundOfImageBoxBlack()
        {

            int width = review_footage_imageBox.Width;
            int height = review_footage_imageBox.Height;
            Image<Bgr, byte> black_image = new Image<Bgr, byte>(width, height, new Bgr(0, 0, 0));
            review_footage_imageBox.ClearOperation();
            review_footage_imageBox.Image = black_image;
            review_footage_imageBox.Image = black_image;
            //live_stream_imageBox.ClearOperation();
            //live_stream_imageBox.Image = black_image;
            //live_stream_imageBox.Image = black_image;
        }

        private void pause_button_Click(object sender, EventArgs e)
        {
            if (pause_button.Text == PAUSE_BUTTON_TEXT)
            {
                PauseVideo();
            }
            else
            {
                ResumeVideo();
            }
        }

        //ATTEMPTS TO PAUSE A RUNNING VIDEO
        public void PauseVideo() 
        {
            video_updater.paused = true;
            pause_button.Text = PLAY_BUTTON_TEXT;
        }

        //ATTEMPTS TO RESUME A PREVIOUSLY PAUSED VIDEO
        public void ResumeVideo() 
        {
            video_updater.paused = false;
            pause_button.Text = PAUSE_BUTTON_TEXT;
        }

        private void stop_button2_Click(object sender, EventArgs e)
        {
            try
            {
                StopThreads();
                DisableButtons();
                MakeBackgroundOfImageBoxBlack();
                ReleaseResources();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void show_detected_faces2_CheckedChanged(object sender, EventArgs e)
        {
            DisplayUpdaterThread.show_deteted_faces_is_checked = !DisplayUpdaterThread.show_deteted_faces_is_checked;
        }

        private void colorSlider1_Scroll(object sender, ScrollEventArgs e)
        {
            Debug.WriteLine("Slider Scroll Event");
            double ratio=(e.NewValue/100);
            GoToThatPartOfTheVideo(ratio);
        }

        private void GoToThatPartOfTheVideo(double ratio)
        {
            PauseThreads();
            ClearDataStores();
            video_from_file_grabber.RewindOrForwardVideo(ratio);
            ResumeThreads();

        }

        private void colorSlider1_ValueChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Value Changed Event");
            double ratio=((sender as ColorSlider).Value/100);
            //GoToThatPartOfTheVideo(ratio);
        }




    }
}