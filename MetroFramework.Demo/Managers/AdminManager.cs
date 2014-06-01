﻿using MetroFramework.Demo.Entitities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MetroFramework.Demo.Managers
{
    public class AdminManager : Manager
    {
        public const String TABLE_NAME  = "ADMINISTRATORS";
        public const int USERNAME       = 1;
        public const int PASSWORD       = 2;
        public const int TYPE           = 3;

        public static void CreateTable()
        {
            try
            {
                String create_sql       = "CREATE TABLE IF NOT EXISTS " + TABLE_NAME + " (ID INT AUTO_INCREMENT PRIMARY KEY,USERNAME VARCHAR(30),PASSWORD VARCHAR(30),USERTYPE VARCHAR(10) )";
                sql_command             = new MySqlCommand();
                sql_command.Connection  = (MySqlConnection)database.OpenConnection();
                sql_command.CommandText = create_sql;
                sql_command.Prepare();
                database.Update(sql_command);
            }
            finally 
            {
                database.CloseConnection();
            }
        }

        public static void DropTable()
        {
            try
            { 
            String drop_sql             = "DROP TABLE IF EXISTS "+TABLE_NAME;
            sql_command                 = new MySqlCommand();
            sql_command.Connection      = (MySqlConnection)database.OpenConnection();
            sql_command.CommandText     = drop_sql;
            sql_command.Prepare();
            database.Update(sql_command);
            }
            finally
            {
                database.CloseConnection();
            }
        }

        public static void PopulateTable()
        {

        }
        public static Admin GetAdmin(String username, String password)
        {
            //resultant object
            Admin admin                 = null;

            try
            {
                //sql
                String select_sql       = "SELECT * FROM " + TABLE_NAME + " WHERE USERNAME=@username AND PASSWORD=@password";

                //Sql command
                sql_command             = new MySqlCommand();
                sql_command.Connection  = (MySqlConnection)database.OpenConnection();
                sql_command.CommandText = select_sql;
                
                sql_command.Parameters.AddWithValue("@username", username);
                sql_command.Parameters.AddWithValue("@password", password);
                sql_command.Prepare();
                
                //execute command
                data_reader             = database.Select(sql_command);

                //while there are results
                if (data_reader.Read())
                {
                    //create object
                    String type         = data_reader.GetString(TYPE);
                    admin               = new Admin(username, password, type);
                }
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                //close reader
                data_reader.Close();
                database.CloseConnection();
            }
            return admin;
        }


        public static bool Exists(String username)
        {
         
            try
            {
                //sql
                String select_sql       = "SELECT * FROM " + TABLE_NAME + " WHERE USERNAME=@username";

                //Sql command
                sql_command             = new MySqlCommand();
                sql_command.Connection  = (MySqlConnection)database.OpenConnection();
                sql_command.CommandText = select_sql;

                sql_command.Parameters.AddWithValue("@username", username);
                sql_command.Prepare();

                //execute command
                data_reader             = database.Select(sql_command);

                //while there are results
                if (data_reader.Read())
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                //close reader
                data_reader.Close();
                database.CloseConnection();
            }
            return false;
        }

        public static Admin[] GetAllAdmins()
        {
            //resultant object
            List<Admin> admins          = new List<Admin>();

            try
            {
                //sql
                String select_sql       = "SELECT * FROM " + TABLE_NAME;

                //Sql command
                sql_command             = new MySqlCommand();
                sql_command.CommandText = select_sql;
                sql_command.Prepare();

                //execute command
                data_reader             = database.Select(sql_command);


                //while there are results
                if (data_reader.Read())
                {
                    //create object
                    String username     = data_reader.GetString(USERNAME);
                    String password     = data_reader.GetString(PASSWORD);
                    String type         = data_reader.GetString(TYPE);
                    Admin admin         = new Admin(username, password, type);
                    admins.Add(admin);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                //close reader
                data_reader.Close();
                database.CloseConnection();
            }
            return admins.ToArray();
        }

        public static bool Save(Admin admin)
        {
            try 
            { 
           
                String insert_sql       = "INSERT INTO "+TABLE_NAME+" (USERNAME,PASSWORD,USERTYPE) values(@username,@password,@usertype)";
                                                                                                   
                //Sql command
                sql_command             = new MySqlCommand();
                sql_command.Connection  = (MySqlConnection)database.OpenConnection();
                sql_command.CommandText = insert_sql;
                sql_command.Parameters.AddWithValue("@username", admin.user_name);
                sql_command.Parameters.AddWithValue("@password", admin.password);
                sql_command.Parameters.AddWithValue("@usertype",admin.user_type);
                sql_command.Prepare();

                database.Insert(sql_command);

                return true;
            }
            finally
            {
                database.CloseConnection();
            }
            
        }

        public static bool Delete(Admin admin)
        {
            throw new NotImplementedException();
        }

        public static bool Update(Admin admin)
        {
            try
            {
                String update_sql       = "UPDATE " + TABLE_NAME + " SET USERNAME=@username ,PASSWORD=@password,USERTYPE=@type WHERE ID=@id";

                //Sql command
                sql_command             = new MySqlCommand();
                sql_command.CommandText = update_sql;

                sql_command.Parameters.AddWithValue("@id", admin.id);
                sql_command.Parameters.AddWithValue("@username", admin.user_name);
                sql_command.Parameters.AddWithValue("@password", admin.password);
                sql_command.Parameters.AddWithValue("@type", admin.user_type);

                sql_command.Prepare();

                //execute command
                database.Update(sql_command);
                return true;
            }
            finally
            {
                database.CloseConnection();
            }
        }
    }
}
