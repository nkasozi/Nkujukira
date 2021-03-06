﻿using Emgu.CV;
using Emgu.CV.UI;
using Nkujukira.Demo.Entitities;
using Nkujukira.Demo.Managers;
using Nkujukira.Demo.Singletons;
using Nkujukira.Demo.Threads;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nkujukira.Demo.Factories
{
    public static class ThreadFactory
    {
        public const String PERP_ALERT_THREAD         = "perp_alert";
        public const String STUDENT_ALERT_THREAD      = "student_alert";
        public const String CAMERA_THREAD             = "camera_output";
        public const String CAMERA_THREAD_USING_VIDEO = "camera_output_using_video";
        public const String REVIEW_DISPLAY_UPDATER    = "review_display_updater";
        public const String LIVE_DISPLAY_UPDATER      = "livedisplay_updater";
        public const String REVIEW_FACE_DETECTOR      = "review_face_detector";
        public const String LIVE_FACE_DETECTOR        = "live_face_detector";
        public const String FACE_DRAWER               = "face_drawer";
        public const String FOOTAGE_SAVER             = "footage_saver";
        public const String VIDEO_THREAD              = "video_from_file";
        public const String VIDEO_THREAD_USING_CAMERA = "video_from_file_using_camera";
        public const String PERP_RECOGNIZER           = "perpetrator_recognizer";
        public const String PROGRESS_THREAD           = "face_recog_progress";
        public const String PROGRESS_THREAD_2         = "face_recog_progress_2";

        public static String[] ALL_THREADS = { 
                                                 PERP_ALERT_THREAD,
                                                 STUDENT_ALERT_THREAD,
                                                 CAMERA_THREAD,
                                                 CAMERA_THREAD_USING_VIDEO,
                                                 REVIEW_DISPLAY_UPDATER,
                                                 LIVE_DISPLAY_UPDATER, 
                                                 REVIEW_FACE_DETECTOR,
                                                 LIVE_FACE_DETECTOR,
                                                 FACE_DRAWER,       
                                                 FOOTAGE_SAVER, 
                                                 VIDEO_THREAD,
                                                 VIDEO_THREAD_USING_CAMERA,
                                                 PROGRESS_THREAD,
                                                  PERP_RECOGNIZER
                                              };
        public static String[] ALL_LIVE_THREADS = { 
                                                    PERP_ALERT_THREAD,                                                                                
                                                    CAMERA_THREAD, 
                                                    CAMERA_THREAD_USING_VIDEO,
                                                    LIVE_DISPLAY_UPDATER,                                                                                 
                                                    LIVE_FACE_DETECTOR,                                                                                       
                                                    FOOTAGE_SAVER,                                                                                
                                                    PROGRESS_THREAD,
                                                    PERP_RECOGNIZER
                                                   };

        public static String[] ALL_REVIEW_THREADS = {                                                                                
                                                        STUDENT_ALERT_THREAD,                                                                               
                                                        REVIEW_DISPLAY_UPDATER,                                                                                 
                                                        REVIEW_FACE_DETECTOR,                                                                                
                                                        FACE_DRAWER,                                                                                       
                                                        VIDEO_THREAD,
                                                        VIDEO_THREAD_USING_CAMERA                                                                                                     
                                                      };

        //ALL THREADS AVAILABLE
        private static CameraOutputGrabberThread cam_output                       = null;
        private static CameraOutputGrabberThreadUsingVideo cam_output_using_video = null;
        private static VideoFromFileThread video_from_file_grabber                = null;
        private static VideoFromFileThreadUsingCamera video_from_camera           = null;
        private static ReviewFaceDetectingThread review_face_detector             = null;
        private static LiveStreamFaceDetectingThread live_face_detector           = null;
        private static DisplayUpdaterThread live_display_updater                  = null;
        private static DisplayUpdaterThread review_display_updater                = null;
        private static PerpetratorAlertThread perp_alert_thread                   = null;
        private static StudentAlertThread student_alert_thread                    = null;
        private static FaceDrawingThread face_drawer                              = null;
        private static FootageSavingThread footage_saver                          = null;
        private static PerpetratorRecognitionThread perp_recognizer               = null;
        private static FaceRecognitionProgressThread face_recog_progress          = null;
        private static FaceRecognitionProgressThread face_recog_progress_2 = null;

        //STARTS THE THREADS RESPONSIBLEFOR STREAMING LIVE FOOTAGE FROM CCTV CAMERAS
        public static bool StartLiveStreamThreads(Camera camera)
        {

            CreateNewPerpetratorRecognitionThread();
            CreateNewCameraOutputGrabberThread(camera);
            CreateLiveStreamFaceDetectorThread();
            CreateFaceRecogProgressThread();
            CreateFootageSaverThread(camera);
            CreateNewPerpAlertThread();
            CreateLiveDisplayUpdaterThread(camera.camera_imagebox);

            return true;
        }

        public static bool StartReviewFootageThreadsUsingCamera(Camera camera)
        {
            CreateNewVideoFromCameraThread(camera);
            CreateReviewFaceDetectingThread();
            CreateNewStudentAlertThread();
            CreateReviewDisplayUpdaterThread();
            return true;
        } 

        //STARTS THE THREADS RESPONSIBLEFOR STREAMING LIVE FOOTAGE FROM CCTV CAMERAS
        public static bool StartLiveStreamThreadsUsingVideo()
        {
            if (Singleton.CURRENT_VIDEO_FILE != null)
            {
                CreateNewPerpetratorRecognitionThread();
                CreateNewCameraOutputGrabberThreadUsingVideo(Singleton.CURRENT_VIDEO_FILE);
                CreateLiveStreamFaceDetectorThread();
                CreateFaceRecogProgressThread();
                CreateNewPerpAlertThread();

                MainWindow.MainWindowControls imagebox_name = MainWindow.MainWindowControls.live_stream_image_box1;
                ImageBox live_stream_imagebox = (ImageBox)Singleton.MAIN_WINDOW.GetControl(imagebox_name);
                CreateLiveDisplayUpdaterThread(live_stream_imagebox);

                return true;
            }

            throw new ArgumentNullException();
        }

        //STARTS THREADS RESPONSIBLE FOR STREAMING FRAMES FROM A VIDEO FILE
        public static bool StartReviewFootageThreads()
        {
            if (Singleton.CURRENT_VIDEO_FILE != null)
            {
                CreateVideoFileGrabberThread(Singleton.CURRENT_VIDEO_FILE);
                CreateReviewFaceDetectingThread();
                CreateNewStudentAlertThread();
                CreateReviewDisplayUpdaterThread();

                return true;
            }

            throw new ArgumentNullException();
        }



        public static FaceRecognitionProgressThread CreateFaceRecogProgressThread()
        {

            face_recog_progress = new FaceRecognitionProgressThread();
            face_recog_progress_2 = new FaceRecognitionProgressThread();
            face_recog_progress.StartWorking();
            face_recog_progress_2.StartWorking();

            return face_recog_progress;
        }

        public static PerpetratorRecognitionThread CreateNewPerpetratorRecognitionThread()
        {
            perp_recognizer = new PerpetratorRecognitionThread();
            perp_recognizer.StartWorking();

            return perp_recognizer;
        }


        //STARTS A NEW ALERT GENERATION THREAD FOR IDENTIFIED PERPS
        public static PerpetratorAlertThread CreateNewPerpAlertThread()
        {

            perp_alert_thread = new PerpetratorAlertThread();
            perp_alert_thread.StartWorking();

            return perp_alert_thread;
        }

        //STARTS A NEW ALERT GENERATION THREAD FOR IDENTIFIED STUDENTS
        public static StudentAlertThread CreateNewStudentAlertThread()
        {
            student_alert_thread = new StudentAlertThread();
            student_alert_thread.StartWorking();

            return student_alert_thread;
        }


        //STARTS A CONTINUOUS RUNNING THREAD TO GRAB FRAMES FROM THE CAMERA IN THE BACKGROUND
        private static CameraOutputGrabberThread CreateNewCameraOutputGrabberThread(Camera camera)
        {

            cam_output = new CameraOutputGrabberThread(camera);
            cam_output.StartWorking();
            return cam_output;
        }

        //STARTS A CONTINUOUS RUNNING THREAD TO GRAB FRAMES FROM THE CAMERA IN THE BACKGROUND
        private static CameraOutputGrabberThreadUsingVideo CreateNewCameraOutputGrabberThreadUsingVideo(VideoFile video_file)
        {

            cam_output_using_video = new CameraOutputGrabberThreadUsingVideo(video_file);
            cam_output_using_video.StartWorking();

            return cam_output_using_video;
        }

        //STARTS A CONTINUOUS RUNNING THREAD TO GRAB FRAMES FROM THE CAMERA IN THE BACKGROUND
        private static VideoFromFileThreadUsingCamera CreateNewVideoFromCameraThread(Camera camera)
        {

            video_from_camera = new VideoFromFileThreadUsingCamera(camera);
            video_from_camera.StartWorking();

            return video_from_camera;
        }

        //STARTS A CONTINUOUS RUNNING THREAD TO GRAB FRAMES FROM THE VIDEO FILE IN THE BACKGROUND
        private static VideoFromFileThread CreateVideoFileGrabberThread(VideoFile video_file)
        {

            video_from_file_grabber = new VideoFromFileThread(video_file);
            video_from_file_grabber.StartWorking();

            return video_from_file_grabber;
        }

        //STARTS THREAD TO DETECT FACES IN FRAME OFF THE MAIN THREAD
        private static ReviewFaceDetectingThread CreateReviewFaceDetectingThread()
        {
            var controls_name = MainWindow.MainWindowControls.review_image_box;
            int width = Singleton.MAIN_WINDOW.GetControl(controls_name).Width;
            int height = Singleton.MAIN_WINDOW.GetControl(controls_name).Width;
            Size frame_size = new Size(width, height);
            review_face_detector = new ReviewFaceDetectingThread(frame_size);
            review_face_detector.StartWorking();

            return review_face_detector;
        }

        //STARTS THREAD TO DETECT FACES IN FRAME OFF THE MAIN THREAD
        private static LiveStreamFaceDetectingThread CreateLiveStreamFaceDetectorThread()
        {
            var controls_name = MainWindow.MainWindowControls.live_stream_image_box1;
            int width = Singleton.MAIN_WINDOW.GetControl(controls_name).Width;
            int height = Singleton.MAIN_WINDOW.GetControl(controls_name).Width;
            Size frame_size = new Size(width, height);
            live_face_detector = new LiveStreamFaceDetectingThread(frame_size);
            live_face_detector.StartWorking();

            return live_face_detector;
        }

        //STARTS A NEW FOOTAGE SAVING THREAD
        private static FootageSavingThread CreateFootageSaverThread(Camera camera)
        {
            footage_saver = new FootageSavingThread(camera);
            footage_saver.StartWorking();

            return footage_saver;
        }

        //STARTS A THREAD TO CONTINUOUSLY UPDATE THE VIDEO DISPLAY
        private static DisplayUpdaterThread CreateReviewDisplayUpdaterThread()
        {

            var controls_name = MainWindow.MainWindowControls.review_image_box;
            review_display_updater = new ReviewDisplayUpdater((ImageBox)Singleton.MAIN_WINDOW.GetControl(controls_name));
            review_display_updater.StartWorking();

            return review_display_updater;
        }

        public static DisplayUpdaterThread CreateLiveDisplayUpdaterThread(ImageBox image_box)
        {

            live_display_updater = new LiveDisplayUpdater(image_box);
            live_display_updater.StartWorking();
            return live_display_updater;
        }


        //RETURNS A THREAD BASED ON ITS ID
        public static AbstractThread GetThread(String thread_id)
        {
            switch (thread_id)
            {
                case ThreadFactory.PERP_ALERT_THREAD:
                    return perp_alert_thread;

                case ThreadFactory.STUDENT_ALERT_THREAD:
                    return student_alert_thread;

                case ThreadFactory.CAMERA_THREAD:
                    return cam_output;

                case ThreadFactory.CAMERA_THREAD_USING_VIDEO:
                    return cam_output_using_video;

                case ThreadFactory.REVIEW_DISPLAY_UPDATER:
                    return review_display_updater;

                case ThreadFactory.LIVE_DISPLAY_UPDATER:
                    return live_display_updater;

                case ThreadFactory.REVIEW_FACE_DETECTOR:
                    return review_face_detector;

                case ThreadFactory.LIVE_FACE_DETECTOR:
                    return live_face_detector;

                case ThreadFactory.FACE_DRAWER:
                    return face_drawer;

                case ThreadFactory.PERP_RECOGNIZER:
                    return perp_recognizer;

                case ThreadFactory.PROGRESS_THREAD:
                    return face_recog_progress;

                case ThreadFactory.FOOTAGE_SAVER:
                    return footage_saver;

                case ThreadFactory.VIDEO_THREAD:
                    return video_from_file_grabber;

                case ThreadFactory.VIDEO_THREAD_USING_CAMERA:
                    return video_from_camera;
                    
            }
            return null;
        }

        //THIS PAUSES A THREAD GIVEN ITS ID
        public static bool PauseThread(String thread_id)
        {
            switch (thread_id)
            {
                case ThreadFactory.PERP_ALERT_THREAD:
                    if (perp_alert_thread != null) { perp_alert_thread.Pause(); }
                    break;

                case ThreadFactory.STUDENT_ALERT_THREAD:
                    if (student_alert_thread != null) { student_alert_thread.Pause(); }
                    break;

                case ThreadFactory.CAMERA_THREAD:
                    if (cam_output != null) { cam_output.Pause(); }
                    break;

                case ThreadFactory.CAMERA_THREAD_USING_VIDEO:
                    if (cam_output_using_video != null) { cam_output_using_video.Pause(); }
                    break;

                case ThreadFactory.LIVE_DISPLAY_UPDATER:
                    if (live_display_updater != null) { live_display_updater.Pause(); }
                    break;

                case ThreadFactory.REVIEW_DISPLAY_UPDATER:
                    if (review_display_updater != null) { review_display_updater.Pause(); }
                    break;

                case ThreadFactory.REVIEW_FACE_DETECTOR:
                    if (review_face_detector != null) { review_face_detector.Pause(); }
                    break;

                case ThreadFactory.LIVE_FACE_DETECTOR:
                    if (live_face_detector != null) { live_face_detector.Pause(); }
                    break;

                case ThreadFactory.PERP_RECOGNIZER:
                    if (perp_recognizer != null) { perp_recognizer.Pause(); }
                    break;

                case ThreadFactory.PROGRESS_THREAD:
                    if (face_recog_progress != null) { face_recog_progress.Pause(); face_recog_progress_2.Pause(); }
                    break;

                case ThreadFactory.FACE_DRAWER:
                    break;

                case ThreadFactory.FOOTAGE_SAVER:
                    if (footage_saver != null) { footage_saver.Pause(); }
                    break;

                case ThreadFactory.VIDEO_THREAD:
                    if (video_from_file_grabber != null) { video_from_file_grabber.Pause(); }
                    break;

                case ThreadFactory.VIDEO_THREAD_USING_CAMERA:
                    if (video_from_camera != null) { video_from_camera.Pause(); }
                    break;
            }
            return true;
        }

        //THIS PAUSES ALL RUNNING THREADS
        public static bool PauseAllLiveStreamThreads()
        {
            foreach (var thread in ThreadFactory.ALL_LIVE_THREADS)
            {
                PauseThread(thread);
            }
            return true;
        }

        //THIS PAUSES ALL RUNNING THREADS
        public static bool PauseAllReviewFootageThreads()
        {
            foreach (var thread in ThreadFactory.ALL_REVIEW_THREADS)
            {
                PauseThread(thread);
            }
            return true;
        }

        //THIS PAUSES ALL RUNNING THREADS
        public static bool PauseAllThreads()
        {
            foreach (var thread in ThreadFactory.ALL_THREADS)
            {
                PauseThread(thread);
            }
            return true;
        }

        //THIS RESUMES A THREAD GIVEN ITS ID
        public static bool ResumeThread(String thread_id)
        {
            switch (thread_id)
            {
                case ThreadFactory.PERP_ALERT_THREAD:
                    if (perp_alert_thread != null) { perp_alert_thread.Resume(); }
                    break;

                case ThreadFactory.STUDENT_ALERT_THREAD:
                    if (student_alert_thread != null) { student_alert_thread.Resume(); }
                    break;

                case ThreadFactory.CAMERA_THREAD:
                    if (cam_output != null) { cam_output.Resume(); }
                    break;

                case ThreadFactory.CAMERA_THREAD_USING_VIDEO:
                    if (cam_output_using_video != null) { cam_output_using_video.Resume(); }
                    break;

                case ThreadFactory.LIVE_DISPLAY_UPDATER:
                    if (live_display_updater != null) { live_display_updater.Resume(); }
                    break;

                case ThreadFactory.REVIEW_DISPLAY_UPDATER:
                    if (review_display_updater != null) { review_display_updater.Resume(); }
                    break;

                case ThreadFactory.REVIEW_FACE_DETECTOR:
                    if (review_face_detector != null) { review_face_detector.Resume(); }
                    break;

                case ThreadFactory.LIVE_FACE_DETECTOR:
                    if (live_face_detector != null) { live_face_detector.Resume(); }
                    break;

                case ThreadFactory.FACE_DRAWER:
                    break;

                case ThreadFactory.PERP_RECOGNIZER:
                    if (perp_recognizer != null) { perp_recognizer.Resume(); }
                    break;

                case ThreadFactory.PROGRESS_THREAD:
                    if (face_recog_progress != null) { face_recog_progress.Resume(); face_recog_progress_2.Resume(); }
                    break;

                case ThreadFactory.FOOTAGE_SAVER:
                    if (footage_saver != null) { footage_saver.Resume(); }
                    break;

                case ThreadFactory.VIDEO_THREAD:
                    if (video_from_file_grabber != null) { video_from_file_grabber.Resume(); }
                    break;

                case ThreadFactory.VIDEO_THREAD_USING_CAMERA:
                    if (video_from_camera != null) { video_from_camera.Resume(); }
                    break;
            }
            return true;
        }

        //THIS RESUMES ALL RUNNING THREADS
        public static bool ResumeAllLiveStreamThreads()
        {
            foreach (var thread in ThreadFactory.ALL_LIVE_THREADS)
            {
                ResumeThread(thread);
            }
            return true;
        }

        //THIS RESUMES ALL RUNNING THREADS
        public static bool ResumeAllReviewFootageThreads()
        {
            foreach (var thread in ThreadFactory.ALL_REVIEW_THREADS)
            {
                ResumeThread(thread);
            }
            return true;
        }

        //THIS RESUMES ALL RUNNING THREADS
        public static bool ResumeAllThreads()
        {
            foreach (var thread in ThreadFactory.ALL_THREADS)
            {
                ResumeThread(thread);
            }
            return true;
        }

        //THIS STOPS A RUNNING THREAD GIVEN ITS iD
        public static bool StopThread(String thread_id)
        {
            switch (thread_id)
            {
                case ThreadFactory.PERP_ALERT_THREAD:
                    if (perp_alert_thread != null) { perp_alert_thread.RequestStop(); }
                    break;

                case ThreadFactory.STUDENT_ALERT_THREAD:
                    if (student_alert_thread != null) { student_alert_thread.RequestStop(); }
                    break;

                case ThreadFactory.CAMERA_THREAD:
                    if (cam_output != null) { cam_output.RequestStop(); }
                    break;

                case ThreadFactory.CAMERA_THREAD_USING_VIDEO:
                    if (cam_output_using_video != null) { cam_output_using_video.RequestStop(); };
                    break;

                case ThreadFactory.LIVE_DISPLAY_UPDATER:
                    if (live_display_updater != null) { live_display_updater.RequestStop(); }
                    break;

                case ThreadFactory.REVIEW_DISPLAY_UPDATER:
                    if (review_display_updater != null) { review_display_updater.RequestStop(); }
                    break;

                case ThreadFactory.REVIEW_FACE_DETECTOR:
                    if (review_face_detector != null) { review_face_detector.RequestStop(); }
                    break;

                case ThreadFactory.LIVE_FACE_DETECTOR:
                    if (live_face_detector != null) { live_face_detector.RequestStop(); }
                    break;

                case ThreadFactory.PERP_RECOGNIZER:
                    if (perp_recognizer != null) { perp_recognizer.RequestStop(); }
                    break;

                case ThreadFactory.FACE_DRAWER:
                    break;

                case ThreadFactory.PROGRESS_THREAD:
                    if (face_recog_progress != null)
                    {
                        face_recog_progress.RequestStop();
                        face_recog_progress_2.RequestStop();
                    }
                    break;

                case ThreadFactory.FOOTAGE_SAVER:
                    if (footage_saver != null) { footage_saver.RequestStop(); }
                    break;

                case ThreadFactory.VIDEO_THREAD:
                    if (video_from_file_grabber != null) { video_from_file_grabber.RequestStop(); }
                    break;

                case ThreadFactory.VIDEO_THREAD_USING_CAMERA:
                    if (video_from_camera != null) { video_from_camera.RequestStop(); }
                    break;
            }
            return true;
        }

        //THIS STOPS ALL RUNNING THREADS
        public static bool StopAllThreads()
        {

            foreach (var thread in ThreadFactory.ALL_THREADS)
            {
                StopThread(thread);
            }
            return true;
        }

        public static bool StopLiveStreamThreads()
        {
            foreach (var thread in ALL_LIVE_THREADS)
            {
                StopThread(thread);
            }

            return true;
        }

        public static bool StopReviewFootageThreads()
        {
            foreach (var thread in ALL_REVIEW_THREADS)
            {
                StopThread(thread);
            }
            return true;
        }

        //THIS RELEASES ALL RESOURCES CONSUMED BY A THREAD GIVEN ITS ID
        public static bool ReleaseThreadResources(String thread_id)
        {
            switch (thread_id)
            {
                case ThreadFactory.PERP_ALERT_THREAD:
                    
                    perp_alert_thread       = null;
                    break;

                case ThreadFactory.STUDENT_ALERT_THREAD:
                    student_alert_thread    = null;
                    break;

                case ThreadFactory.CAMERA_THREAD:
                    cam_output              = null;
                    break;

                case ThreadFactory.CAMERA_THREAD_USING_VIDEO:
                    cam_output_using_video  = null;
                    break;

                case ThreadFactory.LIVE_DISPLAY_UPDATER:
                    live_display_updater    = null;
                    break;

                case ThreadFactory.REVIEW_DISPLAY_UPDATER:
                    review_display_updater  = null;
                    break;

                case ThreadFactory.REVIEW_FACE_DETECTOR:
                    review_face_detector    = null;
                    break;

                case ThreadFactory.LIVE_FACE_DETECTOR:
                    live_face_detector      = null;
                    break;

                case ThreadFactory.PERP_RECOGNIZER:
                    perp_recognizer         = null;
                    break;

                case ThreadFactory.PROGRESS_THREAD:
                    face_recog_progress     = null;
                    face_recog_progress_2   = null;
                    break;

                case ThreadFactory.FACE_DRAWER:
                    face_drawer             = null;
                    break;

                case ThreadFactory.FOOTAGE_SAVER:
                    footage_saver           = null;
                    break;

                case ThreadFactory.VIDEO_THREAD:
                    video_from_file_grabber = null;
                    break;

                case ThreadFactory.VIDEO_THREAD_USING_CAMERA:
                    video_from_camera       = null;
                    break;
            }
            return true;
        }

        //RELEASES ALL RESOURCES CONSUMED BY A THREAD
        public static bool ReleaseAllThreadResources()
        {
            foreach (var thread in ThreadFactory.ALL_THREADS)
            {
                ReleaseThreadResources(thread);
            }
            return true;
        }

        //RELEASES ALL RESOURCES CONSUMED BY A THREAD
        public static bool ReleaseLiveStreamThreadsResources()
        {
            foreach (var thread in ThreadFactory.ALL_LIVE_THREADS)
            {
                ReleaseThreadResources(thread);
            }
            return true;
        }


        //RELEASES ALL RESOURCES CONSUMED BY A THREAD
        public static bool ReleaseReviewThreadsResources()
        {
            foreach (var thread in ThreadFactory.ALL_REVIEW_THREADS)
            {
                ReleaseThreadResources(thread);
            }
            return true;
        }


       
    }
}
