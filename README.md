# TODO API Refactoring Exercise

## Overview

You've inherited a TODO API that was built quickly to meet a deadline. While it works, it needs significant refactoring to meet production standards. Your task is to refactor the codebase, fix architectural issues and add/fix unit tests.

## Time Limit

**Maximum: 2 days**

You may submit earlier if you complete the requirements. Quality matters more than speed.

## Your Task

1. **Review** the existing codebase
2. **Identify** architectural and design problems
3. **Refactor** the application following best practices
4. **Document** your decisions and reasoning

## Requirements

### Core Functionality
The API should support basic TODO operations:
- Create a new TODO item
- Retrieve TODO items
- Update an existing TODO item by id
- Delete a TODO item by id

## Deliverables

### 1. Refactored Codebase
A working application with all the improvements implemented.

### 2. Documentation
Include a `SOLUTION.md` file that explains:
- **Problems Identified**: What issues did you find in the original implementation?
- **Architectural Decisions**: What architecture did you choose and why?
- **How to Run**: Clear instructions for running the application and tests
- **API Documentation**: How to use the endpoints
- **Future Improvements**: What would you do if you had more time?

### 3. Tests
- Tests should be meaningful and test actual behavior
- Include both positive and negative test cases
- Tests should be maintainable and well-organized

## Project Structure

```
interview-problem/
├── TodoApi/                  # Main API project
│   ├── Controllers/          # API Controllers
│   ├── Models/              # Data models
│   ├── Services/            # Business logic
│   └── Program.cs           # Application entry point
├── TodoApi.Tests/           # Test project
└── TodoApi.sln              # Solution file
```

## Getting Started

### Step 1: Fork the Repository

1. Navigate to the template repository: **https://github.com/madalincapris/dotnet-interview**
2. Click the **"Fork"** button in the top-right corner of the page
3. Select your personal GitHub account as the destination
4. GitHub will create a copy of the repository under your account: `https://github.com/YOUR-USERNAME/iss-interview`

### Step 2: Clone Your Fork

Clone your forked repository to your local machine:

```bash
git clone https://github.com/YOUR-USERNAME/iss-interview.git
cd iss-interview
```

### Step 3: Review and Understand the Code

1. Review the existing code thoroughly
2. Identify architectural issues and areas for improvement
3. Build and run the project to understand current behavior:
   ```bash
   dotnet build
   dotnet run --project TodoApi
   ```
4. Run the tests to see current test quality:
   ```bash
   dotnet test
   ```

### Step 4: Implement Your Solution

1. Plan your refactoring approach
2. Implement your changes incrementally
3. Test as you go
4. Commit your changes regularly with clear commit messages
5. Document your decisions in `SOLUTION.md`

## Submission

### Step 1: Finalize Your Work

1. Ensure all tests pass: `dotnet test`
2. Complete your `SOLUTION.md` documentation
3. Commit and push all changes to your forked repository:
   ```bash
   git add .
   git commit -m "Complete refactoring exercise"
   git push origin main
   ```

### Step 2: Submit Your Solution

Send an email or message to the interviewer with:
- **Link to your forked repository**: `https://github.com/YOUR-USERNAME/iss-interview`
- Your name
- Brief summary of your approach
- Any additional context or notes

## Questions?

If you have clarifying questions about requirements, please reach out to our recruitment team.
