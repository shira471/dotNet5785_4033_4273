using DalApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BlApi;

    /// <summary>
    /// ממשק ראשי לשכבה הלוגית
    /// </summary>
    public interface IBl
    {
        /// <summary>
        /// גישה ליישות שירות לוגית מתנדבים
        /// </summary>
        IVolunteer Volunteer { get; }

        /// <summary>
        /// גישה ליישות שירות לוגית קריאות
        /// </summary>
        ICall Call { get; }

        /// <summary>
        /// גישה ליישות שירות לוגית ניהול
        /// </summary>
        IAdmin Admin { get; }
    }

