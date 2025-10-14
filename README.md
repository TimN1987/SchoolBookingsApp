# 🏫 School Bookings App

[![.NET](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-WPF-purple?logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A **desktop booking system** designed for managing parents' evening schedules and student records in schools. It stores parent and student data, booking dates, and meeting notes in a **SQLite database**, providing an intuitive **WPF interface** for creating, updating, and searching data.

## ✨ Features:

- 📊 **Dashboard Overview**
  Displays upcoming bookings and key information at a glance.
- 🧑‍🤝‍🧑 **Student and Parent Management**
  Add and update personal records and relationships with ease.
- 🗓️ **Booking Management**
  Schedule meetings for students, avoiding clashes.
- 📈 **Manage Student Data**
  Input, edit and view attainment data for each student.
- 📝 **Meeting Records**
  Create and view detailed records of meetings.
- 🔍 **Search functionality**
  - Basic keyword search for quick look-ups.
  - Advanced multi-criteria searches for student records.

## 🖼️ Screenshots:

<img width="950" height="480" alt="Dashboard showing key booking information." src="https://github.com/user-attachments/assets/9a5bc76c-9c07-493f-9fbc-157631e7c48d" />

| Add Student | Add Parent |
|-------------|------------|
| <img width="500" height="300" alt="Add student view." src="https://github.com/user-attachments/assets/fd6d182f-47e9-49f2-86ec-ed0ae297aec9" /> | <img width="400" height="300" alt="Add parent view." src="https://github.com/user-attachments/assets/ff6a0e67-e0fd-4fa8-b50e-ee67e1e1bb7d" /> |
| Add Data | Student Search |
| <img width="500" height="300" alt="Add booking view with data displayed." src="https://github.com/user-attachments/assets/233bcb05-b908-4226-919f-6db8463480f2" /> | <img width="400" height="300" alt="Student search view." src="https://github.com/user-attachments/assets/9e7e125f-2f49-432d-bec6-60a8711d35e5" /> |

## Getting started:

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/vs/community/)
  - With the **“.NET Desktop Development”** workload installed

### Run Locally

1. Clone the repository.
   ```bash
   git clone https://github.com/TimN1987/SchoolBookingsApp.git
2. Open the solution in Visual Studio.
3. Run the application.
  - Set **SchoolBookingsApp** as the startup project.
  - Build with **F5**.

> The application will automatically create a local **Sqlite database** on its first launch. This will be stored in AppData/Roaming/SchoolBookingsApp/Database.

## Tech stack:

| Technology | Role |
|------------|------|
| WPF | UI Framework |
| C# | Backend (Models and ViewModels) |
| XAML | UI Markup |
| Sqlite | Database management |
| MVVM | Architectural pattern |

## Project Structure:

```
SchoolBookingsApp/
├── MVVM/
|  ├── Commands
|  ├── Converters
|  ├── CustomControls
|  ├── Database
|  ├── Enums
|  ├── Factories
|  ├── Model
|  ├── Services
|  ├── Struct
|  ├── View
|  └── ViewModel
|    └── Base
├── Resources/
|  └── Images/
|    ├── AddBookingView
|    └── MainWindow
└── App.xaml
SchoolBookingsAppTests/
├── ConverterTests
├── DatabaseTests
├── FactoryTests
├── Mocks
└── ViewModelTests

>The project structure could be improved to ensure that files are more logically ordered and to show a more clear separation of concerns. For example, Services should wit within **SchoolBookingsApp** but outside **MVVM**. This has not been corrected yet to avoid issues with changing namespaces.
