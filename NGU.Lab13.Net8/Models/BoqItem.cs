using System;

namespace NGU.Lab13.Models
{
    /// <summary>
    /// Model class representing a Bill of Quantities (BOQ) item for a Revit Room.
    /// This is a simple, clean data holder (POCO - Plain Old CLR Object) designed to be beginner-friendly.
    /// In the MVVM pattern, the Model holds the raw data that the ViewModel will expose to the View.
    /// </summary>
    public class BoqItem
    {
        /// <summary>
        /// Gets or sets the name of the room (e.g., "Kitchen", "Living Room").
        /// </summary>
        public string RoomName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the room number (e.g., "101", "102").
        /// </summary>
        public string RoomNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the area of the room in square meters (m²).
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Gets or sets the level of the room (e.g., "Level 1", "Ground Floor").
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Helper property that formats the double Area value into a user-friendly string
        /// with two decimal places and the "m²" suffix (e.g., "12.45 m²").
        /// DataGrid binds directly to this to show formatted data.
        /// </summary>
        public string AreaFormatted => $"{Area:F2} m²";
    }
}
