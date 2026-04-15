# Smart_Report

Smart_Report is a lightweight cross-platform mobile reporting application built with .NET MAUI and C#. It allows users to register, log in, submit reports with photos and location data, and manage personal todo items. The project uses SQLite for local data storage and is designed as a simple mobile app prototype for Android, iOS, MacCatalyst, and Windows.

---

## Overview

The purpose of Smart_Report is to provide a simple mobile reporting system where users can quickly record issues, attach evidence such as photos and location coordinates, and keep track of their own tasks in one app.

The application focuses on:

- user authentication
- issue reporting
- local data persistence
- personal task management
- simple multi-page mobile navigation

---

## Features

### User Authentication
- User registration with unique email validation
- User login and logout
- Session restore using local preferences
- Display current logged-in user information

### Report Management
- Create a report with a title and description
- Take or attach a photo
- Capture current location coordinates
- Save report data into the local SQLite database
- Display report history in a list
- Store each report with user ownership

### Todo Management
- Add todo items
- Mark todo items as completed
- Delete todo items
- Load todos based on the current logged-in user

### Navigation
- Tab-based navigation using `AppShell`
- Includes Home, Report, and Todos pages
- Separate Login and Register pages

### Native Mobile Support
- Camera integration for report photos
- Location support for latitude and longitude capture
- Haptic or vibration feedback on supported devices

---

## Technologies Used

- .NET MAUI
- C#
- .NET 10
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
│   ├── AuthService.cs
│   └── NativeFeedbackService.cs
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
