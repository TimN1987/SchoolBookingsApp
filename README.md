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
|----------|----------------|
| <img width="500" height="300" alt="Add booking view with data displayed." src="https://github.com/user-attachments/assets/233bcb05-b908-4226-919f-6db8463480f2" /> | <img width="400" height="300" alt="Student search view." src="https://github.com/user-attachments/assets/9e7e125f-2f49-432d-bec6-60a8711d35e5" /> |

## 🚀 Getting started:

### 📃 Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/vs/community/)
  - With the **“.NET Desktop Development”** workload installed

### 💻 Run Locally

1. Clone the repository.
   ```bash
   git clone https://github.com/TimN1987/SchoolBookingsApp.git
   ```
2. Open the solution in Visual Studio.
3. Run the application.
    - Set **SchoolBookingsApp** as the startup project.
    - Build with **F5**.

*The application will automatically create a local **Sqlite database** on its first launch. This will be stored in AppData/Roaming/SchoolBookingsApp/Database.*

## ⚙️ Tech stack:

| Technology | Role |
|------------|------|
| WPF | UI Framework |
| C# | Backend (Models and ViewModels) |
| XAML | UI Markup |
| Sqlite | Database management |
| MVVM | Architectural pattern |

## 📂 Project Structure:

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
```

🔧 *The project structure will be improved in future iterations to ensure better separation of concerns (e.g. placing **Services** outside of **MVVM**. Current structure retained to avoid breaking namespace references.*

## 🛠️ How it's made:

The goal of this project was to strengthen my confidence in using a **relational database** to manage structured data, applying **SQL commands** to perform a full range of **CRUD operations** across multiple related tables. The application provides a simple yet scalable system for teachers to schedule and manage parents’ meetings, prepare data, and record meeting notes efficiently.

It was built with **C#** (.NET 8) and **XAML** using **Windows Presentation Foundation (WPF)**, following the **MVVM** (Model–View–ViewModel) design pattern to ensure a clean, maintainable, and testable architecture.

Development followed **Test-Driven Development (TDD)** principles, supported by **xUnit tests** for core logic and ViewModels. This enabled the setup of a **Continuous Integration (CI)** pipeline using **GitHub Actions**, ensuring that all commits trigger automated builds and test validation.

### Why WPF and MVVM?

The **School Bookings App** was designed as a personal desktop application, making **WPF** a natural choice for its rich UI capabilities and seamless integration with .NET. The **MVVM** pattern complements the app’s core functionality (inputting, managing, and displaying data) through a clean separation of concerns and efficient *data bindings* powered by *INotifyPropertyChanged*.

This architecture ensures that once data is read from the database, it can be displayed dynamically and reliably, while the expressive nature of **XAML** enables clear, maintainable, and accessible layouts. This project also created opportunities to develop confidence using *custom controls* and highly customised styles in **XAML**.

Adopting the MVVM pattern also allowed the project to be developed in stages, aligning with Test-Driven Development (TDD) principles. Early development focused on building and testing a robust Model layer to handle all database CRUD operations. This approach kept database logic modular and independent from the UI, simplifying scalability and future maintenance.

The next stage of development involved creating *ViewModels* that exposed key data properties to the *Views*. This required careful planning to define which properties should be accessible and how different data entities interact — for example, ensuring that either a Booking or a Student could be selected, but not both. This design foresight made the creation of *Views* straightforward, with a focus on precise *data bindings* and polished, consistent **XAML** styling.

### Why Sqlite?

**Sqlite** provided the ideal data management solution for this application due to its lightweight and self-contained **relational database** structure. This eliminated any need for server usage, ensuring that the application can run smoothly on any **Windows** desktop with minimal configuration.

As a **relational database**, **SQLite** offered a realistic environment for handling data persistence while keeping setup simple. It supported full **CRUD** functionality across multiple tables, providing valuable experience working with *primary keys*, *foreign keys*, *relationship tables*, and *joins*.

Its lightweight nature also made it ideal for testing and debugging throughout development. The application automatically creates and initialises a local database on startup if one does not already exist, ensuring that users can begin using the system immediately without any setup overhead.

## 🔨 Future Improvements:

## 💡 Lessons Learned:

## 📜 License:

This project is licensed under the MIT License.
