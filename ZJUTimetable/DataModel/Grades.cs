using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Media.Ocr;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Security.Credentials;
using AngleSharp;
using AngleSharp.Parser.Html;
using AngleSharp.Extensions;
using AngleSharp.Dom;


//remain problems : can only refresh the data one time per launch, the second time webrequest get the same checkcode,and "object removed warning"
namespace ZJUTimetable.DataModel
{
    enum Season { 春, 夏, 秋, 冬} 

    [Table("Courses")]
    class Course
    {
        public string TermNumber { get; set; }

        public string CourseNumber { get; set; }
        public string CourseName { get; set; }
        public string Grades { get; set; }
        public float Credits { get; set; }
        public float GradePoints { get; set; }
        public string MakeUpExamGrades { get; set; }

        public Course()
        {

        }
        public Course(String term, String courseNumber, String courseName, String grades, float credits, float gradePoints, String makeUpExamGrades)
        {
            this.TermNumber = term;
            this.CourseNumber = courseNumber;
            this.CourseName = courseName;
            this.Grades = grades;
            this.Credits = credits;
            this.GradePoints = gradePoints;
            this.MakeUpExamGrades = makeUpExamGrades;
        }
    }


    [Table("Exams")]
    class Exam
    {
        private static List<Exam> exams = new List<Exam>();

        public string CourseName { get; set; }
        public float Credits { get; set; }
        public string Term { get; set; }
        public string ExamTime { get; set; }
        public string ExaminationPlace { get; set; }
        public string SeatNumber { get; set; }
        public Exam()
        {

        }

        public Exam(string courseName, float credits, string term, string examTime, string examinationPlace, string seatNumber)
        {
            if (examTime.Length>5)
            {
                this.ExamTime = examTime.Substring(5);
            }
            this.CourseName = courseName;
            this.Credits = credits;
            this.Term = term;            
            this.ExaminationPlace = examinationPlace;
            this.SeatNumber = seatNumber;
        }

        public static async Task<List<Exam>> getExamsAsync()
        {
            if (exams.Count != 0)
            {
                return exams;
            }
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Data.db");
            await conn.CreateTableAsync<Exam>();
            exams = await conn.Table<Exam>().OrderBy(exam => exam.ExamTime).ToListAsync();
            return exams;
        }

        public static void ClearData()
        {
           exams = new List<Exam>();
        }
    }


    [Table("Terms")]
    class Term
    {
        private static List<Term> terms = new List<Term>();     
        public static float[] statistics = new float[4] { 0, 0, 0, 0 }; //total credits,total points,total common sense class credits, credits of H/I/L

        public string TermName { get; set; }
        public string TermNumber { get; set; }
        public float AveragePoints { get; set; }
        public float TermTotalCredits { get;set; }
        public float TermCommonClassCredits { get; set; } //term common class credits
        public float TermHILClassCredits { get;set; } //term common class credits

        [Ignore]
        public List<Course> Courses { get; set; }

        public Term() { }

        //public Term(string termName, string termNumber, float averagePoints, float termTotalCredits, float termCommonClassCredits,
        //    float termHILClassCredits)
        //{
        //    this.TermName = termName;
        //    this.TermNumber = termNumber;
        //    this.AveragePoints = averagePoints;
        //    this.TermTotalCredits = termTotalCredits;
        //    this.TermCommonClassCredits = termCommonClassCredits;
        //    this.TermHILClassCredits = termHILClassCredits;
        //}

        public static async Task<List<Term>> GetTermsAsync()
        {
            if (terms.Count == 0)
            {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Data.db");
                await conn.CreateTablesAsync<Course, Term>();
                terms = await conn.Table<Term>().OrderBy(term => term.TermNumber).ToListAsync();

                statistics[0] = statistics[1] = statistics[2] = statistics[3] = 0;

                // the following foreach and for cannot be put together, for Async problem,there will occur a mixup for i;
                foreach (var term in terms)
                {
                    statistics[0] += term.TermTotalCredits;
                    statistics[1] += term.AveragePoints * term.TermTotalCredits;
                    statistics[2] += term.TermCommonClassCredits;
                    statistics[3] += term.TermHILClassCredits;
                }

                for (int i = 0; i < terms.Count; i++)
                {
                    var term = terms[i];
                    term.Courses = await conn.Table<Course>().Where(course => course.TermNumber == term.TermNumber).ToListAsync();
                }
            }
            return terms;
        }

        public static void ClearData()
        {
            terms = new List<Term>();
        }

    }

    [Table("Lessons")]
    class Lesson
    {
        public string LessonName { get; set; }
        public string Teacher { get; set; }
        public string TermName { get; set; }        
        public string Day { get; set; }
        public string Class { get; set; }
        public string Time { get; set; }
        public string LessonPlace { get; set; }       

        public Lesson() { }

        public Lesson(string lessonName, string teacher,string termName, string day, string _class,string time,string lessonPlace)
        {
            this.LessonName = lessonName;
            this.Teacher = teacher;
            this.TermName = termName;
            this.Day = day;
            this.Class = _class;
            this.Time = time;
            this.LessonPlace = lessonPlace;        
        }
    }

    class WeekDay
    {
        private static List<WeekDay> weekDays = new List<WeekDay>();

        public string DayName { get; set; }
        public List<Lesson> Lessons { get; set; }

        public WeekDay(string dayName, List<Lesson> lessons)
        {
            this.DayName = dayName;
            this.Lessons = lessons;
        }

        public static void ClearData()
        {
            weekDays = new List<WeekDay>();
        }

        public static async Task<List<WeekDay>> GetWeekdaysAsync()
        {
            if (weekDays.Count == 0)
            {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Data.db");
                await conn.CreateTableAsync<Lesson>();

                string[] dayNames = new string[7] { "周一", "周二", "周三", "周四", "周五", "周六", "周日" };
                string currentSeason = DataHelper.getCurrentSeason().ToString();

                foreach (string dayName in dayNames)
                {
                    var lessons = await conn.Table<Lesson>().Where(lesson => lesson.TermName.Contains(currentSeason) && lesson.Day == dayName).ToListAsync();
                    weekDays.Add(new WeekDay(dayName, lessons.OrderBy(lesson => int.Parse(lesson.Class.Substring(0, lesson.Class.IndexOf(',')))).ToList()));
                }
            }

            return weekDays;
        }
    }

    

    //used to refresh data
    class DataHelper
    {
        public static async Task<bool> UpdateDataAsync()
        {
            bool updated = false;
            WebHelper webHelper = new WebHelper();

            if (await webHelper.getAllData())
            {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Data.db");
                await conn.DropTableAsync<Term>();
                await conn.DropTableAsync<Lesson>();
                await conn.DropTableAsync<Exam>();//删除之前的数据
                await conn.DropTableAsync<Course>();//不提示成绩更新情况下，直接删除成绩
                await conn.CreateTablesAsync<Course, Term, Lesson, Exam>();

                await conn.InsertAllAsync(webHelper.Exams);//update exams; 
                await conn.InsertAllAsync(webHelper.Terms);
                await conn.InsertAllAsync(webHelper.Lessons);
                await conn.InsertAllAsync(webHelper.Courses);
                Term.ClearData();
                Exam.ClearData();
                WeekDay.ClearData();

                updated = true;
            }

            return updated;
            ////区分有无成绩更新，方便后续扩展出成绩提示
            //var previousCourses = terms.SelectMany(term => term.Courses);

            //var updateCourses = from course in webHelper.Courses
            //                    let previousCourse = from tempCourse in previousCourses
            //                                         where tempCourse.CourseName == course.CourseName &&
            //                                         tempCourse.TermNumber == course.TermNumber &&
            //                                         (tempCourse.GradePoints != course.GradePoints | tempCourse.MakeUpExamGrades != course.MakeUpExamGrades)
            //                                         select tempCourse
            //                    where previousCourse.Any()
            //                    select course;

            //var newCourses = from course in webHelper.Courses
            //                 let previousCourse = from tempCourse in previousCourses
            //                                      where tempCourse.CourseName == course.CourseName &&
            //                                      tempCourse.TermNumber == course.TermNumber
            //                                      select tempCourse
            //                 where !previousCourse.Any()
            //                 select course;

            //var updatedCourse = updateCourses.ToList();
            // var newcourses = newCourses.ToList();
            //if (updateCourses.Any() | newCourses.Any())
            //{
            //    try
            //    {
            //        await conn.InsertAllAsync(newCourses);
            //        await conn.UpdateAllAsync(updateCourses);

            //    }
            //    catch (Exception)
            //    {
            //        await conn.DropTableAsync<Course>();
            //        var message = new Windows.UI.Popups.MessageDialog("更新本地数据出错，建议或重试，或进入设置删除本地账号和数据或重启应用后再导入");
            //        await message.ShowAsync();
            //    }
            //    updated = true;
            //}
        }

        public static async Task DeleteDatabaseAsync()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Data.db");          
            await conn.DropTableAsync<Course>();
            await conn.DropTableAsync<Term>();
            await conn.DropTableAsync<Exam>();
            await conn.DropTableAsync<Lesson>();
            Term.ClearData();
            Exam.ClearData();
            WeekDay.ClearData();
        }

        public static Season getCurrentSeason()
        {
            Season currentSeason;
            var localData = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
            if (localData.ContainsKey("season"))
            {
                currentSeason = (Season)Enum.Parse(typeof(Season), localData["season"].ToString());
            }
            else
            {                      
                currentSeason = (Season)(System.DateTime.Now.Month / 3); //for convinience,set Term spring=0,1,2,Summer=3,4,5
            }
            return currentSeason;
        }
    }

    class WebHelper
    {
        public List<Term> Terms { get; set; }
        public List<Course> Courses { get; set; }        
        public List<Lesson> Lessons { get; set; }
        public List<Exam> Exams { get; set; }

        private CookieContainer cookieContainer = new CookieContainer();
        private string userName, password;

        public WebHelper()
        {
            this.Terms = new List<Term>();
            this.Courses = new List<Course>();
            this.Exams = new List<Exam>();
            this.Lessons = new List<Lesson>();

        }
        public async Task<bool> getAllData()
        {
            #region Get password.
            //PasswordVault value = new PasswordVault();
            //if (value.FindAllByResource("zju") != null)
            //{
            //    await showWebConnectionError();
            //    return;
            //}

            //var passwordCredential = value.FindAllByResource("zju").First();
            //userName = passwordCredential.UserName;
            //password = passwordCredential.Password;

            bool haveGetData = false;
            var localData = Windows.Storage.ApplicationData.Current.LocalSettings.Values;

            if (localData.ContainsKey("userName") && localData.ContainsKey("password"))
            {
                userName = localData["userName"].ToString();
                password = localData["password"].ToString();
            }
            else
            {
                await showWebConnectionError("尚未输入教务网账号信息，无法同步");
                return haveGetData;
            }
            #endregion

            if (await login(await getCheckCode()) == HttpStatusCode.OK)
            {               
                if (await getGrades()) //不会出现登陆失败，提示object moved
                {
                    await getExams();
                    await getLessons();
                    haveGetData = true;
                }
                else
                {
                    await showWebConnectionError("请重启app后再次同步");
                }
            }
            else
            {
                await showWebConnectionError("未能自动登陆，请1.检查网络连接和教务网账号信息；2.重启应用");
            }
            return haveGetData;
        }

        private async Task<bool> getGrades()
        {
            //data eg:<td>(2015-2016-1)-11120151-0003419-1</td><td>软件技术基础</td><td>2.5</td><td>&nbsp;</td><td>崔超</td><td>秋</td><td>2015年11月12日(08:00-10:00)</td><td>玉泉教4-302(多)</td><td>&nbsp;</td>
            //regex formulation
            //学期,课程号,课程名,成绩,学分,绩点,补考成绩
            //<td>\((\d{4}-\d{4}-\d)\)-(.*?)-.*?</td><td>(.*?)</td><td>(.*?)</td><td>(.*?)</td><td>(.*?)</td><td>(.*?)</td>


            string postdataGrades = "__VIEWSTATE=dDw0NzAzMzE4ODg7dDw7bDxpPDE%2BOz47bDx0PDtsPGk8Mj47aTw1PjtpPDI1PjtpPDI3PjtpPDQxPjtpPDQzPjtpPDQ1PjtpPDQ3Pjs%2BO2w8dDx0PDt0PGk8MTY%2BO0A8XGU7MjAwMS0yMDAyOzIwMDItMjAwMzsyMDAzLTIwMDQ7MjAwNC0yMDA1OzIwMDUtMjAwNjsyMDA2LTIwMDc7MjAwNy0yMDA4OzIwMDgtMjAwOTsyMDA5LTIwMTA7MjAxMC0yMDExOzIwMTEtMjAxMjsyMDEyLTIwMTM7MjAxMy0yMDE0OzIwMTQtMjAxNTsyMDE1LTIwMTY7PjtAPFxlOzIwMDEtMjAwMjsyMDAyLTIwMDM7MjAwMy0yMDA0OzIwMDQtMjAwNTsyMDA1LTIwMDY7MjAwNi0yMDA3OzIwMDctMjAwODsyMDA4LTIwMDk7MjAwOS0yMDEwOzIwMTAtMjAxMTsyMDExLTIwMTI7MjAxMi0yMDEzOzIwMTMtMjAxNDsyMDE0LTIwMTU7MjAxNS0yMDE2Oz4%2BOz47Oz47dDx0PHA8cDxsPERhdGFUZXh0RmllbGQ7RGF0YVZhbHVlRmllbGQ7PjtsPHh4cTt4cTE7Pj47Pjt0PGk8OD47QDxcZTvmmKU75aSPO%2BefrTvnp4s75YasO%2BefrTvmmpE7PjtAPFxlOzJ85pilOzJ85aSPOzJ855%2BtOzF856eLOzF85YasOzF855%2BtOzF85pqROz4%2BOz47Oz47dDxwPDtwPGw8b25jbGljazs%2BO2w8d2luZG93LnByaW50KClcOzs%2BPj47Oz47dDxwPDtwPGw8b25jbGljazs%2BO2w8d2luZG93LmNsb3NlKClcOzs%2BPj47Oz47dDxAMDw7Ozs7Ozs7Ozs7Pjs7Pjt0PEAwPDs7Ozs7Ozs7Ozs%2BOzs%2BO3Q8QDA8Ozs7Ozs7Ozs7Oz47Oz47dDxwPHA8bDxUZXh0Oz47bDxaSkRYOz4%2BOz47Oz47Pj47Pj47PkBsaV5B%2FCa01w1HqSY%2Fcrk9veyD&Button2=%D4%DA%D0%A3%D1%A7%CF%B0%B3%C9%BC%A8%B2%E9%D1%AF";
            string websiteGrades = "http://jwbinfosys.zju.edu.cn/xscj.aspx?xh=" + userName;

            var grades = await getDataFromWeb(websiteGrades, postdataGrades);
            if (grades != null)
            {
                var termsDictionary = new Dictionary<string, Term>();               

                foreach (var courseHtml in grades)
                {
                    var courseDatas = courseHtml.QuerySelectorAll("td");
                    string makeUpExamGrades = courseDatas[5].TextContent;
                    //Course(String term, String courseNumber, String courseName, String grades, float credits, float gradePoints, String makeUpExamGrades)
                    var courseInfo = courseDatas[0].TextContent;
                    var course = new Course(courseInfo.Substring(1,11), courseInfo.Substring(14, 8),
                        courseDatas[1].TextContent, courseDatas[2].TextContent,
                        float.Parse(courseDatas[3].TextContent), float.Parse(courseDatas[4].TextContent), makeUpExamGrades);

                    string termNumber = course.TermNumber;
                    if (!termsDictionary.ContainsKey(termNumber))
                    {
                        termsDictionary[termNumber] = new Term() { TermNumber = termNumber, TermName = termNumber.Substring(2, 3) + termNumber.Substring(7, 2) + (termNumber.EndsWith("1") ? "秋冬" : "春夏") };
                    }

                    if (course.GradePoints >= 1.5 || course.MakeUpExamGrades.Contains("补及格")) //课程及格才会加到总学分里面，对于补考尚不确定；简单处理剔除重修的课  && !this.Courses.Where(tempCourse => tempCourse.CourseNumber==course.CourseNumber).Any()
                    {
                        termsDictionary[termNumber].TermTotalCredits += course.Credits;
                        termsDictionary[termNumber].AveragePoints += course.GradePoints * course.Credits;

                        if (course.CourseNumber.Contains('J') | course.CourseNumber.Contains('K') | course.CourseNumber.Contains('M') |
                            course.CourseNumber.Contains('H') | course.CourseNumber.Contains('I') | course.CourseNumber.Contains('L'))
                        {
                            termsDictionary[termNumber].TermCommonClassCredits += course.Credits;

                            if (course.CourseNumber.Contains('H') | course.CourseNumber.Contains('I') | course.CourseNumber.Contains('L'))
                            {
                                termsDictionary[termNumber].TermHILClassCredits += course.Credits;
                            }
                        }
                    }
                    this.Courses.Add(course);
                }         

                this.Terms = termsDictionary.Values.ToList();
                foreach (var term in this.Terms)
                {
                    term.AveragePoints = term.TermTotalCredits == 0 ? 0 : term.AveragePoints / term.TermTotalCredits;
                }
                return true;
            }
            else
            {              
                return false;
            }
        }

        private async Task<bool> getExams()
        {          
            var now = System.DateTime.Now;
            string websiteExams = "http://jwbinfosys.zju.edu.cn/xskscx.aspx?xh=" + userName;           

            int currentSeason = (int)DataHelper.getCurrentSeason();
            string[] postdatas = new string[4] {
                (now.Year - 1) + "-" + now.Year + "&xqd=2%7C%B4%BA",
                (now.Year - 1) + "-" + now.Year + "&xqd=2%7C%B4%BA",
                now.Year + "-" + (now.Year + 1) + "&xqd=1%7C%C7%EF",
                now.Year + "-" + (now.Year + 1) + "&xqd=1%7C%B6%AC" };
            var viewstate = "__VIEWSTATE=dDwxODk5Mjk0MTA1O3Q8O2w8aTwxPjs%2BO2w8dDw7bDxpPDE%2BO2k8NT47PjtsPHQ8dDxwPHA8bDxEYXRhVGV4dEZpZWxkO0RhdGFWYWx1ZUZpZWxkOz47bDx4bjt4bjs%2BPjs%2BO3Q8aTw0PjtAPDIwMTUtMjAxNjsyMDE0LTIwMTU7MjAxMy0yMDE0OzIwMTItMjAxMzs%2BO0A8MjAxNS0yMDE2OzIwMTQtMjAxNTsyMDEzLTIwMTQ7MjAxMi0yMDEzOz4%2BO2w8aTwwPjs%2BPjs7Pjt0PHQ8cDxwPGw8RGF0YVRleHRGaWVsZDtEYXRhVmFsdWVGaWVsZDs%2BO2w8eHhxO3hxMTs%2BPjs%2BO3Q8aTw3PjtAPOenizvlhqw755%2BtO%2BaakTvmmKU75aSPO%2BefrTs%2BO0A8MXznp4s7MXzlhqw7MXznn607MXzmmpE7MnzmmKU7MnzlpI87Mnznn607Pj47bDxpPDA%2BOz4%2BOzs%2BOz4%2BOz4%2BOz4sR449m7l4Pp10L36HjJa5fRCctg%3D%3D&xnd=";
            var exams = await getDataFromWeb(websiteExams, viewstate + postdatas[currentSeason]);

            if (exams != null)
            {                
                foreach (var examHtml in exams)
                {
                    var examDatas = examHtml.QuerySelectorAll("td");
                    //string courseName, float credits, string term, string examTime, string examinationPlace, string seatNumber
                    this.Exams.Add(new Exam(examDatas[1].TextContent, float.Parse(examDatas[2].TextContent), examDatas[5].TextContent,
                        examDatas[6].TextContent, examDatas[7].TextContent, examDatas[8].TextContent));
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> getLessons()
        {
            var now = System.DateTime.Now;
            int month = now.Month;
            string term;         
            if (month >= 8 || month <= 2)
            {
                term = "&xnd=" + now.Year + "-" + (now.Year + 1) + "&xqd=1%7C%C7%EF%A1%A2%B6%AC";
            }
            else
            {                
                term = "&xnd=" + (now.Year - 1) + "-" + now.Year + "&xqd=2%7C%B4%BA%A1%A2%CF%C4";
            }

            string websiteLessons = "http://jwbinfosys.zju.edu.cn/xskbcx.aspx?xh=" + userName;
            string postdata = "__EVENTTARGET=xqd&__EVENTARGUMENT=" + term + "&xxms=%C1%D0%B1%ED&kcxx=&__VIEWSTATE=" +
                "dDwtMjQ5Nzk5MzUyO3Q8O2w8aTwwPjs%2BO2w8dDw7bDxpPDE%2BO2k8Mz47aTw1PjtpPDg%2BO2k8MTA%2BO2k8MTI%2BO2k8MTQ%2BO2k8MTY%2BO2k8MTg%2BO2k8MjI%2BO2k8MjY%2BO2k8Mjg%2BOz47bDx0PHQ8OztsPGk8MD47Pj47Oz47dDx0PHA8cDxsPERhdGFUZXh0RmllbGQ7RGF0YVZhbHVlRmllbGQ7PjtsPHhuO3huOz4%2BOz47dDxpPDQ%2BO0A8MjAxNS0yMDE2OzIwMTQtMjAxNTsyMDEzLTIwMTQ7MjAxMi0yMDEzOz47QDwyMDE1LTIwMTY7MjAxNC0yMDE1OzIwMTMtMjAxNDsyMDEyLTIwMTM7Pj47bDxpPDA%2BOz4%2BOzs%2BO3Q8dDxwPHA8bDxEYXRhVGV4dEZpZWxkO0RhdGFWYWx1ZUZpZWxkOz47bDxkeXhxO3hxMTs%2BPjs%2BO3Q8aTwyPjtAPOaYpeOAgeWkjzvnp4vjgIHlhqw7PjtAPDJ85pil44CB5aSPOzF856eL44CB5YasOz4%2BO2w8aTwxPjs%2BPjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOWtpuWPt%2B%2B8mjMxMjAxMDM4NDM7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOWnk%2BWQje%2B8muW0lOi2hTs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w85a2m6Zmi77ya5L%2Bh5oGv5LiO55S15a2Q5bel56iL5a2m6ZmiOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznsbso5LiT5LiaKe%2B8mueUteWtkOenkeWtpuS4juaKgOacrzs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w86KGM5pS%2F54%2Bt77ya55S15a2Q56eR5a2m5LiO5oqA5pyvMTIwMzs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8XGU7Pj47Pjs7Pjt0PEAwPHA8cDxsPFZpc2libGU7UGFnZUNvdW50O18hSXRlbUNvdW50O18hRGF0YVNvdXJjZUl0ZW1Db3VudDtEYXRhS2V5czs%2BO2w8bzx0PjtpPDE%2BO2k8Nz47aTw3PjtsPD47Pj47Pjs7Ozs7Ozs7Ozs%2BO2w8aTwwPjs%2BO2w8dDw7bDxpPDE%2BO2k8Mj47aTwzPjtpPDQ%2BO2k8NT47aTw2PjtpPDc%2BOz47bDx0PDtsPGk8MD47aTwxPjtpPDI%2BO2k8Mz47aTw0PjtpPDU%2BO2k8Nj47aTw3Pjs%2BO2w8dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTUtMjAxNi0xKS0xMDE5MjE4MzMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPjEwMTkyMTgzXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTUtMjAxNi0xKS0xMDE5MjE4MzMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPumdouWQkUlDIENBROeahOi9r%2BS7tuaKgOacr1w8L0FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9VCgyMDE1LTIwMTYtMSktMTAxOTIxODMzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD7lj7Lls6VcPC9hXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOenizs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w85ZGo5LqM56ysMSwy6IqCXDxiclw%2B5ZGo5Zub56ysMyw0LDXoioI7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOeOieazieaVmTctMjA0KOWkmilcPGJyXD7njonms4nmlZk3LTIwNCjlpJopOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwyMDE1LTA2LTE3IDE3OjUzOjQ2Oz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwxOz4%2BOz47Oz47Pj47dDw7bDxpPDA%2BO2k8MT47aTwyPjtpPDM%2BO2k8ND47aTw1PjtpPDY%2BO2k8Nz47PjtsPHQ8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9MSgyMDE1LTIwMTYtMSktMTExMjAxNTEyMDEzMTEwMDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPjExMTIwMTUxXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0xKDIwMTUtMjAxNi0xKS0xMTEyMDE1MTIwMTMxMTAwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B6L2v5Lu25oqA5pyv5Z%2B656GAXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0xKDIwMTUtMjAxNi0xKS0xMTEyMDE1MTIwMTMxMTAwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B6LCi56uLXDwvYVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznp4s7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOWRqOS6jOesrDksMTDoioJcPGJyXD7lkajlm5vnrKwxLDLoioJcPGJyXD7lkajlm5vnrKw5LDEw6IqCOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznjonms4nlpJbnu4%2FotLjmpbwtMTEzKOWkmilcPGJyXD7njonms4nlpJbnu4%2FotLjmpbwtMTEzKOWkmilcPGJyXD7ntKvph5HmuK%2FmnLrmiL87Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDIwMTUtMDYtMjkgMTA6NDc6MjU7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDE7Pj47Pjs7Pjs%2BPjt0PDtsPGk8MD47aTwxPjtpPDI%2BO2k8Mz47aTw0PjtpPDU%2BO2k8Nj47aTw3Pjs%2BO2w8dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTUtMjAxNi0xKS0xMTEyMDE3MDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPjExMTIwMTcwXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTUtMjAxNi0xKS0xMTEyMDE3MDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPuaVsOWtl%2BS%2FoeWPt%2BWkhOeQhlw8L0FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9VCgyMDE1LTIwMTYtMSktMTExMjAxNzAzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD7lvpDlhYPmrKNcPC9hXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOeni%2BWGrDs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w85ZGo5LqU56ysMSwy6IqCe%2BWPjOWRqH1cPGJyXD7lkajkupTnrKwzLDQsNeiKgjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8546J5rOJ5pWZNy01MDQo5aSaKVw8YnJcPueOieazieaVmTctNTA0KOWkmik7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDIwMTUtMDYtMTcgMTc6NDc6MTA7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDE7Pj47Pjs7Pjs%2BPjt0PDtsPGk8MD47aTwxPjtpPDI%2BO2k8Mz47aTw0PjtpPDU%2BO2k8Nj47aTw3Pjs%2BO2w8dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0wKDIwMTUtMjAxNi0xKS0xMTE4ODI2MDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPjExMTg4MjYwXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0wKDIwMTUtMjAxNi0xKS0xMTE4ODI2MDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPueUteWtkOW3peiJuuWunuS5oFw8L0FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9MCgyMDE1LTIwMTYtMSktMTExODgyNjAzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD7pn6npm4FcPGJyXD7lrZnpopZcPGJyXD7mnY7otKHnpL5cPC9hXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOefrTs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8Jm5ic3BcOzs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8Jm5ic3BcOzs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8MjAxNS0wNy0wNiAxMDoyNjoxNDs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8MTs%2BPjs%2BOzs%2BOz4%2BO3Q8O2w8aTwwPjtpPDE%2BO2k8Mj47aTwzPjtpPDQ%2BO2k8NT47aTw2PjtpPDc%2BOz47bDx0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPVQoMjAxNS0yMDE2LTEpLTExMTg4MjcwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2BMTExODgyNzBcPC9BXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPVQoMjAxNS0yMDE2LTEpLTExMTg4MjcwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B6auY57qn5pWw5a2X57O757uf5a6e6aqM6K%2B%2BXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTUtMjAxNi0xKS0xMTE4ODI3MDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPuWxiOawkeWGm1w8YnJcPuWUkOWllVw8L2FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w855%2BtOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwmbmJzcFw7Oz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwmbmJzcFw7Oz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwyMDE1LTA2LTE3IDIxOjE0OjMzOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwxOz4%2BOz47Oz47Pj47dDw7bDxpPDA%2BO2k8MT47aTwyPjtpPDM%2BO2k8ND47aTw1PjtpPDY%2BO2k8Nz47PjtsPHQ8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9MSgyMDE1LTIwMTYtMSktNjcxMjAwMzAyMDEyMTEzMzMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPjY3MTIwMDMwXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0xKDIwMTUtMjAxNi0xKS02NzEyMDAzMDIwMTIxMTMzMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B55S15a2Q56eR5a2m5LiO5oqA5pyv5LiT6aKY5a6e6aqMXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0xKDIwMTUtMjAxNi0xKS02NzEyMDAzMDIwMTIxMTMzMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B5p2o5bu65LmJXDwvYVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDzlhqw7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPCZuYnNwXDs7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPCZuYnNwXDs7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDIwMTUtMDYtMTYgMTE6NTA6NTE7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDE7Pj47Pjs7Pjs%2BPjt0PDtsPGk8MD47aTwxPjtpPDI%2BO2k8Mz47aTw0PjtpPDU%2BO2k8Nj47aTw3Pjs%2BO2w8dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTUtMjAxNi0xKS02NzE5MDAyMDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPjY3MTkwMDIwXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTUtMjAxNi0xKS02NzE5MDAyMDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPuiuoeeul%2Bacuue7hOaIkOS4juiuvuiuoVw8L0FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9VCgyMDE1LTIwMTYtMSktNjcxOTAwMjAzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD7njovnu7TkuJxcPGJyXD7llJDlpZVcPGJyXD7lsYjmsJHlhptcPC9hXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOeni%2BWGrDs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w85ZGo5LiJ56ysOSwxMOiKgnvljZXlkah9XDxiclw%2B5ZGo5LiJ56ysMyw0LDXoioI7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOeOieazieesrDEx5pWZ5a2m5aSn5qW8LTQwMFw8YnJcPueOieazieaVmTctMjAyKOWkmik7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDIwMTUtMDYtMTcgMjA6NDg6NTA7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDE7Pj47Pjs7Pjs%2BPjs%2BPjs%2BPjt0PEAwPHA8cDxsPFBhZ2VDb3VudDtfIUl0ZW1Db3VudDtfIURhdGFTb3VyY2VJdGVtQ291bnQ7RGF0YUtleXM7PjtsPGk8MT47aTwwPjtpPDA%2BO2w8Pjs%2BPjs%2BOzs7Ozs7Ozs7Oz47Oz47dDw7bDxpPDM%2BOz47bDx0PEAwPDs7Ozs7Ozs7Ozs%2BOzs%2BOz4%2BOz4%2BOz4%2BOz6L8VtroSn5hjEUtzKdCQzPW8brnA%3D%3D"; //
            var lessons = await getDataFromWeb(websiteLessons, postdata);

            if (lessons != null)
            {                               
                foreach (var lessonHtml in lessons)
                {
                    var lessonDatas = lessonHtml.QuerySelectorAll("td");
                    if (lessonDatas[3].TextContent== "短" || lessonDatas[4].TextContent=="")
                    {
                        continue;
                    }

                    string[] splitString = { "<br>" };
                    var stringClasses = lessonDatas[4].InnerHtml.Split(splitString, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var stringPlaces = lessonDatas[5].InnerHtml.Split(splitString, StringSplitOptions.RemoveEmptyEntries).ToList();

                    for (int i = 0; i < stringClasses.Count; i++)
                    {
                        string lessonPlace;
                        if (stringPlaces.Count == 1)
                        {
                            lessonPlace = stringPlaces[0];
                        }
                        else
                        {
                            lessonPlace = stringPlaces[i];
                        }

                        string _class = stringClasses[i].Substring(3);
                        //Lesson(lessonName, teacher, termName, day, _class, time, lessonPlace)
                        var lesson = new Lesson(lessonDatas[1].TextContent,
                            lessonDatas[2].TextContent,
                            lessonDatas[3].TextContent,
                            stringClasses[i].Substring(0, 2), _class, getTime(_class), lessonPlace);
                        this.Lessons.Add(lesson);
                    }
                }

                return true;
            }
            else
            {               
                return false;
            }
        }
        private string getTime(string classes)
        {
            var firstClass = int.Parse(classes.Substring(0, classes.IndexOf(',')));
            var lastClass = int.Parse(classes.Remove(classes.LastIndexOf('节')).Substring(1 + classes.LastIndexOf(',')));

            string[] startTimes=new string[13] { "8:00", "8:50", "9:50", "10:40",
                "11:30", "13:15", "14:05", "14:55", "15:55", "16:45", "18:30", "19:20", "20:10" };

            string[] endTimes=new string[13] {"8:45", "9:35","10:35","11:25","12:15",
                "14:00", "14:50", "15:40", "16:40", "17:30", "19:15", "20:05", "20:55" };

            return startTimes[firstClass - 1] + "-" + endTimes[lastClass - 1];
        }

        private async Task showWebConnectionError()
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog("连接教务网错误，请尝试:1.检查账号和网络连接。2.重启应用。若无效请反馈给开发者");
            await messageDialog.ShowAsync();
        }

        private async Task showWebConnectionError(string errorInfo)
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog(errorInfo);
            await messageDialog.ShowAsync();
        }

        private async Task<string> getCheckCode()
        {            
            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create("http://jwbinfosys.zju.edu.cn/CheckCode.aspx");
            request.Method = "GET";
            request.CookieContainer =cookieContainer;
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)await request.GetResponseAsync();
            }
            catch
            {               
                return null;
            }

            //HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            //保存cookie            
            cookieContainer.Add(new Uri("http://jwbinfosys.zju.edu.cn/CheckCode.aspx"), response.Cookies);

            //get the checkcode.gif and resize,for OCR requires image size>40*40
            StorageFile checkCodeImage = await ApplicationData.Current.LocalFolder.CreateFileAsync("checkCode.gif", CreationCollisionOption.ReplaceExisting);

            byte[] pixels;
            BitmapTransform transform = new BitmapTransform();
            transform.ScaledWidth = 120; //the length and width were doubled.
            transform.ScaledHeight = 44;
            //start            
            using (Stream webResponseStream = response.GetResponseStream())
            using (var responseStream = await checkCodeImage.OpenStreamForWriteAsync())
            {
                await webResponseStream.CopyToAsync(responseStream);
            }
            using (var responseStream = await checkCodeImage.OpenAsync(FileAccessMode.ReadWrite))
            {
                var decoder = await BitmapDecoder.CreateAsync(responseStream);
                PixelDataProvider pix = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.ColorManageToSRgb);
                pixels = pix.DetachPixelData();
            }

            var checkCode = new WriteableBitmap(120, 44);
            using (Stream stream = checkCode.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(pixels, 0, pixels.Length);
            }

            ////recognize the checkcode with orc
            OcrEngine ocrEngine = new OcrEngine(OcrLanguage.English);
            var ocrResult = await ocrEngine.RecognizeAsync(44, 120, checkCode.PixelBuffer.ToArray());

            if (ocrResult.Lines != null)
            {               
                return ocrResult.Lines[0].Words[0].Text;
            }          
            return null;
        }

        private async Task<HttpStatusCode> login(string checkCode)
        {
            if (checkCode != null)
            {
                HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create("http://jwbinfosys.zju.edu.cn/default2.aspx");
                string postdata =
                     "__EVENTTARGET=Button1&TextBox1=" + userName + "&TextBox2=" + password + "&Textbox3=" +
                    checkCode + "&RadioButtonList1=学生&Text1=";
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = cookieContainer;

                byte[] postdatabytes = Encoding.UTF8.GetBytes(postdata);

                using (Stream stream = await request.GetRequestStreamAsync())
                {
                    await stream.WriteAsync(postdatabytes, 0, postdatabytes.Length);
                }

                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                return HttpStatusCode.OK;
            }
            
            return HttpStatusCode.ExpectationFailed;

        }

        private async Task<IHtmlCollection<IElement>> getDataFromWeb(string website,string postdata)
        {
            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(website);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookieContainer;

            byte[] postdatabytes = Encoding.UTF8.GetBytes(postdata);
            using (Stream stream = await request.GetRequestStreamAsync())
            {
                await stream.WriteAsync(postdatabytes, 0, postdatabytes.Length);
            }

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader streamReader = new StreamReader(responseStream, await DBCSCodePage.DBCSEncoding.GetDBCSEncoding("gb2312")))
            {
                //return await streamReader.ReadToEndAsync();    
                var document = new HtmlParser().Parse(await streamReader.ReadToEndAsync());
                return document.QuerySelectorAll("tr.datagridhead~tr");
            }
        }
    }
}
