﻿using System;
using System.Drawing;
using System.Windows.Forms;

using MetroFramework.Forms;
using System.Collections.Concurrent;
using Emgu.CV;
using Emgu.CV.Structure;
using Nkujukira.Entities;
using Nkujukira.Threads;
using System.Diagnostics;
using Emgu.CV.UI;
using MB.Controls;
using MetroFramework.Demo.Managers;
using MetroFramework.Demo.Singletons;
using MetroFramework.Demo.Factories;
using System.Data;
using MetroFramework.Demo.Views;
using MetroFramework.Demo.FactoryMethod;

namespace MetroFramework.Demo
{
    public partial class MainWindow : MetroForm
    {
        private const string SELECT_VIDEO_MESSAGE = "Please Select a Video file";
        private const string LOAD_CAMERA_FOOTAGE_MESSAGE = "You Are Loading Footage From Your camera!!";
        private const string FILE_FILTER = "All files (*.*)|*.*";
        private const string MESSAGE_BOX_TITLE = "Message!!";
        private const string PAUSE_BUTTON_TEXT = "Pause";
        private const string PLAY_BUTTON_TEXT = "Play";
        public static DataTable userDataTable;
        public string DATABASE = "MYSQL";
        DataBaseInterface dataBaseFactory;

        public void generateUserTable()
        {
            dataBaseFactory = new DataBaseFactory().getDataBase(DATABASE);
            userDataTable = dataBaseFactory.generateUsersDataTable();
            userTable.DataSource = userDataTable;
            DataGridViewColumn column1 = userTable.Columns[1];
            DataGridViewColumn column2 = userTable.Columns[2];
            column1.Width = 200;
            column2.Width = 200;

        }


        public MainWindow()
        {
            InitializeComponent();

            Singleton.MAIN_WINDOW = this;

            metroStyleManager.Style = MetroColorStyle.Red;

            this.MaximizeBox = false;

            DisableControls();
            generateUserTable();
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
            try
            {
                new ChangeUserLoginDetails().ShowDialog();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            //MetroMessageBox.Show(this, "This is a sample MetroMessagebox `OK` only button", "MetroMessagebox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void metroButton10_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "This is a sample MetroMessagebox `OK` and `Cancel` button", "MetroMessagebox", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        }

        private void metroButton7_Click(object sender, EventArgs e)
        {
            try
            {
                new AddNewUser().ShowDialog();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            //MetroMessageBox.Show(this, "This is a sample MetroMessagebox `Yes` and `No` button", "MetroMessagebox", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        private void metroButton8_Click(object sender, EventArgs e)
        {
            new AddStudentDialog().ShowDialog();
            //MetroMessageBox.Show(this, "This is a sample MetroMessagebox `Yes`, `No` and `Cancel` button", "MetroMessagebox", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        }

        private void metroButton11_Click(object sender, EventArgs e)
        {
            try
            {
                int Row = userTable.CurrentRow.Index;
                String id = (String)userTable[0, Row].Value;
                String user = (String)userTable[1, Row].Value;
                String role = (String)userTable[2, Row].Value;
                ChangeUserTypeDialog.id = id;
                ChangeUserTypeDialog.user = user;
                ChangeUserTypeDialog.user_role = role;
                new ChangeUserTypeDialog().ShowDialog();
                //MetroMessageBox.Show(this,id+user+role, "MetroMessagebox", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            //MetroMessageBox.Show(this, "This is a sample MetroMessagebox `Retry` and `Cancel` button.  With warning style.", "MetroMessagebox", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
        }

        private void metroButton9_Click(object sender, EventArgs e)
        {
            DialogResult response = MetroMessageBox.Show(this, "Are You Sure You Want To Delete The Selected User", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (response == DialogResult.Yes)
            {
                int Row = userTable.CurrentRow.Index;
                String id = (String)userTable[0, Row].Value;
                if (dataBaseFactory.deleteUser(id))
                {
                    MetroMessageBox.Show(this, "User Deleted Succefully", "", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
                else
                {
                    MetroMessageBox.Show(this, "Unexpected error occured. Please try again", "ERROR");
                }
            }
            else if (response == DialogResult.No)
            {
                MetroMessageBox.Show(this, "No...", "");
            }
            //MetroMessageBox.Show(this, "This is a sample MetroMessagebox `Abort`, `Retry` and `Ignore` button.  With Error style.", "MetroMessagebox", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
        }

        private void metroButton12_Click(object sender, EventArgs e)
        {
            
            StudentManager.printNumberOfPhotosInFolder(dataBaseFactory.getStudentDetails());
            //MetroMessageBox.Show(this, "This is a sample `default` MetroMessagebox ", "MetroMessagebox");
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            ThreadManager.StartNewThread(ThreadFactory.ALERT_THREAD);

        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            ThreadManager.StopThread(ThreadFactory.ALERT_THREAD);
            //PerpetratorDetails form = new PerpetratorDetails();
            //form.Show();
        }

        //THIS DISPLAYS A DIALOG ALLOWING A USER TO LOAD A VIDEO
        //IT THEN STARTS THREADS TO LOAD AND PROCESS VIDEO FRAME BY FRAME
        private void pick_video_button_Click(object sender, EventArgs e)
        {
            try
            {
                ThreadManager.StopAllThreads();
                Singleton.ClearDataStores();
                ThreadManager.ReleaseAllThreadResources();

                String file_name = LoadVideoFromFile();

                if (file_name != null)
                {
                    InitializeStuff();
                    StartThreads(file_name);
                    EnableControls();
                    return;
                }
                else
                {

                    MetroMessageBox.Show(this, SELECT_VIDEO_MESSAGE, MESSAGE_BOX_TITLE, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    return;
                }


            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in Pick Video" + ex.Message);
            }
        }

        //PAUSES THE RUNNING VIDEO UPON CALL
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

        //STOPS RUNNING VIDEO UPON CALL [CLICK OF STOP BUTTON]
        private void StopButton_Click(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Stopping Threads");
                ThreadManager.StopAllThreads();
                Debug.WriteLine("Disabling Buttons");
                //DisableButtons();
                Debug.WriteLine("Releasing Resources");
                Singleton.ClearDataStores();
                ThreadManager.ReleaseAllThreadResources();
            }
            catch (Exception)
            {


            }
        }

        
        //THIS HANDLES THE REVIEW FOOTAGE IMAGE BOX SLIDER SCROLL EVENT
        private void SlidersScroll(object sender, ScrollEventArgs e)
        {
            try
            {
                if (!DisplayUpdaterThread.WORK_DONE)
                {
                    //GET VALUE USER HAS SCROLLED TO
                    int value = (sender as ColorSlider).Value;

                    //GET THE PERCENTAGE REPRESENTING THAT VALUE
                    double ratio = ((((double)value) / ((double)100)));

                    //FORWARD TO THAT PART OF THE VIDEO
                    GoToThatPartOfTheVideo(ratio);

                }
                else
                {
                    slider_review_footage.Value = 0;
                    slider_review_footage.Enabled = false;
                }

            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.Message);
            }

        }

        //STARTS ALL NECESSARY THREADS
        private void StartThreads(String file_name)
        {
            Singleton.CURRENT_FILE_NAME = file_name;
            ThreadManager.StartIntroThreads(true);
        }

        //ATTEMPTS TO PAUSE A RUNNING VIDEO
        public void PauseVideo()
        {
            ThreadManager.PauseAllThreads();
            pause_button.Text = PLAY_BUTTON_TEXT;
        }

        //ATTEMPTS TO RESUME A PREVIOUSLY PAUSED VIDEO
        public void ResumeVideo()
        {
            ThreadManager.ResumeAllThreads();
            pause_button.Text = PAUSE_BUTTON_TEXT;
        }

        //ENABLES DRAWING OF DETECTED FACES ON TO THE FRAMES
        private void show_detected_faces2_CheckedChanged(object sender, EventArgs e)
        {
            FaceDetectingThread.draw_detected_faces = !FaceDetectingThread.draw_detected_faces;
        }

        //FORWARDS TO A CERTAIN PART OF A VIDEO
        private void GoToThatPartOfTheVideo(double ratio)
        {
            //PAUSE THE VIDEO
            ThreadManager.PauseAllThreads();

            //CLEAR ALL THE DATA STORES 
            Singleton.ClearDataStores();

            //GET THE MILLESCONDS TO FORWARD TO
            double millescond_to_jump_to = ratio * VideoFromFileThread.VIDEO_LENGTH;

            //FORWARD THE VIDEO
            ((VideoFromFileThread)ThreadManager.GetThread(ThreadFactory.VIDEO_THREAD)).RewindOrForwardVideo(millescond_to_jump_to);

            //SET THE TIME ELAPSED ON THE VIDEO
            ((DisplayUpdaterThread)ThreadManager.GetThread(ThreadFactory.DISPLAY_UPDATER)).SetTimeElapsed(millescond_to_jump_to);
            
            //RESUME PLAYING THE VIDEO
            ThreadManager.ResumeAllThreads();
        }


        //THIS RETURNS A FILEPATH TO A GIVEN VIDEO 
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
        public void InitializeStuff()
        {
            Singleton.FRAMES_TO_BE_PROCESSED = new ConcurrentQueue<Image<Bgr, byte>>();
            Singleton.FRAMES_TO_BE_DISPLAYED = new ConcurrentQueue<Image<Bgr, byte>>();
            Singleton.DETECTED_FACES_DATASTORE = new ConcurrentDictionary<int, Face>();
            show_detected_faces2.Checked = false;
        }

        //THIS DISABLES UNECESSARY CONTROLS
        public void DisableControls()
        {
            if (pause_button != null)
            {
                pause_button.Enabled = false;
            }
            if (stop_button_1 != null)
            {
                stop_button_1.Enabled = false;
            }
            if (stop_button_2 != null)
            {
                stop_button_2.Enabled = false;
            }
            if (slider_review_footage != null)
            {
                slider_review_footage.Enabled = false;
            }
            ResetButtonText();
            //ENABLE THE PICK VIDEO BUTTON
            //SO THE USER HAS THE ABILITY TO PICK 
            //ANOTHER VIDEO
            pick_video_button.Enabled = true;

        }

        //THIS ENABLES THE NECESSARY CONTROLS
        public void EnableControls()
        {
            if (pause_button != null)
            {
                pause_button.Enabled = true;
            }
            if (stop_button_1 != null)
            {
                stop_button_1.Enabled = true;
            }
            if (stop_button_2 != null)
            {
                stop_button_2.Enabled = true;
            }
            if (slider_review_footage != null)
            {
                slider_review_footage.Enabled = true;
            }
            ResetButtonText();
            //DISABLE THE PICK VIDEO BUTTON
            //SO THE USER CAN NOT PICK ANOTHER VIDEO
            //WHILE THE CURRENT ONE IS STILL RUNNING
            pick_video_button.Enabled = true;
        }

        //RESETS NECESSARY BUTTON TEXTS TO DEFAULTS
        private void ResetButtonText()
        {
            pause_button.Text = "Pause";
        }

        //RETURNS THE REVIEW FOOTAGE IMAGE BOX
        public ImageBox GetReviewFootageImageBox()
        {
            return this.review_footage_image_box;
        }

        //RETURNS THE DETECTED FACES PANEL
        public Panel GetDetectedFacesPanel()
        {
            return panel_for_detected_faces;
        }

        public ColorSlider GetColorSlider() 
        {
            return review_footage_color_slider;
        }

        public Label GetLabel(String name) 
        {
            switch (name) 
            {
                case "time_elapsed":
                    return time_elapsed_label;
                case "total_time":
                    return total_time_label;
            }
            return null;
        }

        //HANDLES A CLICK EVENT IN THE REVIEW FOOTAGE BOX
        private void review_footage_image_box_Click(object sender, EventArgs e)
        {
            if (((DisplayUpdaterThread)ThreadManager.GetThread(ThreadFactory.DISPLAY_UPDATER)) != null)
            {
                PauseVideo();
                SelectPerpetrator form = new SelectPerpetrator((Image<Bgr, byte>)review_footage_image_box.Image);
                form.Show();
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }

        private void metroTabPage5_Click(object sender, EventArgs e)
        {

        }

        private void metroButton6_Click_1(object sender, EventArgs e)
        {
            try
            {
                new ChangeUserLoginDetails().ShowDialog();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void metroButton9_Click_1(object sender, EventArgs e)
        {
            DialogResult response = MetroMessageBox.Show(this, "Are You Sure You Want To Delete The Selected User", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (response == DialogResult.Yes)
            {
                int Row = userTable.CurrentRow.Index;
                String id = (String)userTable[0, Row].Value;
                if (dataBaseFactory.deleteUser(id))
                {
                    MetroMessageBox.Show(this, "User Deleted Succefully", "", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
                else
                {
                    MetroMessageBox.Show(this, "Unexpected error occured. Please try again", "ERROR");
                }
            }
            else if (response == DialogResult.No)
            {
                //MetroMessageBox.Show(this, "No...", "");
            }
        }

        private void metroButton7_Click_1(object sender, EventArgs e)
        {
            try
            {
                new AddNewUser().ShowDialog();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void metroButton11_Click_1(object sender, EventArgs e)
        {
            try
            {
                int Row = userTable.CurrentRow.Index;
                String id = (String)userTable[0, Row].Value;
                String user = (String)userTable[1, Row].Value;
                String role = (String)userTable[2, Row].Value;
                ChangeUserTypeDialog.id = id;
                ChangeUserTypeDialog.user = user;
                ChangeUserTypeDialog.user_role = role;
                new ChangeUserTypeDialog().ShowDialog();
                //MetroMessageBox.Show(this,id+user+role, "MetroMessagebox", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void metroButton8_Click_1(object sender, EventArgs e)
        {
            new AddStudentDialog().ShowDialog();
        }





       
    }
}
