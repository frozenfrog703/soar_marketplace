
# Overview

This project simulates the process of an alert in a Security Orchestration Automation and Response (SOAR) system. It involves building a full-stack application with .NET 6 for the backend and Angular for the frontend. The system interacts with Google Cloud Pub/Sub to pull Indicators of Compromise (IoCs), queries VirusTotal for analysis, and displays enriched results in a frontend UI.

# Requirements

- Google Cloud Platform (GCP) account and access to Pub/Sub.

- VirusTotal API Key for querying IoC data.

- .NET 6+ for backend services.

- Angular for the frontend.

- Python 3.x for running the publisher service.

# Tech Stack

## Backend (.NET 6)
### Ingestion Service
This service pulls messages containing IoCs from Google Cloud Pub/Sub and processes them into alert objects. The service should:

Authenticate with GCP using the provided service account.

Pull messages from the specified Pub/Sub subscription.

Process each IoC into an alert and pass it to the Enrichment Service.

### Enrichment Service
This service is triggered upon receiving an alert. It:

Queries the VirusTotal API for each IoC.

Determines whether the IoC is malicious or not.

Computes the alert severity based on the percentage of malicious IoCs.

Saves the results as a .json file with execution time as the filename.

## Frontend (Angular)
The frontend will display enriched IoCs as widgets. Each widget will contain:

### Top Pane: 
IoC identifier and risk ("Malicious" / "Not Malicious").

### Left Pane: 
IoC Meta Data (Country, Tags, and Report Link).

### Right Pane: 
IoC Analysis (Last Update Time and Last Analysis Results).

## Testing (xUnit)

# Setup Instruction

Backend Setup:

Clone the repository and navigate to the /app/Backend directory.
Run the following command to restore dependencies:
    
```bash
    dotnet restore
    dotnet build
    dotnet run
```

Frontend Setup:

Navigate to the /app/Frontend directory.

Install dependencies by running:


```bash
  npm install
  ng serve
```

Testing:

```bash
  dotnet clean
  dotnet build
  dotnet test
```
    
# Project Structure

```javascript
/app
    /Backend       (contains .NET 6 backend code)
        /ChronicleSOARMarketplace
            /ChronicleSOARMarketplace
                /Controllers
                /Models
                /Output
                /Properties
                /Services
                /Utils
        /ChronicleSOARMarketplace.Test
    /Frontend      (contains Angular frontend code)
        /src

/publisher_service
    publisher.py   (Python script to simulate publishing IoCs to Pub/Sub)
    requirements.txt  (dependencies for the publisher)

.gitignore
README.md

