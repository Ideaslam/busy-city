using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Maps;
using Google.Apis;

namespace busy_city
{
    static class Program
    {
        /// <summary>
        //
        /// The main entry point for the application.
        /// </summary>
        /// 

        // Key = "AIzaSyBX44diSFIMl07UUFO_dY33DOW6JyWGFQU";
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            List<Citizen> Citis = new List<Citizen>();
            List<Scooter> Scoots = new List<Scooter>();
            //area of interest, rad.10km, square
            city.max_y = 0.008983 * 5 + 13.3813604;
            city.min_y = 13.3813604 - 0.008983 * 5;
            city.max_x = 0.008983 * 5 + 52.4984103;
            city.min_x = 52.4984103 - 0.008983 * 5;
            //1degree=111.196672km
            //10km=0.08983degree
            //creating 10 citizens
            Random rand = new Random();
            //initialising citizens
            System.IO.StreamWriter file = new System.IO.StreamWriter("C:\\Users\\dashersw\\Desktop\\positions1.txt");

            for (int i = 0; i < 10; i++)
            {


                Citizen Cit = new Citizen();
                Cit.pos_x = rand.NextDouble() * (city.max_x - city.min_x) + city.min_x;
                //Citis[i] = new Citizen();

                Cit.pos_y = rand.NextDouble() * (city.max_y - city.min_y) + city.min_y;

                Cit.dest_x = rand.NextDouble() * (city.max_x - city.min_x) + city.min_x;

                Cit.dest_y = rand.NextDouble() * (city.max_y - city.min_y) + city.min_y;
                Cit.status = "walking";
                Cit.timeToWait = 0;
                Cit.velocity = 0.001559;//10m jump per iteration

                Citis.Add(Cit);

                //Citis.Insert(1, Cit);

            }
            //initialising scooters

            for (int i = 0; i < 20; i++)
            {

                Scooter Sco = new Scooter();
                Sco.pos_x = rand.NextDouble() * (city.max_x - city.min_x) + city.min_x;

                Sco.pos_y = rand.NextDouble() * (city.max_y - city.min_y) + city.min_y;
                Sco.idle = true;
                Sco.velocity = 0;
                Scoots.Add(Sco);

                file.WriteLine("{ \"type\": \"scooter\", \"name\": \"Scooter " + i + "\", \"pos_x\": " + Sco.pos_x + ", \"pos_y\": " + Sco.pos_y + "}");
            }

            string Orig;
            string dest;
            double x_step;
            double y_step;
            double diff_x;
            double diff_y;
            
            double jump;
            
            double ScootsDist = 90000000000000;
            int closestScootInd = 55;
            string export;
            var g = new Coding();
            //!!!!!!!!!!!!paste your own Google Api Key here: 
            g.Key = "AIzaSyAe5AUU7KMt30oUalLL22tO4uVUQvUZiAk";
            
            //while (true)
            for (int kj=0;kj<20000;kj++)
            {
                for (int i = 0; i < 10; i++)
                {

                    if (Citis[i].timeToWait == 0)
                    {
                        Orig = Convert.ToString(Citis[i].pos_x) + "," + Convert.ToString(Citis[i].pos_y);

                        //finding closest scooter
                        if (Citis[i].status == "walking")
                        {
                            //1st case scenario
                            //finding route
                            if (Citis[i].stepsToDir == null)
                            {
                                closestScootInd = 0;
                                for (int j = 0; j < 20; j++)
                                {
                                    if (Scoots[j].idle && (Math.Pow(Citis[i].pos_x - Scoots[j].pos_x, 2) + Math.Pow(Citis[i].pos_y - Scoots[j].pos_y, 2) < ScootsDist))
                                        closestScootInd = j;

                                    ScootsDist = (Math.Pow(Citis[i].pos_x - Scoots[j].pos_x, 2) + Math.Pow(Citis[i].pos_y - Scoots[j].pos_y, 2));
                                }
                                Citis[i].CitsScooter = closestScootInd;
                                dest = Convert.ToString(Scoots[Citis[i].CitsScooter].pos_x) + "," + Convert.ToString(Scoots[Citis[i].CitsScooter].pos_y);
                                var r_cit = g.DirInfo(new DirParameters { Origin = Orig, Destination = dest, Mode = Citis[i].status, Units = "metric", Avoid = "highways" });
                                Citis[i].stepCount = r_cit.routes[0].Legs[0].Steps.Length;
                                Citis[i].stepsToDir = new Location[Citis[i].stepCount];
                                for (int j1 = 0; j1 < r_cit.routes[0].Legs[0].Steps.Length; j1++)
                                {
                                    Citis[i].stepsToDir[j1] = r_cit.routes[0].Legs[0].Steps[j1].EndLocation;
                                }
                                //making 1st step
                                x_step = Citis[i].stepsToDir[0].Lat;
                                y_step = Citis[i].stepsToDir[0].Long;
                                diff_x = x_step- Citis[i].pos_x;
                                diff_y = y_step - Citis[i].pos_y;

                                jump = Citis[i].velocity / Math.Pow((Math.Pow(diff_x, 2) + Math.Pow(diff_y, 2)), (0.5));
                                if (jump > 1) jump = 1;
                                Citis[i].pos_x += diff_x * jump;
                                Citis[i].pos_y += diff_y * jump;
                                //if 1st step made:
                                if (Math.Abs(Citis[i].pos_x - Citis[i].stepsToDir[0].Lat) < 0.00001 && Math.Abs(Citis[i].pos_y - Citis[i].stepsToDir[0].Long) < 0.00001)
                                {
                                    Citis[i].stepInd = 1;
                                }

                            }
                            //2nd,3rd and 4th cases scenarios
                            //if route known
                            else
                            {
                                //if at dest (scoot's pos)
                                if (Citis[i].stepInd >= Citis[i].stepCount)
                                {
                                    //4th case scenario
                                    if (Scoots[Citis[i].CitsScooter].idle)
                                    {
                                        //start a scooter
                                        Citis[i].status = "driving";
                                        Citis[i].velocity = 0.0076;
                                        //Citis[i].CitsScooter = closestScootInd;
                                        Scoots[Citis[i].CitsScooter].idle = false;
                                        Citis[i].stepsToDir = null;
                                        Citis[i].stepInd = 0;
                                    }
                                    //3rd case scenario
                                    //find another scooter
                                    else
                                    {
                                        Citis[i].stepsToDir = null;
                                    }
                                }
                                //2nd case scenario
                                else
                                {
                                    x_step = Citis[i].stepsToDir[Citis[i].stepInd].Lat;
                                    y_step = Citis[i].stepsToDir[Citis[i].stepInd].Long;
                                    diff_x =  x_step- Citis[i].pos_x;
                                    diff_y = y_step-Citis[i].pos_y ;

                                    jump = Citis[i].velocity / Math.Pow((Math.Pow(diff_x, 2) + Math.Pow(diff_y, 2)), (0.5));
                                    if (jump > 1) 
                                    jump = 1;
                                    Citis[i].pos_x += diff_x * jump;
                                    Citis[i].pos_y += diff_y * jump;
                                    //if step done:
                                    if (Math.Abs(Citis[i].pos_x - Citis[i].stepsToDir[Citis[i].stepInd].Lat) < 0.0001 && Math.Abs(Citis[i].pos_y - Citis[i].stepsToDir[Citis[i].stepInd].Long) < 0.0001)
                                    {
                                        Citis[i].stepInd++;
                                    }


                                }

                            }

                            //starting a scooter
                            /*if (Math.Abs(Citis[i].pos_x- Scoots[closestScootInd].pos_x)<0.001 && Math.Abs(Citis[i].pos_y - Scoots[closestScootInd].pos_y)<0.001 && closestScootInd != 55)
                            {
                                Citis[i].status = "driving";
                                Citis[i].velocity = 0.0016;
                                Citis[i].CitsScooter = closestScootInd;
                                Scoots[Citis[i].CitsScooter].idle = false;
                            }*/
                        }
                        //when driving
                        if (Citis[i].status == "driving")
                        {
                            dest = Convert.ToString(Citis[i].dest_x) + "," + Convert.ToString(Citis[i].dest_y);
                            
                            //5th case scenario
                            if (Citis[i].stepsToDir == null)
                            {
                                var r_cit = g.DirInfo(new DirParameters { Origin = Orig, Destination = dest, Mode = Citis[i].status, Units = "metric", Avoid = "highways" });
                                Citis[i].stepCount = r_cit.routes[0].Legs[0].Steps.Length;
                                Citis[i].stepsToDir = new Location[Citis[i].stepCount];
                                for (int j1 = 0; j1 < r_cit.routes[0].Legs[0].Steps.Length; j1++)
                                {
                                    Citis[i].stepsToDir[j1] = r_cit.routes[0].Legs[0].Steps[j1].EndLocation;
                                }
                                //making 1st step
                                x_step = Citis[i].stepsToDir[0].Lat;
                                y_step = Citis[i].stepsToDir[0].Long;
                                diff_x = Citis[i].pos_x - x_step;
                                diff_y = Citis[i].pos_y - y_step;

                                jump = Citis[i].velocity / Math.Pow((Math.Pow(diff_x, 2) + Math.Pow(diff_y, 2)), (0.5));
                                if (jump > 1) jump = 1;
                                Citis[i].pos_x += diff_x * jump;
                                Citis[i].pos_y += diff_y * jump;
                                Scoots[Citis[i].CitsScooter].pos_x = Citis[i].pos_x;
                                Scoots[Citis[i].CitsScooter].pos_y = Citis[i].pos_y;
                                //if 1st step made:
                                if (Math.Abs(Citis[i].pos_x - Citis[i].stepsToDir[0].Lat) < 0.00001 && Math.Abs(Citis[i].pos_y - Citis[i].stepsToDir[0].Long) < 0.00001)
                                {
                                    Citis[i].stepInd = 1;
                                }
                            }
                            //6th and 7th case scenarios
                            else
                            {
                                //7th case scenario
                                if (Citis[i].stepInd == Citis[i].stepCount)
                                {

                                    Scoots[Citis[i].CitsScooter].idle = true;
                                    Citis[i].timeToWait = 100;
                                    Citis[i].stepInd = 0;
                                    Citis[i].stepsToDir = null;
                                    Citis[i].dest_x = rand.NextDouble() * (city.max_x - city.min_x) + city.min_x;
                                    Citis[i].dest_y = rand.NextDouble() * (city.max_y - city.min_y) + city.min_y;
                                    Citis[i].status = "walking";
                                }
                                //6th case scenario
                                else
                                {
                                    x_step = Citis[i].stepsToDir[Citis[i].stepInd].Lat;
                                    y_step = Citis[i].stepsToDir[Citis[i].stepInd].Long;
                                    diff_x = Citis[i].pos_x - x_step;
                                    diff_y = Citis[i].pos_y - y_step;

                                    jump = Citis[i].velocity / Math.Pow((Math.Pow(diff_x, 2) + Math.Pow(diff_y, 2)), (0.5));
                                    if (jump > 1) jump = 1;
                                    Citis[i].pos_x += diff_x * jump;
                                    Citis[i].pos_y += diff_y * jump;
                                    Scoots[Citis[i].CitsScooter].pos_x = Citis[i].pos_x;
                                    Scoots[Citis[i].CitsScooter].pos_y = Citis[i].pos_y;

                                    //if step done:
                                    if (Math.Abs(Citis[i].pos_x - Citis[i].stepsToDir[0].Lat) < 0.0001 && Math.Abs(Citis[i].pos_y - Citis[i].stepsToDir[0].Long) < 0.0001)
                                    {
                                        Citis[i].stepInd++;
                                    }
                                }
                                
                            }
                        }
                    }


                    else
                    {
                        Citis[i].timeToWait = Citis[i].timeToWait - 1;
                    }
                    export = "{ \"iteration\": " + kj + ", \"type\": \"citizen\", \"name\": \"Citizen " + i + "\", \"pos_x\": " + Citis[i].pos_x + ", \"pos_y\": " + Citis[i].pos_y + ", \"stepIndex\": " + Citis[i].stepInd+ ", \"status\": \"" + Citis[i].status + "\", \"scooterId\": " + Citis[i].CitsScooter + " }";

                    file.WriteLine(export);

                    var sc = Scoots[Citis[i].CitsScooter];
                    export = "{ \"iteration\": " + kj + ", \"type\": \"scooter\", \"name\": \"Scooter " + Citis[i].CitsScooter + "\", \"pos_x\": " + sc.pos_x + ", \"pos_y\": " + sc.pos_y + "}";
                    file.WriteLine(export);
                }

            }
            file.Close();
        }
        


        }
}

