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
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Security.Credentials;
using Windows.Storage.Pickers;


//remain problems : can only refresh the data one time per launch, the second time webrequest get the same checkcode,and "object removed warning"
namespace ZJUTimetable_Windows_.DataModel
{
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
            this.CourseName = courseName;
            this.Credits = credits;
            this.Term = term;
            this.ExamTime = examTime;
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

        public Term(string termName,string termNumber, float averagePoints, float termTotalCredits,float termCommonClassCredits,
            float termHILClassCredits)
        {
            this.TermName = termName;
            this.TermNumber = termNumber;
            this.AveragePoints = averagePoints;
            this.TermTotalCredits = termTotalCredits;
            this.TermCommonClassCredits = termCommonClassCredits;
            this.TermHILClassCredits = termHILClassCredits;
        }
      
        public static async Task<List<Term>> GetTermsAsync()
        {
            if (terms.Count != 0)
            {
                return terms;
            }
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Data.db");                    
            await conn.CreateTablesAsync<Course, Term>();
            terms = await conn.Table<Term>().OrderBy(term => term.TermNumber).ToListAsync();

            statistics[0]= statistics[1] = statistics[2] = statistics[3] =0;           
            foreach (var term in terms)
            {
                statistics[0] += term.TermTotalCredits;
                statistics[1] += term.AveragePoints* term.TermTotalCredits;
                statistics[2] += term.TermCommonClassCredits;
                statistics[3] += term.TermHILClassCredits;
            }
            foreach (var term in terms)
            {
                term.Courses = await conn.Table<Course>().Where(course => course.TermNumber == term.TermNumber).ToListAsync();
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
        public string Day1 { get; set; }
        public string Class1 { get; set; }
        public string Time1 { get; set; }
        public string LessonPlace1 { get; set; }
        public string Day2 { get; set; }
        public string Class2 { get; set; }
        public string Time2 { get; set; }
        public string LessonPlace2 { get; set; }

        public Lesson() { }

        public Lesson(string lessonName, string teacher,string termName, string day1, string class1,string time1,string lessonPlace1, string day2, string class2, string time2, string lessonPlace2)
        {
            this.LessonName = lessonName;
            this.Teacher = teacher;
            this.TermName = termName;
            this.Day1 = day1;
            this.Class1 = class1;
            this.Time1 = time1;
            this.LessonPlace1 = lessonPlace1;
            this.Day2 = day2;
            this.Class2 = class2;
            this.Time2 = time2;
            this.LessonPlace2 = lessonPlace2;

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
            if (weekDays.Count != 0)
            {
                return weekDays;
            }
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Data.db");
            await conn.CreateTableAsync<Lesson>();
            List<string> dayNames =new List<string>() { "周一", "周二", "周三", "周四", "周五", "周六", "周日" };

            string currentSeason;
            Windows.Storage.ApplicationDataContainer localsettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localsettings.Values.ContainsKey("season"))
            {
                currentSeason = localsettings.Values["season"].ToString();               
            }
            else
            {
                int month = System.DateTime.Now.Month;
                if (month >= 8 && month < 11)
                {
                    currentSeason = "秋";
                }
                else if (month >= 11 && month < 2)
                {
                    currentSeason = "冬";
                }
                else if (month >= 2 && month < 4)
                {
                    currentSeason = "春";
                }
                else
                {
                    currentSeason = "夏";
                }
            }


            foreach (string dayName in dayNames)
            {
                var lessons =await conn.Table<Lesson>().Where(lesson => lesson.TermName.Contains(currentSeason) && (lesson.Day1 == dayName || lesson.Day2 == dayName)).ToListAsync();

                if (lessons.Count!=0)
                {
                    foreach (var lesson in lessons)
                    {
                        if (lesson.Day2 == dayName) //set the value of day2 to day1, make it easier to bind in xmal.
                        {
                            lesson.Class1 = lesson.Class2;
                            lesson.Time1 = lesson.Time2;
                            lesson.LessonPlace1 = lesson.LessonPlace2;
                        }                      
                    }                    
                    weekDays.Add(new WeekDay(dayName, lessons.OrderBy(lesson => int.Parse(lesson.Class1.Substring(0, lesson.Class1.IndexOf(',')))).ToList()));                    
               }               
            }
            return weekDays;
        }
    }

    //private string getCurrentSeason()
    //{
    //    string currentSeason;
    //    Windows.Storage.ApplicationDataContainer localsettings = Windows.Storage.ApplicationData.Current.LocalSettings;
    //    if (localsettings.Values.ContainsKey("season"))
    //    {
    //        currentSeason = localsettings.Values["season"].ToString();
    //    }
    //    else
    //    {
    //        int month = System.DateTime.Now.Month;
    //        if (month >= 8 && month < 11)
    //        {
    //            currentSeason = "秋";
    //        }
    //        else if (month >= 11 && month < 2)
    //        {
    //            currentSeason = "冬";
    //        }
    //        else if (month >= 2 && month < 4)
    //        {
    //            currentSeason = "春";
    //        }
    //        else
    //        {
    //            currentSeason = "夏";
    //        }
    //    }
    //    return currentSeason;
    //}

    //used to refresh data
    class DataHelper
    {

        public static async Task<bool> UpdateDataAsync()
        {
            bool updated = false; 
            //var terms = await Term.GetTermsAsync();
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Data.db");
            await conn.DropTableAsync<Term>();
            await conn.DropTableAsync<Lesson>();
            await conn.DropTableAsync<Exam>();//删除之前的数据
            await conn.DropTableAsync<Course>();//不提示成绩更新情况下，直接删除成绩

            await conn.CreateTablesAsync<Course, Term, Lesson, Exam>();
            WebHelper webHelper = new WebHelper();
            await webHelper.getAllData();

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

            await conn.InsertAllAsync(webHelper.Exams);//update exams; 
            await conn.InsertAllAsync(webHelper.Terms);
            await conn.InsertAllAsync(webHelper.Lessons);
            await conn.InsertAllAsync(webHelper.Courses);

            Term.ClearData();
            Exam.ClearData();
            WeekDay.ClearData();
            return updated;


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
        public async Task getAllData()
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
           
            Windows.Storage.ApplicationDataContainer localsettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localsettings.Values.ContainsKey("userName") && localsettings.Values.ContainsKey("password"))
            {
                userName = localsettings.Values["userName"].ToString();
                password = localsettings.Values["password"].ToString();
            }
            else
            {
                await showWebConnectionError("尚未输入教务网账号信息，无法同步");
                return;
            }
            #endregion

            if (await login(await getCheckCode()) == HttpStatusCode.OK)
            {
                if (await getGrades()) //不会出现登陆失败，提示object moved
                {
                    await getExams();
                    await getLessons();
                }
                else
                {
                    await showWebConnectionError("每次打开app只能同步一次，请重新打开app");
                }             
            }
            else
            {
                await showWebConnectionError("未能自动登陆，请1.检查网络连接和教务网账号信息；2.重启应用");
            }
        }

        private async Task<bool> getGrades()
        {
            //data eg:<td>(2012-2013-1)-021E0010-0091135-1</td><td>思想道德修养与法律基础</td><td>86</td><td>2.5</td><td>4.10</td><td>&nbsp;</td>
            //regex formulation
            /*
            学期,课程号,课程名,成绩,学分,绩点,补考成绩
            \((\d{4}-\d{4}-\d)\)- 
            (\w{8})-\w*-\d</td><td>
            (\w+.{0,5})</td><td>
            (\w+)</td><td>
            (\d\.*\d*)</td><td>
            (\d\.*\d*)</td><td>
            (\d*\.*\d*)</td>  //补考成绩 （）
            so we get:
            \((\d{4}-\d{4}-\d)\)-(\w{8})-\w*-\d</td><td>(\w+.{0,5})</td><td>(\w+)</td><td>(\d\.*\d*)</td><td>(\d\.*\d*)</td><td>(\d*\.*\d*)
            */

            string postdataGrades = "__VIEWSTATE=dDw0NzAzMzE4ODg7dDw7bDxpPDE%2BOz47bDx0PDtsPGk8Mj47aTw1PjtpPDI1PjtpPDI3PjtpPDQxPjtpPDQzPjtpPDQ1PjtpPDQ3Pjs%2BO2w8dDx0PDt0PGk8MTY%2BO0A8XGU7MjAwMS0yMDAyOzIwMDItMjAwMzsyMDAzLTIwMDQ7MjAwNC0yMDA1OzIwMDUtMjAwNjsyMDA2LTIwMDc7MjAwNy0yMDA4OzIwMDgtMjAwOTsyMDA5LTIwMTA7MjAxMC0yMDExOzIwMTEtMjAxMjsyMDEyLTIwMTM7MjAxMy0yMDE0OzIwMTQtMjAxNTsyMDE1LTIwMTY7PjtAPFxlOzIwMDEtMjAwMjsyMDAyLTIwMDM7MjAwMy0yMDA0OzIwMDQtMjAwNTsyMDA1LTIwMDY7MjAwNi0yMDA3OzIwMDctMjAwODsyMDA4LTIwMDk7MjAwOS0yMDEwOzIwMTAtMjAxMTsyMDExLTIwMTI7MjAxMi0yMDEzOzIwMTMtMjAxNDsyMDE0LTIwMTU7MjAxNS0yMDE2Oz4%2BOz47Oz47dDx0PHA8cDxsPERhdGFUZXh0RmllbGQ7RGF0YVZhbHVlRmllbGQ7PjtsPHh4cTt4cTE7Pj47Pjt0PGk8OD47QDxcZTvmmKU75aSPO%2BefrTvnp4s75YasO%2BefrTvmmpE7PjtAPFxlOzJ85pilOzJ85aSPOzJ855%2BtOzF856eLOzF85YasOzF855%2BtOzF85pqROz4%2BOz47Oz47dDxwPDtwPGw8b25jbGljazs%2BO2w8d2luZG93LnByaW50KClcOzs%2BPj47Oz47dDxwPDtwPGw8b25jbGljazs%2BO2w8d2luZG93LmNsb3NlKClcOzs%2BPj47Oz47dDxAMDw7Ozs7Ozs7Ozs7Pjs7Pjt0PEAwPDs7Ozs7Ozs7Ozs%2BOzs%2BO3Q8QDA8Ozs7Ozs7Ozs7Oz47Oz47dDxwPHA8bDxUZXh0Oz47bDxaSkRYOz4%2BOz47Oz47Pj47Pj47PkBsaV5B%2FCa01w1HqSY%2Fcrk9veyD&Button2=%D4%DA%D0%A3%D1%A7%CF%B0%B3%C9%BC%A8%B2%E9%D1%AF";
            string websiteGrades = "http://jwbinfosys.zju.edu.cn/xscj.aspx?xh=" + userName;

            var gradesData = await getDataFromWeb(websiteGrades, postdataGrades);
            if (gradesData != null)
            {
                var termsDictionary = new Dictionary<string, Term>();
                string patternGrades = @"\((\d{4}-\d{4}-\d)\)-(\w{8})-\w*-\d</td><td>(\w+.{0,5})</td><td>(\w+)</td><td>(\d\.*\d*)</td><td>(\d\.*\d*)</td><td>(\d*\.*\d*)</td>";

                MatchCollection matches = Regex.Matches(gradesData, patternGrades); //await getAllGrades() returns the grades table html

                foreach (Match match in matches)
                {
                    string makeUpExamGrades = match.Groups[7].Value == "" ? "无" : match.Groups[7].Value;
                    var course = new Course(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value,
                        float.Parse(match.Groups[5].Value), float.Parse(match.Groups[6].Value), makeUpExamGrades);

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
            //课程名</td><td>(\w+.{0,5})</td><td>
            //学分(\d\.*\d*)</td><td>.{0,8}</td><td>\w*</td><td>
            //学期(\w{0,4})</td><td>
            //时间(.{0,24})</td><td>          (\d{4}年\d{2}月\d{2}日\(\d{1,2}:\d{2}-\d{1,2}:\d{2}\))          
            //地点(.{0,8})</td><td>
            //座位(.{0,8})</td>
            var now = System.DateTime.Now;
            string websiteExams = "http://jwbinfosys.zju.edu.cn/xskscx.aspx?xh=" + userName;
            string examsData;

            string currentSeason;
            Windows.Storage.ApplicationDataContainer localsettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localsettings.Values.ContainsKey("season"))
            {
                currentSeason = localsettings.Values["season"].ToString();
            }
            else
            {
                int month = System.DateTime.Now.Month;
                if (month >= 8 && month < 11)
                {
                    currentSeason = "秋";
                }
                else if (month >= 11 || month < 2)
                {
                    currentSeason = "冬";
                }
                else if (month >= 2 && month < 4)
                {
                    currentSeason = "春";
                }
                else
                {
                    currentSeason = "夏";
                }
            }

            string postdata;
            switch (currentSeason)
            {
                case "秋":
                    postdata = "__VIEWSTATE=dDwxODk5Mjk0MTA1O3Q8O2w8aTwxPjs%2BO2w8dDw7bDxpPDE%2BO2k8NT47PjtsPHQ8dDxwPHA8bDxEYXRhVGV4dEZpZWxkO0RhdGFWYWx1ZUZpZWxkOz47bDx4bjt4bjs%2BPjs%2BO3Q8aTwzPjtAPDIwMTQtMjAxNTsyMDEzLTIwMTQ7MjAxMi0yMDEzOz47QDwyMDE0LTIwMTU7MjAxMy0yMDE0OzIwMTItMjAxMzs%2BPjtsPGk8MD47Pj47Oz47dDx0PHA8cDxsPERhdGFUZXh0RmllbGQ7RGF0YVZhbHVlRmllbGQ7PjtsPHh4cTt4cTE7Pj47Pjt0PGk8Nz47QDzmmKU75aSPO%2BefrTvnp4s75YasO%2BefrTvmmpE7PjtAPDJ85pilOzJ85aSPOzJ855%2BtOzF856eLOzF85YasOzF855%2BtOzF85pqROz4%2BO2w8aTwxPjs%2BPjs7Pjs%2BPjs%2BPjs%2B9vB0WVKK7d1afj3vqGL6z1CrRV4%3D&xnd=" +
                        now.Year + "-" + (now.Year + 1) + "&xqd=1%7C%C7%EF";
                    break;
                case "冬":
                    postdata = "__VIEWSTATE=dDwxODk5Mjk0MTA1O3Q8O2w8aTwxPjs+O2w8dDw7bDxpPDE+O2k8NT47PjtsPHQ8dDxwPHA8bDxEYXRhVGV4dEZpZWxkO0RhdGFWYWx1ZUZpZWxkOz47bDx4bjt4bjs+Pjs+O3Q8aTwzPjtAPDIwMTQtMjAxNTsyMDEzLTIwMTQ7MjAxMi0yMDEzOz47QDwyMDE0LTIwMTU7MjAxMy0yMDE0OzIwMTItMjAxMzs+PjtsPGk8MD47Pj47Oz47dDx0PHA8cDxsPERhdGFUZXh0RmllbGQ7RGF0YVZhbHVlRmllbGQ7PjtsPHh4cTt4cTE7Pj47Pjt0PGk8Nz47QDzmmKU75aSPO+efrTvnp4s75YasO+efrTvmmpE7PjtAPDJ85pilOzJ85aSPOzJ855+tOzF856eLOzF85YasOzF855+tOzF85pqROz4+O2w8aTwxPjs+Pjs7Pjs+Pjs+Pjs+9vB0WVKK7d1afj3vqGL6z1CrRV4=&xnd=" +
                    now.Year + "-" + (now.Year + 1) + "&xqd=1%7C%B6%AC";
                    break;
                case "春":
                    postdata = "__VIEWSTATE=dDwxODk5Mjk0MTA1O3Q8O2w8aTwxPjs%2BO2w8dDw7bDxpPDE%2BO2k8NT47PjtsPHQ8dDxwPHA8bDxEYXRhVGV4dEZpZWxkO0RhdGFWYWx1ZUZpZWxkOz47bDx4bjt4bjs%2BPjs%2BO3Q8aTwzPjtAPDIwMTQtMjAxNTsyMDEzLTIwMTQ7MjAxMi0yMDEzOz47QDwyMDE0LTIwMTU7MjAxMy0yMDE0OzIwMTItMjAxMzs%2BPjtsPGk8MD47Pj47Oz47dDx0PHA8cDxsPERhdGFUZXh0RmllbGQ7RGF0YVZhbHVlRmllbGQ7PjtsPHh4cTt4cTE7Pj47Pjt0PGk8Nz47QDzmmKU75aSPO%2BefrTvnp4s75YasO%2BefrTvmmpE7PjtAPDJ85pilOzJ85aSPOzJ855%2BtOzF856eLOzF85YasOzF855%2BtOzF85pqROz4%2BO2w8aTwzPjs%2BPjs7Pjs%2BPjs%2BPjs%2BnI%2BrKFh8iifxy0ebnXIK9k6mprA%3D&xnd=" +
                                                (now.Year - 1) + "-" + now.Year + "&xqd=2%7C%B4%BA";
                    break;
                case "夏":
                    postdata = "__VIEWSTATE=dDwxODk5Mjk0MTA1O3Q8O2w8aTwxPjs%2BO2w8dDw7bDxpPDE%2BO2k8NT47PjtsPHQ8dDxwPHA8bDxEYXRhVGV4dEZpZWxkO0RhdGFWYWx1ZUZpZWxkOz47bDx4bjt4bjs%2BPjs%2BO3Q8aTwzPjtAPDIwMTQtMjAxNTsyMDEzLTIwMTQ7MjAxMi0yMDEzOz47QDwyMDE0LTIwMTU7MjAxMy0yMDE0OzIwMTItMjAxMzs%2BPjtsPGk8MD47Pj47Oz47dDx0PHA8cDxsPERhdGFUZXh0RmllbGQ7RGF0YVZhbHVlRmllbGQ7PjtsPHh4cTt4cTE7Pj47Pjt0PGk8Nz47QDzmmKU75aSPO%2BefrTvnp4s75YasO%2BefrTvmmpE7PjtAPDJ85pilOzJ85aSPOzJ855%2BtOzF856eLOzF85YasOzF855%2BtOzF85pqROz4%2BO2w8aTwwPjs%2BPjs7Pjs%2BPjs%2BPjs%2BnUvGO8Clb51TvbCgaqqM8%2Fp9zHI%3D&xnd=" +
                                                (now.Year - 1) + "-" + now.Year + "&xqd=2%7C%CF%C4";
                    break;
                default:
                    postdata = "__VIEWSTATE=dDwxODk5Mjk0MTA1O3Q8O2w8aTwxPjs%2BO2w8dDw7bDxpPDE%2BO2k8NT47PjtsPHQ8dDxwPHA8bDxEYXRhVGV4dEZpZWxkO0RhdGFWYWx1ZUZpZWxkOz47bDx4bjt4bjs%2BPjs%2BO3Q8aTwzPjtAPDIwMTQtMjAxNTsyMDEzLTIwMTQ7MjAxMi0yMDEzOz47QDwyMDE0LTIwMTU7MjAxMy0yMDE0OzIwMTItMjAxMzs%2BPjtsPGk8MD47Pj47Oz47dDx0PHA8cDxsPERhdGFUZXh0RmllbGQ7RGF0YVZhbHVlRmllbGQ7PjtsPHh4cTt4cTE7Pj47Pjt0PGk8Nz47QDzmmKU75aSPO%2BefrTvnp4s75YasO%2BefrTvmmpE7PjtAPDJ85pilOzJ85aSPOzJ855%2BtOzF856eLOzF85YasOzF855%2BtOzF85pqROz4%2BO2w8aTwzPjs%2BPjs7Pjs%2BPjs%2BPjs%2BnI%2BrKFh8iifxy0ebnXIK9k6mprA%3D&xnd=" +
                            (now.Year - 1) + "-" + now.Year + "&xqd=2%7C%B4%BA";
                    break;
            }
            examsData = await getDataFromWeb(websiteExams, postdata) ;

            if (examsData != null)
            {
                string patternExams = @"</td><td>(\w+.{0,5})</td><td>(\d\.*\d*)</td><td>.{0,8}</td><td>\w*</td><td>(\w{0,4})</td><td>\w{5}?(.{0,19})</td><td>(.{0,8})</td><td>(.{0,8})</td>";
                MatchCollection examMatches = Regex.Matches(examsData, patternExams);
                foreach (Match match in examMatches)
                {
                    //string courseName, float credits, string term, string examTime, string examinationPlace, string seatNumber
                    this.Exams.Add(new Exam(match.Groups[1].Value, float.Parse(match.Groups[2].Value), match.Groups[3].Value,
                        match.Groups[4].Value, match.Groups[5].Value, match.Groups[6].Value));
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
            int month = System.DateTime.Now.Month;
            string academicYear,longTerm;
            if (month >= 8 || month <= 2)
            {
                longTerm = "1%7C%C7%EF%A1%A2%B6%AC";
                academicYear = now.Year + "-" + (now.Year + 1);
            }
            else
            {
                longTerm = "2%7C%B4%BA%A1%A2%CF%C4";
                academicYear = (now.Year-1) + "-" + now.Year;
            }

            string websiteLessons = "http://jwbinfosys.zju.edu.cn/xskbcx.aspx?xh="+userName;



            string postdata = "__EVENTARGUMENT=&__EVENTTARGET=xqd&__VIEWSTATE=dDwtMjQ5Nzk5MzUyO3Q8O2w8aTwwPjs%2BO2w8dDw7bDxpPDE%2BO2k8Mz47aTw1PjtpPDg%2BO2k8MTA%2BO2k8MTI%2BO2k8MTQ%2BO2k8MTY%2BO2k8MTg%2BO2k8MjI%2BO2k8MjY%2BO2k8Mjg%2BOz47bDx0PHQ8OztsPGk8MD47Pj47Oz47dDx0PHA8cDxsPERhdGFUZXh0RmllbGQ7RGF0YVZhbHVlRmllbGQ7PjtsPHhuO3huOz4%2BOz47dDxpPDQ%2BO0A8MjAxNS0yMDE2OzIwMTQtMjAxNTsyMDEzLTIwMTQ7MjAxMi0yMDEzOz47QDwyMDE1LTIwMTY7MjAxNC0yMDE1OzIwMTMtMjAxNDsyMDEyLTIwMTM7Pj47bDxpPDE%2BOz4%2BOzs%2BO3Q8dDxwPHA8bDxEYXRhVGV4dEZpZWxkO0RhdGFWYWx1ZUZpZWxkOz47bDxkeXhxO3hxMTs%2BPjs%2BO3Q8aTwyPjtAPOeni%2BOAgeWGrDvmmKXjgIHlpI87PjtAPDF856eL44CB5YasOzJ85pil44CB5aSPOz4%2BO2w8aTwwPjs%2BPjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOWtpuWPt%2B%2B8mjMxMjAxMDM4NDM7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOWnk%2BWQje%2B8muW0lOi2hTs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w85a2m6Zmi77ya5L%2Bh5oGv5LiO55S15a2Q5bel56iL5a2m6ZmiOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznsbso5LiT5LiaKe%2B8mueUteWtkOenkeWtpuS4juaKgOacrzs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w86KGM5pS%2F54%2Bt77ya55S15a2Q56eR5a2m5LiO5oqA5pyvMTIwMzs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8XGU7Pj47Pjs7Pjt0PEAwPHA8cDxsPFZpc2libGU7UGFnZUNvdW50O18hSXRlbUNvdW50O18hRGF0YVNvdXJjZUl0ZW1Db3VudDtEYXRhS2V5czs%2BO2w8bzx0PjtpPDE%2BO2k8OT47aTw5PjtsPD47Pj47Pjs7Ozs7Ozs7Ozs%2BO2w8aTwwPjs%2BO2w8dDw7bDxpPDE%2BO2k8Mj47aTwzPjtpPDQ%2BO2k8NT47aTw2PjtpPDc%2BO2k8OD47aTw5Pjs%2BO2w8dDw7bDxpPDA%2BO2k8MT47aTwyPjtpPDM%2BO2k8ND47aTw1PjtpPDY%2BO2k8Nz47PjtsPHQ8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9MSgyMDE0LTIwMTUtMSktMDMxRTAwMzEyMDEyYTEwNDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPjAzMUUwMDMxXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0xKDIwMTQtMjAxNS0xKS0wMzFFMDAzMTIwMTJhMTA0MzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B5q%2Bb5rO95Lic5oCd5oOz5ZKM5Lit5Zu954m56Imy56S%2B5Lya5Li75LmJ55CG6K665L2T57O75qaC6K66XDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0xKDIwMTQtMjAxNS0xKS0wMzFFMDAzMTIwMTJhMTA0MzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B5p2o5YaA6L6wXDwvYVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznp4vlhqw7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOWRqOS4gOesrDboioJcPGJyXD7lkajkuIDnrKw3LDjoioJcPGJyXD7lkajkuInnrKw5LDEw6IqCOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznjonms4nmm7nlhYnlvarkuozmnJ8tMjA1KOWkmik7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDIwMTQtMDUtMjkgMTM6NDA6Mzc7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDE7Pj47Pjs7Pjs%2BPjt0PDtsPGk8MD47aTwxPjtpPDI%2BO2k8Mz47aTw0PjtpPDU%2BO2k8Nj47aTw3Pjs%2BO2w8dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTQtMjAxNS0xKS0wNTFGMDI1MDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPjA1MUYwMjUwXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTQtMjAxNS0xKS0wNTFGMDI1MDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPuiLseivreS8muivneaKgOW3p1w8L0FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9VCgyMDE0LTIwMTUtMSktMDUxRjAyNTAzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD7lvKDlu7rnkIZcPC9hXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOeni%2BWGrDs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w85ZGo5LiJ56ysNyw46IqCOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznjonms4nmlZk0LTQwOCjlpJopOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwyMDE0LTA1LTMwIDE0OjUyOjIwOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwxOz4%2BOz47Oz47Pj47dDw7bDxpPDA%2BO2k8MT47aTwyPjtpPDM%2BO2k8ND47aTw1PjtpPDY%2BO2k8Nz47PjtsPHQ8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9MSgyMDE0LTIwMTUtMSktMTExMjAxODIyMDEyMTEzMzMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPjExMTIwMTgyXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0xKDIwMTQtMjAxNS0xKS0xMTEyMDE4MjIwMTIxMTMzMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B6YCa5L%2Bh5Y6f55CG77yI5LmZ77yJXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0xKDIwMTQtMjAxNS0xKS0xMTEyMDE4MjIwMTIxMTMzMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B5p2o5bu65LmJXDxiclw%2B6YeR5ZCR5LicXDwvYVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznp4vlhqw7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPCZuYnNwXDs7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPCZuYnNwXDs7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDIwMTQtMDUtMjkgMTI6MDg6MzE7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPDE7Pj47Pjs7Pjs%2BPjt0PDtsPGk8MD47aTwxPjtpPDI%2BO2k8Mz47aTw0PjtpPDU%2BO2k8Nj47aTw3Pjs%2BO2w8dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD0xKDIwMTQtMjAxNS0xKS0xMTEyMDM0MDIwMTIxMTMzMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2BMTExMjAzNDBcPC9BXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPTEoMjAxNC0yMDE1LTEpLTExMTIwMzQwMjAxMjExMzMzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD7nlLXno4HlnLrkuI7lvq7ms6Llrp7pqoxcPC9BXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPTEoMjAxNC0yMDE1LTEpLTExMTIwMzQwMjAxMjExMzMzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD7njovlrZDnq4tcPC9hXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOeni%2BWGrDs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w85ZGo5Zub56ysNiw3LDgsOeiKgnvljZXlkah9Oz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDzntKvph5HmuK%2FkuJw0LTIyNzs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8MjAxNC0wNS0yOSAxMjowODozMTs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8MTs%2BPjs%2BOzs%2BOz4%2BO3Q8O2w8aTwwPjtpPDE%2BO2k8Mj47aTwzPjtpPDQ%2BO2k8NT47aTw2PjtpPDc%2BOz47bDx0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPVQoMjAxNC0yMDE1LTEpLTExMTg4MjMwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2BMTExODgyMzBcPC9BXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPVQoMjAxNC0yMDE1LTEpLTExMTg4MjMwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B55S15a2Q55S16Lev5a6J6KOF5LiO6LCD6K%2BV5a6e6Le1XDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTQtMjAxNS0xKS0xMTE4ODIzMDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPuadjumUoeWNjlw8L2FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w855%2BtOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwmbmJzcFw7Oz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwmbmJzcFw7Oz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwyMDE0LTA1LTMwIDE0OjQ5OjQ0Oz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwxOz4%2BOz47Oz47Pj47dDw7bDxpPDA%2BO2k8MT47aTwyPjtpPDM%2BO2k8ND47aTw1PjtpPDY%2BO2k8Nz47PjtsPHQ8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9VCgyMDE0LTIwMTUtMSktMTExOTM1MTAzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD4xMTE5MzUxMFw8L0FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9VCgyMDE0LTIwMTUtMSktMTExOTM1MTAzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD7kv6Hmga%2FnlLXlrZDlrabniannkIbln7rnoYBcPC9BXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPVQoMjAxNC0yMDE1LTEpLTExMTkzNTEwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B5p2o5Yas5pmTXDwvYVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznp4s7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOWRqOS4ieesrDMsNCw16IqCXDxiclw%2B5ZGo5LqU56ysMyw0LDXoioI7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOeOieazieaVmTctMjA4KOWkmilcPGJyXD7njonms4nmlZk3LTIwOCjlpJopOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwyMDE0LTA1LTMwIDE4OjE4OjAxOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwxOz4%2BOz47Oz47Pj47dDw7bDxpPDA%2BO2k8MT47aTwyPjtpPDM%2BO2k8ND47aTw1PjtpPDY%2BO2k8Nz47PjtsPHQ8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9VCgyMDE0LTIwMTUtMSktMTExOTM2MTAzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD4xMTE5MzYxMFw8L0FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8XDxBIGhyZWY9JyMnIG9uY2xpY2s9IndpbmRvdy5vcGVuKCd4c3hqcy5hc3B4P3hra2g9VCgyMDE0LTIwMTUtMSktMTExOTM2MTAzMTIwMTAzODQzJywna2NiJywndG9vbGJhcj0wLGxvY2F0aW9uPTAsZGlyZWN0b3JpZXM9MCxzdGF0dXM9MCxtZW51YmFyPTAsc2Nyb2xsYmFycz0xLHJlc2l6YWJsZT0xJykiXD7lm7rkvZPniannkIbkuI7ljYrlr7zkvZPniannkIZcPC9BXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPVQoMjAxNC0yMDE1LTEpLTExMTkzNjEwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B5rGq5bCP55%2BlXDxiclw%2B5p6X5pe26IOcXDwvYVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznp4vlhqw7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOWRqOS6jOesrDEsMuiKglw8YnJcPuWRqOWbm%2BesrDMsNCw16IqCOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDznjonms4nmlZk3LTMwMijlpJop5Yi25Zu%2BXDxiclw%2B546J5rOJ5pWZNy0zMDIo5aSaKeWItuWbvjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8MjAxNC0wNS0zMCAxODozNzoyMjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8MTs%2BPjs%2BOzs%2BOz4%2BO3Q8O2w8aTwwPjtpPDE%2BO2k8Mj47aTwzPjtpPDQ%2BO2k8NT47aTw2PjtpPDc%2BOz47bDx0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPVQoMjAxNC0yMDE1LTEpLTY3MTIwMDIwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2BNjcxMjAwMjBcPC9BXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPVQoMjAxNC0yMDE1LTEpLTY3MTIwMDIwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B5b6u55S15a2Q5Zmo5Lu25LiO55S16LevXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTQtMjAxNS0xKS02NzEyMDAyMDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPuaxquWwj%2BefpVw8L2FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w856eL5YasOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDzlkajkuIDnrKwzLDQsNeiKgjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8546J5rOJ5pWZNy0yMDgo5aSaKTs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8MjAxNC0wNS0zMCAxNTozMTo1MDs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w8MTs%2BPjs%2BOzs%2BOz4%2BO3Q8O2w8aTwwPjtpPDE%2BO2k8Mj47aTwzPjtpPDQ%2BO2k8NT47aTw2PjtpPDc%2BOz47bDx0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPVQoMjAxNC0yMDE1LTEpLTY3MTIwMDgwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2BNjcxMjAwODBcPC9BXD47Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPFw8QSBocmVmPScjJyBvbmNsaWNrPSJ3aW5kb3cub3BlbigneHN4anMuYXNweD94a2toPVQoMjAxNC0yMDE1LTEpLTY3MTIwMDgwMzEyMDEwMzg0MycsJ2tjYicsJ3Rvb2xiYXI9MCxsb2NhdGlvbj0wLGRpcmVjdG9yaWVzPTAsc3RhdHVzPTAsbWVudWJhcj0wLHNjcm9sbGJhcnM9MSxyZXNpemFibGU9MScpIlw%2B5pWw5YC85YiG5p6Q5pa55rOVXDwvQVw%2BOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDxcPEEgaHJlZj0nIycgb25jbGljaz0id2luZG93Lm9wZW4oJ3hzeGpzLmFzcHg%2FeGtraD1UKDIwMTQtMjAxNS0xKS02NzEyMDA4MDMxMjAxMDM4NDMnLCdrY2InLCd0b29sYmFyPTAsbG9jYXRpb249MCxkaXJlY3Rvcmllcz0wLHN0YXR1cz0wLG1lbnViYXI9MCxzY3JvbGxiYXJzPTEscmVzaXphYmxlPTEnKSJcPum%2BmuWwj%2BiwqFw8L2FcPjs%2BPjs%2BOzs%2BO3Q8cDxwPGw8VGV4dDs%2BO2w85YasOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDzlkajkuIDnrKwxLDLoioJcPGJyXD7lkajkuInnrKwxLDLoioI7Pj47Pjs7Pjt0PHA8cDxsPFRleHQ7PjtsPOeOieazieaVmTctMTA4KOWkmilcPGJyXD7njonms4nmlZk3LTEwOCjlpJopOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwyMDE0LTA1LTMwIDE0OjQ3OjUwOz4%2BOz47Oz47dDxwPHA8bDxUZXh0Oz47bDwxOz4%2BOz47Oz47Pj47Pj47Pj47dDxAMDxwPHA8bDxQYWdlQ291bnQ7XyFJdGVtQ291bnQ7XyFEYXRhU291cmNlSXRlbUNvdW50O0RhdGFLZXlzOz47bDxpPDE%2BO2k8MD47aTwwPjtsPD47Pj47Pjs7Ozs7Ozs7Ozs%2BOzs%2BO3Q8O2w8aTwzPjs%2BO2w8dDxAMDw7Ozs7Ozs7Ozs7Pjs7Pjs%2BPjs%2BPjs%2BPjs%2BTJd4hz5ARqKTlLzY2jjIurAH8E8%3D"+
                "&kcxx=&xnd="+academicYear+"&xqd="+longTerm+"&xxms=%C1%D0%B1%ED";

            var lessonsData = await getDataFromWeb(websiteLessons, postdata);          
            if (lessonsData != null)
            {
                string patternLessons = @"[0-9A-Z]+</A></td><td><A.{120,200}resizable=1.{0,4}>([\u4e00-\u9fa5]+.{0,5})</A></td><td><A.{120,200}resizable=1.{0,4}>([\u4e00-\u9fa5]{1,5}.{0,15})</a></td><td>([\u4e00-\u9fa5]{1,4})</td><td>(周.{0,80})</td><td>\d{4}";
                MatchCollection lessonMatches = Regex.Matches(lessonsData, patternLessons);

                foreach (Match match in lessonMatches)
                {
                    string[] splitString = { "<br>", "</td><td>" };
                    var strings = match.Groups[4].Value.Split(splitString, StringSplitOptions.RemoveEmptyEntries).ToList();
                    string day1, class1, time1, lessonPlace1, day2, class2, time2, lessonPlace2;

                    day1 = strings[0].Substring(0, 2);
                    class1 = strings[0].Substring(3);
                    time1 = getTime(class1);
                    if (strings.Count == 2)// for some lesson, there might obe 2 or 3 or 4 substrings, depend on the website. two place are the same and one of it didn't appear
                    {
                        lessonPlace1 = strings[1];
                        day2 = "";
                        class2 = "";
                        time2 = "";
                        lessonPlace2 = "";
                    }
                    else
                    {
                        lessonPlace1 = strings[2];
                        day2 = strings[1].Substring(0, 2);
                        class2 = strings[1].Substring(3);
                        time2 = getTime(class2);

                        if (strings.Count == 4)
                        {
                            lessonPlace2 = strings[3];
                        }
                        else
                        {
                            lessonPlace2 = strings[2];
                        }
                    }

                    var lesson = new Lesson(match.Groups[1].Value, match.Groups[2].Value.Replace("<br>", ","), match.Groups[3].Value, day1, class1, time1, lessonPlace1, day2, class2, time2, lessonPlace2);
                    this.Lessons.Add(lesson);
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
            string startTime;
            var firstClass = classes.Substring(0, classes.IndexOf(','));
            switch (firstClass)
            {
                case "1":
                    startTime = "8:00";
                    break;
                case "2":
                    startTime = "8:50";
                    break;
                case "3":
                    startTime = "9:50";
                    break;
                case "4":
                    startTime = "10:40";
                    break;
                case "5":
                    startTime = "11:30";
                    break;
                case "6":
                    startTime = "13:15";
                    break;
                case "7":
                    startTime = "14:05";
                    break;
                case "8":
                    startTime = "14:55";
                    break;
                case "9":
                    startTime = "15:55";
                    break;
                case "10":
                    startTime = "16:45";
                    break;
                case "11":
                    startTime = "18:30";
                    break;
                case "12":
                    startTime = "19:20";
                    break;
                case "13":
                    startTime = "20:10";
                    break;
                default:
                    startTime = "00:00";
                    break;
            }

            string endTime;
            var lastClass = classes.Remove(classes.LastIndexOf('节')).Substring(1 + classes.LastIndexOf(','));
            switch (lastClass)
            {
                case "1":
                    endTime = "8:45";
                    break;
                case "2":
                    endTime = "9:35";
                    break;
                case "3":
                    endTime = "10:35";
                    break;
                case "4":
                    endTime = "11:25";
                    break;
                case "5":
                    endTime = "12:15";
                    break;
                case "6":
                    endTime = "14:00";
                    break;
                case "7":
                    endTime = "14:50";
                    break;
                case "8":
                    endTime = "15:40";
                    break;
                case "9":
                    endTime = "16:40";
                    break;
                case "10":
                    endTime = "17:30";
                    break;
                case "11":
                    endTime = "19:15";
                    break;
                case "12":
                    endTime = "20:05";
                    break;
                case "13":
                    endTime = "20:55";
                    break;
                default:
                    endTime = "00:00";
                    break;
            }
            return startTime + "-" + endTime;
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
            //StorageFile checkCodeImage = await ApplicationData.Current.LocalFolder.CreateFileAsync("checkCode.gif", CreationCollisionOption.ReplaceExisting);
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add(".png");
            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.FileTypeFilter.Add(".jpeg");
            folderPicker.FileTypeFilter.Add(".bmp");
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            StorageFile checkCodeImage = await folder.CreateFileAsync("checkCode.gif", CreationCollisionOption.ReplaceExisting);


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

            //using (var responseStream = await checkCodeImage.OpenAsync(FileAccessMode.ReadWrite))
            //{
            //    var decoder = await BitmapDecoder.CreateAsync(responseStream);
            //    PixelDataProvider pix = await decoder.GetPixelDataAsync(
            //        BitmapPixelFormat.Bgra8,
            //        BitmapAlphaMode.Straight,
            //        transform,
            //        ExifOrientationMode.IgnoreExifOrientation,
            //        ColorManagementMode.ColorManageToSRgb);
            //    pixels = pix.DetachPixelData();
            //}

            //StorageFile testcheckCodeImage = await folder.CreateFileAsync("testcheckCode.jpg", CreationCollisionOption.ReplaceExisting);
            //using (Stream testStream = pixels.AsBuffer().AsStream())
            //using (var testresponseStream = await testcheckCodeImage.OpenStreamForWriteAsync())
            //{
            //    await testStream.CopyToAsync(testresponseStream);
            //    //return "";
            //    //var testdecoder = await BitmapDecoder.CreateAsync(testresponseStream.AsRandomAccessStream());
            //    //SoftwareBitmap testcheckCode = await testdecoder.GetSoftwareBitmapAsync();
            //    //////recognize the checkcode with orc           
            //    //OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("zh-CN"));
            //    //var ocrResult = await ocrEngine.RecognizeAsync(testcheckCode);

            //    //if (ocrResult.Lines != null)
            //    //{
            //    //    return ocrResult.Lines[0].Words[0].Text;
            //    //}
            //}
            using (var responseStream = await checkCodeImage.OpenAsync(FileAccessMode.ReadWrite))
            {
                var testdecoder = await BitmapDecoder.CreateAsync(responseStream);
                SoftwareBitmap testcheckCode = await testdecoder.GetSoftwareBitmapAsync();
                ////recognize the checkcode with orc           
                OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("zh-CN"));
                var ocrResult = await ocrEngine.RecognizeAsync(testcheckCode);

                if (ocrResult.Lines != null)
                {
                    //return ocrResult.Lines[0].Words[0].Text;
                    return "";
                }
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

        private async Task<string> getDataFromWeb(string website,string postdata)
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
                string content = await streamReader.ReadToEndAsync();
                if (!content.Contains("Object moved"))
                {
                    int gradesDataStartIndex = content.IndexOf("</tr><tr>");
                    if (gradesDataStartIndex > 0)
                    {
                        int gradesDataEndIndex = content.IndexOf("</table>", gradesDataStartIndex);
                        return (content.Remove(gradesDataEndIndex - 2).Remove(0, gradesDataStartIndex + 5)).Replace("&nbsp;", "");
                    }
                }
                else
                {
                   return null;
                }
                return "";               
            }
        }
    }
}
