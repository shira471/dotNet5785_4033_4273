﻿using Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



internal static class Config
{
    // שמות קובצי ה-XML שיאחסנו את הנתונים
    internal const string s_data_config_xml = "data-config.xml"; // קובץ תצורה מרכזי
    internal const string s_volunteer_xml = "volunteer.xml";       // קובץ סטודנטים
    internal const string s_assignments_xml = "assignment.xml"; // קובץ משימות

    // תכונה לניהול המספר הרץ עבור Assignments
    internal static int NextAssignmentId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]

        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextAssignmentId");
        [MethodImpl(MethodImplOptions.Synchronized)]

        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextAssignmentId", value);
    }
    internal static int NextCallId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]

        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextCallId");
        [MethodImpl(MethodImplOptions.Synchronized)]

        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextCallId", value);
    }
    // תכונה לניהול השעון (Clock)
    internal static DateTime Clock
    {
        [MethodImpl(MethodImplOptions.Synchronized)]

        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        [MethodImpl(MethodImplOptions.Synchronized)]

        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    // תכונה לניהול טווח הזמן (Risk Range)

    internal static DateTime RiskRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]

        get => XMLTools.GetConfigDateVal(s_data_config_xml, "RiskRange");
        [MethodImpl(MethodImplOptions.Synchronized)]

        set => XMLTools.SetConfigDateVal(s_data_config_xml, "RiskRange", value);
    }


    // מתודת Reset שתאפס את הנתונים ותשמור אותם בקובץ
    [MethodImpl(MethodImplOptions.Synchronized)]

    internal static void Reset()
    {
        NextCallId = 1;
        NextAssignmentId = 1; // אתחול המספר הרץ
        Clock = DateTime.Now; // אתחול השעון
        // RiskRange = TimeSpan.Zero; // אתחול טווח הזמן
    }

    internal const string s_calls_xml = "call.xml"; // קובץ קריאות
    internal const string s_volunteers_xml = "volunteer.xml";   // קובץ מתנדבים

}

