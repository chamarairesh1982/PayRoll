# PayrollSolution

This repository hosts a starter payroll platform that includes:

- **Backend** – ASP.NET Core Web API split into Domain, Application, Infrastructure, and API layers under `backend/`.
- **Frontend** – An Angular client located under `frontend/payroll-web/`.

## Frontend notes

- Run `npm install` inside `frontend/payroll-web` to restore dependencies before running `npm start` for local development.
- The generated `package-lock.json` **should be committed** so everyone uses the same dependency versions; only `node_modules/` should remain untracked.

Use the solution file `PayrollSolution.sln` to load all backend and frontend projects into a single workspace.
