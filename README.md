# HNG-13-PROFILE-

Here’s a ready-to-paste `README.md` for your GitHub repo — fully aligned with the Stage 0 task requirements:

```markdown
# Backend Wizards — Stage 0: Dynamic Profile Endpoint

## Overview

This project implements the **Stage 0 task** for the HNG Internship Backend Wizards program.  
The API exposes a single endpoint:

```

GET /me

````

It returns your profile information along with a dynamic cat fact fetched from the **Cat Facts API** (`https://catfact.ninja/fact`).  

The API demonstrates:

- Consumption of third-party APIs  
- Graceful error handling and timeouts  
- JSON response formatting with ISO 8601 timestamps  
- Use of **User Secrets** for sensitive data  
- Dependency Injection and clean .NET practices  

---

## Response Example

```json
{
  "status": "success",
  "user": {
    "email": "your.email@example.com",
    "name": "Your Full Name",
    "stack": "C#/.NET"
  },
  "timestamp": "2025-10-19T15:12:34.567Z",
  "fact": "Cats sleep for 70% of their lives."
}
````

* `status`: always `"success"`
* `user`: your email, full name, and backend stack
* `timestamp`: current UTC time in ISO 8601 format
* `fact`: a random cat fact fetched dynamically on each request

---

## Features & Best Practices

* **Dynamic Cat Facts**: Fetches a new fact on every request; fallback message provided if API fails.
* **Timeout Handling**: HTTP requests timeout after 5 seconds.
* **Logging**: Basic logging of requests and errors.
* **User Secrets**: Sensitive data (email, name, stack) stored outside the codebase.
* **CORS Support**: Allows cross-origin requests during development.
* **Swagger UI**: Optional interactive API documentation for local testing.

---

## Installation & Setup

1. Clone the repository:

   ```bash
   git clone <your-repo-url>
   cd <repo-folder>
   ```

2. Open the solution in **Visual Studio 2022/2023**.

3. Manage User Secrets:

   * Right-click the project → **Manage User Secrets**
   * Paste the following JSON (replace with your info):

   ```json
   {
     "Profile": {
       "Email": "your.email@example.com",
       "FullName": "Your Full Name",
       "Stack": "C#/.NET"
     }
   }
   ```

4. Build the project (`Build → Build Solution`).

5. Run the project (green ▶ button). Visual Studio will launch Swagger UI or a browser.

6. Test the endpoint:

   ```
   https://localhost:<port>/me
   ```

   Replace `<port>` with the port Visual Studio shows.

---

## Project Structure

```
ProfileApi/
 ├─ Controllers/
 │   └─ ProfileController.cs  # API endpoint /me
 ├─ Models/
 │   └─ ProfileOptions.cs    # Typed config for User Secrets
 ├─ Services/
 │   └─ CatFactService.cs    # Handles fetching cat facts with error handling
 ├─ Program.cs               # Application startup & DI setup
 ├─ appsettings.json         # Default app configuration
 ├─ ProfileApi.csproj
```

---

## How it Works

1. **ProfileController** calls **CatFactService** to get a random cat fact.
2. **CatFactService** uses `HttpClient` with a 5-second timeout.
3. If the Cat Facts API fails, a fallback message is returned.
4. `timestamp` reflects the current UTC time in ISO 8601 format.
5. JSON response strictly matches the required schema.

---

## Notes

* Tested on **.NET 9**, Visual Studio 2022
* Works offline for fallback responses if Cat Facts API is unreachable
* User Secrets ensure your personal data is **never committed to GitHub**

---

## Submission Instructions

1. Verify the API works from multiple networks.

2. Run `/stage-zero-backend` on the **#track-backend** Slack channel.

3. Provide the following:

   * Your **server IP or URL** (e.g., `http://<your-ip>:5000/me`)
   * **GitHub repo link**
   * **Full name**
   * **Email**
   * **Stack** (C#/.NET)


> Developed as part of **HNG Internship Backend Wizards — Stage 0 Task**
