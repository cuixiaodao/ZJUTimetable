// Decompiled with JetBrains decompiler
// Type: ZJUTimetable_Windows_.MainPage
// Assembly: ZJUTimetable(Windows), Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ADA35F28-3635-429E-8BB4-CAF05C294EA5
// Assembly location: D:\OneDrive\Documents\Visual Studio 2015\Projects\ZJUTimetable\ZJUTimetable(Windows)\bin\x86\Debug\ZJUTimetable(Windows).exe

using Syncfusion.UI.Xaml.Controls.Notification;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Navigation;
using ZJUTimetable_Windows_.Common;
using ZJUTimetable_Windows_.DataModel;

namespace ZJUTimetable_Windows_
{
  public sealed class MainPage : Page, IComponentConnector, IComponentConnector2
  {
    private const string GradePoints = "GradePoints";
    private const string GradesTrend = "GradesTrend";
    private const string Exams = "Exams";
    private const string WeekDays = "WeekDays";
    private readonly NavigationHelper navigationHelper;
    private readonly ObservableDictionary defaultViewModel;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private ToggleButton splitViewToggle;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private SfBusyIndicator ProcessorRing;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private Grid ClassesGrid;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private Grid GradesTrendGrid;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private Grid GradesDetailGrid;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private Grid ExamsGrid;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private Grid SettingPanel;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private ComboBox Season;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private ComboBox CurrentWeekNumber;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private ComboBoxItem FirstTerm;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private ComboBoxItem SecondTerm;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private Image testImage;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private Button SaveButton;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private PasswordBox MyPasswordBox;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private TextBox UserName;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private CollectionViewSource TermGroupedGradess;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private ListView Grades;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private TextBlock CommanCognitionTextBlock;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private TextBlock TotalCreditsTextBlock;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private TextBlock AveragePointsTextBlock;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private CollectionViewSource WeekDayGroupedLessons;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private DataTemplate ItemTemplate;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private TextBlock TermAndWeekInfomation;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private SemanticZoom semanticZoom;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private ListView Lessons;
    [GeneratedCode("Microsoft.Windows.UI.Xaml.Build.Tasks", " 14.0.0.0")]
    private bool _contentLoaded;
    private MainPage.IMainPage_Bindings Bindings;

    public NavigationHelper NavigationHelper
    {
      get
      {
        return this.navigationHelper;
      }
    }

    public ObservableDictionary DefaultViewModel
    {
      get
      {
        return this.defaultViewModel;
      }
    }

    public MainPage()
    {
      base.\u002Ector();
      this.InitializeComponent();
      this.put_NavigationCacheMode((NavigationCacheMode) 1);
      this.navigationHelper = new NavigationHelper((Page) this);
      this.navigationHelper.LoadState += new LoadStateEventHandler(this.NavigationHelper_LoadState);
      this.navigationHelper.SaveState += new SaveStateEventHandler(this.NavigationHelper_SaveState);
    }

    private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
    }

    private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
    }

    protected virtual async void OnNavigatedTo(NavigationEventArgs e)
    {
      this.navigationHelper.OnNavigatedTo(e);
      List<WeekDay> list = await WeekDay.GetWeekdaysAsync();
      List<WeekDay> weekdays = list;
      list = (List<WeekDay>) null;
      this.DefaultViewModel["WeekDays"] = (object) weekdays;
      string dayOfWeek = this.getDayOfWeek();
      for (int i = 0; i < weekdays.Count; ++i)
      {
        if (weekdays[i].DayName == dayOfWeek)
        {
          ((ListViewBase) this.Lessons).ScrollIntoView(((IList<object>) ((ItemsControl) this.Lessons).get_Items())[Math.Min(i + 1, weekdays.Count - 1)]);
          break;
        }
      }
      this.getTermAndWeekInfo();
    }

    protected virtual void OnNavigatedFrom(NavigationEventArgs e)
    {
      this.navigationHelper.OnNavigatedFrom(e);
    }

    private void ClassesGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
      this.changePanel(0);
      this.getTermAndWeekInfo();
    }

    private async void GradesTrendGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
      this.changePanel(1);
      ObservableDictionary observableDictionary = this.DefaultViewModel;
      List<Term> list = await Term.GetTermsAsync();
      object obj = (object) list;
      observableDictionary["GradesTrend"] = obj;
      observableDictionary = (ObservableDictionary) null;
      list = (List<Term>) null;
      obj = (object) null;
      this.showStatisticalInfo();
    }

    private async void GradesDetailGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
      this.changePanel(2);
      ObservableDictionary observableDictionary = this.DefaultViewModel;
      List<Term> list = await Term.GetTermsAsync();
      IEnumerable<Term> source = (IEnumerable<Term>) list;
      object obj = (object) Enumerable.Reverse<Term>(source);
      observableDictionary["GradePoints"] = obj;
      observableDictionary = (ObservableDictionary) null;
      list = (List<Term>) null;
      source = (IEnumerable<Term>) null;
      obj = (object) null;
    }

    private async void ExamsGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
      this.changePanel(3);
      ObservableDictionary observableDictionary = this.DefaultViewModel;
      List<Exam> list = await Exam.getExamsAsync();
      object obj = (object) list;
      observableDictionary["Exams"] = obj;
      observableDictionary = (ObservableDictionary) null;
      list = (List<Exam>) null;
      obj = (object) null;
    }

    private void SettingGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
      this.changePanel(4);
      int month = DateTime.Now.Month;
      if (month >= 2 && month <= 8)
      {
        ((ContentControl) this.FirstTerm).put_Content((object) "春");
        ((ContentControl) this.SecondTerm).put_Content((object) "夏");
      }
      else
      {
        ((ContentControl) this.FirstTerm).put_Content((object) "秋");
        ((ContentControl) this.SecondTerm).put_Content((object) "冬");
      }
      ApplicationDataContainer localSettings = ApplicationData.get_Current().get_LocalSettings();
      if (((IDictionary<string, object>) localSettings.get_Values()).ContainsKey("userName") && ((IDictionary<string, object>) localSettings.get_Values()).ContainsKey("password"))
      {
        this.UserName.put_Text(((IDictionary<string, object>) localSettings.get_Values())["userName"].ToString());
        this.MyPasswordBox.put_Password(((IDictionary<string, object>) localSettings.get_Values())["password"].ToString());
        ((ContentControl) this.SaveButton).put_Content((object) "删除");
      }
      if (((IDictionary<string, object>) localSettings.get_Values()).ContainsKey("season"))
      {
        string str = ((IDictionary<string, object>) localSettings.get_Values())["season"].ToString();
        ((Selector) this.Season).put_SelectedIndex(str == "春" || str == "秋" ? 0 : 1);
      }
      if (!((IDictionary<string, object>) localSettings.get_Values()).ContainsKey("weekNumber") || !((IDictionary<string, object>) localSettings.get_Values()).ContainsKey("weekDate"))
        return;
      int num = (int) ((IDictionary<string, object>) localSettings.get_Values())["weekDate"];
      ((Selector) this.CurrentWeekNumber).put_SelectedIndex(Math.Abs((int) ((IDictionary<string, object>) localSettings.get_Values())["weekNumber"] + (DateTime.Today.DayOfYear - num) / 7) % 8);
    }

    private async void RefreshData_Tapped(object sender, TappedRoutedEventArgs e)
    {
      this.showProcessorRing();
      int num = await DataHelper.UpdateDataAsync() ? 1 : 0;
      List<Term> list1 = await Term.GetTermsAsync();
      List<Term> gradesTrend = list1;
      list1 = (List<Term>) null;
      this.DefaultViewModel["GradesTrend"] = (object) gradesTrend;
      this.DefaultViewModel["GradePoints"] = (object) Enumerable.Reverse<Term>((IEnumerable<Term>) gradesTrend);
      ObservableDictionary observableDictionary1 = this.DefaultViewModel;
      List<Exam> list2 = await Exam.getExamsAsync();
      object obj1 = (object) list2;
      observableDictionary1["Exams"] = obj1;
      observableDictionary1 = (ObservableDictionary) null;
      list2 = (List<Exam>) null;
      obj1 = (object) null;
      ObservableDictionary observableDictionary2 = this.DefaultViewModel;
      List<WeekDay> list3 = await WeekDay.GetWeekdaysAsync();
      object obj2 = (object) list3;
      observableDictionary2["WeekDays"] = obj2;
      observableDictionary2 = (ObservableDictionary) null;
      list3 = (List<WeekDay>) null;
      obj2 = (object) null;
      this.showStatisticalInfo();
      this.hideProcessorRing();
    }

    private void changePanel(int v)
    {
      switch (v)
      {
        case 0:
          ((UIElement) this.ClassesGrid).put_Visibility((Visibility) 0);
          Grid grid1 = this.GradesTrendGrid;
          Grid grid2 = this.GradesDetailGrid;
          Grid grid3 = this.ExamsGrid;
          Visibility visibility1;
          ((UIElement) this.SettingPanel).put_Visibility((Visibility) (int) (visibility1 = (Visibility) 1));
          Visibility visibility2;
          Visibility visibility3 = visibility2 = visibility1;
          ((UIElement) grid3).put_Visibility(visibility2);
          Visibility visibility4;
          Visibility visibility5 = visibility4 = visibility3;
          ((UIElement) grid2).put_Visibility(visibility4);
          Visibility visibility6 = visibility5;
          ((UIElement) grid1).put_Visibility(visibility6);
          break;
        case 1:
          ((UIElement) this.GradesTrendGrid).put_Visibility((Visibility) 0);
          Grid grid4 = this.ClassesGrid;
          Grid grid5 = this.GradesDetailGrid;
          Grid grid6 = this.ExamsGrid;
          Visibility visibility7;
          ((UIElement) this.SettingPanel).put_Visibility((Visibility) (int) (visibility7 = (Visibility) 1));
          Visibility visibility8;
          Visibility visibility9 = visibility8 = visibility7;
          ((UIElement) grid6).put_Visibility(visibility8);
          Visibility visibility10;
          Visibility visibility11 = visibility10 = visibility9;
          ((UIElement) grid5).put_Visibility(visibility10);
          Visibility visibility12 = visibility11;
          ((UIElement) grid4).put_Visibility(visibility12);
          break;
        case 2:
          ((UIElement) this.GradesDetailGrid).put_Visibility((Visibility) 0);
          Grid grid7 = this.GradesTrendGrid;
          Grid grid8 = this.ClassesGrid;
          Grid grid9 = this.ExamsGrid;
          Visibility visibility13;
          ((UIElement) this.SettingPanel).put_Visibility((Visibility) (int) (visibility13 = (Visibility) 1));
          Visibility visibility14;
          Visibility visibility15 = visibility14 = visibility13;
          ((UIElement) grid9).put_Visibility(visibility14);
          Visibility visibility16;
          Visibility visibility17 = visibility16 = visibility15;
          ((UIElement) grid8).put_Visibility(visibility16);
          Visibility visibility18 = visibility17;
          ((UIElement) grid7).put_Visibility(visibility18);
          break;
        case 3:
          ((UIElement) this.ExamsGrid).put_Visibility((Visibility) 0);
          Grid grid10 = this.GradesTrendGrid;
          Grid grid11 = this.GradesDetailGrid;
          Grid grid12 = this.ClassesGrid;
          Visibility visibility19;
          ((UIElement) this.SettingPanel).put_Visibility((Visibility) (int) (visibility19 = (Visibility) 1));
          Visibility visibility20;
          Visibility visibility21 = visibility20 = visibility19;
          ((UIElement) grid12).put_Visibility(visibility20);
          Visibility visibility22;
          Visibility visibility23 = visibility22 = visibility21;
          ((UIElement) grid11).put_Visibility(visibility22);
          Visibility visibility24 = visibility23;
          ((UIElement) grid10).put_Visibility(visibility24);
          break;
        case 4:
          ((UIElement) this.SettingPanel).put_Visibility((Visibility) 0);
          Grid grid13 = this.GradesTrendGrid;
          Grid grid14 = this.GradesDetailGrid;
          Grid grid15 = this.ExamsGrid;
          Visibility visibility25;
          ((UIElement) this.ClassesGrid).put_Visibility((Visibility) (int) (visibility25 = (Visibility) 1));
          Visibility visibility26;
          Visibility visibility27 = visibility26 = visibility25;
          ((UIElement) grid15).put_Visibility(visibility26);
          Visibility visibility28;
          Visibility visibility29 = visibility28 = visibility27;
          ((UIElement) grid14).put_Visibility(visibility28);
          Visibility visibility30 = visibility29;
          ((UIElement) grid13).put_Visibility(visibility30);
          break;
        default:
          ((UIElement) this.ClassesGrid).put_Visibility((Visibility) 0);
          Grid grid16 = this.GradesTrendGrid;
          Grid grid17 = this.GradesDetailGrid;
          Grid grid18 = this.ExamsGrid;
          Visibility visibility31;
          ((UIElement) this.SettingPanel).put_Visibility((Visibility) (int) (visibility31 = (Visibility) 1));
          Visibility visibility32;
          Visibility visibility33 = visibility32 = visibility31;
          ((UIElement) grid18).put_Visibility(visibility32);
          Visibility visibility34;
          Visibility visibility35 = visibility34 = visibility33;
          ((UIElement) grid17).put_Visibility(visibility34);
          Visibility visibility36 = visibility35;
          ((UIElement) grid16).put_Visibility(visibility36);
          break;
      }
    }

    private void showStatisticalInfo()
    {
      if (!this.defaultViewModel.ContainsKey("GradesTrend"))
        return;
      TextBlock textBlock1 = this.TotalCreditsTextBlock;
      string format1 = "总学分:{0}";
      object[] objArray1 = new object[1];
      int index1 = 0;
      // ISSUE: variable of a boxed type
      __Boxed<float> local1 = (ValueType) Term.statistics[0];
      objArray1[index1] = (object) local1;
      string str1 = string.Format(format1, objArray1);
      textBlock1.put_Text(str1);
      TextBlock textBlock2 = this.AveragePointsTextBlock;
      string format2 = "平均绩点:{0:F}";
      object[] objArray2 = new object[1];
      int index2 = 0;
      // ISSUE: variable of a boxed type
      __Boxed<float> local2 = (ValueType) (float) ((double) Term.statistics[1] / (double) Term.statistics[0]);
      objArray2[index2] = (object) local2;
      string str2 = string.Format(format2, objArray2);
      textBlock2.put_Text(str2);
      TextBlock textBlock3 = this.CommanCognitionTextBlock;
      string format3 = "通识总学分（不包括通核、新生研讨课、学科导论）{0},其中人文社科组{1}学分";
      object[] objArray3 = new object[2];
      int index3 = 0;
      // ISSUE: variable of a boxed type
      __Boxed<float> local3 = (ValueType) Term.statistics[2];
      objArray3[index3] = (object) local3;
      int index4 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<float> local4 = (ValueType) Term.statistics[3];
      objArray3[index4] = (object) local4;
      string str3 = string.Format(format3, objArray3);
      textBlock3.put_Text(str3);
    }

    private void getTermAndWeekInfo()
    {
      ApplicationDataContainer localSettings = ApplicationData.get_Current().get_LocalSettings();
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("你好 ^.^，今天是" + this.getDayOfWeek() + ",");
      if (((IDictionary<string, object>) localSettings.get_Values()).ContainsKey("season"))
        stringBuilder.Append(((IDictionary<string, object>) localSettings.get_Values())["season"].ToString() + "学期");
      if (((IDictionary<string, object>) localSettings.get_Values()).ContainsKey("weekNumber") && ((IDictionary<string, object>) localSettings.get_Values()).ContainsKey("weekDate"))
      {
        int num = Math.Abs((int) ((IDictionary<string, object>) localSettings.get_Values())["weekNumber"] + (DateTime.Today.DayOfYear - (int) ((IDictionary<string, object>) localSettings.get_Values())["weekDate"]) / 7) % 8 + 1;
        stringBuilder.Append("第" + num.ToString() + "周");
      }
      this.TermAndWeekInfomation.put_Text(stringBuilder.ToString());
    }

    private string getDayOfWeek()
    {
      string str;
      switch (DateTime.Today.DayOfWeek)
      {
        case DayOfWeek.Sunday:
          str = "周日";
          break;
        case DayOfWeek.Monday:
          str = "周一";
          break;
        case DayOfWeek.Tuesday:
          str = "周二";
          break;
        case DayOfWeek.Wednesday:
          str = "周三";
          break;
        case DayOfWeek.Thursday:
          str = "周四";
          break;
        case DayOfWeek.Friday:
          str = "周五";
          break;
        case DayOfWeek.Saturday:
          str = "周六";
          break;
        default:
          str = "神奇的一天";
          break;
      }
      return str;
    }

    private void showProcessorRing()
    {
      ((UIElement) this.ProcessorRing).put_Visibility((Visibility) 0);
      this.ProcessorRing.IsBusy = true;
    }

    private void hideProcessorRing()
    {
      this.ProcessorRing.IsBusy = false;
      ((UIElement) this.ProcessorRing).put_Visibility((Visibility) 1);
    }

    private void Season_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ((IDictionary<string, object>) ApplicationData.get_Current().get_LocalSettings().get_Values())["season"] = ((ContentControl) ((Selector) this.Season).get_SelectedItem()).get_Content();
    }

    private void CurrentWeekNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ApplicationDataContainer localSettings = ApplicationData.get_Current().get_LocalSettings();
      ((IDictionary<string, object>) localSettings.get_Values())["weekNumber"] = (object) ((Selector) this.CurrentWeekNumber).get_SelectedIndex();
      IPropertySet values = localSettings.get_Values();
      string index = "weekDate";
      DateTime today = DateTime.Today;
      int dayOfYear = today.DayOfYear;
      today = DateTime.Today;
      int num;
      if (today.DayOfWeek != DayOfWeek.Sunday)
      {
        today = DateTime.Today;
        num = (int) (today.DayOfWeek - 1);
      }
      else
        num = 6;
      // ISSUE: variable of a boxed type
      __Boxed<int> local = (ValueType) (dayOfYear - num);
      ((IDictionary<string, object>) values)[index] = (object) local;
    }

    private void UserName_GotFocus(object sender, RoutedEventArgs e)
    {
      this.UserName.SelectAll();
    }

    private void MyPasswordBox_GotFocus(object sender, RoutedEventArgs e)
    {
      this.MyPasswordBox.SelectAll();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
      ApplicationDataContainer localsettings = ApplicationData.get_Current().get_LocalSettings();
      ((IDictionary<string, object>) localsettings.get_Values())["season"] = ((ContentControl) ((Selector) this.Season).get_SelectedItem()).get_Content();
      ((IDictionary<string, object>) localsettings.get_Values())["weekNumber"] = (object) ((Selector) this.CurrentWeekNumber).get_SelectedIndex();
      IPropertySet values = localsettings.get_Values();
      string index = "weekDate";
      DateTime today = DateTime.Today;
      int dayOfYear = today.DayOfYear;
      today = DateTime.Today;
      int num1;
      if (today.DayOfWeek != DayOfWeek.Sunday)
      {
        today = DateTime.Today;
        num1 = (int) (today.DayOfWeek - 1);
      }
      else
        num1 = 6;
      // ISSUE: variable of a boxed type
      __Boxed<int> local = (ValueType) (dayOfYear - num1);
      ((IDictionary<string, object>) values)[index] = (object) local;
      if (((ContentControl) this.SaveButton).get_Content().ToString() == "确认")
      {
        if (this.UserName.get_Text() != "" && this.MyPasswordBox.get_Password() != "")
        {
          ((IDictionary<string, object>) localsettings.get_Values())["userName"] = (object) this.UserName.get_Text();
          ((IDictionary<string, object>) localsettings.get_Values())["password"] = (object) this.MyPasswordBox.get_Password();
          ((ContentControl) this.SaveButton).put_Content((object) "删除");
        }
        else
        {
          MessageDialog message = new MessageDialog("不要卖萌，没有数据怎么确认~");
          TaskAwaiter<IUICommand> awaiter = (TaskAwaiter<IUICommand>) WindowsRuntimeSystemExtensions.GetAwaiter<IUICommand>((IAsyncOperation<M0>) message.ShowAsync());
          if (!awaiter.IsCompleted)
          {
            int num2;
            // ISSUE: reference to a compiler-generated field
            this.\u003C\u003E1__state = num2 = 0;
            TaskAwaiter<IUICommand> taskAwaiter = awaiter;
            // ISSUE: variable of a compiler-generated type
            MainPage.\u003CSaveButton_Click\u003Ed__31 stateMachine = this;
            // ISSUE: reference to a compiler-generated field
            this.\u003C\u003Et__builder.AwaitUnsafeOnCompleted<TaskAwaiter<IUICommand>, MainPage.\u003CSaveButton_Click\u003Ed__31>(ref awaiter, ref stateMachine);
          }
          else
          {
            awaiter.GetResult();
            awaiter = new TaskAwaiter<IUICommand>();
            message = (MessageDialog) null;
          }
        }
      }
      else
      {
        ((IDictionary<string, object>) localsettings.get_Values()).Remove("userName");
        ((IDictionary<string, object>) localsettings.get_Values()).Remove("password");
        await DataHelper.DeleteDatabaseAsync();
        this.UserName.put_Text("");
        this.MyPasswordBox.put_Password("");
        ((ContentControl) this.SaveButton).put_Content((object) "确认");
      }
    }

    private void Account_TextChanged(object sender, RoutedEventArgs e)
    {
      ((ContentControl) this.SaveButton).put_Content((object) "确认");
    }
  }
}
