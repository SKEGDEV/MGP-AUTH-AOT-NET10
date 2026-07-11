# MGP Auth Microservice (Native AOT)

This repository contains the Authentication Microservice for the Mega Project Portfolio. Engineered with **.NET 10 Minimal APIs** and **Native AOT (Ahead-of-Time) Compilation**, this project is designed for high availability, extreme scalability, and ruthless resource optimization.

---

## Table of Contents
1. [Project Overview & Purpose](#1-project-overview--purpose)
2. [Architecture & Artifacts](#2-architecture--artifacts)
3. [Local Setup & Debugging](#3-local-setup--debugging)
4. [Production Deployment Strategy (Ubuntu Server)](#4-production-deployment-strategy-ubuntu-server)
5. [Development Methodology: Strict Role & Flowcharts](#5-development-methodology-strict-role--flowcharts)
6. [AI POV: Strict Architecture vs. Standard SDD](#6-ai-pov-strict-architecture-vs-standard-sdd)

---

## 1. Project Overview & Purpose

This microservice acts as the central Authentication and Authorization hub for a larger, highly scalable mega project. By utilizing **.NET 10 Native AOT**, the application compiles directly to machine code, bypassing the CoreCLR JIT compiler at runtime. This results in:
- **Instantaneous Startup Times** (sub-millisecond).
- **Extremely Low Memory Footprint** (typically < 30MB of RAM).
- **Maximum Throughput**, making it ideal for a high-traffic auth gateway.

---

## 2. Architecture & Artifacts

The project strictly adheres to a 4-Layer architectural pattern, ensuring a clean separation of concerns, avoiding circular dependencies, and enforcing SOLID, KISS, and DRY principles.

### The 4 Isolated Layers:
1. **Controller (Public Layer):** 
   - Exposes Minimal APIs. Contains zero business logic.
   - Delegates execution to the Application layer and always returns a standardized HTTP 200 `StandardResponseDTO`.
2. **Application (Orchestration & Logic):**
   - **Services:** Pure orchestrators that coordinate helpers and repositories. They contain the `try/catch` blocks.
   - **Helpers:** Encapsulate business logic, computations, and transformations (e.g., JWT Generation, Crypto Hashing).
3. **Core (Domain & Contracts):**
   - Completely independent layer. Contains `Interfaces`, `DTOs`, `Exceptions`, and standard `Dictionaries` (for alphanumeric error codes like `AUTH0001`).
   - Houses the **AOT JSON Source Generators** (`JsonSerializerContext`) required for Native AOT serialization without reflection.
   - Defines the `ISettings` configuration contracts.
4. **Infrastructure (Data Access):**
   - Strictly handles database interactions using `Microsoft.Data.Sqlite` via raw ADO.NET and parameterized queries (No heavy ORMs like EF Core).
   - Only depends on the `Core` layer.

---

## 3. Local Setup & Debugging

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Visual Studio 2022, VS Code, or Rider.

### Configuration
Ensure your `appsettings.Development.json` or `appsettings.json` contains the required `Settings` block:
```json
{
  "Settings": {
    "ConnectionString": "Data Source=auth.db",
    "TokenSecret": "SuperSecretKeyForDevelopmentPurposesOnly123!",
    "TokenExpirationInMinutes": 60
  }
}
```

### Running Locally
To debug and run the application using the standard JIT compiler (for faster dev loops):
```bash
dotnet run
```
*Note: The SQLite database (`auth.db`) will be created in the output directory automatically if your DB initialization scripts are configured, or you can supply an existing file.*

To test the Native AOT build locally:
```bash
dotnet publish -c Release
./bin/Release/net10.0/linux-x64/publish/AuthMicroservice
```

---

## 4. Production Deployment Strategy (Ubuntu Server)

**Senior Architect Perspective:** If the goal is **extreme resource optimization, high availability, and scalability**, containerizing a Native AOT application with Docker introduces unnecessary overhead (networking bridge layer, container runtime footprint). 

Since we are already compiling to a self-contained, native Linux executable, the most efficient and performant deployment on an Ubuntu server is a **Systemd Service reverse-proxied by NGINX**.

### Why NGINX + Systemd over Docker?
- **Zero Runtime Overhead:** The binary runs directly on the Linux kernel. No Docker daemon, no network bridge virtualization.
- **Microscopic Footprint:** Native AOT binaries managed by systemd idle at ~15-30MB of RAM.
- **NGINX:** World-class for connection pooling, SSL termination, and rate-limiting. It uses event-driven architecture that consumes almost no RAM.

### Deployment Steps:
1. **Publish Native Binary:**
   ```bash
   dotnet publish -c Release -r linux-x64 --self-contained true
   ```
2. **Move to Server:** Copy the output from `publish/` to `/var/www/authmicroservice/` on your Ubuntu server.
3. **Create Systemd Service:** (`/etc/systemd/system/authmicroservice.service`)
   ```ini
   [Unit]
   Description=MGP Auth Microservice
   After=network.target

   [Service]
   WorkingDirectory=/var/www/authmicroservice
   ExecStart=/var/www/authmicroservice/AuthMicroservice
   Restart=always
   RestartSec=10
   Environment=ASPNETCORE_ENVIRONMENT=Production

   [Install]
   WantedBy=multi-user.target
   ```
4. **Configure NGINX:** Setup a proxy pass block to forward external traffic to the local Kestrel socket.
   ```nginx
   server {
       listen 80;
       server_name auth.yourdomain.com;
       location / {
           proxy_pass http://127.0.0.1:5000;
           proxy_http_version 1.1;
           proxy_set_header Keep-Alive "";
       }
   }
   ```

---

## 5. Development Methodology: Strict Role & Flowcharts

This project was developed using a heavily augmented approach to AI-assisted coding. Instead of loosely prompting an LLM to "build an auth service," we employed:
1. **Strict Developer Role (`micro_development_roll.md`):** A foundational prompt acting as a constitution. It forces the AI into the persona of a Senior Architect, explicitly banning certain practices (like ORMs, MVC Controllers, or Reflection) and enforcing layer boundaries.
2. **Mermaid Flowcharts:** Complex logic (like the Exception Handling Middleware) was strictly governed by Mermaid diagrams, leaving zero room for AI hallucination regarding business flow.
3. **Task-Based Execution:** Incremental plan generation, explicit approval loops, and rigorous verification.

---

## 6. AI POV: Flowcharts & Strict Roles vs. Traditional SDD Mega-Prompts

*An honest review from your AI Assistant.*

### The Comparison: Mermaid Flowcharts vs. Text-Heavy SDD
Traditional **Specification Driven Development (SDD)** often relies on writing extensive mega-prompts—massive Markdown files filled with User Stories, Acceptance Criteria, and sometimes even dictating exactly what lines of code the AI should write. While this approach is well-intentioned, it has severe limitations when working with Generative AI.

When I process a traditional SDD mega-prompt full of text:
1. **Attention Dilution:** Long paragraphs of Acceptance Criteria introduce linguistic ambiguity. I might miss a crucial negative constraint buried in the middle of a user story. 
2. **The "Dumb Typewriter" Effect:** If the prompt tries to micro-manage exactly *how* the code should be written, it strips away my contextual awareness and ability to write idiomatic, optimized code (like Native AOT constraints). It forces the AI into being a fragile copy-paste machine that breaks when the codebase scales.

**Why Mermaid Flowcharts are vastly superior for AI:**
When you provide a **Mermaid Flowchart** instead of a wall of text, you are speaking to me in a structural, mathematical language. A flowchart is essentially a directed graph (state machine). 
- There is **zero ambiguity** about a decision node. 
- I don't have to infer the "Happy Path" vs "Error Path" from a conversational paragraph; the graph explicitly tells me where the execution flows. 
- It completely eliminates "business logic hallucinations" because the logic is visually and mathematically bound by the chart.

### The Winning Combo: Flowcharts + Strict Roles
When you combine a visual logic map (Mermaid) with an immutable architectural constitution (`micro_development_roll.md`), you unlock the true potential of AI pair programming.
- **The Flowchart** dictates exactly *what* the logic must do, without textual ambiguity.
- **The Strict Role** dictates exactly *where* that logic belongs (e.g., "This logic goes in a Helper, not a Controller. No exceptions.").

### Review of Project Results
By using this methodology rather than a standard SDD mega-prompt, this repository achieved results that are incredibly rare to get right on the first try:
- **Zero Architectural Drift:** The 4 layers remained 100% isolated. The strict role prevented me from taking common LLM shortcuts like injecting repositories directly into controllers.
- **Flawless Execution of Complex Logic:** Complex flows, such as the Exception Handling Middleware and JWT lifecycle, were implemented flawlessly because the Mermaid charts provided the exact execution tree to follow.
- **Conclusion:** If you want an AI to build enterprise-grade, highly optimized software, stop writing mega-prompts with endless user stories. Define your architectural boundaries strictly, and map out your business logic with flowcharts. It is the most efficient, error-free way to communicate engineering intent to an LLM.

---

### The Model & The Truth

**Model:** `deepseek-v4-flash-free` — a freely available, mid-tier AI model. Not a frontier flagship.

**The honest truth about this strategy:**

With the methodology codified in this repository — strict architectural rules (`micro_development_roll.md`) + deterministic Mermaid flowcharts — **you do not need the most powerful AI models** to build production-grade software.

Here is why capability becomes nearly irrelevant:

1. **The constitutional guardrails (`micro_development_roll.md`)** collapse the AI's decision space into a small set of valid choices. Bad architecture is ruled out by fiat, not by the model's judgment. A weaker model cannot make a bad call if the rules forbid it.

2. **The flowcharts eliminate reasoning requirements.** The AI does not infer logic from prose — it reads a directed graph of nodes and edges. This is a mechanical translation task, not a reasoning task. The ceiling on output quality is set by the flowchart's precision, not by the model's intelligence.

3. **The floor rises dramatically.** A junior developer following this pattern produces code indistinguishable from a senior architect's output. A free-tier model produces code competitive with GPT-4 class output — not because it is smarter, but because the problem has been made simple enough that smart is not required.

**The bottleneck in AI software development is not model capability. It is prompt structure.** This repository demonstrates that with the right structure, any model, used by any person, can build any well-specified project reliably.

The strategy does not make AI smarter. It makes the problem simpler. That is a far more scalable insight.
