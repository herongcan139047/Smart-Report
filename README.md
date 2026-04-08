# Smart_Report

Smart_Report is a lightweight mobile reporting application built with .NET MAUI. It allows users to register, log in, create reports, attach photos, capture location data, and manage personal todo items. The project uses SQLite for local data storage and is designed as a simple cross-platform mobile app prototype for Android, iOS, MacCatalyst, and Windows.

---

## Features

### User Authentication
- User registration with unique email
- User login and logout
- Session restore using local preferences
- Display current user information

### Report Management
- Create a report with a title and description
- Take or attach a photo
- Capture location information
- Save reports into the local SQLite database
- Display report history in a list

### Todo Management
- Add todo items
- Mark todo items as completed
- Delete todo items
- Load todos based on the current logged-in user

### Navigation
- Tab-based navigation using AppShell
- Includes Home, Report, and Todos pages
- Separate Login and Register pages

---

## Technologies Used

- .NET MAUI
- C#
- SQLite
- sqlite-net-pcl
- CommunityToolkit.Mvvm
- Microsoft.Extensions.Logging.Debug

---

## Project Structure

```text
Smart_Report
│
├── Models
│   ├── User.cs
│   ├── TodoItem.cs
│   └── ReportItem.cs
│
├── Data
│   └── AppDb.cs
│
├── Services
│   └── AuthService.cs
│
├── Views
│   ├── LoginPage.xaml / .cs
│   ├── RegisterPage.xaml / .cs
│   ├── HomePage.xaml / .cs
│   ├── ReportPage.xaml / .cs
│   ├── TodoPage.xaml / .cs
│   └── AccountPage.xaml / .cs
│
├── App.xaml
├── App.xaml.cs
├── AppShell.xaml
├── AppShell.xaml.cs
└── MauiProgram.cs


How the Project Works
Database

The application uses AppDb to manage SQLite database operations. When the app starts, the database is initialized and tables are created if they do not already exist.

Authentication

AuthService handles:

user registration
login validation
logout
restoring the current session
Reports

Users can create a report by entering:

title
description
optional photo
optional location

The report is then saved into the database and displayed in the report list.

Todos

Each logged-in user can manage their own todo items. Todo data is loaded from the database and displayed in the UI.

Database Models
User

Stores account information:

Id
Email
DisplayName
PasswordHash
Salt
TodoItem

Stores todo records:

Id
UserId
Title
IsDone
CreatedAt
ReportItem

Stores report records:

Id
UserId
Title
Description
PhotoPath
Latitude
Longitude
CreatedAt
Setup Instructions
Requirements
Visual Studio 2022
.NET MAUI workload installed
Android Emulator or Windows machine
Steps
Open the project in Visual Studio 2022.
Restore NuGet packages.
Build the solution.
Run the app on Android Emulator, Windows, or another supported platform.
Current Pages
LoginPage - user login
RegisterPage - user registration
HomePage - main page after login
ReportPage - create and view reports
TodoPage - manage todo items
AccountPage - display current user information
Highlights
Cross-platform mobile app built with .NET MAUI
Local database integration with SQLite
User authentication with session restore
Report creation with photo and location support
Personal todo management
Clean multi-page navigation structure
Future Improvements
Admin management functions
Report status update features
Better UI design and validation
Cloud database support
Push notifications
Image upload optimization
