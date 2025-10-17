🕹️ Easy Games Project – Group 13

A Complete E-Commerce and Retail Management System (ASP.NET MVC)

📘 Overview

The Easy Games Project is a web-based e-commerce and retail management solution that integrates online and in-store operations under a unified platform.
It allows customers to browse and purchase products online, while shop proprietors can manage inventory, process sales, and track profit margins through a Point-of-Sale (POS) system.

This project was developed as part of the Charles Darwin University HITXXX Capstone/Group Project by Group 13.

🎯 Features

Multi-shop management system

Online and POS sales integration

Stock CRUD operations with buy/sell price and profit margin tracking

Customer registration and tier-based loyalty discounts

Email notifications for tier updates

Role-based login (Owner / Proprietor / Customer)

Responsive user interface for web devices

⚙️ Technologies Used

Frontend: HTML5, CSS3, Bootstrap, Razor Views

Backend: ASP.NET Core MVC (.NET 6.0)

Database: Microsoft SQL Server Express / LocalDB

IDE: Visual Studio 2022 (Community Edition)

Version Control : GitHub

🧩 Prerequisites

Before running the project, ensure that the following tools are installed:

Visual Studio 2022 (Community Edition or higher)

.NET 6.0 SDK or higher

Microsoft SQL Server Express / LocalDB

Internet connection (for restoring NuGet packages)

🚀 Installation & Setup

Download the Project

Extract the Easy Games ZIP file or clone the repository.

Open the solution file EasyGames.sln in Visual Studio.

Restore NuGet Packages

Go to Tools > NuGet Package Manager > Restore Packages.

Configure the Database

Open appsettings.json and edit the connection string to match your SQL Server instance.

Example test connection string:

"Server=(localdb)\\mssqllocaldb;Database=MvcMovieContext-4ebefa10-de29-4dea-b2ad-8a8dc6bcf374;Trusted_Connection=True;MultipleActiveResultSets=true"


Apply Migrations
Open the Package Manager Console and run:

Add-Migration InitialSetup  
Update-Database  


Build and Run

Press Ctrl + Shift + B to build.

Start the project (F5 or ▶ Start Debugging).

The application opens automatically in your browser (https://localhost:####/).

🔑 Default Login Accounts
Role	Email	Password
Owner	owner@easygamesstore.com
	Owner@123
Proprietor	shop1@easygamesstore.com
	Shop@123
🧠 How to Use

Log in as Owner to configure shops and manage users.

Log in as Proprietor to perform POS sales and manage inventory.

Register as a Customer to browse products and make purchases.

🧰 Troubleshooting
Issue	Possible Solution
Database not found	Verify SQL Server instance and rerun migration.
Login fails	Use default credentials or register a new account.
Missing packages	Rebuild project and restore NuGet packages.
Port conflict	Change launch port via Project Properties → Debug.
👥 Team Members
Name	Role
Shishtata Bhandari	Project Manager / Documentation Lead
Dipan Shrestha	Backend Developer
Sujan Kandel	Frontend & UI Developer
Tirth Jyotikar	Tester / Quality Assurance
