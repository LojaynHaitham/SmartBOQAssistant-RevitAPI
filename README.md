# Smart BOQ Assistant – Revit API Add-in

## Overview

Smart BOQ Assistant is a Revit 2025 Add-in developed using C#, .NET 8, WPF MVVM, Revit API, and Google Gemini AI.

The tool automatically extracts room information from a Revit model, generates a Bill of Quantities (BOQ) summary, exports project data, and uses AI to provide BIM-focused analysis and recommendations.

---

## Features

### Revit API Features
- Scan Revit rooms using FilteredElementCollector
- Extract:
  - Room Name
  - Room Number
  - Room Area
  - Level
- Generate room quantity summaries
- Calculate total floor area
- Export BOQ data to CSV
- Interactive WPF Dashboard

### AI Features
- Google Gemini API integration
- Automatic BIM project analysis
- Space optimization recommendations
- Occupancy estimation
- Architectural planning suggestions
- Schematic cost estimation
- BIM data quality review

---

## Technologies Used

- Autodesk Revit 2025
- Revit API
- C#
- .NET 8
- WPF
- MVVM Architecture
- Google Gemini API
- JSON Serialization
- HttpClient

---

## Project Workflow

### Conventional Workflow

1. Open Revit model
2. Create room schedules manually
3. Export schedules
4. Calculate quantities manually
5. Analyze spaces manually
6. Produce reports manually

### Automated Workflow

1. Open Revit model
2. Launch Smart BOQ Assistant
3. Click Scan Model
4. Generate BOQ automatically
5. Generate AI Analysis automatically
6. Export results instantly

---

## Dashboard Functions

### 1. Scan Model
Scans the active Revit document and extracts all valid room information.

### 2. Generate BOQ
Creates a room quantity summary including room counts and total areas.

### 3. Generate AI Report
Uses Google Gemini AI to analyze project data and generate BIM recommendations.

### 4. Export CSV
Exports room information into a CSV file for further processing.

---

## Project Structure

```text
NGU.Lab13.Net8
│
├── Commands
├── Helpers
├── Models
├── Resources
├── Ribbon
├── ViewModels
├── Views
└── revit-dlls
```

---

## Example Output

### BOQ Summary

- Total Rooms: 54
- Total Floor Area: 5,237.68 m²

### AI Report

- Space Optimization Analysis
- Occupancy Estimation
- Architectural Recommendations
- Cost Estimation
- BIM Data Quality Notes

---

## Installation

1. Clone or download this repository.
2. Open the solution in Visual Studio 2022.
3. Restore NuGet packages.
4. Build the project.
5. Copy the generated DLL and .addin file to the Revit Addins folder.

Typical Revit Addins Path:

```text
C:\ProgramData\Autodesk\Revit\Addins\2025
```

---

## Requirements

- Autodesk Revit 2025
- Visual Studio 2022
- .NET 8 SDK
- Google Gemini API Key

---

## Future Improvements

- Live cost databases
- Material quantity takeoff
- Element-based BOQ generation
- Multi-discipline analysis
- PDF report export
- AI chat assistant
- Voice commands
- BIM compliance checking

---

## Demo Video

LinkedIn Demo Video:

https://www.linkedin.com/posts/lojayn-haitham_revitapi-autodeskrevit-bim-ugcPost-7469557287590752256-RUjc/?utm_source=share&utm_medium=member_desktop&rcm=ACoAAEB51CEB_FqQCmIHtS6Vxt6FKAUvzYtcgiU

---

## GitHub Repository

https://github.com/LojaynHaitham/SmartBOQAssistant-RevitAPI

---

## Author

Lojayn Haitham

Revit API Final Project

New Giza University

---

## Disclaimer

This project was developed for educational purposes as part of a Revit API course project. AI-generated results are intended for conceptual analysis and should be reviewed by qualified professionals before use in real projects.
