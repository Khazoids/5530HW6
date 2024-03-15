using Microsoft.Maui.Controls;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
/*
Author: Daniel Kopta and ...
Chess browser backend 
*/

namespace ChessBrowser
{
    internal class Queries
    {

        /// <summary>
        /// This function runs when the upload button is pressed.
        /// Given a filename, parses the PGN file, and uploads
        /// each chess game to the user's database.
        /// </summary>
        /// <param name="PGNfilename">The path to the PGN file</param>
        internal static async Task InsertGameData(string PGNfilename, MainPage mainPage)
        {
            // This will build a connection string to your user's database on atr,
            // assuimg you've typed a user and password in the GUI
            string connection = mainPage.GetConnectionString();


            // Load PGN file into string array
            string[] data = File.ReadAllLines(PGNfilename);
           
            // used to update the loading bar in the UI
            mainPage.SetNumWorkItems(games);

            int blankCounter = 0;

            
            using (MySqlConnection conn = new MySqlConnection(connection))
            {

                try
                {
                    // Open a connection
                    conn.Open();
                    
                    // Create prepared statement
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "INSERT INTO Players (Elo, Name) VALUES (@val1, @val2) ON DUPLICATE KEY UPDATE Elo = IF(@val1 > Elo, @val1, Elo);" +
                       "INSERT INTO Players (Elo, Name) VALUES (@val3, @val4) ON DUPLICATE KEY UPDATE Elo = IF(@val3 > Elo, @val3, Elo);" +
                       "INSERT IGNORE INTO Events (Name, Site, Date) VALUES (@val5, @val6, @val7);" +
                       "INSERT INTO Games (Round, Result, Moves, BlackPlayer, WhitePlayer, eID) VALUES (@val8, @val9, @val10, (SELECT pID FROM Players WHERE Name = @val4), (SELECT pID FROM Players WHERE Name = @val2), (SELECT eID FROM Events WHERE Name=@val5 AND Date=DATE(@val7) AND Site=@val6));";

                    // Initialize prepared statement with placeholder values;
                    for (int i = 0; i <= 10; i++)
                    {
                        command.Parameters.AddWithValue(
                            "@val" + i, "");
                    }
                    command.Prepare();

                    string moves = "";
                    foreach (string line in data)
                    {   
                        // if we encounter a blank line, increment and continue
                        if(line == "")
                        {
                            blankCounter++;
                            continue;
                        }

                        // After the first blank, we are iterating over the moves
                        if(blankCounter == 1)
                        {
                            moves += line; 
                            continue;
                        }

                        if (blankCounter == 2)
                        {
                            command.Parameters["@val10"].Value = moves;  // If it's just the moves, we can set the parameter without any additional parsing
                           
                            command.ExecuteNonQuery();  
                            await mainPage.NotifyWorkItemCompleted();   // notify loading bar on UI that a game has been added

                            // reset counters for next game
                            blankCounter = 0;
                            moves = "";
                        }

                        if (line.StartsWith('['))    // Check if we're evaluating a new tag(row)
                        {
                            string[] tagAndValue = line.Substring(1, line.Length - 2).Split(" ", 2); // Get rid of the brackets from the string and split the tag from the actual string value. Ex. [ Event "Chess Tournament ] -> "Event", "Chess Tournament"
                            string tag = tagAndValue[0];
                            string value = tagAndValue[1];

                            int parameterNumber = getColumnNum(tag);


                            if(parameterNumber == 1 || parameterNumber == 3)    // If the tag is WhiteElo or BlackElo, then we need to convert to an int
                            { 
                                command.Parameters["@val" + parameterNumber].Value = int.Parse(value.Substring(1, value.Length - 2));
                            }
                            else if(parameterNumber == 9)   // If the tag is Result, then we need to format it into 'W', 'B', or 'D' depending on who won
                            {
                                char result;
                                if (value.Equals("1-0"))
                                {
                                    result = 'W';
                                }
                                else if (value.Equals("0-1"))
                                {
                                    result = 'B';
                                }
                                else
                                {
                                    result = 'D';
                                }

                                command.Parameters["@val" + parameterNumber].Value = result;

                            }
                            else if (parameterNumber == 7)
                            {
                                if (line.Contains('?'))
                                {
                                    command.Parameters["@val" + parameterNumber].Value = "0000-00-00";
                                } 
                                else
                                { 
                                    command.Parameters["@val" + parameterNumber].Value = value.Substring(1, value.Length - 2).Replace('.', '-');   // Get the @val we want to update by passing the tag into getColumnNum. Then set its 
                                }
                            }
                            else 
                            {
                                command.Parameters["@val" + parameterNumber].Value = value;   // Get the @val we want to update by passing the tag into getColumnNum. Then set its value
                            }
                            
                            
                        }
                        
                    }

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

        }


        /// <summary>
        /// Queries the database for games that match all the given filters.
        /// The filters are taken from the various controls in the GUI.
        /// </summary>
        /// <param name="white">The white player, or null if none</param>
        /// <param name="black">The black player, or null if none</param>
        /// <param name="opening">The first move, e.g. "1.e4", or null if none</param>
        /// <param name="winner">The winner as "W", "B", "D", or null if none</param>
        /// <param name="useDate">True if the filter includes a date range, False otherwise</param>
        /// <param name="start">The start of the date range</param>
        /// <param name="end">The end of the date range</param>
        /// <param name="showMoves">True if the returned data should include the PGN moves</param>
        /// <returns>A string separated by newlines containing the filtered games</returns>
        internal static string PerformQuery(string white, string black, string opening,
          string winner, bool useDate, DateTime start, DateTime end, bool showMoves,
          MainPage mainPage)
        {
            // This will build a connection string to your user's database on atr,
            // assuimg you've typed a user and password in the GUI
            string connection = mainPage.GetConnectionString();

            // Build up this string containing the results from your query
            string parsedResult = "";

            // Use this to count the number of rows returned by your query
            // (see below return statement)
            int numRows = 0;
            
       
            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {

                    // Open a connection
                    conn.Open();

                    // TODO:
                    //       Generate and execute an SQL command,
                    //       then parse the results into an appropriate string and return it.
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

            return numRows + " results\n" + parsedResult;
        }


        private static int getColumnNum(string tag)
        {
            switch(tag)
            {
                case "Event":
                    return 5;
                case "Site":
                    return 6;
                case "Round":
                    return 8;
                case "White":
                    return 2;
                case "Black":
                    return 4;
                case "Result":
                    return 9;
                case "WhiteElo":
                    return 1;
                case "BlackElo":
                    return 3;
                case "EventDate":
                    return 7;
                default:
                    return 0;
            }
        }

 
    } 
   
}

