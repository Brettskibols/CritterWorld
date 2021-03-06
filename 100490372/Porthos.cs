﻿using CritterController;
using System;
using System.Drawing;
using System.IO;

namespace _100490372
{
    public class Porthos : ICritterController
    {
        Point goal = new Point(-1, -1);
        System.Timers.Timer getInfoTimer;
        bool headingForGoal = false;

        public string Name { get; set; }

        public Send Responder { get; set; }

        public Send Logger { get; set; }

        public string Filepath { get; set; }

        public int EatSpeed { get; set; } = 5;

        public int HeadForExitSpeed { get; set; } = 5;

        public int ScoreCheck { get; set; } = 0;

        public bool isHungryBoy = false;

        public string HealthCheck { get; set; } = "";

        private static Point PointFrom(string coordinate)
        {
            string[] coordinateParts = coordinate.Substring(1, coordinate.Length - 2).Split(',');
            string rawX = coordinateParts[0].Substring(2);
            string rawY = coordinateParts[1].Substring(2);
            int x = int.Parse(rawX);
            int y = int.Parse(rawY);
            return new Point(x, y);
        }

        private void Log(string message)
        {
            if (Logger == null)
            {
                Console.WriteLine(message);
            }
            else
            {
                Logger(message);
            }
        }

        //Sets Destination for critter method
        private void SetDestination(Point coordinate, int speed)
        {
            Responder("SET_DESTINATION:" + coordinate.X + ":" + coordinate.Y + ":" + speed);
        }

        //level time remaining method for counting tick of time
        private void Tick()
        {
            Responder("GET_LEVEL_TIME_REMAINING:1");
        }

        //loads settings from config at runtime
        private void LoadSettings()
        {
            string fileName = "Porthos.cfg";
            string fileSpec = Filepath + "/" + fileName;
            try
            {
                using (StreamReader reader = new StreamReader(fileSpec))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] lineParts = line.Split('=');
                        switch (lineParts[0])
                        {
                            case "EatSpeed":
                                EatSpeed = int.Parse(lineParts[1]);
                                break;
                            case "HeadForExitSpeed":
                                HeadForExitSpeed = int.Parse(lineParts[1]);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log("Reading configuration " + fileSpec + " failed due to " + e);
            }
        }

        //Saves settings after changed at runtime for future rounds
        public void SaveSettings()
        {
            string fileName = "Porthos.cfg";
            string fileSpec = Filepath + "/" + fileName;
            try
            {
                using (StreamWriter writer = new StreamWriter(fileSpec, false)) {
                    writer.WriteLine("EatSpeed=" + EatSpeed);
                    writer.WriteLine("HeadForExitSpeed=" + HeadForExitSpeed);
                }
            }
            catch (Exception e)
            {
                Log("Writing configuration " + fileSpec + " failed due to " + e);
            }
        }

        public Porthos(string name)
        {
            Name = name;
        }

        //Launches UI from cog icon in game UI
        public void LaunchUI()
        {
            PorthosSettings settings = new PorthosSettings(this);
            settings.Show();
            settings.Focus();
        }

        //Messages sent by the enviroment to the critter ((MESSAGES RECEIVED FROM THE CRITTERWORLD ENVIROMENT IN READING PAGE 8
        public void Receive(string message)
        {
            Log("Message from body for " + Name + ": " + message);
            string[] msgParts = message.Split(':');
            string notification = msgParts[0];
            switch (notification)
            {
                case "LAUNCH":
                    LoadSettings();
                    headingForGoal = false;
                    Responder("STOP");
                    Responder("SCAN:1");
                    getInfoTimer = new System.Timers.Timer();
                    getInfoTimer.Interval = 2000;
                    getInfoTimer.Elapsed += (obj, evt) => Tick();
                    getInfoTimer.Start();
                    break;
                case "SHUTDOWN":
                    getInfoTimer.Stop();
                    break;
                case "SCAN":
                    Scan(message);
                    break;
                case "REACHED_DESTINATION":
                case "FIGHT":
                case "BUMP":
                    Responder("RANDOM_DESTINATION");
                    break;
                case "SEE":
                    See(message);
                    break;
                case "LEVEL_TIME_REMAINING":
                    int secondsRemaining = int.Parse(msgParts[2]);
                    if (secondsRemaining < 30 && ScoreCheck >= 60) //Porthos will be the greediest of the muskeeters . . . critters
                    {
                        Log("Now heading for goal.");
                        headingForGoal = true;                     //Porthos will only head for goal after 30 secs remaining and 60 points scored or more.
                        SetDestination(goal, HeadForExitSpeed);
                    }
                    break;
                case "SCORED":
                    ScoreCheck++;  //Updates public int ScoreCheck with the actual score so we can use it to base behavior off of, actions after 5 points etc!!!
                    break;
                case "ERROR":
                    Log(message);
                    break;
                case "GET_ENERGY":
                    int energyScore = int.Parse(msgParts[2]);  //Splits received healthscore into value, rather than request/value/string (strong weak etc)
                    if (energyScore < 99)     //Porthos is a chuckey critter and will therefore constantly feed when able
                    {
                        isHungryBoy = true;  //toggles if critter is hungry, if its true toggles permission to set destination to food later in code.
                    }
                    else
                        isHungryBoy = false;
                    break;
            }
        }


        //Critters actual view range - VERY SHORT RANGE
        private void See(string message)
        {
            string[] newlinePartition = message.Split('\n');
            string[] whatISee = newlinePartition[1].Split('\t');
            foreach (string thing in whatISee)
            {
                string[] thingAttributes = thing.Split(':');
                if (thingAttributes[0] == "Nothing")
                {
                    Log("I see nothing. Maybe aim for the escape hatch.");
                    if (headingForGoal && goal != new Point(-1, -1))
                    {
                        SetDestination(goal, HeadForExitSpeed);
                    }
                }
                else
                {
                    Point location = PointFrom(thingAttributes[1]);
                    switch (thingAttributes[0])
                    {
                        case "Food":
                            Log("Food is at " + location);
                            if (isHungryBoy == true)    //Critter should only knowingly go after food when hungry and bool set to true by healthscore parameter. 
                                SetDestination(location, 10);  //Porthos will persue food at fastest speed available in enviroment. 
                            break;
                        case "Gift":
                            Log("Gift is at " + location);
                            SetDestination(location, 10); //Porthos is extremely greedy for gifts and will race to collect them at any oppertunity
                            break;
                        case "Bomb":
                            Log("Bomb is at " + location);
                            Responder("RANDOM_DESTINATION");
                            break;
                        case "EscapeHatch":
                            if (headingForGoal)
                            {
                                SetDestination(location, HeadForExitSpeed);
                            }
                            Log("EscapeHatch is at " + location);
                            break;
                        case "Terrain":
                            Log("Terrain is at " + location);
                            Responder("RANDOM_DESTINATION");
                            break;
                        case "Critter":
                            int critterNumber = int.Parse(thingAttributes[2]);
                            string nameAndAuthor = thingAttributes[3];
                            string strength = thingAttributes[4];
                            bool isDead = thingAttributes[5] == "Dead";
                            Log("Critter at " + location + " is #" + critterNumber + " who is " + nameAndAuthor + " with strength " + strength + " is " + (isDead ? "dead" : "alive"));
                            if (nameAndAuthor != "Aramis by Brett Jones 100490372" || nameAndAuthor != "Dartagnan by Brett Jones 100490372"
                                || nameAndAuthor != "Porthos by Brett Jones 100490372")
                            {
                                if (strength == "Weak" && !isDead)
                                {
                                    SetDestination(location, 10);
                                }
                                else if (strength == "Adequate" && !isDead)
                                {
                                    SetDestination(location, 10);
                                }

                                else if (strength == "Ok" && !isDead)  //Porthos is the strongest critter with the most energy and will therefore fight everyone.
                                {                                      //But not someone with nearly max health
                                    SetDestination(location, 10);
                                }
                            }
                            break;
                            //problem in the fact that contact with critters changing critters direction affects pathfinding badly, remove? or limit?
                    }
                }
            }
        }

        //Scan method, makes critter scan enviroment short and long range
        private void Scan(string message)
        {
            string[] newlinePartition = message.Split('\n');
            string[] whatISense = newlinePartition[1].Split('\t');
            foreach (string thing in whatISense)
            {
                string[] thingAttributes = thing.Split(':');
                Point location = PointFrom(thingAttributes[1]);
                switch (thingAttributes[0])
                {
                    case "EscapeHatch":
                        Log("Escape hatch is at " + location);
                        goal = location;
                        SetDestination(location, 5);
                        break;
                }
            }
        }

    }
}
