
# WebApp Base .NET 8 + Angular 19

This repository serves as a starter template for building modern, full-stack web applications using **.NET 8** as the backend and **Angular 19** for the frontend. The project is designed to integrate seamlessly, enabling developers to focus on building features rather than configuring environments.

## Features
- **Backend**: Powered by .NET 8 with RESTful APIs, JWT authentication, and a scalable architecture.
- **Frontend**: Developed with Angular 19 for a responsive and dynamic user interface.
- **Database**: Configured to work with SQL Server, using Entity Framework Core for ORM.
- **Authentication**: JWT-based user authentication for secure API communication.
- **Swagger Integration**: Interactive API documentation out of the box.
- **CORS Policy**: Configurable CORS settings to support cross-origin requests.

---

## Prerequisites
Before setting up the project, ensure you have the following installed:
- **.NET SDK 8.0**: [Download](https://dotnet.microsoft.com/download)
- **Node.js (v18 or later)**: [Download](https://nodejs.org)
- **Angular CLI**: Install via `npm install -g @angular/cli`
- **SQL Server**: [Download](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

---

## Setup Instructions

### Backend (.NET 8)
1. Clone the repository:
   ```bash
   git clone https://github.com/omargalo/webapp-net8-angular18.git
   cd webapp-net8-angular18
   ```

2. Navigate to the backend folder (e.g., `src/Backend`) and restore dependencies:
   ```bash
   cd src/Backend
   dotnet restore
   ```

3. Configure environment variables:
   - Create an `appsettings.json` file or set environment variables for:
     - `PROJECT_SQL_HOST`
     - `PROJECT_SQL_DB`
     - `PROJECT_SQL_USER`
     - `PROJECT_SQL_PASSWORD`
     - `PROJECT_JWT_SECRET_KEY`

4. Run database migrations:
   ```bash
   dotnet ef database update
   ```

5. Start the backend server:
   ```bash
   dotnet run
   ```

6. Visit Swagger documentation at `https://localhost:<port>/swagger`.

### Frontend (Angular 19)
1. Navigate to the frontend folder (e.g., `src/Frontend`):
   ```bash
   cd src/Frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the Angular development server:
   ```bash
   ng serve
   ```

4. Open the app in your browser at `http://localhost:4200`.

---

## Folder Structure
```
webapp-net8-angular18/
├── src/
│   ├── Backend/        # .NET 8 backend API
│   └── Frontend/       # Angular 19 frontend application
├── README.md           # Project documentation
├── LICENSE             # License details
└── .gitignore          # Git ignore file
```

---

## Contributing
Contributions are welcome! To get started:
1. Fork this repository.
2. Create a new branch (`git checkout -b feature-name`).
3. Commit your changes (`git commit -m "Add feature-name"`).
4. Push to your branch (`git push origin feature-name`).
5. Open a pull request.

---

## Roadmap
- [ ] Add user registration and authentication features.
- [ ] Create sample frontend components with reusable services.
- [ ] Expand unit tests for both backend and frontend.
- [ ] Automate CI/CD pipelines for deployment.

---

## License
This project is licensed under the [MIT License](LICENSE).

---

## Contact
For questions or suggestions, feel free to reach out via [GitHub Issues](https://github.com/omargalo/webapp-net8-angular18/issues).
