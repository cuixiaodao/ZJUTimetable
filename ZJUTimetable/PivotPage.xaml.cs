using ZJUTimetable.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZJUTimetable.DataModel;
using System.Threading.Tasks;
using System.Text;
using Windows.Storage;

// “透视应用程序”模板在 http://go.microsoft.com/fwlink/?LinkID=391641 上有介绍

namespace ZJUTimetable
{
    public sealed partial class PivotPage : Page
    {
        private const string GradePoints = "GradePoints";
        private const string GradesTrend = "GradesTrend";
        private const string Exams = "Exams";
        private const string WeekDays = "WeekDays";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// 获取与此 <see cref="Page"/> 关联的 <see cref="NavigationHelper"/>。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// 获取此 <see cref="Page"/> 的视图模型。
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// 使用在导航过程中传递的内容填充页。在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源；通常为 <see cref="NavigationHelper"/>。
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 的字典。首次访问页面时，该状态将为 null。</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。值必须符合序列化
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        /// <param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/>。</param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: 在此处保存页面的唯一状态。
        }

        /// <summary>
        /// 滚动到视图中后，为第二个数据透视项加载内容。
        /// </summary>
        private async void SecondPivot_Loaded(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel[GradesTrend] = await Term.GetTermsAsync();
            showStatisticalInfo();
        }

        private async void ThirdPivot_Loaded(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel[GradePoints] = (await Term.GetTermsAsync()).Reverse<Term>();
        }

        private async void ForthPivot_Loaded(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel[Exams] = await Exam.getExamsAsync();
        }

        private void showStatisticalInfo()
        {
            if (defaultViewModel.ContainsKey(GradesTrend))
            {
                TotalCreditsTextBlock.Text = string.Format("总学分:{0}", Term.statistics[0]);
                AveragePointsTextBlock.Text = string.Format("平均绩点:{0:F}", Term.statistics[1] / Term.statistics[0]);
                CommanCognitionTextBlock.Text = string.Format("通识总学分（不包括通核、新生研讨课、学科导论）{0},其中人文社科组{1}学分",
                    Term.statistics[2], Term.statistics[3]);
            }
        }

        #region NavigationHelper 注册

        /// <summary>
        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// <para>
        /// 应将页面特有的逻辑放入用于
        /// <see cref="NavigationHelper.LoadState"/>
        /// 和 <see cref="NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。
        /// </para>
        /// </summary>
        /// <param name="e">提供导航方法数据和
        /// 无法取消导航请求的事件处理程序。</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            var weekdays = await WeekDay.GetWeekdaysAsync();

            this.DefaultViewModel[WeekDays] = weekdays;                    

            string dayOfWeek = getDayOfWeek();
            for (int i = 0; i < weekdays.Count; i++)
            {
                if (weekdays[i].DayName == dayOfWeek)
                {
                    Lessons.ScrollIntoView(Lessons.Items[Math.Min(i+1,weekdays.Count-1)]);
                    break;
                }
            }

            getTermAndWeekInfo();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private void getTermAndWeekInfo()
        {
            Windows.Storage.ApplicationDataContainer localsettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            StringBuilder currentTermAndWeek = new StringBuilder();

            currentTermAndWeek.Append( "你好 :)，今天是" + getDayOfWeek());
            if (localsettings.Values.ContainsKey("season"))
            {
                currentTermAndWeek.Append("," + localsettings.Values["season"].ToString() + "学期");
            }
            if (localsettings.Values.ContainsKey("weekNumber") && localsettings.Values.ContainsKey("weekDate"))
            {
                int weekNumber = Math.Abs((int)localsettings.Values["weekNumber"] + (System.DateTime.Today.DayOfYear - (int)localsettings.Values["weekDate"]) / 7) % 8 + 1;
                currentTermAndWeek.Append("第" + weekNumber.ToString() + "周了");
            }
            TermAndWeekInfomation.Text = currentTermAndWeek.ToString();

        }
        private string getDayOfWeek()
        {
           string[] daysOfWeek=new string[7] { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };
           return daysOfWeek[(int)System.DateTime.Today.DayOfWeek];
        }
        #endregion

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            showProcessorRing();
            await DataHelper.UpdateDataAsync();

            var gradesTrend = await Term.GetTermsAsync();
            this.DefaultViewModel[GradesTrend] = gradesTrend;
            this.DefaultViewModel[GradePoints] = gradesTrend.Reverse<Term>();
            this.DefaultViewModel[Exams] = await Exam.getExamsAsync();
            this.DefaultViewModel[WeekDays] = await WeekDay.GetWeekdaysAsync();
            showStatisticalInfo();
            hideProcessorRing();
        }

        private void AcountButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Account));
        }

        private async void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.Email.EmailMessage mail = new Windows.ApplicationModel.Email.EmailMessage();
            mail.Subject = "浙大课表 Windows客户端问题反馈与功能建议";
            mail.Body = "感谢参与！";
            mail.To.Add(new Windows.ApplicationModel.Email.EmailRecipient("cuichao@zju.edu.cn", "cuichao"));
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(mail);
        }

        private async void CommentButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:reviewapp?appid={0}", Windows.ApplicationModel.Store.CurrentApp.AppId)));
        }

        private void showProcessorRing()
        {
            ProcessorRing.Visibility = Visibility.Visible;
            ProcessorRing.IsBusy = true;
        }
        private void hideProcessorRing()
        {
            ProcessorRing.IsBusy = false;
            ProcessorRing.Visibility = Visibility.Collapsed;
        }


    }
}
