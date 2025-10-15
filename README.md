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
| SQLite | Database management |
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

It was built with **C#** (.NET 8) and **XAML** using **Windows Presentation Foundation (WPF)**, following the **MVVM** (Model–View–ViewModel) design pattern to ensure a clean, maintainable, and testable architecture. Each **View** is implemented as an individual *UserControl*, displayed within a *ContentControl* in the **MainWindow**. Views are resolved from the *Dependency Injection Container* using a **Factory** pattern, with their corresponding **ViewModel** injected via constructor injection. 

Development followed **Test-Driven Development (TDD)** principles, supported by **xUnit tests** for core logic and ViewModels. This enabled the setup of a **Continuous Integration (CI)** pipeline using **GitHub Actions**, ensuring that all commits trigger automated builds and test validation.

### Why WPF and MVVM?

The **School Bookings App** was designed as a personal desktop application, making **WPF** a natural choice for its rich UI capabilities and seamless integration with .NET. The **MVVM** pattern complements the app’s core functionality (inputting, managing, and displaying data) through a clean separation of concerns and efficient *data bindings* powered by *INotifyPropertyChanged*.

This architecture ensures that once data is read from the database, it can be displayed dynamically and reliably, while the expressive nature of **XAML** enables clear, maintainable, and accessible layouts. This project also created opportunities to develop confidence using *custom controls* and highly customised styles in **XAML**.

Adopting the MVVM pattern also allowed the project to be developed in stages, aligning with Test-Driven Development (TDD) principles. Early development focused on building and testing a robust Model layer to handle all database CRUD operations. This approach kept database logic modular and independent from the UI, simplifying scalability and future maintenance.

The next stage of development involved creating *ViewModels* that exposed key data properties to the *Views*. This required careful planning to define which properties should be accessible and how different data entities interact — for example, ensuring that either a Booking or a Student could be selected, but not both. This design foresight made the creation of *Views* straightforward, with a focus on precise *data bindings* and polished, consistent **XAML** styling.

### Why Dependency Injection (DI)?

The application uses **IServiceCollection** and **IServiceProvider** to create a dedicated **Dependency Injection container**, ensuring that all required dependencies are cleanly resolved through constructor injection.

Given the range of **CRUD** operations and the integration of **Prism’s EventAggregator**, it was essential to maintain strong *separation of concerns* while enabling cross-component communication. **Dependency Injection** made it possible to inject shared services (such as data access layers and event aggregators) across different ViewModels without resorting to static references or multiple service instances.

All services are registered as **singletons**, guaranteeing consistent state and availability throughout the application’s lifecycle. Meanwhile, Views and ViewModels are registered as **transient**, ensuring that each navigation or screen load creates a fresh instance.

This architectural choice supports clean memory management and reduces unintended data persistence between Views. However, it also means that certain navigation features (such as Back and Forward buttons) are intentionally omitted, since **transient** views do not retain unsaved state between transitions.

### Why SQLite?

**SQLite** provided the ideal data management solution for this application due to its lightweight and self-contained **relational database** structure. This eliminated any need for server usage, ensuring that the application can run smoothly on any **Windows** desktop with minimal configuration.

As a **relational database**, **SQLite** offered a realistic environment for handling data persistence while keeping setup simple. It supported full **CRUD** functionality across multiple tables, providing valuable experience working with *primary keys*, *foreign keys*, *relationship tables*, and *joins*.

Its lightweight nature also made it ideal for testing and debugging throughout development. The application automatically creates and initialises a local database on startup if one does not already exist, ensuring that users can begin using the system immediately without any setup overhead.

### Why TDD and CI?

The use of **TDD** supported the development of the application by ensuring that there was a clear focus on the expected behavior and outcomes in the development of each class and its methods. This led to more maintainable and thoughtful logic, backed by automated tests. Using **xUnit**, I wrote a comprehensive suite of tests covering database operations, input validation, and ViewModel logic. This ensured that issues were detected early, updates remained reliable, and behaviour stayed consistent across refactors.

The use of **TDD** also enabled seamless integration through **GitHub Actions**, triggering automated builds and test runs on every commit. This not only increased confidence in code quality but also strengthened my understanding of modern **CI/CD** workflows.

### Why asynchronous operations?

The majority of database operations were implemented **asynchronously** to ensure smooth background processing of data without blocking the UI thread. Due to the small-scale nature of the application and database, only one **SQLite connection** was opened and so tasks were not run in parallel (although this approach was considered). Although the application’s data load is lightweight, implementing **asynchronous** methods ensures better scalability and makes the underlying logic more reusable for future, larger applications.

## 🔨 Future Improvements:

- 🧑‍🎨 UI and UX Enhancements:
  Expand the dashboard to include richer visualisations and data summaries, such as upcoming meetings, average attainment scores, and bookings by class. Implement interactive graphs (e.g., bar charts for student performance by subject) to make data easier to interpret at a glance.
- 📂 Cleaner Project Structure:
  Reorganise folders to better reflect *separation of concerns*. For example, moving the *Services* directory outside the *MVVM* folder and updating namespaces accordingly. This will align the solution more closely with professional .NET conventions.
- 📈 Enable data tracking:
  Add functionality to track multiple sets of attainment data and meeting records per student. This would provide deeper insights, enabling teachers to monitor student progress over time and refer back to prior meeting records to action discussion points.
- 📨 Refactor Status Messaging:
  Centralise all user-facing messages in a dedicated *static* **StatusMessage** class to improve consistency and maintainability. Replace the existing **Task.Delay**–based message clearing with **cancellation tokens** for more robust and responsive UI updates.
- 📅 Improve Booking View:
  Implement a **calendar-based layout** to present bookings in a more visual and intuitive format. Provide users with the option to toggle between *calendar* and *list* views, enhancing usability and making it easier to manage and review appointments.

## 🌟 Project Highlights:

- 💻 Full CRUD functionality:
  Designed and implemented complete **Create**, **Read**, **Update**, and **Delete** operations across multiple tables, ensuring consistent data handling and validation. Included a range of flexible data retrieval methods, including the ability to search by multiple criteria for more refined and efficient data access.
- 🔍 Relational Database Design:
  Introduced a relationship table to manage *many-to-many* links between students and parents, demonstrating practical use of **foreign keys** and **JOIN** operations in **SQLite**. Implemented *cascading deletes* to maintain data integrity when records are removed, and extended read operations across related tables for combined data queries.
- 🧩 Dependency Injection and Modularity:
  Used **IServiceCollection** and **IServiceProvider** for clean and testable dependency management. This ensured that all components (including **CRUD** services and **EventAggregator**) were easily maintainable and decoupled.
- 📊 Dynamic Data Visualisation:
  Implemented a **custom control** for pie charts to display key data summaries, such as student booking distribution, making data more engaging and accessible. Used *trigonometric calculations*, *converters* and *Geometry paths* in **XAML** to create accurate and responsive pie chart segments.
- 🎨 Customised and Consistent XAML UI:
  Developed a clear and modern UI using **XAML**, featuring *DataTemplates*, *ControlTemplates*, and *Styles* for a cohesive appearance across all **Views**. Emphasis was placed on clarity, maintainability, and ease of navigation. Incorporated *VisualStateManager* with Storyboard animations and *Template Triggers* for smoother visual feedback and dynamic UI interactions.
- 🧪 Testing and Continuous Integration:
  Followed **Test-Driven Development (TDD)** principles using **xUnit**, ensuring that logic for database access, ViewModels, and validation was fully testable. Integrated **GitHub Actions** to automatically build and run all tests on each commit, supporting a professional **CI pipeline** and maintaining code reliability throughout development.

## 💡 Lessons Learned:

- 📝 Careful Planning:
  The project scope grew quickly during development, and at times I spent more time refactoring than expected. Early **CRUD** services included unused methods due to initial uncertainties in planning. As development progressed, I planned each **View** in more detail, clarifying which *properties*, *commands*, and *methods* were needed in the associated **ViewModel** and how they should interact.
- 💻 CRUD Organisation:
  Structuring **CRUD** operations by type (Create, Read, Update, Delete) led to **ViewModels** requiring multiple injected dependencies. A more purpose-driven approach, e.g., ParentManager and StudentManager, would have simplified dependency management and improved maintainability.
- 🧪 Test-Driven Development (TDD) & CI:
  The use of **TDD** involved an initial learning curve, but when followed rigorously, it significantly reduced bugs and unexpected UI behavior. Integrating **TDD** with **GitHub Actions** for **CI** created a workflow that automatically validated all commits, improving confidence in code quality and maintainability.
- 🔗 SQLite Connection Management:
  In hindsight, keeping a single **SQLite connection** open for the service lifetime is suboptimal, but the use of a **ConnectionFactory** clarified how connections could be scoped properly within methods. This also highlighted opportunities to introduce concurrency in future projects for larger databases by running *asynchronous tasks* in *parallel*.
- 🧑‍🎨 CustomControls & XAML:
  Developing **CustomControls** with **DependencyProperties** enabled reusable, personalised UI elements. This improved control over how data was displayed and reduced repetitive code. Animations, triggers, and templating further enhanced UI clarity and polish.

## 📜 License:

This project is licensed under the MIT License.
