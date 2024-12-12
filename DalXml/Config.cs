using Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



internal static class Config
{
    // שמות קובצי ה-XML שיאחסנו את הנתונים
    internal const string s_data_config_xml = "data-config.xml"; // קובץ תצורה מרכזי
    internal const string s_students_xml = "students.xml";       // קובץ סטודנטים
    internal const string s_assignments_xml = "assignments.xml"; // קובץ משימות

    // תכונה לניהול המספר הרץ עבור Assignments
    internal static int NextAssignmentId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextAssignmentId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextAssignmentId", value);
    }

    // תכונה לניהול השעון (Clock)
    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    // תכונה לניהול טווח הזמן (Risk Range)
  
    internal static DateTime RiskRange
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "RiskRange");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "RiskRange", value);
    }


    // מתודת Reset שתאפס את הנתונים ותשמור אותם בקובץ

    internal static void Reset()
    {
        NextAssignmentId = 1; // אתחול המספר הרץ
        Clock = DateTime.Now; // אתחול השעון
      //  RiskRange = TimeSpan.Zero; // אתחול טווח הזמן
    }

    internal const string s_calls_xml = "calls.xml"; // קובץ קריאות
    internal const string s_volunteers_xml = "volunteers.xml";   // קובץ מתנדבים

}

