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
using System.Text.RegularExpressions;
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
                int m = 1; // months

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
                // For debugging purposes only
                /*
                FileStream fs = new FileStream(@"C:\dev\medicin\MedicinSkema\H01semHold3.ics", FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string ical = sr.ReadToEnd();
                sr.Close();
                */
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

        public static string correctTime(DateTime dateTime)
        {
            string output = oneDigit(dateTime.Year) 
                + "-" + oneDigit(dateTime.Month) 
                + "-" + oneDigit(dateTime.Day) 
                + "T" + oneDigit(dateTime.Hour) 
                + ":" + oneDigit(dateTime.Minute) 
                + ":" + oneDigit(dateTime.Second);
            return output;                
        }
        public static string oneDigit(int s)
        {
            string temp = s.ToString();
            if (temp.Count() == 1)
            {
                temp = "0" + temp;
            }
            return temp;
        }

        public static string correctTitle(string title)
        {
            try
            {
                var temp = title.Substring(title.LastIndexOf('.') + 1).Trim().Split('_');
                string result = "";
                Dictionary<string, string> names = new Dictionary<string, string>();
                // 1. semester
                names.Add("GENETIK", "Medicinsk genetik");
                names.Add("MIKRO", "Mikroskopisk anatomi");
                names.Add("STUDIESTAR", "Studiestart og introduktion");
                names.Add("STUDIESTART", "Studiestart og introduktion");
                names.Add("FILOSOFI", "Medicinsk filosofi og videnskabsteori");
                // 2. semester
                names.Add("MAKRO", "Makroskopisk anatomi");
                // 3. semester
                names.Add("BIOKEMI", "Medicinsk Biokemi");
                // 4. semester
                names.Add("SUNDPSYK", "Sundhedspsykologi");
                names.Add("FYSIOLOGI", "Fysiologi");

                if (names.ContainsKey(temp[0]))
                {
                    result = names[temp[0]];
                    if (temp.Length > 1)
                    {
                        switch (temp.Last().ToUpper().First())
                        {
                            case 'F':
                                result = result + " - Forelæsning";
                                break;
                            case 'H':
                                result = result + " - Holdtime";
                                break;
                        }
                    }
                }
                else
                {
                    result = title;
                }

                return result;
            }
            catch (Exception)
            {
                return title;
            }            
        }

        public static string correctColor(string title)
        {
            try
            {
                var temp = title.Substring(title.LastIndexOf('.') + 1).Trim();
                Dictionary<string, string> colors = new Dictionary<string, string>();
                colors.Add("GENETIK_HOL", "#F7D6E0");
                colors.Add("GENETIK_FL", "#F2B5D4");
                colors.Add("MIKRO_HOLD", "#B2F7EF");
                colors.Add("MIKRO_FL", "#48BEFF");
                colors.Add("STUDIESTART", "#7BDFF2");
                colors.Add("STUDIESTAR", "#7BDFF2");
                colors.Add("FILOSOFI_HO", "#FAB161");
                colors.Add("FILOSOFI__F", "#FFB8D1");
                colors.Add("BIOKEMI_HOL", "#8789C0");
                colors.Add("BIOKEMI_FL", "#9CADCE");
                colors.Add("MAKRO_FL", "#A06CD5");
                colors.Add("MAKRO_HOLD", "#AFC97E");
                colors.Add("MAKRO_DISS.", "#B0C4B1");
                colors.Add("SUNDPSYK_FL", "#7F7CAF");
                colors.Add("FYSIOLOGI_H", "#75DDDD");
                colors.Add("FYSIOLOGI_F", "#C1BCAC");

                if (colors.ContainsKey(temp))
                {
                    return colors[temp];
                }
                else
                {
                    return "#AEB4B3";
                }
            }
            catch (Exception)
            {
                return "#AEB4B3";
            }
            
        }

        public static string[] correctLocation(string loc)
        {
            string temp = loc;
            string[] result = temp.Split('-');
            return result;
        }

        // ------------------------ WEBUNTIS PROGRESS ------------------------

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
            IList<Team> teams = getTeams();

            RestClient client = new RestClient("https://webuntis.dk");

            foreach (Team t in teams.Where(t => t.name == "MED_BA_1.sem.H3"))
            {
                updateICS(path, t, client);
            }
            return View();
        }

        public static void updateICS(string path, Team team, RestClient client)
        {
            RestRequest req = new RestRequest("WebUntis/Ical.do");
            req.Method = Method.GET;
            req.AddParameter("elemType", 1);
            req.AddCookie("schoolname", "_YXVfaGVhbHRo");
            req.AddOrUpdateParameter("elemId", team.id);

            var date = DateTime.Now.Date;

            using (StreamWriter sw = new StreamWriter(path + team.name + ".ics", false))
            {
                sw.WriteLine("BEGIN:VCALENDAR");
                sw.WriteLine("VERSION: 2.0");
                sw.WriteLine("CALSCALE: GREGORIAN");

                for (int i = 0; i < 30; i++)
                {
                    req.AddOrUpdateParameter("rpt_sd", date.ToString("yyyy-MM-dd"));
                    var res = client.Execute(req);
                    var content = res.Content;
                    string toBeSearched = "CALSCALE:GREGORIAN";
                    string toBeSearched2 = "END:VCALENDAR";
                    content = content.Substring(content.IndexOf(toBeSearched) + toBeSearched.Length + 2);
                    content = content.Substring(0, content.IndexOf(toBeSearched2) - 2);
                    //content = content.Replace("BEGIN:VCALENDAR", String.Empty)
                    //.Replace("PRODID:-//Ben Fortuna//iCal4j 1.0//EN", String.Empty)
                    //.Replace("VERSION:2.0", String.Empty)
                    //.Replace("CALSCALE:GREGORIAN", String.Empty)
                    //.Replace("END:VCALENDAR", String.Empty)
                    //.Replace("\n\n", "\n");
                    //content = Regex.Replace(content, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                    sw.WriteLine(content);
                    date.AddDays(7);
                }

                sw.WriteLine("END:VCALENDAR");
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