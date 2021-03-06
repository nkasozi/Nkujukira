﻿using Nkujukira.Demo.Entitities;
using Nkujukira.Demo.Managers;
using Nkujukira.Demo.Singletons;
using Nkujukira.Demo.Threads;
using MetroFramework.Forms;
using ProgressControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Nkujukira.Demo.Views
{
    public partial class VictimsDetailsForm : MetroForm
    {
        //THE PERPETRATOR OF THE CRIME COMMITED AGAINIST THE VICTIM
        Perpetrator perpetrator;

        //THE CRIME COMMITTED AGAINIST THE VICTIM
        Crime crime;

        //THE VICTIM OF THE CRIME
        Victim victim;

        //FLAG INDICATING WHETHER THERE ARE OTHER VICTIMS OF THIS CRIME
        private bool close_after_saving                       = false;
        //private bool another_crime;

        //NEW VICTIM CONSTRUCTOR
        public VictimsDetailsForm(Perpetrator perp,Crime crime)
        {
            //this.another_crime = false;
            this.perpetrator                                  = perp;
            this.crime                                        = crime;
            InitializeComponent();
            ResetTextValues();
        }

        //DISPLAY VICTIMS DETAILS CONSTRUCTOR
        public VictimsDetailsForm(Victim victim)
        {
            this.victim                                       = victim;
            InitializeComponent();
            SetVictimDetails(victim);
            DisableControls();
            
        }

        //DISABLES NECESSARY CONTROLS ON FORM WHEN DISPLAYING DETAILS 
        //OF VICTIM
        private void DisableControls()
        {
            this.name_text_box.Enabled                        = false;
            this.date_of_birth.Enabled                        = false;
            this.gender_comoboBox.Enabled                     = false;
            this.button_save.Visible                          = false;
            this.is_a_student_comboBox.Enabled                = false;
            this.items_lost_textbox.Enabled                   = false;
            this.label1.Visible                               = false;
            this.button_another_victim.Visible                = false;
            this.panel1.Size = new Size(548, 350);
            this.Size = new Size(568, 400);
        }

        //SETS TEXT OF CONTROLS WHEN DISPLAYING DETAILS OF VICTIMS
        private void SetVictimDetails(Victim victim)
        {
            if (victim == null) { throw new ArgumentNullException(); }

            //SET HIS PERSONAL DETAILS
            this.name_text_box.Text                           = victim.name;
            this.date_of_birth.Text                           = victim.date_of_birth;
            this.gender_comoboBox.Text                        = victim.gender;
            this.is_a_student_comboBox.Text                   = victim.is_a_student.ToString();

            //SET THE ITEMS STOLEN FIELD
            for(int i=0;i<victim.items_stolen.Length;i++)
            {
                if (i == 0)
                {
                    this.items_lost_textbox.Text += victim.items_stolen[i].name_of_item;
                    continue;
                }

                this.items_lost_textbox.Text += "," + victim.items_stolen[i].name_of_item;
            }
        }

        //HANDLER FOR WHEN THE SAVE BUTTON IS CLICKED
        private void save_button_Click(object sender, EventArgs e)
        {
            timer1.Start();

            //INDICATE WHETHER THERE IS ANOTHER VICTIM
            close_after_saving                                = true;

            //INDICATE THAT THIS IS THE ONLY CRIME THE PERP HAS COMMITTED
            PerpetratorDetailsForm.another_crime = false;

            //save victim details
            SaveVictimDetails();        

        }

        //SAVES THE VICTIMS DETAILS AND THE CRIMES AGAINIST HIM IN THE DATABASE
        private void SaveVictimDetails()
        {
            //get victim details
            button_save.Enabled                               = false;
            button_another_victim.Enabled                     = false;
            button_another_crime.Enabled                      = false;

            //get victim details
            String name                                       = name_text_box.Text;
            String d_o_b                                      = date_of_birth.Text;
            String gender                                     = gender_comoboBox.Text;
            bool is_a_student                                 = is_a_student_comboBox.Text.Equals("Yes") ? true : false;
            

            //save perp
            if (close_after_saving) 
            { 
                //PerpetratorsManager.Save(perpetrator);

                //set the perp id in the crime
               // crime.perpetrator_id = perpetrator.id;

                //save crime
               // CrimesManager.Save(crime);
            }

            StolenItem[] items_lost                           = GetItemsLost();

            //create victims object
            victim                                            = new Victim(name, d_o_b, items_lost, gender, is_a_student, crime.id);

            //save victim
            //VictimsManager.Save(victim);

            //SAVE EACH STOLEN ITEM IN THE DATABASE
            foreach (var item in items_lost) 
            {
                //item.victims_id = victim.id;
                //StolenItemsManager.Save(item);
            }

    
        }


        //GETS THE ITEMS LOST OR STOLEN FROM THE VICTIM
        private StolenItem[] GetItemsLost()
        {
            String items_stolen                               = items_lost_textbox.Text;
            String[] items                                    = items_stolen.Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries);
            List<StolenItem> stolen                           = new List<StolenItem>();

            foreach (var item in items) 
            {
                StolenItem stolen_item                        = new StolenItem(item, -1);
                stolen.Add(stolen_item);
            }

            return stolen.ToArray();

        }

        //HELPS A USER ADD ANOTHER VICTIM TO THE CRIME
        private void another_victim_button_Click(object sender, EventArgs e)
        {
            //START TIMER
            timer1.Start();

            //DO NOT CLOSE FORM AFTER SAVING
            close_after_saving                                = false;

            //SAVE THE DETAILS OF THE VICTIM
            SaveVictimDetails();

            //RESET TEXT VALUES
            ResetTextValues();

        }

        //resets the text values of all controls on form
        private void ResetTextValues()
        {
            items_lost_textbox.Text                           = "";
            name_text_box.Text                                = "";
            gender_comoboBox.SelectedIndex                    = 0;
            is_a_student_comboBox.SelectedIndex               = 0;
            label1.Text                                       = "";
            label1.Visible                                    = false;
            
        }

        private void VictimsDetailsForm_Load(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (progressBar.Value < progressBar.Maximum)
            {

                //display loading progress bar
                progressBar.Visible                           = true;
                progressBar.Value++;
                label1.Visible                                = true;
                label1.Text                                   = "Saving.Please Wait...";
            }
            else
            {

                //log user in or display error msg
                DisplayResultsOfSaving();

                //disable timer
                timer1.Stop();
                timer1.Enabled                                = false;

            }
        }

        private void DisplayResultsOfSaving()
        {
            //DISABLE SOME STUFF
            button_save.Enabled = true;
            button_another_victim.Enabled = true;
            button_another_crime.Enabled = true;
            progressBar.Visible                               = false;
            progressBar.Value                                 = 0;
            label1.Visible                                    = true;

            //display results of operations
            label1.ForeColor                                  = Color.Green;
            label1.Text                                       = "Enter Details Of the other Victim";

            //if details were saved
            if (close_after_saving) 
            {
                if (PerpetratorDetailsForm.another_crime) 
                {
                    this.Close();
                    return;
                }
               
                if (perpetrator.is_a_student)
                {
                    
                    //DISABLE MAIN WINDOW CONTROLS
                    Singleton.MAIN_WINDOW.DisableReviewControls();
                   
                    //START FACE RECOGNITION  OFF THE GUI THREAD
                    ThreadPool.QueueUserWorkItem(StartFaceRecognition);   
                }


                //close this form
                this.Close();
                
            }

        }

        public void StartFaceRecognition(Object state) 
        {
            Debug.WriteLine("STARTING FACE RECOGNITION FOR FACE");
            FaceRecognitionThread face_recognizer = new StudentRecognitionThread(perpetrator.faces);
            face_recognizer.StartWorking();
        }

        private void button_another_crime_Click(object sender, EventArgs e)
        {
            //START TIMER
            timer1.Start();

            //DO NOT CLOSE FORM AFTER SAVING
            close_after_saving = true;

            PerpetratorDetailsForm.another_crime = true;

            //SAVE THE DETAILS OF THE VICTIM
            SaveVictimDetails();

        }








    }
}
