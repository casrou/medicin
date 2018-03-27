using Ical.Net;
using Ical.Net.CalendarComponents;
using MedicinSkema.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace MedicinSkema.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int? s, int? h)
        {            
            if(s != null & h != null)
            {
                // months
                int m = 1;

                ViewBag.Semester = s;
                ViewBag.Hold = h;
                string ical = "";
                using (WebClient webClient = new WebClient())
                {
                    using (Stream stream = webClient.OpenRead("http://jhvidt.dk/pyskema/H0" + s + "semHold" + h + ".ics"))
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            ical = sr.ReadToEnd();
                        }
                    }
                }
                //FileStream fs = new FileStream(@"C:\Users\caspe\OneDrive - Aarhus universitet\Projekter\MedicinSkema\MedicinSkema\H01semHold3.ics", FileMode.Open, FileAccess.Read);
                //StreamReader sr = new StreamReader(fs);
                //string ical = sr.ReadToEnd();
                //sr.Close();
                var calendar = Calendar.Load(ical);
                var events = calendar.GetOccurrences(DateTime.Now.AddDays(-5), DateTime.Now.AddMonths(m))
                    .Select(o => o.Source)
                    .Cast<CalendarEvent>()
                    .Distinct()
                    .OrderBy(o => o.DtStart)
                    .ToList();

                return View(events);
            }  else
            {
                return View();
            }          
        }

        public static string correctTime(/*Ical.Net.DataTypes.IDateTime*/ DateTime dateTime)
        {
            string output = oneDigit(dateTime.Year) + "-" + oneDigit(dateTime.Month) + "-" + oneDigit(dateTime.Day) + "T" + oneDigit(dateTime.Hour) + ":" + oneDigit(dateTime.Minute) + ":" + oneDigit(dateTime.Second);
            //string input = dateTime.ToString();
            //string[] temp = input.Split(' ');
            //string output = temp[0] + "T" + temp[1];
            return output;                
        }

        public static string correctTitle(string title)
        {
            string temp = "";
            if (title.ToLower().Contains("studies"))
            {
                temp = "Studiestart og introduktion";
            } else if (title.ToLower().Contains("filoso"))
            {
                temp = "Medicinsk filosofi og videnskabsteori";
            }
            else if (title.ToLower().Contains("mikro"))
            {
                temp = "Mikroskopisk anatomi";
            }
            else if (title.ToLower().Contains("geneti"))
            {
                temp = "Medicinsk genetik";
            }
            else
            {
                temp = title;
            }

            if (title.ToLower().Contains("_ho"))
            {
                temp = temp + " - Holdtime";
            }

            if (title.ToLower().Contains("_f"))
            {
                temp = temp + " - Forelæsning";
            }
            return temp;
        }

        public static string correctColor(string title)
        {
            string temp = "";
            if (title.ToLower().Contains("studies"))
            {
                temp = "#5BC0EB";
            }
            else if (title.ToLower().Contains("filoso"))
            {
                temp = "#FDE74C";
            }
            else if (title.ToLower().Contains("mikro"))
            {
                temp = "#9BC53D";
            }
            else if (title.ToLower().Contains("geneti"))
            {
                temp = "#FA7921";
            }
            else
            {
                temp = title;
            }

            if (title.ToLower().Contains("_ho"))
            {
                temp = temp + " - Holdtime";
            }

            if (title.ToLower().Contains("_f"))
            {
                temp = temp + " - Forelæsning";
            }
            return temp;
        }

        public static string[] correctLocation(string loc)
        {
            string temp = loc;
            string[] result = temp.Split('-');
            return result;
        }

        public static string oneDigit(int s)
        {
            string temp = s.ToString();
            if(temp.Count() == 1)
            {
                temp = "0" + temp;
            }
            return temp;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Update()
        {
            //List<RealClass> all = new List<RealClass>();
            //foreach (Team t in getTeams())
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        var temp = getClasses(t.id, DateTime.Now.AddDays(i * 7));
            //        all.AddRange(temp);
            //    }
            //}

            var path = Server.MapPath("~/skema/");
            getICS(path);
            return View();
        }

        public static void getICS(string path)
        {
            IList<Team> teams = getTeams();
            var date = DateTime.Now.Date;

            RestClient client = new RestClient("https://webuntis.dk");            

            RestRequest req1 = new RestRequest("WebUntis/Ical.do");
            req1.Method = Method.GET;
            req1.AddParameter("elemType", 1);
            req1.AddCookie("schoolname", "_YXVfaGVhbHRo");
            
            foreach(Team t in teams.ToList().GetRange(8,10))
            {                
                //string content = "";
                StreamWriter sw = new StreamWriter(path + t.name + ".ics", false);

                sw.WriteLine("BEGIN:VCALENDAR");
                sw.WriteLine("PRODID:-//Casper Roursgaard Christensen//DK");
                sw.WriteLine("VERSION: 1.0");
                sw.WriteLine("CALSCALE: GREGORIAN");
                sw.WriteLine("X-WR-CALNAME:" + t.name);

                req1.AddOrUpdateParameter("elemId", t.id);
                for (int i = 0; i < 30; i++)
                {                    
                    req1.AddOrUpdateParameter("rpt_sd", date.ToString("yyyy-MM-dd"));     
                    var res = client.Execute(req1);
                    var content = res.Content;
                    content = content.Replace("BEGIN:VCALENDAR", String.Empty)
                    .Replace("PRODID:-//Ben Fortuna//iCal4j 1.0//EN", String.Empty)
                    .Replace("VERSION:2.0", String.Empty)
                    .Replace("CALSCALE:GREGORIAN", String.Empty)
                    .Replace("END:VCALENDAR", String.Empty);
                    sw.WriteLine(content);
                    //content += res.Content;
                    date.AddDays(7);
                }
                
                sw.WriteLine("END:VCALENDAR");

                //System.IO.File.WriteAllText(path + t.name + ".ics", content);
            }

            //string headervalue = res.Headers[6].Value.ToString();
            //var i1 = headervalue.IndexOf('"');
            //var i2 = headervalue.LastIndexOf('"') - i1;
            //string name = headervalue.Substring(i1 + 1, i2 - 1).Replace(" ", String.Empty);

            //string path = HostingEnvironment.MapPath("~/skema/");
            //using (StreamWriter sw = new StreamWriter(HostingEnvironment.MapPath("~/skema/" + name + ".txt"), false))
            //{
            //    sw.WriteLine("BEGIN:VCALENDAR");
            //    sw.WriteLine("PRODID:-//Casper Roursgaard Christensen//DK");
            //    sw.WriteLine("VERSION: 2.0");
            //    sw.WriteLine("CALSCALE: GREGORIAN");
            //    sw.WriteLine("X-WR-CALNAME:" + name);
            //    sw.WriteLine(events);
            //    sw.WriteLine("END:VCALENDAR");
            //}
        }

        public static IList<Team> getTeams()
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();

            RestClient client = new RestClient("https://webuntis.dk");

            RestRequest req1 = new RestRequest("WebUntis/Timetable.do");
            req1.Method = Method.POST;
            req1.AddParameter("ajaxCommand", "getPageConfig");
            req1.AddParameter("type", 1);
            req1.AddParameter("filter.departmentId", 4);
            req1.AddCookie("schoolname", "_YXVfaGVhbHRo");

            var res = client.Execute(req1);

            var con = res.Content;

            JObject jsonText = JObject.Parse(con);

            IList<JToken> results = jsonText["elements"].Children().ToList();

            IList<Team> teams = new List<Team>();

            foreach(JToken result in results)
            {
                Team t = result.ToObject<Team>();
                t.name = t.name.Replace(" ", String.Empty);
                teams.Add(t);
            }

            return teams;
        }

        public static HashSet<RealClass> getClasses(int id, DateTime dt)
        {
            RestClient client = new RestClient("https://webuntis.dk");

            RestRequest req1 = new RestRequest("WebUntis/Timetable.do");
            req1.Method = Method.POST;
            req1.AddParameter("ajaxCommand", "getWeeklyTimetable");
            req1.AddParameter("elementType", 1);
            req1.AddParameter("filter.departmentId", 4);
            req1.AddCookie("schoolname", "_YXVfaGVhbHRo");

            HashSet<RealClass> classes = new HashSet<RealClass>();

            req1.AddParameter("elementId", id); //4857
            req1.AddParameter("date", dt.Date.ToString("yyyyMMdd"));
            var con = client.Execute(req1).Content;
            JObject jsonText = JObject.Parse(con);

            IList<JToken> periodRes = jsonText["result"]["data"]["elementPeriods"][id.ToString()].Children().ToList();
            IList<JToken> elementRes = jsonText["result"]["data"]["elements"].Children().ToList();

            IList <Period> periods = new List<Period>();
            IList<Element> elements = new List<Element>();

            foreach (JToken result in periodRes)
            {
                Period p = result.ToObject<Period>();
                periods.Add(p);
            }       

            foreach (JToken result in elementRes)
            {
                Element e = result.ToObject<Element>();
                elements.Add(e);
            }            

            foreach (Period p in periods)
            {
                RealClass rc = new RealClass();
                rc.period = p;
                foreach(PeriodElement pe in p.elements)
                {
                    Element e = elements.First(el => el.id == pe.id);

                    switch (e.type)
                    {
                        case 3:
                            rc.element = e;
                            break;
                        case 4:
                            rc.location = e;
                            break;
                        default:
                            break;
                    }

                    classes.Add(rc);
                }
            }

            return classes;
        }
    }
}